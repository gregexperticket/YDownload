namespace YDownload.Core.Tests;

public class FileNamesTests
{
    [Theory]
    [InlineData(@"a/b\c:d*e?f""g<h>i|j", "a_b_c_d_e_f_g_h_i_j")]
    [InlineData("  título con espacios  ", "título con espacios")]
    [InlineData("acaba en puntos...", "acaba en puntos")]
    [InlineData("acaba en punto y espacio. ", "acaba en punto y espacio")]
    [InlineData("con\ttabulador", "con_tabulador")]
    public void Sanitize_ReplacesInvalidCharsAndTrims(string input, string expected)
        => Assert.Equal(expected, FileNames.Sanitize(input));

    [Theory]
    [InlineData("CON")]
    [InlineData("con")]
    [InlineData("LPT1")]
    public void Sanitize_PrefixesReservedNames(string name)
        => Assert.Equal("_" + name, FileNames.Sanitize(name));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("...")]
    public void Sanitize_NeverReturnsEmpty(string input)
        => Assert.Equal("_", FileNames.Sanitize(input));

    [Fact]
    public void Sanitize_TruncatesToMaxLength()
        => Assert.Equal(20, FileNames.Sanitize(new string('x', 300), maxLength: 20).Length);

    [Fact]
    public void Unique_AppendsCounterWhenFileExists()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(dir, "tema.mp3"), "");
            File.WriteAllText(Path.Combine(dir, "tema (2).mp3"), "");

            Assert.Equal(Path.Combine(dir, "tema (3).mp3"), FileNames.Unique(dir, "tema", ".mp3"));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
