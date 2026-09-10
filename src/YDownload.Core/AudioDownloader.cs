using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YDownload;

public sealed class AudioDownloader
{
    private readonly HttpClient _http;
    private readonly YoutubeClient _youtube;

    public AudioDownloader(HttpClient? http = null)
    {
        _http = http ?? new HttpClient();
        _youtube = new YoutubeClient(_http);
    }

    public async Task<DownloadResult> DownloadAsync(
        string videoUrlOrId,
        DownloadOptions options,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        var videoId = VideoId.TryParse(videoUrlOrId)
            ?? throw new ArgumentException($"'{videoUrlOrId}' no es una URL ni un id de vídeo de YouTube válido.");

        progress?.Report(new(DownloadStage.Resolving, 0));

        var video = await _youtube.Videos.GetAsync(videoId, ct);
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, ct);
        var stream = manifest.GetAudioOnlyStreams().TryGetWithHighestBitrate()
            ?? throw new InvalidOperationException("El vídeo no tiene ninguna pista de sólo audio.");

        Directory.CreateDirectory(options.OutputDirectory);
        var mp3Path = FileNames.Unique(options.OutputDirectory, FileNames.Sanitize(video.Title), ".mp3");
        var sourcePath = Path.Combine(Path.GetTempPath(), $"ydownload-{videoId}.{stream.Container.Name}");

        try
        {
            await _youtube.Videos.Streams.DownloadAsync(
                stream, sourcePath, StageProgress.For(progress, DownloadStage.Downloading), ct);

            await new FFmpeg(options.FFmpegPath).ConvertToMp3Async(
                sourcePath, mp3Path, options.BitrateKbps, video.Duration,
                StageProgress.For(progress, DownloadStage.Converting), ct);
        }
        catch
        {
            TryDelete(mp3Path);
            throw;
        }
        finally
        {
            TryDelete(sourcePath);
        }

        progress?.Report(new(DownloadStage.Tagging, 0));

        var cover = await TryGetCoverAsync(video, ct);
        Mp3Tags.Write(mp3Path, new Mp3TagInfo(video.Title, video.Author.ChannelTitle, (uint)video.UploadDate.Year, video.Url)
        {
            Cover = cover,
        });

        progress?.Report(new(DownloadStage.Tagging, 1));

        return new DownloadResult(mp3Path, video.Title, video.Author.ChannelTitle, video.Duration, cover is not null);
    }

    private async Task<CoverImage?> TryGetCoverAsync(Video video, CancellationToken ct)
    {
        var thumbnail = video.Thumbnails
            .Where(t => !t.Url.Contains("webp", StringComparison.OrdinalIgnoreCase))
            .MaxBy(t => t.Resolution.Area);

        if (thumbnail is null)
            return null;

        try
        {
            using var response = await _http.GetAsync(thumbnail.Url, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadAsByteArrayAsync(ct);
            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return new CoverImage(data, mimeType);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private sealed class StageProgress(IProgress<DownloadProgress> inner, DownloadStage stage) : IProgress<double>
    {
        public static IProgress<double>? For(IProgress<DownloadProgress>? inner, DownloadStage stage) =>
            inner is null ? null : new StageProgress(inner, stage);

        public void Report(double value) => inner.Report(new DownloadProgress(stage, value));
    }
}
