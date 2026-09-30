namespace YDownload;

public enum DownloadStage
{
    Resolving,
    Downloading,
    Converting,
    Merging,
    Tagging,
}

public readonly record struct DownloadProgress(DownloadStage Stage, double Fraction);
