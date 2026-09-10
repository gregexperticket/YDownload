using TagLib;
using File = TagLib.File;

namespace YDownload;

public sealed record CoverImage(byte[] Data, string MimeType);

public sealed record Mp3TagInfo(string Title, string Artist, uint Year, string Comment)
{
    public CoverImage? Cover { get; init; }
}

public static class Mp3Tags
{
    static Mp3Tags()
    {
        TagLib.Id3v2.Tag.DefaultVersion = 3;
        TagLib.Id3v2.Tag.ForceDefaultVersion = true;
    }

    public static void Write(string mp3Path, Mp3TagInfo info)
    {
        using var file = File.Create(mp3Path);

        file.Tag.Title = info.Title;
        file.Tag.Performers = [info.Artist];
        file.Tag.Year = info.Year;
        file.Tag.Comment = info.Comment;

        if (info.Cover is { } cover)
        {
            file.Tag.Pictures =
            [
                new Picture(new ByteVector(cover.Data))
                {
                    Type = PictureType.FrontCover,
                    MimeType = cover.MimeType,
                },
            ];
        }

        file.Save();
    }
}
