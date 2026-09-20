using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Text;

public sealed class CliTextTests
{
    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData("plain\\path \"<>& é 😀", "plain\\path \"<>& é 😀")]
    [InlineData("a\nb\rc\td", @"a\nb\rc\td")]
    [InlineData("\u0000\u001b\u007f", @"\u0000\u001B\u007F")]
    public void TextEscapesOnlyControls(string input, string expected)
        => Assert.Equal(expected, CliText.Escape(input));

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void UnpairedSurrogatesAreVisibleAndPairsArePreserved()
        => Assert.Equal(@"\uD800x\uDC00😀", CliText.Escape("\ud800x\udc00😀"));

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData("a\nb\rc\r\nd", "a|b|c|d")]
    [InlineData("a\r\nb\nc\rd", "a|b|c|d")]
    [InlineData("path\u2028part\u2029end\nnext", "path\u2028part\u2029end|next")]
    public void FramingUsesPlatformLineEndings(string input, string expected)
        => Assert.Equal(expected.Replace("|", Environment.NewLine), CliText.PlatformLineEndings(input));

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void DiagnosticClampRespectsLimitAndDoesNotSplitAScalar()
    {
        Assert.Equal(new string('x', 240), CliText.Clamp(new string('x', 240), 240));
        Assert.Equal(new string('x', 237) + "...", CliText.Clamp(new string('x', 241), 240));
        Assert.Equal("...", CliText.Clamp("😀abcd", 4));
        Assert.Equal("😀", CliText.Clamp("😀", 2));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void TableAlignsCompleteEscapedCells()
    {
        IReadOnlyList<IReadOnlyList<string>> rows = [new[] { "a", "first" }, new[] { "long", "second" }];
        Assert.Equal("  a     first\n  long  second\n", CliTable.Render(rows));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void TableStylesEscapedCellsAfterMeasuringVisibleWidths()
    {
        IReadOnlyList<IReadOnlyList<string>> rows = [new[] { "a\n", "first" }, new[] { "long😀", "second" }];
        var text = CliTable.Render(rows, static (column, cell) => column == 0 ? CliTextStyle.Color.Subject(cell) : cell);
        Assert.Equal("  \u001b[1ma\\n\u001b[22m    first\n  \u001b[1mlong😀\u001b[22m  second\n", text);
        Assert.Equal("  a\\n    first\n  long😀  second\n", CliTable.Render(rows));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void QuantitiesHaveDeterministicReadableUnits()
    {
        Assert.Equal("entry", CliText.Plural(1, "entry", "entries"));
        Assert.Equal("entries", CliText.Plural(2, "entry", "entries"));
        Assert.Equal("about 8.0k tokens", CliText.Tokens(8000));
        Assert.Equal("1.0 KiB", CliText.Bytes(1024));
    }
}
