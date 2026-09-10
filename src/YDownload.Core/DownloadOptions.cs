namespace YDownload;

public sealed record DownloadOptions
{
    public string OutputDirectory { get; init; } = Environment.CurrentDirectory;
    public int BitrateKbps { get; init; } = 192;
    public string? FFmpegPath { get; init; }
}
