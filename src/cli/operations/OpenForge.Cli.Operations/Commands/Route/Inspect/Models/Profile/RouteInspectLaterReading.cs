using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal enum RouteInspectLaterReadOccasion
{
    ContextRestoration,
    Handoff,
    Closeout,
    FollowupTransition,
}

internal sealed class RouteInspectLaterReading
{
    internal RouteInspectLaterReading(
        bool mayBeReadAgain,
        IEnumerable<RouteInspectLaterReadOccasion> occasions)
    {
        ArgumentNullException.ThrowIfNull(occasions);
        var materialized = occasions.ToArray();
        var seen = new HashSet<RouteInspectLaterReadOccasion>();
        foreach (var occasion in materialized)
        {
            if (!Enum.IsDefined(occasion))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(occasions),
                    occasion,
                    "The route-inspect later-read occasion is not defined.");
            }

            if (!seen.Add(occasion))
            {
                throw new ArgumentException("Later-read occasions must be unique.", nameof(occasions));
            }
        }

        if (!mayBeReadAgain && materialized.Length != 0)
        {
            throw new ArgumentException(
                "A later reading that may not occur again requires no occasions.",
                nameof(occasions));
        }

        if (mayBeReadAgain && materialized.Length == 0)
        {
            throw new ArgumentException(
                "A later reading that may occur again requires at least one occasion.",
                nameof(occasions));
        }

        MayBeReadAgain = mayBeReadAgain;
        Occasions = new ReadOnlyCollection<RouteInspectLaterReadOccasion>(materialized);
    }

    internal bool MayBeReadAgain { get; }

    internal IReadOnlyList<RouteInspectLaterReadOccasion> Occasions { get; }
}
