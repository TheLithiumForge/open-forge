using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMovePlanningProjectionTests
{
    [Fact(DisplayName = "Route Move public ownership omits an unrelated Framework claim")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void PublicOwnershipOmitsUnrelatedClaim()
    {
        var subject = LeafSubject();
        var unrelated = RouteMoveOwnershipProjector.Project(
            Ownership(new OwnedPath(
                ".agents/loader.md",
                OwnedPathManager.Framework,
                "Open Forge")),
            subject);

        Assert.Equal(RouteMoveOwnershipState.Unmanaged, unrelated.State);
        Assert.Equal(RouteMoveOwnershipTrust.Trusted, unrelated.Framework);
        Assert.Equal(RouteMoveOwnershipTrust.Trusted, unrelated.Extensions);
        Assert.Empty(unrelated.Claims);

        var selected = RouteMoveOwnershipProjector.Project(
            Ownership(
                new OwnedPath(
                    ".agents/loader.md",
                    OwnedPathManager.Framework,
                    "Open Forge"),
                new OwnedPath(
                    RouteMoveTestData.SourcePath,
                    OwnedPathManager.Extension,
                    "Selected owner")),
            subject);

        Assert.Equal(RouteMoveOwnershipState.Claimed, selected.State);
        var claim = Assert.Single(selected.Claims);
        Assert.Equal(RouteMoveTestData.SourcePath, claim.Path);
        Assert.Equal(RouteMoveOwnershipManager.Extension, claim.Manager);
        Assert.Equal("Selected owner", claim.Owner);
    }

    [Fact(DisplayName = "Route Move logical leaf ID projects to its canonical markdown destination")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void LogicalLeafIdProjectsToCanonicalDestination()
    {
        var inventory = LeafInventory("memory/archive/new guide");
        var result = RouteMoveDestinationProjector.Project(inventory);
        var projection = Assert.IsType<RouteMoveDestinationProjection>(result.Projection);

        Assert.Equal(".agents/memory/archive/new guide.md", projection.DestinationPath);
        Assert.Equal("memory/archive/new guide", projection.DestinationId);
        Assert.Equal("memory/archive/new guide", projection.Inventory.Subject.Request.DestinationTarget);
        Assert.Equal(
            ".agents/memory/archive/new guide.md",
            Assert.Single(projection.Items).DestinationPath);
    }

    [Theory(DisplayName = "Route Move logical category ID preserves the selected entrypoint filename"),
     InlineData((int)SourceDocumentForm.CanonicalEntrypoint, ".agents/memory/archive/topics/_topics.md"),
     InlineData((int)SourceDocumentForm.IndexEntrypoint, ".agents/memory/archive/topics/index.md"),
     InlineData((int)SourceDocumentForm.UnderscoreIndexEntrypoint, ".agents/memory/archive/topics/_index.md"),
     InlineData((int)SourceDocumentForm.ReferencesEntrypoint, ".agents/memory/archive/topics/references.md"),
     InlineData((int)SourceDocumentForm.UnderscoreReferencesEntrypoint, ".agents/memory/archive/topics/_references.md")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void LogicalCategoryIdPreservesSelectedEntrypointForm(
        int formValue,
        string expectedPath)
    {
        var form = (SourceDocumentForm)formValue;
        var inventory = CategoryInventory(form, "memory/archive/topics");
        var result = RouteMoveDestinationProjector.Project(inventory);
        var projection = Assert.IsType<RouteMoveDestinationProjection>(result.Projection);

        Assert.Equal(expectedPath, projection.DestinationPath);
        Assert.Equal("memory/archive/topics", projection.DestinationId);
        Assert.Equal("memory/archive/topics", projection.Inventory.Subject.Request.DestinationTarget);
    }

    [Fact(DisplayName = "Route Move logical destination rejects an invalid reference without projection")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void LogicalDestinationRejectsInvalidReference()
    {
        var result = RouteMoveDestinationProjector.Project(
            LeafInventory("memory/../archive/new guide"));

        var boundary = Assert.IsType<RouteMoveResultFormation>(result.Boundary);
        Assert.Equal(RouteMoveFindingCode.InvalidDestination, Assert.Single(boundary.Findings).Code);
        Assert.Equal(CliSemanticStatus.Invalid, boundary.Findings[0].Status);
        Assert.Null(result.Projection);
    }

    [Fact(DisplayName = "Route Move logical category destination preserves inside-source refusal")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void LogicalCategoryDestinationInsideSourceRemainsInvalid()
    {
        var result = RouteMoveDestinationProjector.Project(
            CategoryInventory(
                SourceDocumentForm.CanonicalEntrypoint,
                "memory/guides/topics/nested"));

        var boundary = Assert.IsType<RouteMoveResultFormation>(result.Boundary);
        Assert.Equal(RouteMoveFindingCode.DestinationInsideSource, Assert.Single(boundary.Findings).Code);
        Assert.Equal(CliSemanticStatus.Invalid, boundary.Findings[0].Status);
        Assert.Null(result.Projection);
    }

    [Fact(DisplayName = "Route Move physical category destination preserves compatibility-form refusal")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitProjection"), Trait("Boundary", "Processing")]
    public void PhysicalCategoryDestinationPreservesCompatibilityFormRefusal()
    {
        var result = RouteMoveDestinationProjector.Project(
            CategoryInventory(
                SourceDocumentForm.IndexEntrypoint,
                ".agents/memory/archive/topics/_topics.md"));

        var boundary = Assert.IsType<RouteMoveResultFormation>(result.Boundary);
        Assert.Equal(RouteMoveFindingCode.InvalidDestination, Assert.Single(boundary.Findings).Code);
        Assert.Equal(CliSemanticStatus.Invalid, boundary.Findings[0].Status);
        Assert.Null(result.Projection);
    }

    private static WorkspaceOwnershipRead Ownership(
        params OwnedPath[] claims)
    {
        var lifecyclePath = Path.Combine(
            RouteMoveTestData.Workspace().LexicalRoot,
            ".agents/open-forge.lock.json");
        var document = WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("framework", null), [.. claims.Where(claim => claim.Manager == OwnedPathManager.Framework).Select(claim => claim.Path)], []),
            Extensions = [.. claims.Where(claim => claim.Manager == OwnedPathManager.Extension)
                .GroupBy(claim => claim.Owner).Select(group => new ExtensionOwnership(group.Key, null, null, [], [.. group.Select(claim => claim.Path)], []))],
        };
        return new WorkspaceOwnershipRead(WorkspaceOwnershipReadState.Complete, document, lifecyclePath,
            FileStateSnapshot.File(lifecyclePath, lifecyclePath, [0x01]), null);
    }

    private static RouteMoveResolvedSubject LeafSubject()
        => LeafSubject(RouteMoveTestData.DestinationPath);

    private static RouteMoveCategoryInventory LeafInventory(string destinationTarget)
        => new()
        {
            Subject = LeafSubject(destinationTarget),
            Ownership = Ownership(),
        };

    private static RouteMoveResolvedSubject LeafSubject(string destinationTarget)
    {
        var workspace = RouteMoveTestData.Workspace();
        var physicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            RouteMoveTestData.SourcePath.Replace('/', Path.DirectorySeparatorChar)));
        var layer = new SourceLayer(
            RouteMoveTestData.SourcePath,
            physicalPath,
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity(RouteMoveTestData.SourceId, RouteMoveTestData.SourcePath),
            layer);
        var parent = ParentSource(workspace);
        return new RouteMoveResolvedSubject
        {
            Request = new RouteMoveRequest(
                workspace,
                RouteMoveTestData.SourceId,
                destinationTarget,
                RouteMoveMode.DryRun),
            Source = RouteMoveTestData.Formation(RouteMoveMode.DryRun).Source,
            Kind = RouteMoveSubjectKind.Leaf,
            Catalogue = new SourceCatalogue(workspace, [], [source, parent], [], isCancelled: false),
            SelectedSource = source,
            RouteFacts = RouteFacts(parent),
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

    private static RouteMoveCategoryInventory CategoryInventory(
        SourceDocumentForm form,
        string destinationTarget)
    {
        var workspace = RouteMoveTestData.Workspace();
        const string sourceId = "memory/guides/topics";
        var sourcePath = form switch
        {
            SourceDocumentForm.CanonicalEntrypoint => ".agents/memory/guides/topics/_topics.md",
            SourceDocumentForm.IndexEntrypoint => ".agents/memory/guides/topics/index.md",
            SourceDocumentForm.UnderscoreIndexEntrypoint => ".agents/memory/guides/topics/_index.md",
            SourceDocumentForm.ReferencesEntrypoint => ".agents/memory/guides/topics/references.md",
            SourceDocumentForm.UnderscoreReferencesEntrypoint => ".agents/memory/guides/topics/_references.md",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The category test form is not defined."),
        };
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity(sourceId, sourcePath),
            Layer(workspace, sourcePath, form));
        var parent = ParentSource(workspace);
        var subject = new RouteMoveResolvedSubject
        {
            Request = new RouteMoveRequest(
                workspace,
                sourceId,
                destinationTarget,
                RouteMoveMode.DryRun),
            Source = new RouteMoveSource { Requested = sourceId },
            Kind = RouteMoveSubjectKind.Category,
            Catalogue = new SourceCatalogue(workspace, [], [source, parent], [], isCancelled: false),
            SelectedSource = source,
            RouteFacts = RouteFacts(source, parent),
        };
        return new RouteMoveCategoryInventory
        {
            Subject = subject,
            Ownership = Ownership(),
        };
    }

    private static SourceLogicalSource ParentSource(CliWorkspace workspace)
    {
        const string path = ".agents/memory/archive/_archive.md";
        return new SourceLogicalSource(
            new SourceLogicalIdentity("memory/archive", path),
            Layer(workspace, path, SourceDocumentForm.CanonicalEntrypoint));
    }

    private static SourceLayer Layer(
        CliWorkspace workspace,
        string canonicalPath,
        SourceDocumentForm form)
        => new(
            canonicalPath,
            Path.GetFullPath(Path.Combine(
                workspace.LexicalRoot,
                canonicalPath.Replace('/', Path.DirectorySeparatorChar))),
            form,
            SourceLayerKind.Base);

    private static SourceRouteFacts RouteFacts(
        params SourceLogicalSource[] routedSources)
        => new(
            new SourceRouteTopology([], []),
            routedSources.Select(source => new SourceRouteFact(
                source.Identity,
                SourceRouteState.Routed,
                isIdentityUnique: true)),
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: false);
}
