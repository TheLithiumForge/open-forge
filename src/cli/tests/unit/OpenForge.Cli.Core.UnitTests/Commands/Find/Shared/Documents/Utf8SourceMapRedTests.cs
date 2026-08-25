using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Documents;

public sealed class Utf8SourceMapRedTests
{
    [Fact(DisplayName = "Find locations use Unicode-scalar coordinates and UTF-8 bytes across line endings")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void LocationsUseUnicodeScalarsAndUtf8BytesAcrossCrLf()
    {
        const string source = "A😀\r\né\rZ\nΩ";
        var map = new Utf8SourceMap(source);

        Assert.Equal(
            new FindSourceLocation(1, 2, 1, 4),
            map.Map(new MarkdownTextSpan(1, 2)));
        Assert.Equal(
            new FindSourceLocation(2, 1, 7, 2),
            map.Map(new MarkdownTextSpan(5, 1)));
        Assert.Equal(
            new FindSourceLocation(3, 1, 10, 1),
            map.Map(new MarkdownTextSpan(7, 1)));
        Assert.Equal(
            new FindSourceLocation(4, 1, 12, 2),
            map.Map(new MarkdownTextSpan(9, 1)));
    }

    [Fact(DisplayName = "Find locations keep zero-length spans valid at line starts and end of file")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void ZeroLengthAndEndOfFileLocationsRemainValid()
    {
        const string source = "A\r\n😀\r\n";
        var map = new Utf8SourceMap(source);

        Assert.Equal(
            new FindSourceLocation(1, 1, 0, 0),
            map.Map(new MarkdownTextSpan(0, 0)));
        Assert.Equal(
            new FindSourceLocation(2, 1, 3, 0),
            map.Map(new MarkdownTextSpan(3, 0)));
        Assert.Equal(
            new FindSourceLocation(3, 1, 9, 0),
            map.Map(new MarkdownTextSpan(source.Length, 0)));
    }
}
