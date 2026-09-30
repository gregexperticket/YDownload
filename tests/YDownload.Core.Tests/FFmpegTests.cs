using System.Diagnostics;

namespace YDownload.Core.Tests;

public class FFmpegTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory().FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [FFmpegFact]
    public async Task ConvertToMp3_ProducesTaggableFileAndReportsProgress()
    {
        var wav = Path.Combine(_dir, "tono.wav");
        var mp3 = Path.Combine(_dir, "tono.mp3");
        Generate(wav, "sine=frequency=440:duration=2");

        var reported = new List<double>();
        await new FFmpeg().ConvertToMp3Async(wav, mp3, 128, TimeSpan.FromSeconds(2), new SyncProgress(reported.Add));

        Assert.True(new FileInfo(mp3).Length > 0);
        Assert.Equal(1.0, reported.Last());

        Mp3Tags.Write(mp3, new Mp3TagInfo("Tono", "Prueba", 2024, "comentario")
        {
            Cover = new CoverImage([0xFF, 0xD8, 0xFF, 0xD9], "image/jpeg"),
        });

        using var tagged = TagLib.File.Create(mp3);
        Assert.Equal("Tono", tagged.Tag.Title);
        Assert.Equal(new[] { "Prueba" }, tagged.Tag.Performers);
        Assert.Equal(2024u, tagged.Tag.Year);
        Assert.Equal("comentario", tagged.Tag.Comment);
        Assert.Single(tagged.Tag.Pictures);
    }

    [FFmpegFact]
    public async Task MergeToMp4_KeepsBothTracksAndWritesMetadata()
    {
        var video = Path.Combine(_dir, "imagen.mp4");
        var audio = Path.Combine(_dir, "tono.m4a");
        var mp4 = Path.Combine(_dir, "unido.mp4");
        Generate(video, "testsrc=duration=2:size=320x240:rate=25", "-codec:v", "mpeg4");
        Generate(audio, "sine=frequency=440:duration=2", "-codec:a", "aac");

        var reported = new List<double>();
        var metadata = new Dictionary<string, string>
        {
            ["title"] = "Prueba: vídeo",
            ["artist"] = "Canal",
            ["date"] = "2024",
            ["comment"] = "https://www.youtube.com/watch?v=abc",
        };
        await new FFmpeg().MergeToMp4Async(video, audio, mp4, metadata, TimeSpan.FromSeconds(2), new SyncProgress(reported.Add));

        Assert.Equal(1.0, reported.Last());

        using var merged = TagLib.File.Create(mp4);
        Assert.True(merged.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video));
        Assert.True(merged.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio));
        Assert.Equal("Prueba: vídeo", merged.Tag.Title);
        Assert.Equal(new[] { "Canal" }, merged.Tag.Performers);
        Assert.Equal(2024u, merged.Tag.Year);
        Assert.Equal("https://www.youtube.com/watch?v=abc", merged.Tag.Comment);
    }

    [FFmpegFact]
    public async Task ConvertToMp3_FailsWithMessageOnInvalidInput()
    {
        var input = Path.Combine(_dir, "basura.webm");
        File.WriteAllText(input, "esto no es audio");

        var ex = await Assert.ThrowsAsync<FFmpegException>(
            () => new FFmpeg().ConvertToMp3Async(input, Path.Combine(_dir, "basura.mp3"), 128, null));

        Assert.Contains("ffmpeg", ex.Message);
    }

    [Fact]
    public async Task ConvertToMp3_ThrowsWhenExecutableIsMissing()
    {
        var missing = Path.Combine(_dir, "no-existe.exe");

        var ex = await Assert.ThrowsAsync<FFmpegException>(
            () => new FFmpeg(missing).ConvertToMp3Async("in.wav", "out.mp3", 128, null));

        Assert.Contains("No se encuentra", ex.Message);
    }

    private static void Generate(string path, string lavfiSource, params string[] outputArgs)
    {
        var startInfo = new ProcessStartInfo(FFmpeg.Locate())
        {
            ArgumentList = { "-y", "-hide_banner", "-loglevel", "error", "-f", "lavfi", "-i", lavfiSource },
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in outputArgs.Append(path))
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo)!;

        process.WaitForExit();
        Assert.Equal(0, process.ExitCode);
    }

    private sealed class SyncProgress(Action<double> callback) : IProgress<double>
    {
        public void Report(double value) => callback(value);
    }
}
