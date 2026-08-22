using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectMeasurementsBuilder
{
    private readonly RouteInspectResolution _resolution;
    private readonly RouteInspectLoadingFacts _loading;

    internal RouteInspectMeasurementsBuilder(
        RouteInspectResolution resolution,
        RouteInspectLoadingFacts loading,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(loading);
        _resolution = resolution;
        _loading = loading;
        _catalogue = resolution.Graph!.Catalogue;
        _cancellationToken = cancellationToken;
    }

    internal RouteInspectMeasurements Build()
    {
        var selectedPath = _resolution.Identity!.CanonicalWorkspaceRelativePath;
        var own = Measure([selectedPath], "The selected source body is unavailable.");
        var selectedClosure = ReadSelectedClosure();
        var overlap = ReadOverlap();
        var addition = ReadAddition();
        var descendants = ReadDescendants();
        return new RouteInspectMeasurements(own, selectedClosure, overlap, addition, descendants);
    }

    private RouteInspectFact<RouteInspectMeasurement> ReadSelectedClosure()
    {
        if (!_loading.SelectedApplicable)
        {
            return NotApplicable("Selected route context does not apply to this source.");
        }

        return _loading.SelectedAvailable
            ? Measure(_loading.SelectedPaths, "The selected route closure is unavailable.")
            : Unavailable("The selected route closure is unavailable.");
    }

    private RouteInspectFact<RouteInspectMeasurement> ReadOverlap()
    {
        if (!_loading.StartupApplicable)
        {
            return NotApplicable("Loader-rooted task-start comparison does not apply to this source.");
        }

        if (!_loading.StartupAvailable || !_loading.SelectedAvailable)
        {
            return Unavailable("The task-start and selected closures cannot be compared completely.");
        }

        var overlap = _loading.SelectedPaths
            .Where(_loading.StartupPaths.Contains)
            .ToArray();
        return Measure(overlap, "The task-start overlap is unavailable.");
    }

    private RouteInspectFact<RouteInspectMeasurement> ReadAddition()
    {
        if (!_loading.SelectedApplicable || !_loading.StartupApplicable)
        {
            return NotApplicable("Selection addition does not apply to this source.");
        }

        if (!_loading.StartupAvailable || !_loading.SelectedAvailable)
        {
            return Unavailable("The selection addition cannot be established completely.");
        }

        var startupPhysicalPaths = ReadPhysicalPaths(_loading.StartupPaths);
        return Measure(
            _loading.SelectedPaths,
            "The selection addition is unavailable.",
            startupPhysicalPaths);
    }

    private RouteInspectFact<RouteInspectMeasurement> ReadDescendants()
    {
        if (!_loading.NarrowDescendantsApplicable)
        {
            return NotApplicable("LoadNow descendants do not apply to an ordinary source.");
        }

        return _loading.NarrowDescendantsAvailable
            ? Measure(_loading.LoadNowDescendantPaths, "LoadNow descendants are unavailable.")
            : Unavailable("The visible LoadNow descendants cannot be established completely.");
    }

    private static RouteInspectFact<RouteInspectMeasurement> Unavailable(string reason)
    {
        return RouteInspectFact<RouteInspectMeasurement>.Unavailable(reason);
    }

    private static RouteInspectFact<RouteInspectMeasurement> NotApplicable(string reason)
    {
        return RouteInspectFact<RouteInspectMeasurement>.NotApplicable(reason);
    }
}
