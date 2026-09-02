using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdatePlanBuilder
{
    private readonly RouteUpdateTargetObserver _observer;
    private readonly RouteUpdateDestinationPlanner _destinationPlanner;
    private readonly RouteUpdateNavigationPlanner _navigationPlanner;
    private readonly RouteUpdatePlanProjector _projector;

    internal RouteUpdatePlanBuilder(
        RouteUpdateTargetObserver observer,
        RouteUpdateDestinationPlanner destinationPlanner,
        RouteUpdateNavigationPlanner navigationPlanner,
        RouteUpdatePlanProjector projector)
    {
        _observer = observer;
        _destinationPlanner = destinationPlanner;
        _navigationPlanner = navigationPlanner;
        _projector = projector;
    }

    internal async ValueTask<RouteUpdatePlanBuild> BuildAsync(
        RouteUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var observed = await _observer.ObserveAsync(
                new RouteUpdateObservationRequest
                {
                    Request = request,
                },
                cancellationToken)
            .ConfigureAwait(false);
        if (observed.Boundary is { } observationBoundary)
        {
            return Stop(observationBoundary);
        }

        var observation = observed.Observation
            ?? throw new InvalidOperationException(
                "Complete Route Update observation requires one observation.");
        var destination = await _destinationPlanner.BuildAsync(
                new RouteUpdateDestinationInput
                {
                    Observation = observation,
                },
                cancellationToken)
            .ConfigureAwait(false);
        if (destination.Boundary is { } destinationBoundary)
        {
            return Stop(destinationBoundary);
        }

        var completeDestination = destination.Destination
            ?? throw new InvalidOperationException(
                "Complete Route Update destination planning requires one destination.");
        var navigation = await _navigationPlanner.BuildAsync(
                new RouteUpdateNavigationInput
                {
                    Destination = completeDestination,
                },
                cancellationToken)
            .ConfigureAwait(false);
        if (navigation.Boundary is { } navigationBoundary)
        {
            return Stop(navigationBoundary);
        }

        return _projector.Build(
            new RouteUpdatePlanProjectionInput
            {
                Destination = completeDestination,
                Navigation = navigation.Navigation
                    ?? throw new InvalidOperationException(
                        "Complete Route Update navigation requires one plan."),
            });
    }

    private static RouteUpdatePlanBuild Stop(RouteUpdatePlanningBoundary boundary)
        => new()
        {
            Plan = null,
            Formation = boundary.Formation,
        };
}
