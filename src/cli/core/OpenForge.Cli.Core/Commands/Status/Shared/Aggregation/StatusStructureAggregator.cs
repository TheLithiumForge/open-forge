using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusStructureAggregator
{
    internal static StatusStructure Build(
        RouteStatusView routes,
        OperationalInstallationState installation)
    {
        var valueState = CountState(routes.State, installation);
        var available = valueState == OperationalValueState.Available;
        var added = available
            ? routes.CurrentRootCategories.Except(routes.InitialRootCategories, StringComparer.Ordinal).ToArray()
            : [];
        var removed = available
            ? routes.InitialRootCategories.Except(routes.CurrentRootCategories, StringComparer.Ordinal).ToArray()
            : [];
        return new StatusStructure
        {
            RootCategories = new StatusRootCategories
            {
                Count = StatusMeasurementCalculator.Value(valueState, routes.CurrentRootCategories.Count),
                Added = added,
                Removed = removed,
            },
            GeneratedNavigation = routes.GeneratedNavigation
                .OrderBy(target => target.Path, StringComparer.Ordinal)
                .Select(target => Project(target, installation))
                .ToArray(),
        };
    }

    internal static StatusStructure Unavailable()
        => new()
        {
            RootCategories = new StatusRootCategories
            {
                Count = new StatusIntegerValue(OperationalValueState.Unavailable, null),
                Added = [],
                Removed = [],
            },
            GeneratedNavigation = [],
        };

    private static OperationalValueState CountState(
        OperationalViewState view,
        OperationalInstallationState installation)
    {
        if (installation == OperationalInstallationState.Uninstalled)
        {
            return OperationalValueState.NotApplicable;
        }

        return view == OperationalViewState.Complete
            ? OperationalValueState.Available
            : OperationalValueState.Unavailable;
    }

    private static StatusGeneratedNavigation Project(
        GeneratedNavigationTargetObservation target,
        OperationalInstallationState installation)
    {
        var state = installation == OperationalInstallationState.Uninstalled
            ? OperationalGeneratedNavigationState.NotApplicable
            : target.State;
        return new StatusGeneratedNavigation(target.Path, state);
    }
}
