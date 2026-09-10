namespace YDownload;

public enum DownloadStage
{
    Resolving,
    Downloading,
    Converting,
    Tagging,
}

public readonly record struct DownloadProgress(DownloadStage Stage, double Fraction);
