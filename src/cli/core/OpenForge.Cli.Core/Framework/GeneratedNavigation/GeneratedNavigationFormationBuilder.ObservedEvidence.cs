using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation;

internal sealed partial class GeneratedNavigationFormationBuilder
{
    private static RetainedObservedEvidence ReadRetainedObservedEvidence(
        SourceCatalogue observedCatalogue,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        var exactObservedSources = intendedSources
            .Where(source => ReferenceEquals(
                observedCatalogue.FindByPath(source.Identity.CanonicalBasePath),
                source))
            .ToArray();
        var intendedLayerPaths = intendedSources
            .SelectMany(ReadLayerPaths)
            .ToHashSet(StringComparer.Ordinal);
        var removedCandidatePaths = observedCatalogue.Sources
            .SelectMany(ReadLayerPaths)
            .Where(path => !intendedLayerPaths.Contains(path))
            .ToHashSet(StringComparer.Ordinal);
        var candidates = observedCatalogue.Candidates
            .Where(candidate => !removedCandidatePaths.Contains(candidate.CanonicalPath))
            .ToArray();
        var retainedCandidatePaths = candidates
            .Select(candidate => candidate.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var issues = removedCandidatePaths.Count == 0
            ? observedCatalogue.Issues
            : new ReadOnlyCollection<SourceCatalogueIssue>(
                observedCatalogue.Issues
                    .Select(issue => ProjectIssue(issue, retainedCandidatePaths))
                    .Where(issue => issue is not null)
                    .Cast<SourceCatalogueIssue>()
                    .ToArray());
        return new RetainedObservedEvidence(
            candidates: removedCandidatePaths.Count == 0
                ? observedCatalogue.Candidates
                : new ReadOnlyCollection<SourceCandidate>(candidates),
            exactObservedSources: exactObservedSources,
            issues: issues);
    }

    private static IEnumerable<string> ReadLayerPaths(SourceLogicalSource source)
    {
        yield return source.Base.CanonicalPath;
        if (source.Overwrite is { } overwrite)
        {
            yield return overwrite.CanonicalPath;
        }
    }

    private static SourceCatalogueIssue? ProjectIssue(
        SourceCatalogueIssue issue,
        IReadOnlySet<string> retainedCandidatePaths)
    {
        if (issue.Stage is SourceCatalogueIssueStage.Root or SourceCatalogueIssueStage.Directory)
        {
            return issue;
        }

        var relatedPaths = issue.RelatedPaths
            .Where(retainedCandidatePaths.Contains)
            .ToArray();
        if (issue.Code is SourceCatalogueIssueCode.IdentityCollision or SourceCatalogueIssueCode.PhysicalAlias)
        {
            if (relatedPaths.Length < 2)
            {
                return null;
            }

            return new SourceCatalogueIssue(
                stage: issue.Stage,
                code: issue.Code,
                attemptedCanonicalPath: retainedCandidatePaths.Contains(issue.AttemptedCanonicalPath)
                    ? issue.AttemptedCanonicalPath
                    : relatedPaths[0],
                relatedPaths: relatedPaths,
                scopePhysicalPath: issue.ScopePhysicalPath,
                failure: issue.Failure);
        }

        if (!retainedCandidatePaths.Contains(issue.AttemptedCanonicalPath))
        {
            return null;
        }

        return relatedPaths.Length == issue.RelatedPaths.Count
            ? issue
            : new SourceCatalogueIssue(
                stage: issue.Stage,
                code: issue.Code,
                attemptedCanonicalPath: issue.AttemptedCanonicalPath,
                relatedPaths: relatedPaths,
                scopePhysicalPath: issue.ScopePhysicalPath,
                failure: issue.Failure);
    }

    private sealed class RetainedObservedEvidence
    {
        internal RetainedObservedEvidence(
            IReadOnlyList<SourceCandidate> candidates,
            IReadOnlyList<SourceLogicalSource> exactObservedSources,
            IReadOnlyList<SourceCatalogueIssue> issues)
        {
            Candidates = candidates;
            Issues = issues;
            var candidatesByPath = candidates.ToDictionary(
                candidate => candidate.CanonicalPath,
                StringComparer.Ordinal);
            var sourcesByCandidatePath = new Dictionary<string, SourceLogicalSource>(StringComparer.Ordinal);
            foreach (var source in exactObservedSources)
            {
                sourcesByCandidatePath.Add(source.Base.CanonicalPath, source);
                if (source.Overwrite is { } overwrite)
                {
                    sourcesByCandidatePath.Add(overwrite.CanonicalPath, source);
                }
            }

            SourcesByCandidatePath = new ReadOnlyDictionary<string, SourceLogicalSource>(sourcesByCandidatePath);
            MemberCandidatesByPath = new ReadOnlyDictionary<string, SourceCandidate>(
                sourcesByCandidatePath.Keys.ToDictionary(path => path, path => candidatesByPath[path], StringComparer.Ordinal));
        }

        internal IReadOnlyList<SourceCandidate> Candidates { get; }

        internal IReadOnlyDictionary<string, SourceCandidate> MemberCandidatesByPath { get; }

        internal IReadOnlyDictionary<string, SourceLogicalSource> SourcesByCandidatePath { get; }

        internal IReadOnlyList<SourceCatalogueIssue> Issues { get; }
    }
}
