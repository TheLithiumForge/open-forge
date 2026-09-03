using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMovePlanningProjectionTests
{
    [Fact(DisplayName = "Route Move unresolved exact source path retains base-path selection origin")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection")]
    public async Task UnresolvedExactSourcePathRetainsSelectionOrigin()
    {
        using var temporary = TemporaryWorkspace.Create("route-move-missing-source");
        _ = temporary.CreateDirectory(".agents");
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new RouteMoveRequest(
            workspace,
            ".agents/guidance/consumed.md",
            ".agents/archive/consumed.md",
            RouteMoveMode.DryRun);

        var result = await RouteMoveOperationFactory.Create()
            .ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveSourceSelection.BasePath, result.Source.SelectedBy);
        Assert.Null(result.Source.Id);
        Assert.Null(result.Source.Path);
        Assert.Null(result.Source.Form);
    }

    [Fact(DisplayName = "Route Move public ownership omits an unrelated Framework claim")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection")]
    public void PublicOwnershipOmitsUnrelatedClaim()
    {
        var subject = LeafSubject();
        var unrelated = RouteMoveOwnershipProjector.Project(
            Ownership(new LifecycleOwnershipClaim(
                ".agents/loader.md",
                LifecycleOwnershipManager.Framework,
                "Open Forge")),
            subject);

        Assert.Equal(RouteMoveOwnershipState.Unmanaged, unrelated.State);
        Assert.Equal(RouteMoveOwnershipTrust.Trusted, unrelated.Framework);
        Assert.Equal(RouteMoveOwnershipTrust.Trusted, unrelated.Extensions);
        Assert.Empty(unrelated.Claims);

        var selected = RouteMoveOwnershipProjector.Project(
            Ownership(
                new LifecycleOwnershipClaim(
                    ".agents/loader.md",
                    LifecycleOwnershipManager.Framework,
                    "Open Forge"),
                new LifecycleOwnershipClaim(
                    RouteMoveTestData.SourcePath,
                    LifecycleOwnershipManager.Extension,
                    "Selected owner")),
            subject);

        Assert.Equal(RouteMoveOwnershipState.Claimed, selected.State);
        var claim = Assert.Single(selected.Claims);
        Assert.Equal(RouteMoveTestData.SourcePath, claim.Path);
        Assert.Equal(RouteMoveOwnershipManager.Extension, claim.Manager);
        Assert.Equal("Selected owner", claim.Owner);
    }

    private static LifecycleOwnershipReadResult Ownership(
        params LifecycleOwnershipClaim[] claims)
    {
        var lifecyclePath = Path.Combine(
            RouteMoveTestData.Workspace().LexicalRoot,
            ".agents/open-forge.lifecycle.json");
        return new LifecycleOwnershipReadResult(
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Framework,
                LifecycleOwnershipReadState.Trusted,
                cause: null),
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Extensions,
                LifecycleOwnershipReadState.Trusted,
                cause: null),
            claims,
            FileExpectation.File(
                lifecyclePath,
                lifecyclePath,
                FileExpectation.Hash([0x01])),
            []);
    }

    private static RouteMoveResolvedSubject LeafSubject()
    {
        var workspace = RouteMoveTestData.Workspace();
        var physicalPath = Path.Combine(workspace.LexicalRoot, RouteMoveTestData.SourcePath);
        var layer = new SourceLayer(
            RouteMoveTestData.SourcePath,
            physicalPath,
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity(RouteMoveTestData.SourceId, RouteMoveTestData.SourcePath),
            layer);
        return new RouteMoveResolvedSubject
        {
            Request = RouteMoveTestData.Request(RouteMoveMode.DryRun),
            Source = RouteMoveTestData.Formation(RouteMoveMode.DryRun).Source,
            Kind = RouteMoveSubjectKind.Leaf,
            Catalogue = new SourceCatalogue(workspace, [], [source], [], isCancelled: false),
            SelectedSource = source,
            Layers =
            [
                new RouteMoveResolvedLayer
                {
                    Layer = layer,
                    Snapshot = FileStateSnapshot.Missing(physicalPath),
                },
            ],
        };
    }
}
