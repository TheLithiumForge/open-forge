using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record OwnershipObservation(
    WorkspaceOwnershipReadState State,
    string? Cause,
    FileExpectation? Expectation,
    ImmutableArray<OwnedPath> Claims)
{
    internal bool Matches(OwnershipObservation actual)
        => State == actual.State && Cause == actual.Cause && Expectation == actual.Expectation
            && Claims.SequenceEqual(actual.Claims);
}

internal sealed record InventoryObservation(
    RouteMoveItemKind Kind,
    RouteMoveLayerKind? Layer,
    string? SourceId,
    string SourcePath,
    string RelativePath,
    FileExpectation Expectation);

internal sealed record ReferenceObservation(
    RouteMarkdownCatalogueCoverage CatalogueCoverage,
    ImmutableArray<string> SelectedPaths,
    ImmutableArray<RouteMarkdownCatalogueFinding> CatalogueFindings,
    RouteMoveCoverage Coverage,
    int Scanned,
    int Inspected,
    int Occurrences,
    ImmutableArray<RouteMoveReferenceRewrite> Rewrites)
{
    internal bool Matches(ReferenceObservation actual)
        => CatalogueCoverage == actual.CatalogueCoverage
            && SelectedPaths.SequenceEqual(actual.SelectedPaths)
            && CatalogueFindings.SequenceEqual(actual.CatalogueFindings)
            && Coverage == actual.Coverage
            && Scanned == actual.Scanned
            && Inspected == actual.Inspected
            && Occurrences == actual.Occurrences
            && Rewrites.SequenceEqual(actual.Rewrites);
}

internal sealed record ReferenceDocumentObservation(
    string SourcePath,
    string DestinationPath,
    FileExpectation Expectation,
    string IntendedText);

internal sealed record ReferenceEditObservation(
    string SourcePath,
    string DestinationPath,
    SourceLocation Location,
    string Before,
    string Expected);

internal sealed record NavigationObservation(
    RouteMoveCoverage Coverage,
    ImmutableArray<IntendedSourceObservation> IntendedSources)
{
    internal bool Matches(NavigationObservation actual)
        => Coverage == actual.Coverage
            && IntendedSources.SequenceEqual(actual.IntendedSources);
}

internal sealed record IntendedSourceObservation(
    string Id,
    string Path,
    string BasePath,
    SourceDocumentForm BaseForm,
    SourceLayerKind BaseKind,
    string? OverwritePath,
    SourceDocumentForm? OverwriteForm,
    SourceLayerKind? OverwriteKind);

internal sealed record NavigationRegionObservation(
    string Path,
    RouteMoveGeneratedState State,
    ImmutableArray<RouteMoveGeneratedReason> Reasons);

internal sealed record NavigationEditObservation(
    string DestinationPath,
    FileExpectation Expectation,
    SourceLocation Location,
    string BeforeBytes,
    string ExpectedBytes);

internal sealed record SemanticPlanningProjection(
    OwnershipObservation Lifecycle,
    RouteMoveDestination Destination,
    ImmutableArray<InventoryObservation> Inventory);

internal sealed record SemanticReferenceProjection(
    ReferenceObservation Observation,
    ImmutableArray<ReferenceDocumentObservation> Documents,
    ImmutableArray<ReferenceEditObservation> Edits);

internal sealed record SemanticNavigationProjection(
    NavigationObservation Observation,
    ImmutableArray<NavigationRegionObservation> Regions,
    ImmutableArray<NavigationEditObservation> Edits);
