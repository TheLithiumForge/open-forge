using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Locations;

public sealed class Utf8SourceMapTests
{
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
