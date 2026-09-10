namespace YDownload;

public static class FileNames
{
    private const string InvalidChars = @"""<>|:*?\/";

    private static readonly HashSet<string> Reserved = new(
        new[] { "CON", "PRN", "AUX", "NUL" }
            .Concat(Enumerable.Range(1, 9).SelectMany(i => new[] { $"COM{i}", $"LPT{i}" })),
        StringComparer.OrdinalIgnoreCase);

    public static string Sanitize(string name, int maxLength = 150)
    {
        var chars = name.Select(c => InvalidChars.Contains(c) || char.IsControl(c) ? '_' : c).ToArray();
        var result = new string(chars).Trim().TrimEnd('.', ' ');

        if (result.Length > maxLength)
            result = result[..maxLength].TrimEnd('.', ' ');

        if (result.Length == 0)
            return "_";

        return Reserved.Contains(result) ? "_" + result : result;
    }

    public static string Unique(string directory, string baseName, string extension)
    {
        var path = Path.Combine(directory, baseName + extension);
        for (var i = 2; File.Exists(path); i++)
            path = Path.Combine(directory, $"{baseName} ({i}){extension}");

        return path;
    }
}
