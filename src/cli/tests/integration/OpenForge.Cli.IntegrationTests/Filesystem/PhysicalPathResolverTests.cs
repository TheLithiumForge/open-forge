using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Filesystem;

public sealed class PhysicalPathResolverTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Physical walk classifies links without writes")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration")]
    public void ComponentWalkClassifiesInternalExternalDanglingAndCyclicLinksWithoutWrites()
    {
        using var temporary = TemporaryWorkspace.Create("physical-paths");
        var workspace = temporary.CreateDirectory("workspace");
        var internalDirectory = temporary.CreateDirectory("workspace/internal");
        var internalFile = temporary.CreateFile("workspace/internal/value.txt", "value");
        var outside = temporary.CreateDirectory("outside");
        var outsideFile = temporary.CreateFile("outside/external.txt", "external");
        temporary.CreateDirectorySymbolicLink("workspace/internal-alias", internalDirectory);
        temporary.CreateFileSymbolicLink("workspace/internal-file-alias", internalFile);
        temporary.CreateDirectorySymbolicLink("workspace/external-absolute", outside);
        temporary.CreateDirectorySymbolicLink(
            "workspace/external-relative",
            Path.GetRelativePath(workspace, outside));
        temporary.CreateFileSymbolicLink("workspace/final-link", outsideFile);
        temporary.CreateFileSymbolicLink("workspace/dangling", "missing-target");
        temporary.CreateDirectorySymbolicLink("workspace/cycle-a", "cycle-b");
        temporary.CreateDirectorySymbolicLink("workspace/cycle-b", "cycle-a");
        temporary.CreateDirectorySymbolicLink("workspace/self-cycle", "self-cycle");
        var before = temporary.SnapshotHashes();
        var resolver = new PhysicalPathResolver();

        var contained = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "internal", "value.txt"));
        var alias = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "internal-alias", "value.txt"));
        var fileAlias = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "internal-file-alias"));
        var absoluteExternal = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "external-absolute"));
        var relativeExternal = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "external-relative"));
        var finalExternal = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "final-link"));
        var dangling = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "dangling"));
        var cycle = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "cycle-a"));
        var selfCycle = resolver.ResolveCandidate(workspace, workspace, Path.Combine(workspace, "self-cycle"));

        Assert.Equal(PhysicalPathState.Contained, contained.State);
        Assert.Equal(Path.GetFullPath(internalFile), contained.ResolvedPhysicalPath);
        Assert.Equal(PhysicalPathState.Contained, alias.State);
        Assert.Equal(Path.GetFullPath(internalFile), alias.ResolvedPhysicalPath);
        Assert.Equal(PhysicalPathState.Contained, fileAlias.State);
        Assert.Equal(Path.GetFullPath(internalFile), fileAlias.ResolvedPhysicalPath);
        Assert.Equal(PhysicalPathState.External, absoluteExternal.State);
        Assert.Equal(PhysicalPathState.External, relativeExternal.State);
        Assert.Equal(PhysicalPathState.External, finalExternal.State);
        Assert.Equal(PhysicalPathState.Dangling, dangling.State);
        Assert.Equal(PhysicalPathState.Cycle, cycle.State);
        Assert.Equal(PhysicalPathState.Cycle, selfCycle.State);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Physical walk blocks the first external transition before reentry")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration")]
    public void ResolverBlocksFirstExternalTransitionEvenWhenALaterLinkReenters()
    {
        using var temporary = TemporaryWorkspace.Create("leave-reenter");
        var workspace = temporary.CreateDirectory("workspace");
        var reentry = temporary.CreateDirectory("workspace/reentry");
        temporary.CreateFile("workspace/reentry/value.txt", "value");
        var outside = temporary.CreateDirectory("outside");
        temporary.CreateDirectorySymbolicLink("workspace/escape", outside);
        temporary.CreateDirectorySymbolicLink("outside/back", reentry);
        var candidate = Path.Combine(workspace, "escape", "back", "value.txt");

        var result = new PhysicalPathResolver().ResolveCandidate(workspace, workspace, candidate);

        Assert.Equal(PhysicalPathState.External, result.State);
        Assert.Equal(Path.GetFullPath(outside), result.ResolvedPhysicalPath);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Physical walk supports root and unrelated internal aliases")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration")]
    public void ResolverSupportsWorkspaceRootAndUnrelatedInternalAliases()
    {
        using var temporary = TemporaryWorkspace.Create("root-alias");
        var workspace = temporary.CreateDirectory("workspace");
        var shared = temporary.CreateDirectory("workspace/shared");
        temporary.CreateDirectorySymbolicLink("workspace/alias-one", shared);
        temporary.CreateDirectorySymbolicLink("workspace/alias-two", shared);
        temporary.CreateDirectorySymbolicLink("workspace/root-alias", workspace);
        var rootAlias = temporary.CreateDirectorySymbolicLink("workspace-alias", workspace);
        var resolver = new PhysicalPathResolver();

        var root = resolver.ResolveRoot(rootAlias);
        var containedRootPath = Assert.IsType<string>(root.ResolvedPhysicalPath);
        var first = resolver.ResolveCandidate(rootAlias, containedRootPath, Path.Combine(rootAlias, "alias-one"));
        var second = resolver.ResolveCandidate(rootAlias, containedRootPath, Path.Combine(rootAlias, "alias-two"));
        var repeatedAlias = resolver.ResolveCandidate(
            rootAlias,
            containedRootPath,
            Path.Combine(rootAlias, "root-alias", "root-alias"));

        Assert.Equal(PhysicalPathState.Contained, root.State);
        Assert.Equal(Path.GetFullPath(workspace), root.ResolvedPhysicalPath);
        Assert.Equal(Path.GetFullPath(shared), first.ResolvedPhysicalPath);
        Assert.Equal(Path.GetFullPath(shared), second.ResolvedPhysicalPath);
        Assert.Equal(PhysicalPathState.Contained, repeatedAlias.State);
        Assert.Equal(Path.GetFullPath(workspace), repeatedAlias.ResolvedPhysicalPath);
    }
}
