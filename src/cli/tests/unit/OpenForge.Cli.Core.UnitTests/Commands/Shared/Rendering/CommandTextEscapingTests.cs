using OpenForge.Cli.Core.Commands.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Rendering;

public sealed class CommandTextEscapingTests
{
    [Theory(DisplayName = "Command text escaping retains the exact owned Unicode and control spelling"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData("", "")]
    [InlineData("a\"\\b\n\r\t\u0085\u0000", "a\\\"\\\\b\\u000a\\u000d\\u0009\\u0085\\u0000")]
    [InlineData("Café漢字\u2028\u2029", "Café漢字\u2028\u2029")]
    [InlineData("a😀b", "a😀b")]
    public void EscapesCompleteValues(string value, string expected)
    {
        Assert.Equal(expected, CommandTextEscaping.Escape(value));
    }

    [Fact(DisplayName = "Command text escaping escapes an isolated high surrogate without consuming adjacent text"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void EscapesIsolatedHighSurrogate()
    {
        const string value = "a\ud83db";
        const string expected = "a\\ud83db";

        Assert.Equal(expected, CommandTextEscaping.Escape(value));
    }

    [Fact(DisplayName = "Command text escaping escapes an isolated low surrogate"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    public void EscapesIsolatedLowSurrogate()
    {
        const string value = "\ude00";
        const string expected = "\\ude00";

        Assert.Equal(expected, CommandTextEscaping.Escape(value));
    }

    [Theory(DisplayName = "Command text limits retain complete tokens and the exact bounded ellipsis"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData("", 1, "")]
    [InlineData("abc", 3, "abc")]
    [InlineData("abcdef", 1, ".")]
    [InlineData("abcdef", 2, "..")]
    [InlineData("abcdef", 3, "...")]
    [InlineData("abcdef", 4, "a...")]
    [InlineData("\u0001value", 7, "...")]
    [InlineData("\u0001value", 9, "\\u0001...")]
    [InlineData("😀abcd", 4, "...")]
    [InlineData("😀abcd", 5, "😀...")]
    [InlineData("a😀bcd", 5, "a...")]
    [InlineData("\"abcd", 4, "...")]
    [InlineData("\"abcd", 5, "\\\"...")]
    public void LimitsOutputAtCompleteTokens(string value, int maximumLength, string expected)
    {
        Assert.Equal(expected, CommandTextEscaping.Escape(value, maximumLength));
    }

    [Theory(DisplayName = "Command text escaping rejects nonpositive limits with its exact argument facts"), Trait("Feature", "command-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonpositiveLimits(int maximumLength)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CommandTextEscaping.Escape("x", maximumLength));

        Assert.Equal("maximumLength", exception.ParamName);
        Assert.Equal(maximumLength, exception.ActualValue);
        Assert.Equal(
            new ArgumentOutOfRangeException(
                paramName: "maximumLength",
                actualValue: maximumLength,
                message: "The text limit must be positive.").Message,
            exception.Message);
    }
}
