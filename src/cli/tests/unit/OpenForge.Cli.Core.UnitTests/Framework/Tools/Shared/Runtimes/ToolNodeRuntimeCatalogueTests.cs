using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Runtimes;

[Trait("Feature", "tool-private-node"), Trait("Evidence", "Unit")]
public sealed class ToolNodeRuntimeCatalogueTests
{
    [Fact(DisplayName = "Approved portable Node versions cover six targets with official reviewed pins")]
    public void CoversApprovedTargets()
    {
        var groups = ToolNodeRuntimeCatalogue.All.GroupBy(runtime => runtime.Version).ToArray();
        Assert.True(groups.Length >= 2);
        foreach (var group in groups)
        {
            Assert.Equal(6, group.Count());
            Assert.Equal(6, group.Select(runtime => (runtime.Archive.Target.OperatingSystem, runtime.Archive.Target.Cpu)).Distinct().Count());
            foreach (var runtime in group)
            {
                Assert.True(ToolArtifactSource.IsOfficial(runtime.Archive));
                Assert.True(ToolArtifactIntegrity.IsSha256(runtime.Archive.Sha256));
                Assert.Equal(ToolArtifactSource.NodeHost, runtime.SignedChecksums.Host);
                Assert.Equal(40, runtime.VerifiedSigner.Length);
                Assert.Equal(runtime.Version, runtime.Archive.Version);
            }
        }
    }

    [Fact(DisplayName = "Separate approved Node major versions can satisfy distinct bounded runtime needs")]
    public void SelectsDifferentMajors()
    {
        var lower = ToolNodeRuntimeCatalogue.All.Where(runtime => ToolRuntimeRange.Satisfies(runtime.Version, ">=20 <24")).ToArray();
        var higher = ToolNodeRuntimeCatalogue.All.Where(runtime => ToolRuntimeRange.Satisfies(runtime.Version, ">=24 <25")).ToArray();
        Assert.NotEmpty(lower);
        Assert.NotEmpty(higher);
        Assert.Empty(lower.Intersect(higher));
    }
}
