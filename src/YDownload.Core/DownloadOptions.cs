namespace YDownload;

public enum DownloadMode
{
    Audio,
    Video,
}

public sealed record DownloadOptions
{
    public DownloadMode Mode { get; init; } = DownloadMode.Audio;
    public string OutputDirectory { get; init; } = Environment.CurrentDirectory;
    public int BitrateKbps { get; init; } = 192;
    public string? FFmpegPath { get; init; }
}
