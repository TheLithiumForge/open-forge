using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanValues
{
    internal static string Text(string value)
    {
        return RouteInspectTextEscaping.Escape(value, RouteInspectTextEscaping.HumanValueLimit);
    }

    internal static string Status(CliSemanticStatus status)
    {
        return CliStatusDefinitions.Read(status).MachineName;
    }

    internal static string SelectedBy(CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            return "none";
        }

        return workspace.SelectedBy switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(
                nameof(workspace),
                workspace.SelectedBy,
                "The workspace selection method is not defined."),
        };
    }

    internal static string ReferenceKind(RouteInspectReferenceKind kind)
    {
        return kind switch
        {
            RouteInspectReferenceKind.Missing => "missing",
            RouteInspectReferenceKind.SourceId => "source ID",
            RouteInspectReferenceKind.SourcePath => "source path",
            RouteInspectReferenceKind.Invalid => "invalid",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The reference kind is not defined."),
        };
    }

    internal static string SelectionMethod(RouteInspectSelectionMethod method)
    {
        return method switch
        {
            RouteInspectSelectionMethod.Unresolved => "unresolved",
            RouteInspectSelectionMethod.AutomaticId => "automatic ID",
            RouteInspectSelectionMethod.ExactPath => "exact path",
            RouteInspectSelectionMethod.Interactive => "interactive selection",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "The selection method is not defined."),
        };
    }

    internal static string SourceState(RouteInspectIdentity identity)
    {
        return identity.RouteState switch
        {
            RouteInspectRouteState.Detached => "detached entrypoint",
            RouteInspectRouteState.NotRouted => "not routed",
            RouteInspectRouteState.Ambiguous => "ambiguous source",
            RouteInspectRouteState.Unresolved => "unresolved source",
            RouteInspectRouteState.Routed => RoutedSourceState(identity),
            _ => throw new ArgumentOutOfRangeException(
                nameof(identity),
                identity.RouteState,
                "The route state is not defined."),
        };
    }

    internal static string RouteState(RouteInspectRouteState state)
    {
        return state switch
        {
            RouteInspectRouteState.Routed => "routed",
            RouteInspectRouteState.Detached => "detached",
            RouteInspectRouteState.NotRouted => "not routed",
            RouteInspectRouteState.Ambiguous => "ambiguous",
            RouteInspectRouteState.Unresolved => "unresolved",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route state is not defined."),
        };
    }

    internal static string FactState(RouteInspectFactState state)
    {
        return state switch
        {
            RouteInspectFactState.Value => "value",
            RouteInspectFactState.Unavailable => "unavailable",
            RouteInspectFactState.NotApplicable => "not applicable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The fact state is not defined."),
        };
    }

    internal static string Completeness(RouteInspectCompleteness completeness)
    {
        return completeness switch
        {
            RouteInspectCompleteness.Complete => "complete",
            RouteInspectCompleteness.Incomplete => "incomplete",
            RouteInspectCompleteness.NotStarted => "not established",
            _ => throw new ArgumentOutOfRangeException(
                nameof(completeness),
                completeness,
                "The completeness is not defined."),
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

    internal static string LaterOccasion(RouteInspectLaterReadOccasion occasion)
    {
        return occasion switch
        {
            RouteInspectLaterReadOccasion.ContextRestoration => "after context restoration",
            RouteInspectLaterReadOccasion.Handoff => "before handoff",
            RouteInspectLaterReadOccasion.Closeout => "before handoff or closeout",
            RouteInspectLaterReadOccasion.FollowupTransition =>
                "after a change that may affect its follow-up work",
            _ => throw new ArgumentOutOfRangeException(
                nameof(occasion),
                occasion,
                "The later-read occasion is not defined."),
        };
    }

    internal static string LocalAxioms(RouteInspectAxiomsLocalState state)
    {
        return state switch
        {
            RouteInspectAxiomsLocalState.Substantive => "substantive local Axioms",
            RouteInspectAxiomsLocalState.InheritedSentinel => "inherits ancestor Axioms without local rules",
            RouteInspectAxiomsLocalState.Empty => "empty local Axioms",
            RouteInspectAxiomsLocalState.Missing => "no local Axioms section",
            RouteInspectAxiomsLocalState.NotApplicable => "not applicable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The local Axioms state is not defined."),
        };
    }

    private static string RoutedSourceState(RouteInspectIdentity identity)
    {
        return identity.Kind switch
        {
            RouteInspectSourceKind.Entrypoint => identity.Form == RouteInspectSourceForm.CompatibilityEntrypoint
                ? "routed compatibility entrypoint"
                : "routed entrypoint",
            RouteInspectSourceKind.Markdown => "routed Markdown source",
            RouteInspectSourceKind.Native => "routed native source",
            _ => throw new ArgumentOutOfRangeException(nameof(identity), identity.Kind, "The source kind is not defined."),
        };
    }

}
