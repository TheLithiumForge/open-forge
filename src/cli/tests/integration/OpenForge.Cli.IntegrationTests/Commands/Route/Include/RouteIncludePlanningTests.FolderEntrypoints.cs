using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludePlanningTests
{
    private const string FolderNote = RouteInclusionFolderFixture.Folder + "/note" + SourceFormClassifier.MarkdownExtension;
    private const string CompatibilityDocument = "# Existing catalogue\n\n## Entries\n\n- [Note](note.md)\n";

    [Theory(DisplayName = "Folder Include refuses to create a canonical anchor beside a compatibility catalogue"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, SourceFormClassifier.IndexFileName), InlineData(true, SourceFormClassifier.IndexFileName)]
    [InlineData(false, SourceFormClassifier.UnderscoreIndexFileName), InlineData(true, SourceFormClassifier.UnderscoreIndexFileName)]
    [InlineData(false, SourceFormClassifier.ReferencesFileName), InlineData(true, SourceFormClassifier.ReferencesFileName)]
    [InlineData(false, SourceFormClassifier.UnderscoreReferencesFileName), InlineData(true, SourceFormClassifier.UnderscoreReferencesFileName)]
    public async Task CompatibilityCataloguePreventsCompetingAnchor(bool custom, string filename)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var compatibility = RouteInclusionFolderFixture.Folder + "/" + filename;
        workspace.Files.WriteText(compatibility, CompatibilityDocument);
        workspace.Files.WriteText(FolderNote, "# Ordinary note\n");
        var before = workspace.Files.SnapshotHashes();
        var prior = await FolderNoteNode(workspace);
        Assert.Equal(SourceRouteParentState.Resolved, prior.ParentState);
        Assert.Equal(compatibility, Assert.Single(prior.ParentPaths));

        var result = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, RouteInclusionFolderFixture.Token);

        var after = await FolderNoteNode(workspace);
        Assert.True(result.Status == CliSemanticStatus.Blocked,
            $"Status={result.Status}, verified={result.Outcome.Verified}, canonicalCreated={File.Exists(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue))}, beforeParent={prior.ParentState}, afterParent={after.ParentState}");
        Assert.Equal(RouteInclusionFindingCode.WorkspaceUnsafe, result.Outcome.Code);
        Assert.Empty(result.Outcome.Effects);
        Assert.Equal(SourceRouteParentState.Resolved, after.ParentState);
        Assert.Equal(compatibility, Assert.Single(after.ParentPaths));
        Assert.False(File.Exists(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue)));
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Folder Include preserves and refuses already competing entrypoints"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task AmbiguousFolderCannotBeRegistered(bool custom, bool canonical)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, canonical);
        workspace.Files.WriteText(RouteInclusionFolderFixture.Folder + "/" + SourceFormClassifier.IndexFileName, CompatibilityDocument);
        if (!canonical) workspace.Files.WriteText(RouteInclusionFolderFixture.Folder + "/" + SourceFormClassifier.UnderscoreIndexFileName, CompatibilityDocument);
        workspace.Files.WriteText(FolderNote, "# Ordinary note\n");
        Assert.Equal(SourceRouteParentState.Ambiguous, (await FolderNoteNode(workspace)).ParentState);
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Outcome.Status);
        Assert.Equal(RouteInclusionFindingCode.WorkspaceUnsafe, result.Outcome.Code);
        Assert.Null(result.Plan);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "An existing canonical folder anchor remains unique and byte-exact after Include"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task CanonicalAnchorRemainsUnique(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        workspace.Files.WriteText(FolderNote, "# Ordinary note\n");
        var bytes = workspace.Bytes(RouteInclusionFolderFixture.Catalogue);
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder };
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var result = await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.Verified);
        var node = await FolderNoteNode(workspace);
        Assert.Equal(SourceRouteParentState.Resolved, node.ParentState);
        Assert.Equal(RouteInclusionFolderFixture.Catalogue, Assert.Single(node.ParentPaths));
        Assert.Equal(bytes, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
        Assert.True((await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token)).Outcome.NoOp);
    }

    [Theory(DisplayName = "A compatibility catalogue added after planning blocks folder application before effects"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task LateCompatibilityCatalogueBlocksApply(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        workspace.Files.WriteText(RouteInclusionFolderFixture.Folder + "/" + SourceFormClassifier.IndexFileName, CompatibilityDocument);
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInclusionFindingCode.TargetChanged, result.Code);
        Assert.All(result.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(File.Exists(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue)));
    }

    [Theory(DisplayName = "Canonical compatibility names do not permit an additional competing entrypoint"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, "index", false), InlineData(false, "index", true)]
    [InlineData(true, "index", false), InlineData(true, "index", true)]
    [InlineData(false, "references", false), InlineData(false, "references", true)]
    [InlineData(true, "references", false), InlineData(true, "references", true)]
    public async Task CanonicalCompatibilityNameStillRefusesCompetition(bool custom, string name, bool anchor)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var folder = "docs/" + name;
        var catalogue = WorkspaceRoutePath.CanonicalEntrypointPath(folder);
        workspace.Files.CreateDirectory(folder);
        if (anchor) workspace.Files.WriteText(catalogue, RouteInclusionFolderFixture.AuthoredCatalogue);
        workspace.Files.WriteText(folder + "/" + SourceFormClassifier.IndexFileName, CompatibilityDocument);
        workspace.Files.WriteText(folder + "/note" + SourceFormClassifier.MarkdownExtension, "# Ordinary note\n");
        var before = workspace.Files.SnapshotHashes();

        var result = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = folder }, RouteInclusionFolderFixture.Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInclusionFindingCode.WorkspaceUnsafe, result.Outcome.Code);
        Assert.Empty(result.Outcome.Effects);
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    private static async Task<SourceRouteNode> FolderNoteNode(RouteInclusionWorkspace workspace)
    {
        var layout = WorkspaceLayout.Create(workspace.Workspace, RouteInclusionFolderFixture.Settings(workspace).MainRoot)
            .WithOutsideRoutes([RouteInclusionFolderFixture.Folder]);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(layout, [RouteInclusionFolderFixture.Folder]), RouteInclusionFolderFixture.Token);
        return new SourceRouteTopologyBuilder().Build(layout, catalogue.Sources, []).FindByPath(FolderNote)
            ?? throw new InvalidOperationException("The fixture requires its ordinary note in the observed topology.");
    }
}
