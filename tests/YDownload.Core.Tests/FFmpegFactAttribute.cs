using System.ComponentModel;
using System.Diagnostics;

namespace YDownload.Core.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FFmpegFactAttribute : FactAttribute
{
    private static readonly bool Available = Probe();

    public FFmpegFactAttribute()
    {
        if (!Available)
            Skip = "ffmpeg no está disponible";
    }

    private static bool Probe()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(FFmpeg.Locate(), "-version")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            })!;

            process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }
}
