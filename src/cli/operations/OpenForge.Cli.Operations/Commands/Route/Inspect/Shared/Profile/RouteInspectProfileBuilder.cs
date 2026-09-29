using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed class RouteInspectProfileBuilder
{
    internal RouteInspectProfile Build(
        RouteInspectResolution resolution,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? workingPaths = null,
        SourceApplicabilityResult? applicability = null)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        if (resolution.State is not (
                RouteInspectResolutionState.Resolved
                or RouteInspectResolutionState.Incomplete))
        {
            throw new ArgumentException(
                "Only resolved or incomplete resolutions can form a route-inspect profile.",
                nameof(resolution));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var loading = new RouteInspectLoadingFactsBuilder(resolution, cancellationToken, workingPaths).Build();
        cancellationToken.ThrowIfCancellationRequested();
        var reading = new RouteInspectReadingProfileBuilder(resolution, loading, applicability).Build();
        var measurements = new RouteInspectMeasurementsBuilder(
            resolution,
            loading,
            cancellationToken).Build();
        var topology = new RouteInspectTopologyProfileBuilder(resolution, cancellationToken).Build();
        var axioms = new RouteInspectAxiomsProfileBuilder(resolution, cancellationToken).Build();
        var incomplete = resolution.State == RouteInspectResolutionState.Incomplete
            || HasUnavailableReading(reading)
            || HasUnavailableMeasurements(measurements)
            || topology.State == RouteInspectFactState.Unavailable
            || HasUnavailableAxioms(axioms);
        return new RouteInspectProfile(
            reading,
            measurements,
            topology,
            axioms,
            incomplete ? RouteInspectCompleteness.Incomplete : RouteInspectCompleteness.Complete,
            RouteInspectSafety.Safe);
    }

    private static bool HasUnavailableReading(RouteInspectReadingProfile reading)
    {
        return reading.TaskStart.State == RouteInspectFactState.Unavailable
            || reading.Automatic.State == RouteInspectFactState.Unavailable
            || reading.Later.State == RouteInspectFactState.Unavailable;
    }

    private static bool HasUnavailableMeasurements(RouteInspectMeasurements measurements)
    {
        return measurements.OwnSource.State == RouteInspectFactState.Unavailable
            || measurements.SelectedClosure.State == RouteInspectFactState.Unavailable
            || measurements.TaskStartOverlap.State == RouteInspectFactState.Unavailable
            || measurements.SelectionAddition.State == RouteInspectFactState.Unavailable
            || measurements.LoadNowDescendants.State == RouteInspectFactState.Unavailable;
    }

    private static bool HasUnavailableAxioms(RouteInspectFact<RouteInspectAxiomsProfile> axioms)
    {
        if (axioms.State == RouteInspectFactState.Unavailable)
        {
            return true;
        }

        if (axioms.State != RouteInspectFactState.Value)
        {
            return false;
        }

        var value = axioms.ReadValue();
        return value.Inherited.State == RouteInspectFactState.Unavailable
            || value.Local.State == RouteInspectFactState.Unavailable;
    }
}
