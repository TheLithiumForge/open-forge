using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemoveNavigationPlanningRequest
{
    public required RouteRemoveResolvedSubject Subject { get; init; }

    public ImmutableArray<SourceLogicalSource> IntendedSources { get; init; } = [];
}

internal sealed record RouteRemoveNavigationPlan
{
    public required RouteRemoveNavigationPlanningRequest Request { get; init; }

    public required RouteRemoveGeneratedNavigation GeneratedNavigation { get; init; }

    public ImmutableArray<PlannedFileChange> FileChanges { get; init; } = [];

    public ImmutableArray<RouteRemoveNavigationDocumentEdit> DocumentEdits { get; init; } = [];
}

internal sealed record RouteRemoveNavigationDocumentEdit
{
    public required FileStateSnapshot Snapshot { get; init; }

    public required string LogicalPath { get; init; }

    public required SourceLocation Location { get; init; }

    public ImmutableArray<byte> BeforeBytes { get; init; } = [];

    public ImmutableArray<byte> ExpectedBytes { get; init; } = [];
}

internal sealed record RouteRemoveNavigationPlanningResult
{
    internal RouteRemoveNavigationPlanningResult(
        RouteRemoveNavigationPlan? plan,
        RouteRemoveResultFormation? boundary)
    {
        if ((plan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove navigation planning requires exactly one plan or boundary.");
        }

        Plan = plan;
        Boundary = boundary;
    }

    internal RouteRemoveNavigationPlan? Plan { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal enum RouteRemoveNavigationPostRemoveState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteRemoveNavigationPostRemoveResult(
    RouteRemoveNavigationPostRemoveState State,
    string? Cause);
