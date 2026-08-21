using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventoryReaderBoundaryIntegrationTests
{
    [Fact(DisplayName = "Route-list inventory accepts a proven workspace-root alias without changing logical identity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ProvenWorkspaceRootAliasUsesCanonicalLogicalPaths()
    {
        using var temporary = TemporaryWorkspace.Create("route-list-filesystem-root-alias");
        var physicalWorkspace = temporary.CreateDirectory("workspace");
        temporary.CreateFile(
            "workspace/.agents/source.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Aliased workspace source", "Route"));
        Assert.True(
            temporary.TryCreateDirectorySymbolicLink("workspace-alias", physicalWorkspace, out var lexicalWorkspace),
            "This integration case requires real symbolic-link support.");
        var before = temporary.SnapshotHashes();
        var workspace = new CliWorkspace(
            lexicalWorkspace!,
            physicalWorkspace,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var facts = await new RouteListInventoryReader().ReadAsync(
            new RouteListInventoryRequest(workspace, TestContext.Current.CancellationToken));

        Assert.Equal(RouteListInventoryState.Complete, facts.State);
        var source = Assert.Single(facts.Sources);
        Assert.Equal(".agents/source.md", source.Source.CanonicalPath);
        Assert.Equal("Aliased workspace source", source.Metadata.Description);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list inventory retains a bounded read cause for an exclusively locked source")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task LockedSourceRetainsInputOutputOrAccessCause()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(
            ".agents/locked.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Locked source", "Route"));
        var before = workspace.SnapshotHashes();
        RouteListInventoryFacts facts;
        using (new FileStream(
                   workspace.Absolute(".agents/locked.md"),
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        {
            facts = await workspace.ReadAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(RouteListInventoryState.Incomplete, facts.State);
        var finding = Assert.Single(
            facts.Findings,
            candidate => candidate.CanonicalLogicalSubject == ".agents/locked.md");
        Assert.Equal(RouteListFindingCode.ReadUnavailable, finding.Code);
        Assert.DoesNotContain("Exception", finding.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain('\n', finding.Cause);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list inventory blocks an escaping .agents ancestor without outside traversal")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EscapingAgentsAncestorBlocksBeforeDescendantReads()
    {
        using var workspace = TemporaryWorkspace.Create("route-list-filesystem-agents-link");
        using var outside = TemporaryWorkspace.Create("route-list-filesystem-agents-outside");
        outside.CreateFile(
            "outside.md",
            RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata("Outside source", "Route"));
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents", outside.Path, out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        var selectedWorkspace = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var facts = await new RouteListInventoryReader().ReadAsync(
            new RouteListInventoryRequest(selectedWorkspace, TestContext.Current.CancellationToken));

        Assert.Equal(RouteListInventoryState.Blocked, facts.State);
        Assert.Empty(facts.Sources);
        var finding = Assert.Single(facts.Findings);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, finding.Code);
        Assert.Equal(".agents", finding.CanonicalLogicalSubject);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list mid-enumeration cancellation retains known evidence and stops before later roots")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MidEnumerationCancellationRetainsKnownEvidenceWithoutLaterAccess()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-filesystem-cancel-outside");
        var empty = workspace.CreateDirectory(".agents/a-empty");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/b-empty-alias", empty, out _),
            "This integration case requires real symbolic-link support.");
        workspace.Write(".agents/z-slow/large.md", new byte[32 * 1024 * 1024]);
        outside.CreateFile("outside.md", "outside");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/zz-later", outside.Path, out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        var request = new RouteListInventoryRequest(
            workspace.Workspace,
            [
                ".agents/a-empty",
                ".agents/b-empty-alias",
                ".agents/c-missing",
                ".agents/z-slow",
                ".agents/zz-later",
            ],
            cancellation.Token);

        var pending = new RouteListInventoryReader().ReadAsync(request);
        Assert.False(pending.IsCompleted, "The real large-file read must establish a cancellable filesystem boundary.");
        cancellation.Cancel();
        var facts = await pending;

        Assert.Equal(RouteListInventoryState.Interrupted, facts.State);
        Assert.Contains(
            facts.Findings,
            finding => finding.Code == RouteListFindingCode.ReadUnavailable
                && finding.CanonicalLogicalSubject == ".agents/c-missing");
        Assert.Contains(facts.Findings, finding => finding.Code == RouteListFindingCode.Interrupted);
        Assert.Contains(
            facts.PhysicalAliases,
            alias => alias.CanonicalLogicalPath == ".agents/b-empty-alias"
                && alias.FirstCanonicalLogicalPath == ".agents/a-empty");
        Assert.DoesNotContain(
            facts.Findings,
            finding => finding.Code != RouteListFindingCode.Interrupted
                && finding.CanonicalLogicalSubject.StartsWith(".agents/zz-later", StringComparison.Ordinal));
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }
}
