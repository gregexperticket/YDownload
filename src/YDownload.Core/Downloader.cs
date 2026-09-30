using System.Globalization;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YDownload;

public sealed class Downloader
{
    private readonly HttpClient _http;
    private readonly YoutubeClient _youtube;

    public Downloader(HttpClient? http = null)
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

        var video = await _youtube.Videos.GetAsync(videoId, ct).ConfigureAwait(false);
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, ct).ConfigureAwait(false);

        return options.Mode == DownloadMode.Video
            ? await DownloadVideoAsync(video, manifest, options, progress, ct).ConfigureAwait(false)
            : await DownloadAudioAsync(video, manifest, options, progress, ct).ConfigureAwait(false);
    }

    private async Task<DownloadResult> DownloadAudioAsync(
        Video video,
        StreamManifest manifest,
        DownloadOptions options,
        IProgress<DownloadProgress>? progress,
        CancellationToken ct)
    {
        var stream = manifest.GetAudioOnlyStreams().TryGetWithHighestBitrate()
            ?? throw new InvalidOperationException("El vídeo no tiene ninguna pista de sólo audio.");

        Directory.CreateDirectory(options.OutputDirectory);
        var mp3Path = FileNames.Unique(options.OutputDirectory, FileNames.Sanitize(video.Title), ".mp3");
        var sourcePath = TempPath(video.Id, "audio", stream);

        try
        {
            await _youtube.Videos.Streams.DownloadAsync(
                stream, sourcePath, StageProgress.For(progress, DownloadStage.Downloading), ct).ConfigureAwait(false);

            await new FFmpeg(options.FFmpegPath).ConvertToMp3Async(
                sourcePath, mp3Path, options.BitrateKbps, video.Duration,
                StageProgress.For(progress, DownloadStage.Converting), ct).ConfigureAwait(false);
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

        var cover = await TryGetCoverAsync(video, ct).ConfigureAwait(false);
        Mp3Tags.Write(mp3Path, new Mp3TagInfo(video.Title, video.Author.ChannelTitle, (uint)video.UploadDate.Year, video.Url)
        {
            Cover = cover,
        });

        progress?.Report(new(DownloadStage.Tagging, 1));

        return new DownloadResult(mp3Path, video.Title, video.Author.ChannelTitle, video.Duration, cover is not null);
    }

    private async Task<DownloadResult> DownloadVideoAsync(
        Video video,
        StreamManifest manifest,
        DownloadOptions options,
        IProgress<DownloadProgress>? progress,
        CancellationToken ct)
    {
        // YouTube sólo ofrece vídeo y audio juntos en baja calidad, así que se descargan por separado
        // y ffmpeg los une sin recodificar. A igual calidad se prefiere H.264, que se reproduce en
        // cualquier sitio, y AAC, que es el audio nativo de MP4.
        var videoStream = manifest.GetVideoOnlyStreams()
            .OrderByDescending(s => s.VideoQuality)
            .ThenByDescending(s => s.VideoCodec.StartsWith("avc1", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(s => s.Bitrate)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("El vídeo no tiene ninguna pista de vídeo.");

        var audioStream = manifest.GetAudioOnlyStreams()
            .OrderByDescending(s => s.Container == Container.Mp4)
            .ThenByDescending(s => s.Bitrate)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("El vídeo no tiene ninguna pista de sólo audio.");

        Directory.CreateDirectory(options.OutputDirectory);
        var mp4Path = FileNames.Unique(options.OutputDirectory, FileNames.Sanitize(video.Title), ".mp4");
        var videoPath = TempPath(video.Id, "video", videoStream);
        var audioPath = TempPath(video.Id, "audio", audioStream);

        // La barra de descarga avanza en proporción al tamaño de cada pista.
        var totalBytes = videoStream.Size.Bytes + audioStream.Size.Bytes;
        var videoShare = totalBytes > 0 ? (double)videoStream.Size.Bytes / totalBytes : 0.5;

        var metadata = new Dictionary<string, string>
        {
            ["title"] = video.Title,
            ["artist"] = video.Author.ChannelTitle,
            ["date"] = video.UploadDate.Year.ToString(CultureInfo.InvariantCulture),
            ["comment"] = video.Url,
        };

        try
        {
            await _youtube.Videos.Streams.DownloadAsync(
                videoStream, videoPath, StageProgress.For(progress, DownloadStage.Downloading, 0, videoShare), ct).ConfigureAwait(false);

            await _youtube.Videos.Streams.DownloadAsync(
                audioStream, audioPath, StageProgress.For(progress, DownloadStage.Downloading, videoShare, 1 - videoShare), ct).ConfigureAwait(false);

            await new FFmpeg(options.FFmpegPath).MergeToMp4Async(
                videoPath, audioPath, mp4Path, metadata, video.Duration,
                StageProgress.For(progress, DownloadStage.Merging), ct).ConfigureAwait(false);
        }
        catch
        {
            TryDelete(mp4Path);
            throw;
        }
        finally
        {
            TryDelete(videoPath);
            TryDelete(audioPath);
        }

        return new DownloadResult(mp4Path, video.Title, video.Author.ChannelTitle, video.Duration, CoverEmbedded: false);
    }

    private static string TempPath(VideoId videoId, string track, IStreamInfo stream) =>
        Path.Combine(Path.GetTempPath(), $"ydownload-{videoId}-{track}.{stream.Container.Name}");

    private async Task<CoverImage?> TryGetCoverAsync(Video video, CancellationToken ct)
    {
        var thumbnail = video.Thumbnails
            .Where(t => !t.Url.Contains("webp", StringComparison.OrdinalIgnoreCase))
            .MaxBy(t => t.Resolution.Area);

        if (thumbnail is null)
            return null;

        try
        {
            using var response = await _http.GetAsync(thumbnail.Url, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
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

    private sealed class StageProgress(IProgress<DownloadProgress> inner, DownloadStage stage, double start, double length) : IProgress<double>
    {
        public static IProgress<double>? For(IProgress<DownloadProgress>? inner, DownloadStage stage, double start = 0, double length = 1) =>
            inner is null ? null : new StageProgress(inner, stage, start, length);

        public void Report(double value) => inner.Report(new DownloadProgress(stage, start + value * length));
    }
}
