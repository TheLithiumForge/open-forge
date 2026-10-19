using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMovePlanBuilder
{
    private readonly RouteMoveSubjectResolver _subjectResolver;
    private readonly RouteMoveCategoryInventoryReader _inventoryReader;
    private readonly RouteMoveDestinationResolver _destinationResolver;
    private readonly RouteMoveReferencePlanner _referencePlanner;
    private readonly RouteMoveNavigationPlanner _navigationPlanner;

    internal RouteMovePlanBuilder(
        RouteMoveSubjectResolver subjectResolver,
        RouteMoveCategoryInventoryReader inventoryReader,
        RouteMoveDestinationResolver destinationResolver,
        RouteMoveReferencePlanner referencePlanner,
        RouteMoveNavigationPlanner navigationPlanner)
    {
        _subjectResolver = subjectResolver;
        _inventoryReader = inventoryReader;
        _destinationResolver = destinationResolver;
        _referencePlanner = referencePlanner;
        _navigationPlanner = navigationPlanner;
    }

    internal async ValueTask<RouteMovePlanBuild> BuildAsync(
        RouteMoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = await _subjectResolver.ResolveAsync(
            request,
            cancellationToken).ConfigureAwait(false);
        if (subject.Boundary is { } subjectBoundary)
        {
            return new RouteMovePlanBuild(plan: null, subjectBoundary);
        }

        var inventory = await _inventoryReader.ReadAsync(
            subject.Subject
                    ?? throw new InvalidOperationException(
                        "A successful Route Move subject resolution requires its subject."),
            cancellationToken).ConfigureAwait(false);
        if (inventory.Boundary is { } inventoryBoundary)
        {
            return new RouteMovePlanBuild(plan: null, inventoryBoundary);
        }

        var destination = _destinationResolver.Resolve(
            inventory.Inventory
                    ?? throw new InvalidOperationException(
                        "A successful Route Move inventory requires its inventory."));
        if (destination.Boundary is { } destinationBoundary)
        {
            return new RouteMovePlanBuild(plan: null, destinationBoundary);
        }

        return await BuildCompletePlanAsync(
            destination.Destination
                ?? throw new InvalidOperationException(
                    "A successful Route Move destination resolution requires its destination."),
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<RouteMovePlanBuild> BuildCompletePlanAsync(
        RouteMoveResolvedDestination destination,
        CancellationToken cancellationToken)
    {
        var workspace = destination.Inventory.Subject.Request.Workspace;
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
            return new RouteMovePlanBuild(null, RouteMoveBoundary.Stop(RouteMoveBoundary.Start(destination.Inventory.Subject.Request),
                Models.Result.RouteMoveFindingCode.ReferenceCoverageIncomplete, CliSemanticStatus.Incomplete, settings.LogicalPath,
                settings.Cause ?? "The required workspace settings are unavailable or invalid."));
        destination = destination with { Inventory = destination.Inventory with { Settings = settings } };
        var references = await _referencePlanner.BuildAsync(
            new RouteMoveReferencePlanningRequest
            {
                Destination = destination,
                CatalogueRequest = new RouteMarkdownCatalogueRequest(
                    workspace,
                    ["."],
                    [],
                    new RouteMarkdownCatalogueFilters([".md"])),
            },
            cancellationToken).ConfigureAwait(false);
        if (references.Boundary is { } referenceBoundary)
        {
            return new RouteMovePlanBuild(plan: null, referenceBoundary);
        }

        var referencePlan = references.Plan
            ?? throw new InvalidOperationException(
                "Successful Route Move reference planning requires its plan.");
        var navigation = _navigationPlanner.Build(destination);
        if (navigation.Boundary is { } navigationBoundary)
        {
            return new RouteMovePlanBuild(plan: null, navigationBoundary with
            {
                References = referencePlan.References,
            });
        }

        var plan = RouteMovePlanProjector.Build(RouteMoveEffectPlanner.Build(
            destination,
            referencePlan,
            navigation.Plan
                ?? throw new InvalidOperationException(
                    "Successful Route Move navigation planning requires its plan.")));
        return new RouteMovePlanBuild(plan, plan.Preview);
    }

}
