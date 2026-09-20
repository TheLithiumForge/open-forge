using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Framework.Filesystem.Shared.Paths;

public sealed class PortableWorkspacePathTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Portable paths retain the supplied text on both sides of the owned length limit"), Trait("Feature", "workspace-paths"), Trait("Evidence", "Unit")]
    [InlineData(4096, true)]
    [InlineData(4097, false)]
    public void RetainsTextAtLengthBoundary(int length, bool expected)
    {
        var value = new string('a', length);

        var accepted = PortableWorkspacePath.TryNormalize(value, out var normalized);

        Assert.Equal(expected, accepted);
        Assert.Equal(value, normalized);
    }
}
