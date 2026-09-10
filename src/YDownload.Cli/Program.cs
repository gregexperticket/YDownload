using System.Text;
using YDownload;
using YDownload.Cli;
using YoutubeExplode.Exceptions;

Console.OutputEncoding = Encoding.UTF8;

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

        case "-o" or "--out" or "-b" or "--bitrate" or "--ffmpeg":
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
{
    PrintUsage();
    return 2;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var progress = new ConsoleProgress();

try
{
    var result = await new AudioDownloader().DownloadAsync(input, options, progress, cts.Token);
    progress.Finish();

    Console.WriteLine($"Guardado en {result.FilePath}");
    if (!result.CoverEmbedded)
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

static int Usage(string message)
{
    Console.Error.WriteLine(message);
    Console.Error.WriteLine("Usa --help para ver las opciones.");
    return 2;
}

static void PrintUsage()
{
    Console.WriteLine("Uso: YDownload <url o id de vídeo> [opciones]");
    Console.WriteLine();
    Console.WriteLine("Descarga la pista de audio de un vídeo de YouTube y la guarda como MP3 con etiquetas ID3.");
    Console.WriteLine();
    Console.WriteLine("Opciones:");
    Console.WriteLine("  -o, --out <carpeta>     Carpeta de salida (por defecto, la actual)");
    Console.WriteLine("  -b, --bitrate <kbps>    Bitrate del MP3, entre 32 y 320 (por defecto, 192)");
    Console.WriteLine("      --ffmpeg <ruta>     Ejecutable de ffmpeg, si no está en el PATH");
    Console.WriteLine("  -h, --help              Muestra esta ayuda");
}
