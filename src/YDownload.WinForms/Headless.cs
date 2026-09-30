using System.Runtime.InteropServices;
using YoutubeExplode.Exceptions;

namespace YDownload.WinForms;

internal static class Headless
{
    private const int AttachParentProcess = -1;

    public static int Run(string[] args)
    {
        if (AttachConsole(AttachParentProcess) && !Console.IsOutputRedirected)
            Console.WriteLine();

        var options = new DownloadOptions();
        string? input = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var value = i + 1 < args.Length ? args[i + 1] : null;

            switch (arg)
            {
                case "-h" or "--help":
                    PrintUsage();
                    return 0;

                case "-m" or "--mode" when value is not null:
                    DownloadMode? mode = value.ToLowerInvariant() switch
                    {
                        "audio" => DownloadMode.Audio,
                        "video" or "vídeo" => DownloadMode.Video,
                        _ => null,
                    };
                    if (mode is null)
                        return Usage("El modo debe ser audio o video.");
                    options = options with { Mode = mode.Value };
                    i++;
                    break;

                case "-o" or "--out" when value is not null:
                    options = options with { OutputDirectory = value };
                    i++;
                    break;

                case "-b" or "--bitrate" when value is not null:
                    if (!int.TryParse(value, out var kbps) || kbps is < 32 or > 320)
                        return Usage("El bitrate debe ser un entero entre 32 y 320.");
                    options = options with { BitrateKbps = kbps };
                    i++;
                    break;

                case "--ffmpeg" when value is not null:
                    options = options with { FFmpegPath = value };
                    i++;
                    break;

                case "-m" or "--mode" or "-o" or "--out" or "-b" or "--bitrate" or "--ffmpeg":
                    return Usage($"Falta el valor de {arg}.");

                case var _ when arg.StartsWith('-'):
                    return Usage($"Opción desconocida: {arg}");

                default:
                    if (input is not null)
                        return Usage("Sólo se admite un vídeo por ejecución.");
                    input = arg;
                    break;
            }
        }

        if (input is null)
            return Usage("Falta la URL o el id del vídeo.");

        return DownloadAsync(input, options).GetAwaiter().GetResult();
    }

    private static async Task<int> DownloadAsync(string input, DownloadOptions options)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var progress = new ConsoleProgress();

        try
        {
            var result = await new Downloader().DownloadAsync(input, options, progress, cts.Token);
            progress.Finish();

            Console.WriteLine($"Guardado en {result.FilePath}");
            if (options.Mode == DownloadMode.Audio && !result.CoverEmbedded)
                Console.WriteLine("No se ha podido descargar la carátula; el MP3 se ha guardado sin ella.");

            return 0;
        }
        catch (OperationCanceledException)
        {
            progress.Finish();
            Console.Error.WriteLine("Cancelado.");
            return 1;
        }
        catch (Exception ex) when (ex is YoutubeExplodeException or FFmpegException or InvalidOperationException
                                       or ArgumentException or IOException or UnauthorizedAccessException)
        {
            progress.Finish();
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            progress.Finish();
            Console.Error.WriteLine("Error inesperado:");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int Usage(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("Usa --help para ver las opciones.");
        return 2;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Uso: YDownload [<url o id de vídeo> [opciones]]");
        Console.WriteLine();
        Console.WriteLine("Sin argumentos abre la ventana. Con una URL descarga sin mostrar interfaz: el audio");
        Console.WriteLine("como MP3 o, con --mode video, el vídeo completo como MP4.");
        Console.WriteLine();
        Console.WriteLine("Opciones:");
        Console.WriteLine("  -m, --mode <audio|video>  Qué descargar: sólo el audio (por defecto) o el vídeo completo");
        Console.WriteLine("  -o, --out <carpeta>       Carpeta de salida (por defecto, la actual)");
        Console.WriteLine("  -b, --bitrate <kbps>      Bitrate del MP3, entre 32 y 320 (por defecto, 192); sólo audio");
        Console.WriteLine("      --ffmpeg <ruta>       Ejecutable de ffmpeg, si no está en el PATH");
        Console.WriteLine("  -h, --help                Muestra esta ayuda");
    }

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int processId);
}
