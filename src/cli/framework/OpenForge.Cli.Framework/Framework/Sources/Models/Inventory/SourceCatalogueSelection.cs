using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogueSelection
{
    internal SourceCatalogueSelection(
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<SourceCandidate> candidates,
        IEnumerable<SourceCatalogueIssue> issues,
        IEnumerable<SourceCatalogueIssue> rootIssues)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentNullException.ThrowIfNull(rootIssues);

        Sources = new ReadOnlyCollection<SourceLogicalSource>(
            sources
                .Select(source => source ?? throw new ArgumentException("A selection cannot contain a null source.", nameof(sources)))
                .OrderBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
                .ThenBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray());
        Candidates = new ReadOnlyCollection<SourceCandidate>(
            candidates
                .Select(candidate => candidate ?? throw new ArgumentException("A selection cannot contain a null candidate.", nameof(candidates)))
                .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
                .ToArray());
        Issues = MaterializeIssues(issues, expectRoot: false, nameof(issues));
        RootIssues = MaterializeIssues(rootIssues, expectRoot: true, nameof(rootIssues));
    }

    internal IReadOnlyList<SourceLogicalSource> Sources { get; }

    internal IReadOnlyList<SourceCandidate> Candidates { get; }

    internal IReadOnlyList<SourceCatalogueIssue> Issues { get; }

    internal IReadOnlyList<SourceCatalogueIssue> RootIssues { get; }

    private static IReadOnlyList<SourceCatalogueIssue> MaterializeIssues(
        IEnumerable<SourceCatalogueIssue> issues,
        bool expectRoot,
        string parameterName)
    {
        var materialized = issues
            .Select(issue => issue ?? throw new ArgumentException("A selection cannot contain a null issue.", parameterName))
            .ToArray();
        if (materialized.Any(issue => (issue.Stage == SourceCatalogueIssueStage.Root) != expectRoot))
        {
            throw new ArgumentException("Selection issue stages do not match their issue collection.", parameterName);
        }

        return new ReadOnlyCollection<SourceCatalogueIssue>(
            materialized
                .OrderBy(issue => issue.Stage)
                .ThenBy(issue => issue.Code)
                .ThenBy(issue => issue.AttemptedCanonicalPath, StringComparer.Ordinal)
                .ThenBy(issue => string.Join("\u001f", issue.RelatedPaths), StringComparer.Ordinal)
                .ToArray());
    }
}
