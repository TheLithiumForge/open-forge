using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanLaterReading
{
    internal static string Description(IReadOnlyList<RouteInspectLaterReadOccasion> occasions)
    {
        var descriptions = new List<string>();
        if (occasions.Contains(RouteInspectLaterReadOccasion.ContextRestoration))
        {
            descriptions.Add("after context restoration");
        }

        if (occasions.Contains(RouteInspectLaterReadOccasion.Handoff)
            || occasions.Contains(RouteInspectLaterReadOccasion.Closeout))
        {
            descriptions.Add("before handoff or closeout");
        }

        if (occasions.Contains(RouteInspectLaterReadOccasion.FollowupTransition))
        {
            descriptions.Add("after a change that may affect its follow-up work");
        }

        return descriptions.Count switch
        {
            0 => "at the defined later review occasions",
            1 => descriptions[0],
            _ => string.Join(", ", descriptions.Take(descriptions.Count - 1))
                + $", or {descriptions[^1]}",
        };
    }
}
