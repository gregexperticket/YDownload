namespace YDownload;

public sealed record DownloadResult(
    string FilePath,
    string Title,
    string Channel,
    TimeSpan? Duration,
    bool CoverEmbedded);
