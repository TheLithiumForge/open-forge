using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

internal enum RouteInspectAutomaticReadingKind
{
    OnDemand,
    ParentLoadNow,
    EntrypointKeepInMind,
    RoutedFileKeepInMind,
    OverwriteAfterBase,
}

internal enum RouteInspectAutomaticReadingEvent
{
    RouteSelected,
    ExposingParentRead,
    TaskStartVisible,
    ScopeSelected,
    AncestorRequired,
    TaskReview,
    LaterReview,
    BaseRead,
}

internal sealed class RouteInspectAutomaticReading
{
    internal RouteInspectAutomaticReading(
        RouteInspectAutomaticReadingKind kind,
        string? relatedSourceId,
        IEnumerable<RouteInspectAutomaticReadingEvent> events)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The route-inspect automatic-reading kind is not defined.");
        }

        if (kind is RouteInspectAutomaticReadingKind.ParentLoadNow
            or RouteInspectAutomaticReadingKind.OverwriteAfterBase
            or RouteInspectAutomaticReadingKind.EntrypointKeepInMind
            or RouteInspectAutomaticReadingKind.RoutedFileKeepInMind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(relatedSourceId);
        }
        else if (relatedSourceId is not null)
        {
            throw new ArgumentException(
                "This automatic-reading kind cannot contain a related source ID.",
                nameof(relatedSourceId));
        }

        ArgumentNullException.ThrowIfNull(events);
        var materializedEvents = events.ToArray();
        ValidateEvents(kind, materializedEvents);
        Kind = kind;
        RelatedSourceId = relatedSourceId;
        Events = new ReadOnlyCollection<RouteInspectAutomaticReadingEvent>(materializedEvents);
    }

    internal RouteInspectAutomaticReadingKind Kind { get; }

    internal string? RelatedSourceId { get; }

    internal IReadOnlyList<RouteInspectAutomaticReadingEvent> Events { get; }

    private static void ValidateEvents(
        RouteInspectAutomaticReadingKind kind,
        IReadOnlyList<RouteInspectAutomaticReadingEvent> events)
    {
        var seen = new HashSet<RouteInspectAutomaticReadingEvent>();
        foreach (var readingEvent in events)
        {
            if (!Enum.IsDefined(readingEvent))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(events),
                    readingEvent,
                    "The route-inspect automatic-reading event is not defined.");
            }

            if (!seen.Add(readingEvent))
            {
                throw new ArgumentException("Automatic-reading events must be unique.", nameof(events));
            }
        }

        var allowed = ReadAllowedEvents(kind);
        if (kind == RouteInspectAutomaticReadingKind.EntrypointKeepInMind)
        {
            if (events.Count == 0 || !allowed.Where(events.Contains).SequenceEqual(events))
            {
                throw new ArgumentException(
                    "Entrypoint KeepInMind events must be a nonempty canonical subset.",
                    nameof(events));
            }

            return;
        }

        if (!allowed.SequenceEqual(events))
        {
            throw new ArgumentException(
                "Automatic-reading events do not match the reading kind.",
                nameof(events));
        }
    }

    private static RouteInspectAutomaticReadingEvent[] ReadAllowedEvents(
        RouteInspectAutomaticReadingKind kind)
    {
        return kind switch
        {
            RouteInspectAutomaticReadingKind.OnDemand =>
                [RouteInspectAutomaticReadingEvent.RouteSelected],
            RouteInspectAutomaticReadingKind.ParentLoadNow =>
                [RouteInspectAutomaticReadingEvent.ExposingParentRead],
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind =>
            [
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            RouteInspectAutomaticReadingKind.RoutedFileKeepInMind =>
            [
                RouteInspectAutomaticReadingEvent.ExposingParentRead,
                RouteInspectAutomaticReadingEvent.LaterReview,
            ],
            RouteInspectAutomaticReadingKind.OverwriteAfterBase =>
                [RouteInspectAutomaticReadingEvent.BaseRead],
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The route-inspect automatic-reading kind is not defined."),
        };
    }
}
