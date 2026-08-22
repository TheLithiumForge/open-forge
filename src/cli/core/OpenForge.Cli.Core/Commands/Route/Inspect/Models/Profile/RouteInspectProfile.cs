namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal enum RouteInspectCompleteness
{
    Complete,
    Incomplete,
    NotStarted,
}

internal enum RouteInspectSafety
{
    Safe,
    Blocked,
    Unknown,
}

internal sealed class RouteInspectProfile
{
    internal RouteInspectProfile(
        RouteInspectReadingProfile reading,
        RouteInspectMeasurements measurements,
        RouteInspectFact<RouteInspectTopology> topology,
        RouteInspectFact<RouteInspectAxiomsProfile> axioms,
        RouteInspectCompleteness completeness,
        RouteInspectSafety safety)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(measurements);
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(axioms);
        if (!Enum.IsDefined(completeness))
        {
            throw new ArgumentOutOfRangeException(
                nameof(completeness),
                completeness,
                "The route-inspect completeness is not defined.");
        }

        if (!Enum.IsDefined(safety))
        {
            throw new ArgumentOutOfRangeException(
                nameof(safety),
                safety,
                "The route-inspect safety is not defined.");
        }

        Reading = reading;
        Measurements = measurements;
        Topology = topology;
        Axioms = axioms;
        Completeness = completeness;
        Safety = safety;
    }

    internal RouteInspectReadingProfile Reading { get; }

    internal RouteInspectMeasurements Measurements { get; }

    internal RouteInspectFact<RouteInspectTopology> Topology { get; }

    internal RouteInspectFact<RouteInspectAxiomsProfile> Axioms { get; }

    internal RouteInspectCompleteness Completeness { get; }

    internal RouteInspectSafety Safety { get; }
}
