using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectResolutionSupport
{
    internal static RouteInspectSelection UnresolvedSelection(RouteSourceReferenceParseResult parsed)
    {
        ArgumentNullException.ThrowIfNull(parsed);
        if (parsed.State == RouteSourceReferenceParseState.Invalid)
        {
            return new RouteInspectSelection(
                RouteInspectReferenceKind.Invalid,
                RouteInspectSelectionMethod.Unresolved,
                parsed.AttemptedId ?? parsed.AttemptedPath,
                []);
        }

        return parsed.Kind == RouteSourceReferenceKind.SourceId
            ? new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                RouteInspectSelectionMethod.Unresolved,
                parsed.AttemptedId,
                [])
            : new RouteInspectSelection(
                RouteInspectReferenceKind.SourcePath,
                RouteInspectSelectionMethod.Unresolved,
                parsed.AttemptedPath,
                []);
    }

    internal static RouteInspectSelection UnresolvedSelection(RouteInspectSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new RouteInspectSelection(
            selection.ReferenceKind,
            RouteInspectSelectionMethod.Unresolved,
            selection.RequestedReference,
            []);
    }

    internal static RouteInspectResolution CreateBoundaryResolution(
        RouteInspectSelection selection,
        IReadOnlyList<RouteInspectResolutionIssue> issues)
    {
        var state = issues.Any(issue => issue.Code == RouteInspectResolutionIssueCode.UnsafeSource)
            ? RouteInspectResolutionState.Blocked
            : RouteInspectResolutionState.Incomplete;
        return RouteInspectResolution.Create(state, selection, null, null, issues);
    }

    internal static RouteInspectResolution Interrupted(
        RouteInspectSelection selection,
        string subject)
    {
        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Interrupted,
            selection,
            null,
            null,
            [CreateIssue(
                RouteInspectResolutionIssueCode.Interrupted,
                subject,
                "Source resolution was interrupted.")]);
    }

    internal static RouteInspectResolutionState ReadResolutionState(
        IReadOnlyList<RouteInspectResolutionIssue> issues)
    {
        if (issues.Any(issue => issue.Code is
            RouteInspectResolutionIssueCode.AmbiguousRoute
            or RouteInspectResolutionIssueCode.AmbiguousOverwrite
            or RouteInspectResolutionIssueCode.OrphanOverwrite
            or RouteInspectResolutionIssueCode.UnsafeSource
            or RouteInspectResolutionIssueCode.AmbiguousSource))
        {
            return RouteInspectResolutionState.Blocked;
        }

        return issues.Count == 0
            ? RouteInspectResolutionState.Resolved
            : RouteInspectResolutionState.Incomplete;
    }

    internal static RouteInspectResolutionIssue CreateIssue(
        RouteInspectResolutionIssueCode code,
        string subject,
        string message,
        IEnumerable<string>? paths = null)
    {
        var orderedPaths = paths?
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        return new RouteInspectResolutionIssue(code, subject, message, orderedPaths);
    }
}
