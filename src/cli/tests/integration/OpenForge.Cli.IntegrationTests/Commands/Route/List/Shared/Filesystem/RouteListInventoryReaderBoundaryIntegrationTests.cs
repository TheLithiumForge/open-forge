using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
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
        var lexicalWorkspacePath = Assert.IsType<string>(lexicalWorkspace);
        var workspace = new CliWorkspace(
            lexicalWorkspacePath,
            physicalWorkspace,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var documentReader = new SourceDocumentReader(workspace);

        var facts = await new RouteListInventoryReader().ReadAsync(
            new RouteListInventoryRequest(workspace, TestContext.Current.CancellationToken),
            documentReader);

        Assert.Equal(RouteListInventoryState.Complete, facts.State);
        var source = Assert.Single(facts.Sources);
        Assert.Equal(".agents/source.md", source.Source.CanonicalPath);
        Assert.Equal("Aliased workspace source", source.Source.Metadata.Description);
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
        var documentReader = new SourceDocumentReader(selectedWorkspace);

        var facts = await new RouteListInventoryReader().ReadAsync(
            new RouteListInventoryRequest(selectedWorkspace, TestContext.Current.CancellationToken),
            documentReader);

        Assert.Equal(RouteListInventoryState.Blocked, facts.State);
        Assert.Empty(facts.Sources);
        var finding = Assert.Single(facts.Findings);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, finding.Code);
        Assert.Equal(".agents", finding.CanonicalLogicalSubject);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

}
