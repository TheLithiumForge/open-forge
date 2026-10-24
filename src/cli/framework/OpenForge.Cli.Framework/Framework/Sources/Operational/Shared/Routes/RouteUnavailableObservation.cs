using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal static class RouteUnavailableObservation
{
    internal static RouteStatusView Status(WorkspaceLayoutRead observation) => new()
    {
        State = OperationalViewState.Incomplete,
        SourceInventory = RouteSourceInventoryState.Incomplete,
        InitialStartup = RouteContextMeasurementReader.Unavailable(),
        CurrentStartup = RouteContextMeasurementReader.Unavailable(),
        TotalAvailable = RouteContextMeasurementReader.Unavailable(),
        Continuity = RouteContextMeasurementReader.Unavailable(),
        ContinuitySources = [],
        InitialRootCategories = [],
        CurrentRootCategories = [],
        GeneratedNavigation = [],
        ContextUnavailablePath = observation.Issues.FirstOrDefault()?.Path,
        ContextUnavailableCause = observation.Issues.FirstOrDefault()?.Cause,
    };

    internal static RouteDoctorView Doctor(WorkspaceLayoutRead observation) => new()
    {
        State = OperationalViewState.Incomplete,
        SourceInventory = RouteSourceInventoryState.Incomplete,
        Catalogue = null,
        Routes = new SourceRouteFacts(new SourceRouteTopology([], []), [], [], false, false),
        WorkspaceEntry = null,
        Sources = [],
        Metadata = [],
        GeneratedNavigation = [],
        DeclaredRoots = [],
        Shape = [],
        UnavailableCause = observation.Issues.FirstOrDefault()?.Cause,
    };
}
