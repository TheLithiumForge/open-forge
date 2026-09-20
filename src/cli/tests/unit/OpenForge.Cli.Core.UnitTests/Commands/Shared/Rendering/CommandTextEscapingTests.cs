using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Rendering;

public sealed class CommandTextEscapingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Command text uses the shared visible control and Unicode spelling"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData("", "")]
    [InlineData("a\"\\b\n\r\t\u0085\u0000", "a\"\\b\\n\\r\\t\\u0085\\u0000")]
    [InlineData("Café漢字\u2028\u2029", "Café漢字\u2028\u2029")]
    [InlineData("a😀b", "a😀b")]
    public void EscapesCompleteValues(string value, string expected) => Assert.Equal(expected, CliText.Escape(value));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command text escapes isolated surrogates without consuming adjacent text"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void EscapesIsolatedSurrogates()
    {
        Assert.Equal("a\\uD83Db", CliText.Escape("a\ud83db"));
        Assert.Equal("\\uDE00", CliText.Escape("\ude00"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command primary text has no diagnostic length cap"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void PrimaryTextIsNotTruncated()
    {
        var value = new string('x', 5000) + "😀 end";
        Assert.Equal(value, CliText.Escape(value));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Command diagnostic clamping keeps complete Unicode scalars"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData("abcdef", 0, "")]
    [InlineData("abcdef", 1, ".")]
    [InlineData("abcdef", 2, "..")]
    [InlineData("abcdef", 3, "...")]
    [InlineData("abcdef", 4, "a...")]
    [InlineData("😀abcd", 4, "...")]
    [InlineData("😀abcd", 5, "😀...")]
    [InlineData("a😀bcd", 5, "a...")]
    public void DiagnosticClampKeepsScalars(string value, int maximum, string expected)
        => Assert.Equal(expected, CliText.Clamp(value, maximum));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command diagnostic clamping rejects negative limits"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void NegativeLimitIsInvalid() => Assert.Throws<ArgumentOutOfRangeException>(() => CliText.Clamp("x", -1));
}
