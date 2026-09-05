using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemoveReferencePlanningRequest
{
    public required RouteRemoveResolvedSubject Subject { get; init; }

    public required RouteMarkdownCatalogueRequest CatalogueRequest { get; init; }
}

internal sealed record RouteRemoveReferenceDocumentPlan
{
    public required string SourcePath { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }

    public required string IntendedText { get; init; }

    public ImmutableArray<RouteRemoveReferenceDocumentEdit> Edits { get; init; } = [];
}

internal sealed record RouteRemoveReferenceDocumentEdit
{
    public required SourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteRemoveReferencePlan
{
    public required RouteRemoveReferencePlanningRequest Request { get; init; }

    public required RouteMarkdownCatalogue Catalogue { get; init; }

    public required RouteRemoveReferences References { get; init; }

    public ImmutableArray<RouteRemoveReferenceDocumentPlan> Documents { get; init; } = [];

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];
}

internal sealed record RouteRemoveReferenceScanInput
{
    public required RouteRemoveReferencePlanningRequest Request { get; init; }

    public required RouteMarkdownCatalogue Catalogue { get; init; }
}

internal sealed record RouteRemoveReferenceScan
{
    public ImmutableArray<RouteRemoveReferenceDocumentPlan> Documents { get; init; } = [];

    public ImmutableArray<RouteRemoveReferenceDetachment> Detachments { get; init; } = [];

    public required int OccurrenceCount { get; init; }
}

internal sealed record RouteRemoveReferenceScanResult
{
    internal RouteRemoveReferenceScanResult(
        RouteRemoveReferenceScan? scan,
        RouteRemoveResultFormation? boundary)
    {
        if ((scan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove reference scanning requires exactly one scan or boundary.");
        }

        Scan = scan;
        Boundary = boundary;
    }

    internal RouteRemoveReferenceScan? Scan { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal sealed record RouteRemoveReferencePlanningResult
{
    internal RouteRemoveReferencePlanningResult(
        RouteRemoveReferencePlan? plan,
        RouteRemoveResultFormation? boundary)
    {
        if ((plan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove reference planning requires exactly one plan or boundary.");
        }

        Plan = plan;
        Boundary = boundary;
    }

    internal RouteRemoveReferencePlan? Plan { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal enum RouteRemoveReferencePostRemoveState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteRemoveReferencePostRemoveResult(
    RouteRemoveReferencePostRemoveState State,
    string? Cause);
