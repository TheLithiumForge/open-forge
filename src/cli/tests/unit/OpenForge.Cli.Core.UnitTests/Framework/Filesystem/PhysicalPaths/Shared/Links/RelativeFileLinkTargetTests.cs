using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Shared.Links;

namespace OpenForge.Cli.Core.UnitTests.Framework.Filesystem.PhysicalPaths.Shared.Links;

[Trait("Evidence", "Unit"), Trait("Boundary", "Processing"), Trait("Feature", "library-foundation")]
public sealed class RelativeFileLinkTargetTests
{
    [Theory]
    [InlineData("../../team/kind.md", "..\\..\\team\\kind.md", true)]
    [InlineData("../../team/kind.md", "../..\\team/kind.md", true)]
    [InlineData("../../team/kind.md", "../../team/kind.md", true)]
    [InlineData("../../team/kind.md", "../../TEAM/kind.md", false)]
    [InlineData("../../team/kind.md", "../../team/./kind.md", false)]
    [InlineData("../../team/kind.md", "../../other/kind.md", false)]
    [InlineData(null, "../../team/kind.md", false)]
    public void OnlySeparatorDifferencesMatch(string? actual, string? expected, bool matches)
        => Assert.Equal(matches, RelativeFileLinkTarget.SameMapping(actual, expected));
}
