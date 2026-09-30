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

    public Task ConvertToMp3Async(
        string inputPath,
        string outputPath,
        int bitrateKbps,
        TimeSpan? duration,
        IProgress<double>? progress = null,
        CancellationToken ct = default) =>
        RunAsync(
        [
            "-i", inputPath,
            "-vn", "-map_metadata", "-1",
            "-codec:a", "libmp3lame", "-b:a", $"{bitrateKbps}k",
            outputPath,
        ], duration, progress, ct);

    public Task MergeToMp4Async(
        string videoPath,
        string audioPath,
        string outputPath,
        IReadOnlyDictionary<string, string> metadata,
        TimeSpan? duration,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        List<string> args =
        [
            "-i", videoPath,
            "-i", audioPath,
            "-map", "0:v:0", "-map", "1:a:0",
            "-codec", "copy", "-map_metadata", "-1",
        ];

        foreach (var (key, value) in metadata)
            args.AddRange(["-metadata", $"{key}={value}"]);

        args.Add(outputPath);
        return RunAsync(args, duration, progress, ct);
    }

    private async Task RunAsync(IEnumerable<string> args, TimeSpan? duration, IProgress<double>? progress, CancellationToken ct)
    {
        var startInfo = new ProcessStartInfo(_executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var arg in new[] { "-y", "-hide_banner", "-nostats", "-loglevel", "error", "-progress", "pipe:1" }.Concat(args))
            startInfo.ArgumentList.Add(arg);

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
            while (await process.StandardOutput.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
            {
                if (duration is { } total
                    && line.StartsWith(TimePrefix, StringComparison.Ordinal)
                    && long.TryParse(line.AsSpan(TimePrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var elapsedUs))
                {
                    progress?.Report(Math.Clamp(elapsedUs / total.TotalMicroseconds, 0, 1));
                }
            }

            await process.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            throw;
        }

        if (process.ExitCode != 0)
        {
            var error = (await stderr.ConfigureAwait(false)).Trim();
            throw new FFmpegException(error.Length > 0
                ? $"ffmpeg terminó con código {process.ExitCode}: {error}"
                : $"ffmpeg terminó con código {process.ExitCode}.");
        }

        progress?.Report(1);
    }
}
