using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveReferencePlanningRequest
{
    public required RouteMoveResolvedDestination Destination { get; init; }

    public required RouteMarkdownCatalogueRequest CatalogueRequest { get; init; }
}

internal sealed record RouteMoveReferencePlan
{
    public required RouteMoveReferencePlanningRequest Request { get; init; }

    public required RouteMarkdownCatalogue Catalogue { get; init; }

    public required RouteMoveReferences References { get; init; }

    public ImmutableArray<RouteMoveReferenceDocumentPlan> Documents { get; init; } = [];

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];

}

internal sealed record RouteMoveReferenceScanInput
{
    public required RouteMoveReferencePlanningRequest Request { get; init; }

    public required RouteMarkdownCatalogue Catalogue { get; init; }

    public required IReadOnlyDictionary<string, string> MovedPaths { get; init; }
}

internal sealed record RouteMoveReferenceScan
{
    public ImmutableArray<RouteMoveReferenceDocumentPlan> Documents { get; init; } = [];

    public ImmutableArray<RouteMoveReferenceRewrite> Rewrites { get; init; } = [];

    public required int OccurrenceCount { get; init; }
}

internal sealed record RouteMoveReferenceScanResult
{
    internal RouteMoveReferenceScanResult(
        RouteMoveReferenceScan? scan,
        RouteMoveResultFormation? boundary)
    {
        if ((scan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move reference scan requires exactly one scan or boundary.");
        }

        Scan = scan;
        Boundary = boundary;
    }

    internal RouteMoveReferenceScan? Scan { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveReferencePlanningResult
{
    internal RouteMoveReferencePlanningResult(
        RouteMoveReferencePlan? plan,
        RouteMoveResultFormation? boundary)
    {
        if ((plan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move reference planning requires exactly one plan or boundary.");
        }

        Plan = plan;
        Boundary = boundary;
    }

    internal RouteMoveReferencePlan? Plan { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal enum RouteMoveReferencePostMoveState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteMoveReferencePostMoveResult(
    RouteMoveReferencePostMoveState State,
    string? Cause);
