using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateSourceSelection
{
    public required RouteUpdateRequest Request { get; init; }
    public required RouteUpdateTarget Target { get; init; }
    public required SourceCatalogue Catalogue { get; init; }
    public required SourceLogicalSource Source { get; init; }
}

internal sealed class RouteUpdateSourceSelectionBuild
{
    private RouteUpdateSourceSelectionBuild(
        RouteUpdateSourceSelection? selection,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((selection is null) == (boundary is null))
        {
            throw new ArgumentException(
                "A source selection build requires either one selection or one boundary.");
        }

        Selection = selection;
        Boundary = boundary;
    }

    public RouteUpdateSourceSelection? Selection { get; }
    public RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateSourceSelectionBuild Complete(RouteUpdateSourceSelection selection)
        => new(selection, boundary: null);

    internal static RouteUpdateSourceSelectionBuild Stop(RouteUpdatePlanningBoundary boundary)
        => new(selection: null, boundary);
}

internal sealed record RouteUpdateLayerObservationInput
{
    public required CliWorkspace Workspace { get; init; }
    public required SourceLayer Layer { get; init; }
}

internal sealed record RouteUpdateObservationBoundaryInput
{
    public required RouteUpdateRequest Request { get; init; }
    public required RouteUpdateTarget Target { get; init; }
    public required RouteUpdateFindingCode Code { get; init; }
    public required string Cause { get; init; }
    public required bool IsIncomplete { get; init; }
}
