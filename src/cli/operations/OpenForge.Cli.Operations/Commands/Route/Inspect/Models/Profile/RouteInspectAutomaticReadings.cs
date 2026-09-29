using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal sealed class RouteInspectAutomaticReadings
{
    private static readonly RouteInspectAutomaticReadingKind[] CanonicalOrder =
    [
        RouteInspectAutomaticReadingKind.ParentLoadNow,
        RouteInspectAutomaticReadingKind.EntrypointKeepInMind,
        RouteInspectAutomaticReadingKind.RoutedFileKeepInMind,
        RouteInspectAutomaticReadingKind.OnDemand,
        RouteInspectAutomaticReadingKind.OverwriteAfterBase,
    ];

    internal RouteInspectAutomaticReadings(IEnumerable<RouteInspectAutomaticReading> reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        var materialized = reasons.ToArray();
        if (materialized.Length == 0 || materialized.Any(reason => reason is null))
        {
            throw new ArgumentException("Automatic-reading reasons require at least one non-null value.", nameof(reasons));
        }

        var kinds = materialized.Select(reason => reason.Kind).ToArray();
        if (kinds.Distinct().Count() != kinds.Length
            || !CanonicalOrder.Where(kinds.Contains).SequenceEqual(kinds))
        {
            throw new ArgumentException("Automatic-reading reasons must be unique canonical order.", nameof(reasons));
        }

        if (kinds.Contains(RouteInspectAutomaticReadingKind.OnDemand)
            && kinds.Any(kind => kind is not (
                RouteInspectAutomaticReadingKind.OnDemand
                or RouteInspectAutomaticReadingKind.OverwriteAfterBase)))
        {
            throw new ArgumentException("On-demand reading cannot accompany another base trigger.", nameof(reasons));
        }

        if (kinds.Contains(RouteInspectAutomaticReadingKind.EntrypointKeepInMind)
            && kinds.Contains(RouteInspectAutomaticReadingKind.RoutedFileKeepInMind)
            || kinds.All(kind => kind == RouteInspectAutomaticReadingKind.OverwriteAfterBase))
        {
            throw new ArgumentException("Automatic-reading reasons do not describe one logical source.", nameof(reasons));
        }

        Reasons = new ReadOnlyCollection<RouteInspectAutomaticReading>(materialized);
    }

    internal IReadOnlyList<RouteInspectAutomaticReading> Reasons { get; }
}
