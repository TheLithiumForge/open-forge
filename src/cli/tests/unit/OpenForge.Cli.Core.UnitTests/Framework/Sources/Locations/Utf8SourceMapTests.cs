using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Locations;

public sealed class Utf8SourceMapTests
{
    [Trait("Boundary", "Processing"), Trait("Feature", "source-locations"), Trait("Evidence", "Unit")]
    [Fact(DisplayName = "Repeated location queries build one coordinate index only when needed")]
    public void RepeatedQueriesShareOneLazyIndex()
    {
        var builds = 0;
        var source = string.Concat(Enumerable.Repeat("A\U0001F600\r\n", 1000));
        var map = new Utf8SourceMap(source, indexBuilt: () => builds++);
        Assert.Equal(0, builds);
        for (var index = 0; index < 1000; index++)
            Assert.Equal(new SourceLocation(index + 1, 2, index * 7 + 1, 4), map.Map(index * 5 + 1, 2));
        Assert.Equal(1, builds);
    }

    [Trait("Boundary", "Processing"), Trait("Feature", "source-locations"), Trait("Evidence", "Unit")]
    [Theory(DisplayName = "Indexed locations retain coordinates inside CRLF and at every newline boundary")]
    [InlineData(1, 1, 2), InlineData(2, 2, 1), InlineData(3, 2, 1), InlineData(4, 2, 2)]
    [InlineData(5, 3, 1), InlineData(6, 3, 2), InlineData(7, 4, 1)]
    public void RetainsNewlineBoundaryCoordinates(int offset, int line, int column)
        => Assert.Equal(new SourceLocation(line, column, offset, 0), new Utf8SourceMap("A\r\nB\rC\n").Map(offset, 0));

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Source locations use Unicode-scalar coordinates and UTF-8 bytes across line endings")]
    [Trait("Feature", "source-locations"), Trait("Evidence", "Unit")]
    public void LocationsUseUnicodeScalarsAndUtf8BytesAcrossCrLf()
    {
        const string source = "A😀\r\né\rZ\nΩ";
        var map = new Utf8SourceMap(source);

        Assert.Equal(new SourceLocation(1, 2, 1, 4), map.Map(1, 2));
        Assert.Equal(new SourceLocation(2, 1, 7, 2), map.Map(5, 1));
        Assert.Equal(new SourceLocation(3, 1, 10, 1), map.Map(7, 1));
        Assert.Equal(new SourceLocation(4, 1, 12, 2), map.Map(9, 1));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Source locations keep zero-length spans valid at line starts and end of file")]
    [Trait("Feature", "source-locations"), Trait("Evidence", "Unit")]
    public void ZeroLengthAndEndOfFileLocationsRemainValid()
    {
        const string source = "A\r\n😀\r\n";
        var map = new Utf8SourceMap(source);

        Assert.Equal(new SourceLocation(1, 1, 0, 0), map.Map(0, 0));
        Assert.Equal(new SourceLocation(2, 1, 3, 0), map.Map(3, 0));
        Assert.Equal(new SourceLocation(3, 1, 9, 0), map.Map(source.Length, 0));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Source locations reject invalid UTF-16 and spans that split a scalar")]
    [Trait("Feature", "source-locations"), Trait("Evidence", "Unit")]
    public void InvalidUtf16AndSplitScalarSpansAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new Utf8SourceMap("\uD800"));

        var map = new Utf8SourceMap("A😀Z");
        Assert.Throws<ArgumentException>(() => map.Map(2, 1));
        Assert.Throws<ArgumentException>(() => map.Map(1, 1));
    }
}
