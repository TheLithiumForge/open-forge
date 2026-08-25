using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanAutomaticReading
{
    internal static string Explanation(RouteInspectAutomaticReading reading)
    {
        if (reading.Kind == RouteInspectAutomaticReadingKind.ParentLoadNow)
        {
            if (reading.RelatedSourceId is not { } relatedSourceId)
            {
                throw new InvalidOperationException(
                    "A parent-load automatic reading requires a related source ID.");
            }

            return $"{RouteInspectHumanValues.Text(relatedSourceId)} is read";
        }

        return reading.Kind switch
        {
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind => Events(reading.Events),
            RouteInspectAutomaticReadingKind.RoutedFileKeepInMind => Events(reading.Events),
            RouteInspectAutomaticReadingKind.OnDemand => "this route is selected",
            RouteInspectAutomaticReadingKind.OverwriteAfterBase => "the base source is read",
            _ => throw new ArgumentOutOfRangeException(
                nameof(reading),
                reading.Kind,
                "The automatic-reading kind is not defined."),
        };
    }

    private static string Events(
        IReadOnlyList<RouteInspectAutomaticReadingEvent> events)
    {
        var descriptions = events.Select(Event).ToArray();
        if (descriptions.Length == 1)
        {
            return descriptions[0];
        }

        return $"{string.Join(", ", descriptions[..^1])}, or {descriptions[^1]}";
    }

    private static string Event(RouteInspectAutomaticReadingEvent readingEvent)
    {
        return readingEvent switch
        {
            RouteInspectAutomaticReadingEvent.RouteSelected => "its route is selected",
            RouteInspectAutomaticReadingEvent.TaskStartVisible => "it is visible from task-start routing",
            RouteInspectAutomaticReadingEvent.ScopeSelected => "its scope is selected",
            RouteInspectAutomaticReadingEvent.AncestorRequired => "it is needed as an ancestor",
            RouteInspectAutomaticReadingEvent.TaskReview => "a task review point is reached",
            RouteInspectAutomaticReadingEvent.LaterReview => "a later review point is reached",
            _ => throw new ArgumentOutOfRangeException(
                nameof(readingEvent),
                readingEvent,
                "The automatic-reading event is not defined."),
        };
    }
}
