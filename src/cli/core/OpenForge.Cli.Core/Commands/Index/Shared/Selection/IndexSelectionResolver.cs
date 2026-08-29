using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Selection;

internal sealed class IndexSelectionResolver
{
    private readonly SourceReferenceResolver _referenceResolver;

    internal IndexSelectionResolver(SourceReferenceResolver referenceResolver)
    {
        ArgumentNullException.ThrowIfNull(referenceResolver);
        _referenceResolver = referenceResolver;
    }

    internal IndexSelectionResolution Resolve(
        IndexRequest request,
        GeneratedNavigationFormation formation)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(formation);
        var rootFindings = ReadRootFindings(request, formation);
        if (rootFindings.Count != 0)
        {
            return new IndexSelectionResolution(
                IndexSelection.NotEstablished(request.HasExplicitSources
                    ? IndexSelectionOrigin.ExplicitSources
                    : IndexSelectionOrigin.AutomaticLoader),
                [],
                rootFindings);
        }

        return request.HasExplicitSources
            ? ResolveExplicit(request, formation)
            : ResolveAutomatic(formation);
    }

    private IndexSelectionResolution ResolveAutomatic(GeneratedNavigationFormation formation)
    {
        if (formation.Loader is not { } loader)
        {
            return new IndexSelectionResolution(
                IndexSelection.NotEstablished(IndexSelectionOrigin.AutomaticLoader),
                [],
                [Finding(IndexFindingCode.TargetUnexposed, "The selected workspace does not contain the required Loader source.")]);
        }

        var findings = new List<IndexFinding>();
        var normalizedSelection = NormalizeAliases(
            formation,
            [loader],
            IndexFindingCode.SourceUnsafe,
            findings);
        var targets = new List<SourceLogicalSource> { loader };
        foreach (var rootPath in formation.Topology.LoaderRootPaths)
        {
            AddEntrypointClosure(formation, rootPath, targets);
        }

        AddRelevantAmbiguities(formation, normalizedSelection, targets, automatic: true, findings);
        var normalizedTargets = NormalizeAliases(
            formation,
            targets,
            IndexFindingCode.TargetUnsafe,
            findings);
        return new IndexSelectionResolution(
            FormSelection(IndexSelectionOrigin.AutomaticLoader, normalizedSelection, formation),
            findings.Count == 0 ? normalizedTargets : [],
            findings);
    }

    private IndexSelectionResolution ResolveExplicit(
        IndexRequest request,
        GeneratedNavigationFormation formation)
    {
        var resolved = new List<SourceLogicalSource>();
        var findings = new List<IndexFinding>();
        for (var index = 0; index < request.SourceReferences.Count; index++)
        {
            var resolution = _referenceResolver.Resolve(request.SourceReferences[index], formation.Catalogue);
            if (resolution.State == SourceReferenceResolutionState.Resolved)
            {
                resolved.Add(resolution.Source
                    ?? throw new InvalidOperationException("A resolved Index source requires its source fact."));
                continue;
            }

            findings.Add(ResolutionFinding(resolution, index + 1, formation));
        }

        var normalizedSelection = NormalizeAliases(
            formation,
            resolved,
            IndexFindingCode.SourceUnsafe,
            findings);
        var targets = new List<SourceLogicalSource>();
        foreach (var source in normalizedSelection)
        {
            AddExplicitClosure(formation, source, targets, findings);
        }

        AddRelevantAmbiguities(formation, normalizedSelection, targets, automatic: false, findings);
        var normalizedTargets = NormalizeAliases(
            formation,
            targets,
            IndexFindingCode.TargetUnsafe,
            findings);
        return new IndexSelectionResolution(
            FormSelection(IndexSelectionOrigin.ExplicitSources, normalizedSelection, formation),
            findings.Count == 0 ? normalizedTargets : [],
            findings);
    }

    private static void AddExplicitClosure(
        GeneratedNavigationFormation formation,
        SourceLogicalSource source,
        ICollection<SourceLogicalSource> targets,
        ICollection<IndexFinding> findings)
    {
        if (source.Base.Form == SourceDocumentForm.Loader)
        {
            targets.Add(source);
            foreach (var rootPath in formation.Topology.LoaderRootPaths)
            {
                AddEntrypointClosure(formation, rootPath, targets);
            }

            return;
        }

        var node = formation.Topology.FindByPath(source.Identity.CanonicalBasePath);
        if (SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            AddEntrypointClosure(formation, source.Identity.CanonicalBasePath, targets);
            AddDirectParent(formation, source, node, targets, findings);
            return;
        }

        if (node?.ParentState == SourceRouteParentState.Resolved)
        {
            AddTarget(formation, node.ParentPaths[0], targets, findings);
            return;
        }

        var logicalSource = IndexLogicalSourceProjector.Project(
            source: source,
            formation: formation);
        if (node?.ParentState == SourceRouteParentState.Ambiguous)
        {
            findings.Add(new IndexFinding(
                IndexFindingCode.TopologyAmbiguous,
                sourceOccurrence: null,
                source: logicalSource,
                cause: "The selected source has more than one direct exposing parent.",
                candidates: node.ParentPaths
                    .Select(formation.FindSource)
                    .Where(candidate => candidate is not null)
                    .Select(candidate => IndexLogicalSourceProjector.Project(
                        source: candidate ?? throw new InvalidOperationException(
                            "A topology parent path must retain its source."),
                        formation: formation))));
            return;
        }

        findings.Add(new IndexFinding(
            IndexFindingCode.TargetUnexposed,
            sourceOccurrence: null,
            source: logicalSource,
            cause: "The selected source has no mechanically established exposing entrypoint.",
            candidates: []));
    }

    private static void AddDirectParent(
        GeneratedNavigationFormation formation,
        SourceLogicalSource source,
        SourceRouteNode? node,
        ICollection<SourceLogicalSource> targets,
        ICollection<IndexFinding> findings)
    {
        if (formation.Loader is { } loader
            && formation.Topology.LoaderRootPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal))
        {
            targets.Add(loader);
            return;
        }

        if (node?.ParentState == SourceRouteParentState.Resolved)
        {
            AddTarget(formation, node.ParentPaths[0], targets, findings);
            return;
        }

        if (node?.ParentState != SourceRouteParentState.Ambiguous)
        {
            return;
        }

        findings.Add(new IndexFinding(
            IndexFindingCode.TopologyAmbiguous,
            sourceOccurrence: null,
            source: IndexLogicalSourceProjector.Project(
                source: source,
                formation: formation),
            cause: "The selected entrypoint has more than one direct exposing parent.",
            candidates: node.ParentPaths
                .Select(formation.FindSource)
                .Where(candidate => candidate is not null)
                .Select(candidate => IndexLogicalSourceProjector.Project(
                    source: candidate ?? throw new InvalidOperationException(
                        "A topology parent path must retain its source."),
                    formation: formation))));
    }

    private static void AddEntrypointClosure(
        GeneratedNavigationFormation formation,
        string rootPath,
        ICollection<SourceLogicalSource> targets)
    {
        var pending = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        pending.Enqueue(rootPath);
        while (pending.TryDequeue(out var path))
        {
            if (!visited.Add(path) || formation.Topology.FindByPath(path) is not { } node)
            {
                continue;
            }

            var source = formation.FindSource(path)
                ?? throw new InvalidOperationException("A topology node must retain its formation source.");
            if (SourceFormClassifier.IsEntrypoint(source.Base.Form))
            {
                targets.Add(source);
            }

            foreach (var childPath in node.ChildPaths)
            {
                pending.Enqueue(childPath);
            }
        }
    }

    private static void AddTarget(
        GeneratedNavigationFormation formation,
        string path,
        ICollection<SourceLogicalSource> targets,
        ICollection<IndexFinding> findings)
    {
        var source = formation.FindSource(path)
            ?? throw new InvalidOperationException("A topology target must retain its formation source.");
        if (source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form))
        {
            targets.Add(source);
            return;
        }

        findings.Add(new IndexFinding(
            IndexFindingCode.TopologyAmbiguous,
            sourceOccurrence: null,
            source: IndexLogicalSourceProjector.Project(
                source: source,
                formation: formation),
            cause: "The exposing topology target is not a Loader or recognized entrypoint.",
            candidates: []));
    }

    private static void AddRelevantAmbiguities(
        GeneratedNavigationFormation formation,
        IReadOnlyCollection<SourceLogicalSource> selection,
        IReadOnlyCollection<SourceLogicalSource> targets,
        bool automatic,
        ICollection<IndexFinding> findings)
    {
        var relevant = selection
            .Concat(targets)
            .Select(source => source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var ambiguity in formation.Ambiguities.Where(ambiguity =>
                     ambiguity.Kind != GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
                     && (automatic && ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint
                         || relevant.Contains(ambiguity.Subject)
                         || ambiguity.Candidates.Any(candidate => relevant.Contains(candidate.CanonicalPath)))))
        {
            findings.Add(new IndexFinding(
                IndexFindingCode.TopologyAmbiguous,
                sourceOccurrence: null,
                source: formation.FindSource(ambiguity.Subject) is { } source
                    ? IndexLogicalSourceProjector.Project(
                        source: source,
                        formation: formation)
                    : null,
                cause: ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint
                    ? "More than one recognized entrypoint represents the same Loader root."
                    : "The selected route topology is not unique.",
                candidates: ambiguity.Candidates
                    .Select(candidate => formation.FindSource(candidate.CanonicalPath))
                    .Where(candidate => candidate is not null)
                    .Select(candidate => IndexLogicalSourceProjector.Project(
                        source: candidate ?? throw new InvalidOperationException(
                            "An ambiguity candidate must retain its source."),
                        formation: formation))));
        }
    }

    private static IReadOnlyList<SourceLogicalSource> NormalizeAliases(
        GeneratedNavigationFormation formation,
        IEnumerable<SourceLogicalSource> sources,
        IndexFindingCode incompatibleCode,
        ICollection<IndexFinding> findings)
    {
        var normalized = new List<SourceLogicalSource>();
        foreach (var source in sources
                     .DistinctBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                     .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            var group = formation.PhysicalAliasGroups.FirstOrDefault(group => group.Candidates.Any(candidate =>
                string.Equals(candidate.CanonicalPath, source.Identity.CanonicalBasePath, StringComparison.Ordinal)));
            if (group is null)
            {
                normalized.Add(source);
                continue;
            }

            if (group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible)
            {
                findings.Add(new IndexFinding(
                    incompatibleCode,
                    sourceOccurrence: null,
                    source: IndexLogicalSourceProjector.Project(
                        source: source,
                        formation: formation),
                    cause: "The selected source has an incompatible current-host physical alias.",
                    candidates: group.Candidates
                        .Select(candidate => formation.FindSource(candidate.CanonicalPath))
                        .Where(candidate => candidate is not null)
                        .Select(candidate => IndexLogicalSourceProjector.Project(
                            source: candidate ?? throw new InvalidOperationException(
                                "An alias candidate must retain its source."),
                            formation: formation))));
                continue;
            }

            var representative = group.Candidates
                .Select(candidate => formation.FindSource(candidate.CanonicalPath))
                .FirstOrDefault(candidate => candidate is not null)
                ?? throw new InvalidOperationException("A compatible alias group must retain its representative source.");
            normalized.Add(representative);
        }

        return normalized
            .DistinctBy(source => source.Base.PhysicalPath, PhysicalIdentityTracker.PathComparer)
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<IndexFinding> ReadRootFindings(
        IndexRequest request,
        GeneratedNavigationFormation formation)
    {
        var findings = new List<IndexFinding>();
        foreach (var issue in formation.Issues.Where(issue => issue.Stage == SourceCatalogueIssueStage.Root))
        {
            var finding = issue.Code switch
            {
                SourceCatalogueIssueCode.RootMissing when !request.HasExplicitSources => Finding(
                    IndexFindingCode.TargetUnexposed,
                    "The .agents source root is missing, so the required Loader source cannot be established."),
                SourceCatalogueIssueCode.RootMissing => null,
                SourceCatalogueIssueCode.RootUnsafe => Finding(
                    IndexFindingCode.WorkspaceUnsafe,
                    "The .agents source root does not have a safe workspace boundary."),
                SourceCatalogueIssueCode.RootUnavailable => Finding(
                    IndexFindingCode.DiscoveryIncomplete,
                    "The .agents source root could not be completely inspected."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(formation),
                    issue.Code,
                    "The root catalogue issue code is not defined."),
            };
            if (finding is not null)
            {
                findings.Add(finding);
            }
        }

        return findings;
    }

    private static IndexFinding ResolutionFinding(
        SourceReferenceResolution resolution,
        int occurrence,
        GeneratedNavigationFormation formation)
    {
        var code = resolution.State switch
        {
            SourceReferenceResolutionState.Ambiguous => IndexFindingCode.SourceAmbiguous,
            SourceReferenceResolutionState.Unsafe => IndexFindingCode.SourceUnsafe,
            SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unknown
                or SourceReferenceResolutionState.Unsupported => IndexFindingCode.InvalidSource,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution.State, "The unresolved source state is not defined."),
        };
        return new IndexFinding(
            code,
            sourceOccurrence: occurrence,
            source: null,
            cause: resolution.Cause ?? "The source reference could not be resolved.",
            candidates: resolution.Candidates.Select(candidate => IndexLogicalSourceProjector.Project(
                source: candidate,
                formation: formation)));
    }

    private static IndexSelection FormSelection(
        IndexSelectionOrigin origin,
        IReadOnlyList<SourceLogicalSource> sources,
        GeneratedNavigationFormation formation)
    {
        if (sources.Count == 0)
        {
            return IndexSelection.NotEstablished(origin);
        }

        var projected = sources.Select(source => IndexLogicalSourceProjector.Project(
            source: source,
            formation: formation)).ToArray();
        var rooted = projected.Count(source => source.Scope == IndexLogicalSourceScope.Rooted);
        var scope = rooted switch
        {
            0 => IndexSelectionScope.Detached,
            _ when rooted == projected.Length => IndexSelectionScope.Rooted,
            _ => IndexSelectionScope.Mixed,
        };
        return new IndexSelection(origin, scope, projected);
    }

    private static IndexFinding Finding(IndexFindingCode code, string cause)
        => new(
            code,
            sourceOccurrence: null,
            source: null,
            cause: cause,
            candidates: []);

}
