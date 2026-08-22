using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonNames
{
    internal static string Status(CliSemanticStatus status)
    {
        return CliStatusDefinitions.Read(status).MachineName;
    }

    internal static string FactState(RouteInspectFactState state)
    {
        return state switch
        {
            RouteInspectFactState.Value => "value",
            RouteInspectFactState.Unavailable => "unavailable",
            RouteInspectFactState.NotApplicable => "not-applicable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The fact state is not defined."),
        };
    }

    internal static string ReferenceKind(RouteInspectReferenceKind kind)
    {
        return kind switch
        {
            RouteInspectReferenceKind.Missing => "missing",
            RouteInspectReferenceKind.SourceId => "source-id",
            RouteInspectReferenceKind.SourcePath => "source-path",
            RouteInspectReferenceKind.Invalid => "invalid",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The reference kind is not defined."),
        };
    }

    internal static string SelectionMethod(RouteInspectSelectionMethod method)
    {
        return method switch
        {
            RouteInspectSelectionMethod.Unresolved => "unresolved",
            RouteInspectSelectionMethod.AutomaticId => "automatic-id",
            RouteInspectSelectionMethod.ExactPath => "exact-path",
            RouteInspectSelectionMethod.Interactive => "interactive",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The selection method is not defined."),
        };
    }

    internal static string SourceKind(RouteInspectSourceKind kind)
    {
        return kind switch
        {
            RouteInspectSourceKind.Entrypoint => "entrypoint",
            RouteInspectSourceKind.Markdown => "markdown",
            RouteInspectSourceKind.Native => "native",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source kind is not defined."),
        };
    }

    internal static string SourceForm(RouteInspectSourceForm form)
    {
        return form switch
        {
            RouteInspectSourceForm.CanonicalEntrypoint => "canonical",
            RouteInspectSourceForm.CompatibilityEntrypoint => "compatibility",
            RouteInspectSourceForm.Markdown => "markdown",
            RouteInspectSourceForm.Native => "native",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };
    }

    internal static string RouteState(RouteInspectRouteState state)
    {
        return state switch
        {
            RouteInspectRouteState.Routed => "routed",
            RouteInspectRouteState.Detached => "detached",
            RouteInspectRouteState.NotRouted => "not-routed",
            RouteInspectRouteState.Ambiguous => "ambiguous",
            RouteInspectRouteState.Unresolved => "unresolved",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route state is not defined."),
        };
    }

    internal static string LayerRole(RouteInspectLayerRole role)
    {
        return role switch
        {
            RouteInspectLayerRole.Base => "base",
            RouteInspectLayerRole.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The layer role is not defined."),
        };
    }

    internal static string ReadingKind(RouteInspectAutomaticReadingKind kind)
    {
        return kind switch
        {
            RouteInspectAutomaticReadingKind.OnDemand => "on-demand",
            RouteInspectAutomaticReadingKind.ParentLoadNow => "parent-load-now",
            RouteInspectAutomaticReadingKind.EntrypointKeepInMind => "entrypoint-keep-in-mind",
            RouteInspectAutomaticReadingKind.RoutedFileKeepInMind => "routed-file-keep-in-mind",
            RouteInspectAutomaticReadingKind.OverwriteAfterBase => "overwrite-after-base",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The automatic-reading kind is not defined."),
        };
    }

    internal static string ReadingEvent(RouteInspectAutomaticReadingEvent readingEvent)
    {
        return readingEvent switch
        {
            RouteInspectAutomaticReadingEvent.RouteSelected => "route-selected",
            RouteInspectAutomaticReadingEvent.ExposingParentRead => "exposing-parent-read",
            RouteInspectAutomaticReadingEvent.TaskStartVisible => "task-start-visible",
            RouteInspectAutomaticReadingEvent.ScopeSelected => "scope-selected",
            RouteInspectAutomaticReadingEvent.AncestorRequired => "ancestor-required",
            RouteInspectAutomaticReadingEvent.TaskReview => "task-review",
            RouteInspectAutomaticReadingEvent.LaterReview => "later-review",
            RouteInspectAutomaticReadingEvent.BaseRead => "base-read",
            _ => throw new ArgumentOutOfRangeException(nameof(readingEvent), readingEvent, "The reading event is not defined."),
        };
    }

    internal static string LaterOccasion(RouteInspectLaterReadOccasion occasion)
    {
        return occasion switch
        {
            RouteInspectLaterReadOccasion.ContextRestoration => "context-restoration",
            RouteInspectLaterReadOccasion.Handoff => "handoff",
            RouteInspectLaterReadOccasion.Closeout => "closeout",
            RouteInspectLaterReadOccasion.FollowupTransition => "followup-transition",
            _ => throw new ArgumentOutOfRangeException(nameof(occasion), occasion, "The later-read occasion is not defined."),
        };
    }

    internal static string Completeness(RouteInspectCompleteness completeness)
    {
        return completeness switch
        {
            RouteInspectCompleteness.Complete => "complete",
            RouteInspectCompleteness.Incomplete => "incomplete",
            RouteInspectCompleteness.NotStarted => "not-started",
            _ => throw new ArgumentOutOfRangeException(nameof(completeness), completeness, "The completeness is not defined."),
        };
    }

    internal static string Safety(RouteInspectSafety safety)
    {
        return safety switch
        {
            RouteInspectSafety.Safe => "safe",
            RouteInspectSafety.Blocked => "blocked",
            RouteInspectSafety.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(safety), safety, "The safety is not defined."),
        };
    }

    internal static string LocalAxioms(RouteInspectAxiomsLocalState state)
    {
        return state switch
        {
            RouteInspectAxiomsLocalState.Substantive => "substantive",
            RouteInspectAxiomsLocalState.InheritedSentinel => "inherited-sentinel",
            RouteInspectAxiomsLocalState.Empty => "empty",
            RouteInspectAxiomsLocalState.Missing => "missing",
            RouteInspectAxiomsLocalState.NotApplicable => "not-applicable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The local Axioms state is not defined."),
        };
    }
}
