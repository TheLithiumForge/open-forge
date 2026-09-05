using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal sealed class SourceRouteFactsResolver
{
    internal async ValueTask<SourceRouteFacts> ResolveAsync(
        SourceRouteFactsRequest request,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        ValidateBoundary(request, reader);

        var selection = request.Selection;
        var catalogue = request.Catalogue;
        var selectedCandidates = selection.Candidates
            .Select(candidate => candidate.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var selectedSources = selection.Sources
            .Where(source => source.Base.Form != SourceDocumentForm.OverwriteCompanion)
            .ToArray();
        var routeIssues = new List<SourceRouteIssue>();
        var nextOccurrence = 0;
        var loaderRootPaths = new List<string>();
        var areLoaderRootFactsComplete = true;
        var isCancelled = cancellationToken.IsCancellationRequested;

        if (!isCancelled)
        {
            var loaderCandidates = catalogue.Candidates
                .Where(candidate => candidate.Form == SourceDocumentForm.Loader)
                .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
                .ToArray();
            foreach (var loaderCandidate in loaderCandidates)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    isCancelled = true;
                    break;
                }

                if (!selectedCandidates.Contains(loaderCandidate.CanonicalPath))
                {
                    AddIssue(
                        routeIssues,
                        ref nextOccurrence,
                        SourceRouteIssueCode.LoaderUnavailable,
                        loaderCandidate.CanonicalPath,
                        [],
                        "The selected source allowlist does not contain the Loader layer.");
                    areLoaderRootFactsComplete = false;
                    continue;
                }

                var loader = catalogue.FindByPath(loaderCandidate.CanonicalPath);
                if (loader is null
                    || !selectedSources.Contains(loader, ReferenceEqualityComparer.Instance))
                {
                    AddIssue(
                        routeIssues,
                        ref nextOccurrence,
                        SourceRouteIssueCode.LoaderUnavailable,
                        loaderCandidate.CanonicalPath,
                        [],
                        "The selected Loader source is unavailable in the source allowlist.");
                    areLoaderRootFactsComplete = false;
                    continue;
                }

                var read = await reader
                    .ReadAsync(loader.Base, cancellationToken)
                    .ConfigureAwait(false);
                if (read.Verification.State == SourceLayerVerificationState.Cancelled
                    || read.Read?.State == FileReadState.Cancelled)
                {
                    isCancelled = true;
                    break;
                }

                if (read.Verification.State != SourceLayerVerificationState.Verified
                    || read.Read?.State != FileReadState.Complete)
                {
                    AddIssue(
                        routeIssues,
                        ref nextOccurrence,
                        SourceRouteIssueCode.LoaderUnavailable,
                        loader.Identity.CanonicalBasePath,
                        [],
                        ReadLoaderUnavailableCause(read));
                    areLoaderRootFactsComplete = false;
                    continue;
                }

                var loaderContents = read.Read?.Value
                    ?? throw new InvalidOperationException("A complete Loader read requires a value.");
                var parsed = SourceLoaderEntriesParser.Parse(loaderContents);
                if (parsed.State == SourceLoaderEntriesParseState.Malformed)
                {
                    var parseCause = parsed.Cause
                        ?? throw new InvalidOperationException(
                            "A malformed Loader Entries result requires a cause.");
                    AddIssue(
                        routeIssues,
                        ref nextOccurrence,
                        SourceRouteIssueCode.LoaderMalformed,
                        loader.Identity.CanonicalBasePath,
                        parsed.AttemptedDestination is null ? [] : [parsed.AttemptedDestination],
                        parseCause);
                    areLoaderRootFactsComplete = false;
                }

                foreach (var destination in parsed.Destinations)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        isCancelled = true;
                        break;
                    }

                    if (destination.State == SourceLoaderDestinationParseState.Unsafe)
                    {
                        var cause = destination.Cause
                            ?? throw new InvalidOperationException(
                                "An unsafe Loader destination requires a cause.");
                        AddIssue(
                            routeIssues,
                            ref nextOccurrence,
                            SourceRouteIssueCode.LoaderUnsafe,
                            loader.Identity.CanonicalBasePath,
                            [],
                            cause);
                        areLoaderRootFactsComplete = false;
                        continue;
                    }

                    var destinationPath = destination.CanonicalPath
                        ?? throw new InvalidOperationException(
                            "A valid Loader destination requires a canonical path.");

                    var destinationSource = catalogue.FindByPath(destinationPath);
                    if (destinationSource is null)
                    {
                        AddIssue(
                            routeIssues,
                            ref nextOccurrence,
                            SourceRouteIssueCode.LoaderUnavailable,
                            destinationPath,
                            [],
                            "The Loader destination does not identify a retained source.");
                        areLoaderRootFactsComplete = false;
                        continue;
                    }

                    if (!string.Equals(
                            destinationSource.Identity.CanonicalBasePath,
                            destinationPath,
                            StringComparison.Ordinal)
                        || !SourceFormClassifier.IsEntrypoint(destinationSource.Base.Form))
                    {
                        AddIssue(
                            routeIssues,
                            ref nextOccurrence,
                            SourceRouteIssueCode.LoaderMalformed,
                            loader.Identity.CanonicalBasePath,
                            [destinationPath],
                            "A Loader destination must identify a selected entrypoint base source.");
                        areLoaderRootFactsComplete = false;
                        continue;
                    }

                    if (!selectedSources.Contains(destinationSource, ReferenceEqualityComparer.Instance))
                    {
                        AddIssue(
                            routeIssues,
                            ref nextOccurrence,
                            SourceRouteIssueCode.RouteSupportUnavailable,
                            destinationPath,
                            [],
                            "The Loader destination is outside the selected source allowlist.");
                        areLoaderRootFactsComplete = false;
                        continue;
                    }

                    if (loaderRootPaths.Contains(destinationPath, StringComparer.Ordinal))
                    {
                        AddIssue(
                            routeIssues,
                            ref nextOccurrence,
                            SourceRouteIssueCode.LoaderMalformed,
                            loader.Identity.CanonicalBasePath,
                            [destinationPath],
                            "The Loader declares the same canonical root more than once.");
                        areLoaderRootFactsComplete = false;
                        continue;
                    }

                    loaderRootPaths.Add(destinationPath);
                }

                if (isCancelled)
                {
                    break;
                }
            }
        }

        var topologySources = selectedSources
            .Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .ToArray();
        var topology = new SourceRouteTopologyBuilder().Build(topologySources, loaderRootPaths);
        AddExcludedSupportIssues(
            catalogue,
            selectedSources,
            topology,
            routeIssues,
            ref nextOccurrence,
            ref areLoaderRootFactsComplete);

        var routeFacts = new List<SourceRouteFact>();
        foreach (var source in topologySources)
        {
            var identityUnique = selectedSources.Count(candidate =>
                string.Equals(
                    candidate.Identity.AutomaticId,
                    source.Identity.AutomaticId,
                    StringComparison.Ordinal)) == 1;
            var state = SourceRouteStateResolver.Read(
                source,
                topology,
                areLoaderRootFactsComplete);
            routeFacts.Add(new SourceRouteFact(source.Identity, state, identityUnique));
            if (state == SourceRouteState.Ambiguous)
            {
                if (topology.FindByPath(source.Identity.CanonicalBasePath) is not { } node)
                {
                    throw new InvalidOperationException(
                        "An ambiguous source route must identify a topology node.");
                }

                AddIssue(
                    routeIssues,
                    ref nextOccurrence,
                    SourceRouteIssueCode.RouteAmbiguous,
                    source.Identity.CanonicalBasePath,
                    node.ParentPaths,
                    "The selected source has ambiguous authored route meaning.");
            }
        }

        foreach (var source in selectedSources.Where(source =>
                     source.Base.Form != SourceDocumentForm.Loader
                     && routeFacts.All(fact => !ReferenceEquals(fact.Identity, source.Identity))))
        {
            var identityUnique = selectedSources.Count(candidate =>
                string.Equals(
                    candidate.Identity.AutomaticId,
                    source.Identity.AutomaticId,
                    StringComparison.Ordinal)) == 1;
            routeFacts.Add(new SourceRouteFact(
                source.Identity,
                areLoaderRootFactsComplete ? SourceRouteState.Unrouted : SourceRouteState.Unavailable,
                identityUnique));
        }

        return new SourceRouteFacts(
            topology,
            routeFacts,
            routeIssues,
            areLoaderRootFactsComplete,
            isCancelled);
    }

    private static void ValidateBoundary(
        SourceRouteFactsRequest request,
        SourceDocumentReader reader)
    {
        if (!string.Equals(
                request.Catalogue.Workspace.LexicalRoot,
                reader.Workspace.LexicalRoot,
                StringComparison.Ordinal)
            || !PhysicalIdentityTracker.PathComparer.Equals(
                request.Catalogue.Workspace.PhysicalRoot,
                reader.Workspace.PhysicalRoot))
        {
            throw new ArgumentException(
                "The source catalogue, selection, and reader must share one workspace.",
                nameof(reader));
        }

        foreach (var source in request.Selection.Sources)
        {
            if (!ReferenceEquals(
                    request.Catalogue.FindByPath(source.Identity.CanonicalBasePath),
                    source))
            {
                throw new ArgumentException(
                    "The route selection must contain catalogue source members.",
                    nameof(request));
            }
        }

        foreach (var candidate in request.Selection.Candidates)
        {
            if (!ReferenceEquals(
                    request.Catalogue.FindCandidateByPath(candidate.CanonicalPath),
                    candidate))
            {
                throw new ArgumentException(
                    "The route selection must contain catalogue candidate members.",
                    nameof(request));
            }
        }
    }

    private static void AddExcludedSupportIssues(
        SourceCatalogue catalogue,
        IReadOnlyList<SourceLogicalSource> selectedSources,
        SourceRouteTopology topology,
        ICollection<SourceRouteIssue> issues,
        ref int nextOccurrence,
        ref bool areLoaderRootFactsComplete)
    {
        foreach (var source in selectedSources.Where(source =>
                     source.Base.Form != SourceDocumentForm.Loader))
        {
            var node = topology.FindByPath(source.Identity.CanonicalBasePath);
            if (node is not null && node.ParentState != SourceRouteParentState.None)
            {
                continue;
            }

            var expectedDirectory = ReadExpectedParentDirectory(source);
            if (expectedDirectory is null)
            {
                continue;
            }

            var outside = catalogue.Sources
                .Where(candidate =>
                    SourceFormClassifier.IsEntrypoint(candidate.Base.Form)
                    && string.Equals(
                        SourceLogicalPath.ReadParent(candidate.Identity.CanonicalBasePath),
                        expectedDirectory,
                        StringComparison.Ordinal)
                    && !selectedSources.Contains(candidate, ReferenceEqualityComparer.Instance))
                .OrderBy(candidate => candidate.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray();
            foreach (var candidate in outside)
            {
                AddIssue(
                    issues,
                    ref nextOccurrence,
                    SourceRouteIssueCode.RouteSupportUnavailable,
                    candidate.Identity.CanonicalBasePath,
                    [source.Identity.CanonicalBasePath],
                    "A required route-supporting entrypoint is outside the selected source allowlist.");
                areLoaderRootFactsComplete = false;
            }
        }
    }

    private static string? ReadExpectedParentDirectory(SourceLogicalSource source)
    {
        var containingDirectory = SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath);
        if (source.Base.Form == SourceDocumentForm.Markdown)
        {
            return containingDirectory;
        }

        return containingDirectory == SourceLogicalPath.AgentsRoot
            ? null
            : SourceLogicalPath.ReadParent(containingDirectory);
    }

    private static string ReadLoaderUnavailableCause(SourceDocumentReadResult read)
    {
        if (read.Verification.Failure is { } verificationFailure)
        {
            return verificationFailure.DirectCause;
        }

        if (read.Read?.Failure is { } readFailure)
        {
            return readFailure.DirectCause;
        }

        return "The selected Loader body could not be read completely.";
    }

    private static void AddIssue(
        ICollection<SourceRouteIssue> issues,
        ref int occurrence,
        SourceRouteIssueCode code,
        string canonicalPath,
        IEnumerable<string> relatedPaths,
        string cause)
    {
        issues.Add(new SourceRouteIssue(code, canonicalPath, relatedPaths, occurrence, cause));
        occurrence = checked(occurrence + 1);
    }
}
