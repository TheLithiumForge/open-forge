using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogue
{
    private readonly IReadOnlyDictionary<string, SourceCandidate> _candidatesByPath;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<SourceCandidate>> _candidatesById;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<SourceLogicalSource>> _sourcesById;
    private readonly IReadOnlyDictionary<string, SourceLogicalSource> _sourcesByPath;

    internal SourceCatalogue(
        CliWorkspace workspace,
        IEnumerable<SourceCandidate> candidates,
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<SourceCatalogueIssue> issues,
        bool isCancelled)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(issues);

        var orderedCandidates = candidates
            .Select(candidate => candidate ?? throw new ArgumentException("A source catalogue cannot contain a null candidate.", nameof(candidates)))
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedCandidates.Select(candidate => candidate.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedCandidates.Length)
        {
            throw new ArgumentException("A source catalogue requires unique candidate paths.", nameof(candidates));
        }

        var orderedSources = sources
            .Select(source => source ?? throw new ArgumentException("A source catalogue cannot contain a null source.", nameof(sources)))
            .OrderBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (orderedSources.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != orderedSources.Length)
        {
            throw new ArgumentException("A source catalogue requires unique logical source bases.", nameof(sources));
        }

        var orderedIssues = issues
            .Select(issue => issue ?? throw new ArgumentException("A source catalogue cannot contain a null issue.", nameof(issues)))
            .OrderBy(issue => issue.Stage)
            .ThenBy(issue => issue.Code)
            .ThenBy(issue => issue.AttemptedCanonicalPath, StringComparer.Ordinal)
            .ThenBy(issue => string.Join("\u001f", issue.RelatedPaths), StringComparer.Ordinal)
            .ToArray();

        Workspace = workspace;
        Candidates = new ReadOnlyCollection<SourceCandidate>(orderedCandidates);
        Sources = new ReadOnlyCollection<SourceLogicalSource>(orderedSources);
        Issues = new ReadOnlyCollection<SourceCatalogueIssue>(orderedIssues);
        IsCancelled = isCancelled;

        _candidatesByPath = new ReadOnlyDictionary<string, SourceCandidate>(
            orderedCandidates.ToDictionary(candidate => candidate.CanonicalPath, StringComparer.Ordinal));
        _candidatesById = new ReadOnlyDictionary<string, IReadOnlyList<SourceCandidate>>(
            orderedCandidates
                .Where(candidate => candidate.AutomaticId is not null)
                .GroupBy(
                    candidate => candidate.AutomaticId
                        ?? throw new InvalidOperationException("A candidate with an automatic ID must retain that ID."),
                    StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<SourceCandidate>)new ReadOnlyCollection<SourceCandidate>(group.ToArray()),
                    StringComparer.Ordinal));
        _sourcesById = new ReadOnlyDictionary<string, IReadOnlyList<SourceLogicalSource>>(
            orderedSources
                .GroupBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<SourceLogicalSource>)new ReadOnlyCollection<SourceLogicalSource>(group.ToArray()),
                    StringComparer.Ordinal));

        var sourcesByPath = new Dictionary<string, SourceLogicalSource>(StringComparer.Ordinal);
        foreach (var source in orderedSources)
        {
            AddSourcePath(sourcesByPath, source.Identity.CanonicalBasePath, source);
            if (source.Overwrite is not null)
            {
                AddSourcePath(sourcesByPath, source.Overwrite.CanonicalPath, source);
            }
        }

        _sourcesByPath = new ReadOnlyDictionary<string, SourceLogicalSource>(sourcesByPath);
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<SourceCandidate> Candidates { get; }

    internal IReadOnlyList<SourceLogicalSource> Sources { get; }

    internal IReadOnlyList<SourceCatalogueIssue> Issues { get; }

    internal bool IsCancelled { get; }

    internal SourceCandidate? FindCandidateByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _candidatesByPath.TryGetValue(canonicalPath, out var candidate)
            ? candidate
            : null;
    }

    internal IReadOnlyList<SourceCandidate> FindAllCandidatesById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _candidatesById.TryGetValue(id, out var candidates)
            ? candidates.Where(candidate => candidate.Form != SourceDocumentForm.OverwriteCompanion).ToArray()
            : Array.Empty<SourceCandidate>();
    }

    internal IReadOnlyList<SourceLogicalSource> FindAllById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _sourcesById.TryGetValue(id, out var sources)
            ? sources
            : Array.Empty<SourceLogicalSource>();
    }

    internal SourceLogicalSource? FindByPath(string canonicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        return _sourcesByPath.TryGetValue(canonicalPath, out var source)
            ? source
            : null;
    }

    internal SourceCatalogueSelection SelectAll()
    {
        return new SourceCatalogueSelection(
            Sources,
            Candidates,
            Issues.Where(issue => issue.Stage != SourceCatalogueIssueStage.Root),
            Issues.Where(issue => issue.Stage == SourceCatalogueIssueStage.Root));
    }

    internal SourceCatalogueSelection Select(SourceCatalogueSelectionRequest request)
    {
        var selectedSources = request.Sources
            .Select(source => RequireMember(source, nameof(request)))
            .ToArray();
        var selectedPaths = new HashSet<string>(
            selectedSources.Select(source => source.Identity.CanonicalBasePath),
            StringComparer.Ordinal);
        foreach (var source in selectedSources.Where(source => source.Overwrite is not null))
        {
            selectedPaths.Add(
                (source.Overwrite
                    ?? throw new InvalidOperationException("A selected source with an overwrite path must retain its overwrite layer.")).CanonicalPath);
        }

        var selectedCandidates = Candidates
            .Where(candidate => selectedPaths.Contains(candidate.CanonicalPath)
                || !_sourcesByPath.ContainsKey(candidate.CanonicalPath)
                    && IsWithinSelectedScope(candidate, request))
            .ToArray();
        var selectedCandidatePaths = selectedCandidates
            .Select(candidate => candidate.CanonicalPath)
            .ToHashSet(StringComparer.Ordinal);
        var selectedIssues = Issues
            .Where(issue => issue.Stage != SourceCatalogueIssueStage.Root)
            .Select(issue => ProjectIssue(issue, selectedCandidatePaths, selectedSources, request))
            .Where(issue => issue is not null)
            .Cast<SourceCatalogueIssue>()
            .ToArray();

        return new SourceCatalogueSelection(
            selectedSources,
            selectedCandidates,
            selectedIssues,
            Issues.Where(issue => issue.Stage == SourceCatalogueIssueStage.Root));
    }

    private SourceLogicalSource RequireMember(SourceLogicalSource source, string parameterName)
    {
        if (!_sourcesByPath.TryGetValue(source.Identity.CanonicalBasePath, out var member)
            || !ReferenceEquals(member, source))
        {
            throw new ArgumentException("Selection sources must be catalogue members.", parameterName);
        }

        return member;
    }

    private static bool IsWithinSelectedScope(
        SourceCandidate candidate,
        SourceCatalogueSelectionRequest request)
    {
        if (candidate.PhysicalParentPath is null)
        {
            return false;
        }

        var included = request.IncludedScopes.Any(scope =>
            PhysicalContainment.Contains(scope.PhysicalDirectoryPath, candidate.PhysicalParentPath));
        if (!included)
        {
            return false;
        }

        return !request.ExcludedScopes.Any(scope =>
            PhysicalContainment.Contains(scope.PhysicalDirectoryPath, candidate.PhysicalParentPath));
    }

    private SourceCatalogueIssue? ProjectIssue(
        SourceCatalogueIssue issue,
        IReadOnlySet<string> selectedCandidatePaths,
        IReadOnlyList<SourceLogicalSource> selectedSources,
        SourceCatalogueSelectionRequest request)
    {
        if (issue.Code == SourceCatalogueIssueCode.DirectoryUnavailable)
        {
            if (issue.ScopePhysicalPath is null
                || !request.IncludedScopes.Any(scope =>
                    PhysicalContainment.Contains(scope.PhysicalDirectoryPath, issue.ScopePhysicalPath))
                || request.ExcludedScopes.Any(scope =>
                    PhysicalContainment.Contains(scope.PhysicalDirectoryPath, issue.ScopePhysicalPath)))
            {
                return null;
            }

            return issue;
        }

        if (issue.Code is SourceCatalogueIssueCode.IdentityCollision or SourceCatalogueIssueCode.PhysicalAlias)
        {
            var selectedRelatedPaths = issue.RelatedPaths
                .Where(selectedCandidatePaths.Contains)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (issue.Code == SourceCatalogueIssueCode.PhysicalAlias
                && selectedRelatedPaths.Length < 2)
            {
                selectedRelatedPaths = ReadSelectedPhysicalAliases(issue, selectedCandidatePaths);
            }

            if (selectedRelatedPaths.Length < 2)
            {
                return null;
            }

            var attemptedPath = selectedCandidatePaths.Contains(issue.AttemptedCanonicalPath)
                ? issue.AttemptedCanonicalPath
                : selectedRelatedPaths[0];
            return new SourceCatalogueIssue(
                issue.Stage,
                issue.Code,
                attemptedPath,
                selectedRelatedPaths,
                issue.ScopePhysicalPath,
                issue.Failure);
        }

        return selectedCandidatePaths.Contains(issue.AttemptedCanonicalPath)
            ? issue
            : null;
    }

    private string[] ReadSelectedPhysicalAliases(
        SourceCatalogueIssue issue,
        IReadOnlySet<string> selectedCandidatePaths)
    {
        var attemptedCandidate = FindCandidateByPath(issue.AttemptedCanonicalPath);
        if (attemptedCandidate?.PhysicalState != PhysicalPathState.Contained
            || attemptedCandidate.PhysicalPath is null)
        {
            throw new InvalidOperationException("A physical-alias issue requires a contained attempted candidate.");
        }

        return Candidates
            .Where(candidate => candidate.PhysicalState == PhysicalPathState.Contained
                && candidate.PhysicalPath is not null
                && PhysicalIdentityTracker.PathComparer.Equals(
                    candidate.PhysicalPath,
                    attemptedCandidate.PhysicalPath)
                && selectedCandidatePaths.Contains(candidate.CanonicalPath))
            .Select(candidate => candidate.CanonicalPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddSourcePath(
        IDictionary<string, SourceLogicalSource> sourcesByPath,
        string path,
        SourceLogicalSource source)
    {
        if (!sourcesByPath.TryAdd(path, source))
        {
            throw new ArgumentException("Source base and overwrite paths must be unique.", nameof(source));
        }
    }
}
