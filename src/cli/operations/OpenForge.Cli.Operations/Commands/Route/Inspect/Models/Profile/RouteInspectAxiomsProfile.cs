using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal enum RouteInspectAxiomsLocalState
{
    Substantive,
    InheritedSentinel,
    Empty,
    Missing,
    NotApplicable,
}

internal sealed class RouteInspectAxiomsSources
{
    internal RouteInspectAxiomsSources(IEnumerable<string> sourceIds)
    {
        ArgumentNullException.ThrowIfNull(sourceIds);
        var materialized = sourceIds.ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sourceId in materialized)
        {
            if (sourceId is null)
            {
                throw new ArgumentException("Axioms source IDs cannot contain null.", nameof(sourceIds));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
            if (!seen.Add(sourceId))
            {
                throw new ArgumentException("Axioms source IDs must be unique.", nameof(sourceIds));
            }
        }

        SourceIds = new ReadOnlyCollection<string>(materialized);
    }

    internal IReadOnlyList<string> SourceIds { get; }
}

internal sealed class RouteInspectAxiomsProfile
{
    internal RouteInspectAxiomsProfile(
        RouteInspectFact<RouteInspectAxiomsSources> inherited,
        RouteInspectFact<RouteInspectAxiomsLocalState> local)
    {
        ArgumentNullException.ThrowIfNull(inherited);
        ArgumentNullException.ThrowIfNull(local);
        if (local.State == RouteInspectFactState.Value && !Enum.IsDefined(local.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(local),
                local.Value,
                "The route-inspect local Axioms state is not defined.");
        }

        Inherited = inherited;
        Local = local;
    }

    internal RouteInspectFact<RouteInspectAxiomsSources> Inherited { get; }

    internal RouteInspectFact<RouteInspectAxiomsLocalState> Local { get; }
}
