namespace YDownload;

public sealed class FFmpegException : Exception
{
    public FFmpegException(string message) : base(message) { }

    public FFmpegException(string message, Exception inner) : base(message, inner) { }
}
