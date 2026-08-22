using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventoryReaderPhysicalIntegrationTests
{
    [Fact(DisplayName = "Route-list inventory traverses finite contained file and directory aliases deterministically")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ContainedAliasesRemainFiniteFactsAndRetainLogicalSources()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(
            ".agents/physical/target.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Physical target", "Route"));
        workspace.Write(
            ".agents/physical/tree/leaf.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Tree leaf", "Route"));
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/alias.md",
                workspace.Absolute(".agents/physical/target.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(
                ".agents/root/tree-alias",
                workspace.Absolute(".agents/physical/tree"),
                out _),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();

        var first = await workspace.ReadAsync(TestContext.Current.CancellationToken);
        var second = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(RouteListInventoryState.Complete, first.State);
        Assert.Equal(
            [
                ".agents/physical/target.md",
                ".agents/physical/tree/leaf.md",
                ".agents/root/alias.md",
                ".agents/root/tree-alias/leaf.md",
            ],
            first.Sources.Select(source => source.Source.CanonicalPath));
        Assert.Contains(first.PhysicalAliases, alias => alias.CanonicalLogicalPath == ".agents/root/alias.md");
        Assert.Contains(first.PhysicalAliases, alias => alias.CanonicalLogicalPath == ".agents/root/tree-alias");
        Assert.Contains(first.PhysicalAliases, alias => alias.CanonicalLogicalPath == ".agents/root/tree-alias/leaf.md");
        Assert.All(
            first.Findings,
            finding => Assert.Equal(RouteListFindingCode.IdentityCollision, finding.Code));
        Assert.Equal(2, first.Findings.Count);
        Assert.DoesNotContain(first.Findings, finding => finding.Code == RouteListFindingCode.PhysicalBoundary);
        AssertEquivalent(first, second);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list inventory blocks external, reentering, dangling, and cyclic boundaries without outside reads")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnsafePhysicalBoundariesBlockAndRetainSafeSources()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-filesystem-outside");
        workspace.Write(
            ".agents/safe.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Safe source", "Route"));
        workspace.Write(
            ".agents/inside/back.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Reentry target", "Route"));
        var outsideFile = outside.CreateFile("outside.md", "outside");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/external.md", outsideFile, out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            outside.TryCreateDirectorySymbolicLink(
                "back",
                workspace.Absolute(".agents/inside"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/reentry", outside.Path, out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/dangling.md", "missing-target", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/cycle-a", "cycle-b", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/cycle-b", "cycle-a", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/self-cycle", "self-cycle", out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var facts = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Equal(RouteListInventoryState.Blocked, facts.State);
        Assert.Equal(
            [".agents/inside/back.md", ".agents/safe.md"],
            facts.Sources.Select(source => source.Source.CanonicalPath));
        Assert.Equal(
            [
                ".agents/cycle-a",
                ".agents/cycle-b",
                ".agents/dangling.md",
                ".agents/external.md",
                ".agents/reentry",
                ".agents/self-cycle",
            ],
            facts.Findings
                .Where(finding => finding.Code == RouteListFindingCode.PhysicalBoundary)
                .Select(finding => finding.CanonicalLogicalSubject));
        Assert.DoesNotContain(
            facts.Sources,
            source => source.Source.CanonicalPath.StartsWith(".agents/reentry/", StringComparison.Ordinal));
        Assert.All(
            facts.Findings,
            finding => Assert.DoesNotContain(outside.Path, finding.Cause, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    private static void AssertEquivalent(RouteListInventoryFacts first, RouteListInventoryFacts second)
    {
        Assert.Equal(first.State, second.State);
        Assert.Equal(
            first.Sources.Select(SourceProjection),
            second.Sources.Select(SourceProjection));
        Assert.Equal(
            first.Findings.Select(finding => $"{finding.MachineCode}|{finding.CanonicalLogicalSubject}|{finding.Cause}"),
            second.Findings.Select(finding => $"{finding.MachineCode}|{finding.CanonicalLogicalSubject}|{finding.Cause}"));
        Assert.Equal(
            first.PhysicalAliases.Select(alias => $"{alias.CanonicalLogicalPath}|{alias.PhysicalPath}|{alias.FirstCanonicalLogicalPath}"),
            second.PhysicalAliases.Select(alias => $"{alias.CanonicalLogicalPath}|{alias.PhysicalPath}|{alias.FirstCanonicalLogicalPath}"));
    }

    private static string SourceProjection(RouteListInventorySource source)
    {
        return $"{source.Source.CanonicalPath}|{source.Source.PhysicalPath}|{source.Source.Kind}|{source.Source.Base.Form}|"
            + $"{source.Source.Metadata.State}|{source.Source.Metadata.Description}|{string.Join(',', source.Source.Metadata.Tags)}|"
            + $"{source.Source.OverwritePath}|{source.Source.IsRouteAmbiguous}";
    }
}
