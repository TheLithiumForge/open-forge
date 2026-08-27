using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectResolutionSupport
{
    internal static RouteInspectSelection UnresolvedSelection(SourceReferenceParseResult parsed)
    {
        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            return new RouteInspectSelection(
                RouteInspectReferenceKind.Invalid,
                RouteInspectSelectionMethod.Unresolved,
                parsed.AttemptedId ?? parsed.AttemptedPath,
                []);
        }

        return parsed.Kind == SourceReferenceKind.SourceId
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

    internal static IReadOnlyList<RouteInspectResolutionIssue> ReadCatalogueBoundaryIssues(
        SourceCatalogue catalogue)
    {
        var issues = new List<RouteInspectResolutionIssue>();
        foreach (var issue in catalogue.Issues)
        {
            var mapped = issue.Code switch
            {
                SourceCatalogueIssueCode.RootUnsafe => CreateIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    issue.AttemptedCanonicalPath,
                    "The .agents source boundary could not be proved or read."),
                SourceCatalogueIssueCode.RootUnavailable
                    or SourceCatalogueIssueCode.DirectoryUnavailable => CreateIssue(
                        RouteInspectResolutionIssueCode.ReadUnavailable,
                        issue.AttemptedCanonicalPath,
                        "The .agents source boundary could not be proved or read."),
                SourceCatalogueIssueCode.CandidateUnsafe
                    when string.Equals(
                        issue.AttemptedCanonicalPath,
                        SourceLogicalPath.LoaderPath,
                        StringComparison.Ordinal) => CreateIssue(
                        RouteInspectResolutionIssueCode.UnsafeSource,
                        issue.AttemptedCanonicalPath,
                        "The Loader source boundary could not be proved or read."),
                SourceCatalogueIssueCode.CandidateUnavailable
                    when string.Equals(
                        issue.AttemptedCanonicalPath,
                        SourceLogicalPath.LoaderPath,
                        StringComparison.Ordinal) => CreateIssue(
                        RouteInspectResolutionIssueCode.ReadUnavailable,
                        issue.AttemptedCanonicalPath,
                        "The Loader source boundary could not be proved or read."),
                _ => null,
            };
            if (mapped is not null)
            {
                issues.Add(mapped);
            }
        }

        return issues;
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
