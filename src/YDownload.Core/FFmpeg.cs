using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace YDownload;

public sealed class FFmpeg
{
    private const string TimePrefix = "out_time_us=";

    private readonly string _executable;

    public FFmpeg(string? executable = null)
    {
        _executable = executable ?? Locate();
    }

    public static string Locate()
    {
        var name = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var local = Path.Combine(AppContext.BaseDirectory, name);
        return File.Exists(local) ? local : name;
    }

    public async Task ConvertToMp3Async(
        string inputPath,
        string outputPath,
        int bitrateKbps,
        TimeSpan? duration,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var startInfo = new ProcessStartInfo(_executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var arg in new[]
                 {
                     "-y", "-hide_banner", "-nostats", "-loglevel", "error",
                     "-progress", "pipe:1",
                     "-i", inputPath,
                     "-vn", "-map_metadata", "-1",
                     "-codec:a", "libmp3lame", "-b:a", $"{bitrateKbps}k",
                     outputPath,
                 })
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new FFmpegException($"No se encuentra el ejecutable de ffmpeg ({_executable}). Instálalo o indica la ruta correcta.", ex);
        }

        var stderr = process.StandardError.ReadToEndAsync();

        try
        {
            while (await process.StandardOutput.ReadLineAsync(ct) is { } line)
            {
                if (duration is { } total
                    && line.StartsWith(TimePrefix, StringComparison.Ordinal)
                    && long.TryParse(line.AsSpan(TimePrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var elapsedUs))
                {
                    progress?.Report(Math.Clamp(elapsedUs / total.TotalMicroseconds, 0, 1));
                }
            }

            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            throw;
        }

        if (process.ExitCode != 0)
        {
            var error = (await stderr).Trim();
            throw new FFmpegException(error.Length > 0
                ? $"ffmpeg terminó con código {process.ExitCode}: {error}"
                : $"ffmpeg terminó con código {process.ExitCode}.");
        }

        progress?.Report(1);
    }
}
