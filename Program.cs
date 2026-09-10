// See https://aka.ms/new-console-template for more information

using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

Console.WriteLine("Conectando con YouTube");

var videoUrl = "https://www.youtube.com/watch?v=yZujhNsv7Rw";
var outputPath = "c:/YDownloads/";

try
{
    var youtube = new YoutubeClient();
    var video = await youtube.Videos.GetAsync(videoUrl);

    var streamInfoSet = await youtube.Videos.Streams.GetManifestAsync(video.Id);
    var streamInfo = streamInfoSet.GetAudioOnlyStreams().GetWithHighestBitrate();

    if (streamInfo != null!)
    {
        var ext = streamInfo.Container.Name;
        var safeVideoName = SanitizeVideoName(video.Title);

        Console.WriteLine($"Descargando vídeo {video.Title}.{ext}");
        
        var outputFilePath = Path.Combine(outputPath, $"{safeVideoName}.{ext}");
        await youtube.Videos.Streams.DownloadAsync(streamInfo, outputFilePath);
        
        Console.WriteLine($"Vídeo guardado en {outputFilePath}");

        var outputMp3Path = Path.Combine(outputPath, $"{safeVideoName}.mp3");
        ConvertToMp3AndSave(outputFilePath, outputMp3Path);

        Console.WriteLine("Audio convertido y guardado");
        
        Console.WriteLine($"Borrando vídeo");
        File.Delete(outputFilePath);

        Console.WriteLine("Proceso finalizado, pulsa una tecla para salir...");
        Console.ReadKey();
    }
    else
    {
        Console.WriteLine("No se encontró audio disponible para descargar en este video");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error al descargar el audio: {ex.Message}");
    Console.WriteLine("Pulsa una tecla para salir...");
    Console.ReadKey();
}

static void ConvertToMp3AndSave(string inputPath, string outputPath)
{
    Console.WriteLine($"Convirtiendo a Mp3 en {outputPath}");
    using var reader = new MediaFoundationReader(inputPath);
    MediaFoundationEncoder.EncodeToMp3(reader, outputPath);
}

static string SanitizeVideoName(string filename)
{
    var invalids = Path.GetInvalidFileNameChars();
    filename = invalids.Aggregate(filename, (current, c) => current.Replace(c, '_'));

    return filename;
}