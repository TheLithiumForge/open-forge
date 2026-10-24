using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private static RouteListSelectionIssue MapCatalogueRootIssue(WorkspaceLayout layout, SourceCatalogueIssue issue)
    {
        var cause = issue.Failure?.DirectCause
            ?? (issue.Code switch
            {
                SourceCatalogueIssueCode.RootMissing => "The Loader file is missing.",
                SourceCatalogueIssueCode.RootUnsafe => "The Loader physical boundary could not be proved.",
                SourceCatalogueIssueCode.RootUnavailable => "The Loader root could not be inspected.",
                _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The catalogue root issue code is not defined."),
            });
        return issue.Code == SourceCatalogueIssueCode.RootUnsafe
            ? new RouteListSelectionIssue(RouteListFindingCode.PhysicalBoundary, layout.LoaderPath, cause)
            : new RouteListSelectionIssue(RouteListFindingCode.LoaderUnavailable, layout.LoaderPath, cause);
    }

    private static RouteListSelectionIssue MapLoaderIssue(
        SourceRouteIssue issue,
        SourceCatalogue catalogue,
        RouteSourceProjectionSet projectionSet,
        IReadOnlyList<SourceLoaderDestinationParseResult> loaderDestinations)
    {
        var layout = catalogue.Layout;
        var subject = ReadLoaderIssueSubject(layout, issue, loaderDestinations);
        return issue.Code switch
        {
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDestinationMissing
                when IsMalformedLoaderRead(layout, issue, projectionSet) =>
                new RouteListSelectionIssue(RouteListFindingCode.LoaderMalformed, subject, issue.Cause),
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDestinationMissing
                when IsMalformedOverwriteRoot(issue, catalogue) =>
                new RouteListSelectionIssue(RouteListFindingCode.LoaderMalformed, subject, issue.Cause),
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDestinationMissing
                when IsUnsafeCandidate(issue.CanonicalPath, catalogue)
                    || IsUnsafeLoaderProjection(layout, issue, projectionSet) =>
                new RouteListSelectionIssue(RouteListFindingCode.PhysicalBoundary, subject, issue.Cause),
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDestinationMissing
                or SourceRouteIssueCode.RouteSupportUnavailable =>
                new RouteListSelectionIssue(RouteListFindingCode.LoaderUnavailable, subject, issue.Cause),
            SourceRouteIssueCode.LoaderMalformed or SourceRouteIssueCode.LoaderDuplicateRoot =>
                new RouteListSelectionIssue(RouteListFindingCode.LoaderMalformed, subject, issue.Cause),
            SourceRouteIssueCode.LoaderUnsafe =>
                new RouteListSelectionIssue(RouteListFindingCode.PhysicalBoundary, subject, issue.Cause),
            SourceRouteIssueCode.RouteAmbiguous =>
                new RouteListSelectionIssue(RouteListFindingCode.RouteAmbiguous, subject, issue.Cause),
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The source route issue code is not defined."),
        };
    }

    private static IReadOnlyList<SourceLoaderDestinationParseResult> ReadLoaderDestinations(
        WorkspaceLayout layout,
        RouteSourceProjectionSet projectionSet)
    {
        var loader = projectionSet.Projections.SingleOrDefault(projection =>
            projection.LogicalSource.Base.Form == SourceDocumentForm.Loader);
        var read = loader?.BaseRead.Read;
        if (read is null || read.State != FileReadState.Complete)
        {
            return [];
        }

        var body = read.Value
            ?? throw new InvalidOperationException("A complete Loader read requires its body.");
        return SourceLoaderEntriesParser.Parse(layout, body).Destinations;
    }

    private static string ReadLoaderIssueSubject(
        WorkspaceLayout layout,
        SourceRouteIssue issue,
        IReadOnlyList<SourceLoaderDestinationParseResult> destinations)
    {
        if (issue.Code == SourceRouteIssueCode.LoaderUnsafe)
        {
            return destinations.FirstOrDefault(destination =>
                    destination.State == SourceLoaderDestinationParseState.Unsafe
                    && string.Equals(destination.Cause, issue.Cause, StringComparison.Ordinal))
                ?.AttemptedDestination
                ?? layout.LoaderPath;
        }

        var relatedPath = issue.RelatedPaths.FirstOrDefault();
        var canonicalPath = issue.CanonicalPath;
        if (issue.Code != SourceRouteIssueCode.LoaderDestinationMissing
            && relatedPath is not null && layout.IsContentPath(relatedPath))
        {
            canonicalPath = relatedPath;
        }
        if (issue.Code == SourceRouteIssueCode.LoaderMalformed
            && relatedPath is not null
            && !layout.IsContentPath(relatedPath))
        {
            return relatedPath;
        }

        if (issue.Code == SourceRouteIssueCode.LoaderDuplicateRoot)
        {
            return destinations.LastOrDefault(destination =>
                    string.Equals(destination.CanonicalPath, canonicalPath, StringComparison.Ordinal))
                ?.AttemptedDestination
                ?? ReadLoaderDestination(layout, canonicalPath);
        }

        return ReadLoaderAttempt(layout, canonicalPath, destinations);
    }

    private static string ReadLoaderAttempt(
        WorkspaceLayout layout,
        string canonicalPath,
        IReadOnlyList<SourceLoaderDestinationParseResult> destinations)
    {
        return destinations.FirstOrDefault(destination =>
                string.Equals(destination.CanonicalPath, canonicalPath, StringComparison.Ordinal))
            ?.AttemptedDestination
            ?? ReadLoaderDestination(layout, canonicalPath);
    }

    private static bool IsMalformedLoaderRead(
        WorkspaceLayout layout,
        SourceRouteIssue issue,
        RouteSourceProjectionSet projectionSet)
    {
        if (!string.Equals(issue.CanonicalPath, layout.LoaderPath, StringComparison.Ordinal))
        {
            return false;
        }

        var loader = projectionSet.FindByPath(layout.LoaderPath);
        return loader?.Base.ReadState == FileReadState.InvalidEncoding;
    }

    private static bool IsMalformedOverwriteRoot(SourceRouteIssue issue, SourceCatalogue catalogue)
    {
        return catalogue.FindCandidateByPath(issue.CanonicalPath)?.Form == SourceDocumentForm.OverwriteCompanion;
    }

    private static bool IsUnsafeLoaderProjection(
        WorkspaceLayout layout,
        SourceRouteIssue issue,
        RouteSourceProjectionSet projectionSet)
    {
        if (!string.Equals(issue.CanonicalPath, layout.LoaderPath, StringComparison.Ordinal))
        {
            return false;
        }

        var loader = projectionSet.Projections.SingleOrDefault(projection =>
            projection.LogicalSource.Base.Form == SourceDocumentForm.Loader);
        return loader?.BaseRead.Verification.State is SourceLayerVerificationState.Unsafe
            or SourceLayerVerificationState.Unavailable
            or SourceLayerVerificationState.Changed;
    }

    private static bool IsUnsafeCandidate(string canonicalPath, SourceCatalogue catalogue)
    {
        var candidate = catalogue.FindCandidateByPath(canonicalPath);
        if (candidate is not null && candidate.PhysicalState != PhysicalPathState.Contained)
        {
            return true;
        }

        return catalogue.Issues.Any(issue =>
            issue.Code == SourceCatalogueIssueCode.CandidateUnsafe
            && (string.Equals(issue.AttemptedCanonicalPath, canonicalPath, StringComparison.Ordinal)
                || canonicalPath.StartsWith(issue.AttemptedCanonicalPath + "/", StringComparison.Ordinal)));
    }

    private static bool IsDeferredEntriesIssue(WorkspaceLayout layout, SourceRouteIssue issue)
    {
        return issue.Code == SourceRouteIssueCode.LoaderMalformed
            && string.Equals(issue.CanonicalPath, layout.LoaderPath, StringComparison.Ordinal)
            && (issue.RelatedPaths.Count == 0
                || !layout.IsContentPath(issue.RelatedPaths[0]));
    }

    private static string ReadLoaderDestination(WorkspaceLayout layout, string path)
    {
        if (!layout.IsContentPath(path) || string.Equals(path, layout.LoaderPath, StringComparison.Ordinal))
        {
            return path;
        }

        return Path.GetRelativePath(
            SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, layout.MainRoot),
            SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, path)).Replace('\\', '/');
    }
}
