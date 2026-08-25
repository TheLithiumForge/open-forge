using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectProfileTestData
{
    internal static RouteInspectProfile Profile(RouteInspectProfileSpec? spec = null)
    {
        spec ??= new RouteInspectProfileSpec();
        var reading = new RouteInspectReadingProfile(
            spec.TaskStart ?? RouteInspectFact<bool>.Available(false),
            spec.Automatic ?? RouteInspectFact<RouteInspectAutomaticReadings>.Available(OnDemand()),
            spec.Later ?? RouteInspectFact<RouteInspectLaterReading>.Available(NotReadAgain()));
        return new RouteInspectProfile(
            reading,
            spec.Measurements ?? EmptyMeasurements(),
            spec.Topology ?? RouteInspectFact<RouteInspectTopology>.Available(EmptyTopology()),
            spec.Axioms ?? RouteInspectFact<RouteInspectAxiomsProfile>.Available(EmptyAxioms()),
            spec.Completeness,
            spec.Safety);
    }

    internal static RouteInspectAutomaticReadings OnDemand()
    {
        return new RouteInspectAutomaticReadings(
        [
            new RouteInspectAutomaticReading(
                RouteInspectAutomaticReadingKind.OnDemand,
                null,
                [RouteInspectAutomaticReadingEvent.RouteSelected]),
        ]);
    }

    internal static RouteInspectLaterReading NotReadAgain()
    {
        return new RouteInspectLaterReading(false, []);
    }

    internal static RouteInspectLaterReading ReadAgain()
    {
        return new RouteInspectLaterReading(
            true,
            [
                RouteInspectLaterReadOccasion.ContextRestoration,
                RouteInspectLaterReadOccasion.Handoff,
                RouteInspectLaterReadOccasion.Closeout,
                RouteInspectLaterReadOccasion.FollowupTransition,
            ]);
    }

    internal static RouteInspectMeasurements EmptyMeasurements()
    {
        var empty = RouteInspectFact<RouteInspectMeasurement>.Available(new RouteInspectMeasurement(0, 0, 0));
        return new RouteInspectMeasurements(empty, empty, empty, empty, empty);
    }

    internal static RouteInspectMeasurements Measurements(RouteInspectMeasurementsSpec spec)
    {
        return new RouteInspectMeasurements(
            spec.OwnSource,
            spec.SelectedClosure,
            spec.TaskStartOverlap,
            spec.SelectionAddition,
            spec.LoadNowDescendants);
    }

    internal static RouteInspectFact<RouteInspectMeasurement> Measurement(
        long physicalFileCount,
        long unicodeScalarCount,
        long utf8ByteCount)
    {
        return RouteInspectFact<RouteInspectMeasurement>.Available(
            new RouteInspectMeasurement(physicalFileCount, unicodeScalarCount, utf8ByteCount));
    }

    internal static RouteInspectFact<RouteInspectMeasurement> UnavailableMeasurement(string reason = "The fixed measurement is unavailable.")
    {
        return RouteInspectFact<RouteInspectMeasurement>.Unavailable(reason);
    }

    internal static RouteInspectFact<RouteInspectMeasurement> NotApplicableMeasurement(string reason = "The fixed measurement is not applicable.")
    {
        return RouteInspectFact<RouteInspectMeasurement>.NotApplicable(reason);
    }

    internal static RouteInspectTopology Topology(RouteInspectTopologySpec spec)
    {
        return new RouteInspectTopology(spec.RootRoute, spec.RouteChain, spec.ParentId, spec.Depth, spec.Counts);
    }

    internal static RouteInspectFact<RouteInspectTopologyCounts> Counts(
        int directRoutedFileCount,
        int directEntrypointCount,
        int descendantRoutedFileCount,
        int descendantEntrypointCount)
    {
        return RouteInspectFact<RouteInspectTopologyCounts>.Available(
            new RouteInspectTopologyCounts(
                directRoutedFileCount,
                directEntrypointCount,
                descendantRoutedFileCount,
                descendantEntrypointCount));
    }

    internal static RouteInspectAxiomsProfile Axioms(
        RouteInspectFact<RouteInspectAxiomsSources> inherited,
        RouteInspectFact<RouteInspectAxiomsLocalState> local)
    {
        return new RouteInspectAxiomsProfile(inherited, local);
    }

    internal static RouteInspectFact<RouteInspectAxiomsSources> Sources(params string[] sourceIds)
    {
        return RouteInspectFact<RouteInspectAxiomsSources>.Available(new RouteInspectAxiomsSources(sourceIds));
    }

    private static RouteInspectTopology EmptyTopology()
    {
        return Topology(new RouteInspectTopologySpec
        {
            RootRoute = "root",
            RouteChain = ["root"],
            Depth = 1,
            Counts = Counts(0, 0, 0, 0),
        });
    }

    private static RouteInspectAxiomsProfile EmptyAxioms()
    {
        return new RouteInspectAxiomsProfile(
            Sources(),
            RouteInspectFact<RouteInspectAxiomsLocalState>.Available(RouteInspectAxiomsLocalState.Empty));
    }
}

internal sealed record RouteInspectProfileSpec
{
    internal RouteInspectCompleteness Completeness { get; init; } = RouteInspectCompleteness.Complete;

    internal RouteInspectSafety Safety { get; init; } = RouteInspectSafety.Safe;

    internal RouteInspectFact<bool>? TaskStart { get; init; }

    internal RouteInspectFact<RouteInspectAutomaticReadings>? Automatic { get; init; }

    internal RouteInspectFact<RouteInspectLaterReading>? Later { get; init; }

    internal RouteInspectMeasurements? Measurements { get; init; }

    internal RouteInspectFact<RouteInspectTopology>? Topology { get; init; }

    internal RouteInspectFact<RouteInspectAxiomsProfile>? Axioms { get; init; }
}

internal sealed record RouteInspectMeasurementsSpec
{
    internal required RouteInspectFact<RouteInspectMeasurement> OwnSource { get; init; }

    internal required RouteInspectFact<RouteInspectMeasurement> SelectedClosure { get; init; }

    internal required RouteInspectFact<RouteInspectMeasurement> TaskStartOverlap { get; init; }

    internal required RouteInspectFact<RouteInspectMeasurement> SelectionAddition { get; init; }

    internal required RouteInspectFact<RouteInspectMeasurement> LoadNowDescendants { get; init; }
}

internal sealed record RouteInspectTopologySpec
{
    internal required string RootRoute { get; init; }

    internal required IEnumerable<string> RouteChain { get; init; }

    internal string? ParentId { get; init; }

    internal required int Depth { get; init; }

    internal required RouteInspectFact<RouteInspectTopologyCounts> Counts { get; init; }
}
