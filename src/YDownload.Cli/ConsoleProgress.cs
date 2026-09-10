namespace YDownload.Cli;

internal sealed class ConsoleProgress : IProgress<DownloadProgress>
{
    private const int BarWidth = 30;

    private static readonly bool Interactive = !Console.IsOutputRedirected;

    private DownloadStage? _stage;
    private int _percent = -1;

    public void Report(DownloadProgress value)
    {
        var percent = (int)Math.Round(value.Fraction * 100);
        if (value.Stage == _stage && percent == _percent)
            return;

        if (value.Stage != _stage)
        {
            Finish();
            _stage = value.Stage;
            if (!Interactive)
                Console.WriteLine(Label(value.Stage));
        }

        _percent = percent;
        if (!Interactive)
            return;

        if (value.Stage == DownloadStage.Resolving)
        {
            Console.Write("Resolviendo vídeo...");
            return;
        }

        var filled = percent * BarWidth / 100;
        Console.Write($"\r{Label(value.Stage),-12} [{new string('#', filled)}{new string('-', BarWidth - filled)}] {percent,3} %");
    }

    public void Finish()
    {
        if (Interactive && _stage is not null)
            Console.WriteLine();

        _stage = null;
        _percent = -1;
    }

    private static string Label(DownloadStage stage) => stage switch
    {
        DownloadStage.Resolving => "Resolviendo",
        DownloadStage.Downloading => "Descargando",
        DownloadStage.Converting => "Convirtiendo",
        DownloadStage.Tagging => "Etiquetando",
        _ => stage.ToString(),
    };
}
