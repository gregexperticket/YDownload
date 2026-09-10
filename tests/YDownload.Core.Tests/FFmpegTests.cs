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
        GenerateTone(wav, seconds: 2);

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

    private static void GenerateTone(string path, int seconds)
    {
        using var process = Process.Start(new ProcessStartInfo(FFmpeg.Locate())
        {
            ArgumentList = { "-y", "-hide_banner", "-loglevel", "error", "-f", "lavfi", "-i", $"sine=frequency=440:duration={seconds}", path },
            UseShellExecute = false,
            CreateNoWindow = true,
        })!;

        process.WaitForExit();
        Assert.Equal(0, process.ExitCode);
    }

    private sealed class SyncProgress(Action<double> callback) : IProgress<double>
    {
        public void Report(double value) => callback(value);
    }
}
