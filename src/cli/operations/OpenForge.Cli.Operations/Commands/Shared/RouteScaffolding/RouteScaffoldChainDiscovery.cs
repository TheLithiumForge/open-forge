using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;

/// <summary>
/// Finds selected ancestor gaps under existing recognized roots, as defined by
/// Forgiving Maintenance, Entrypoint Chain Completion in Index. Eligibility of
/// authored content remains a caller fact. Discovery reads no files or history.
/// </summary>
internal sealed class RouteScaffoldChainDiscovery
{
    internal RouteScaffoldDiscovery Discover(RouteScaffoldDiscoveryRequest request)
    {
        var catalogue = request.Catalogue;
        var roots = request.RecognizedRootPaths
            .Where(path => SourceFormClassifier.TryClassify(catalogue.Layout, path, out var form) && SourceFormClassifier.IsEntrypoint(form)
                && (catalogue.FindByPath(path) is not null || catalogue.FindCandidateByPath(path) is not null))
            .Select(SourceLogicalPath.ReadParent)
            .Where(folder => SourceLogicalPath.IsPrimaryCategoryDirectory(new SourceContentRoots(catalogue.Layout), folder))
            .Distinct(StringComparer.Ordinal).ToArray();
        var entrypoints = catalogue.Candidates
            .Where(candidate => candidate.Form is { } form && SourceFormClassifier.IsEntrypoint(form))
            .Select(candidate => candidate.CanonicalPath)
            .Concat(catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
                .Select(source => source.Identity.CanonicalBasePath))
            .Distinct(StringComparer.Ordinal)
            .GroupBy(SourceLogicalPath.ReadParent, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var packages = catalogue.Candidates.Where(candidate => candidate.Form == SourceDocumentForm.Skill)
            .Select(candidate => SourceLogicalPath.ReadParent(candidate.CanonicalPath))
            .Concat(catalogue.Sources.Where(source => source.Base.Form == SourceDocumentForm.Skill)
                .Select(source => SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath))).ToArray();
        var folders = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in request.EligibleSources)
        {
            if (source.Base.Form is not SourceDocumentForm.Markdown && !SourceFormClassifier.IsEntrypoint(source.Base.Form))
                continue;
            var path = source.Identity.CanonicalBasePath;
            var root = roots.FirstOrDefault(folder => path.StartsWith(folder + "/", StringComparison.Ordinal));
            if (root is null || IsExcluded(path, request) || packages.Any(package => IsWithin(path, package)))
                continue;
            var chain = new List<string>();
            var folder = SourceLogicalPath.ReadParent(path);
            while (folder != root)
            {
                chain.Add(folder);
                folder = SourceLogicalPath.ReadParent(folder);
            }

            if (chain.Any(item => IsExcluded(item, request)
                || IsExcluded(RouteScaffoldSourceBuilder.CanonicalPath(catalogue.Layout, SourceLogicalPath.ReadSourceCoordinate(catalogue.Layout, item)), request)))
                continue;
            folders.Add(root);
            folders.UnionWith(chain);
        }

        var gaps = ImmutableArray.CreateBuilder<RouteScaffoldGap>();
        var boundaries = ImmutableArray.CreateBuilder<RouteScaffoldBoundary>();
        if (request.Sharing?.FindUnavailableEntrypoint(catalogue) is { } sharedPath)
            boundaries.Add(new(RouteScaffoldBoundaryKind.SharedEntrypointMissing, sharedPath));
        foreach (var folder in folders.OrderBy(item => item.Count(character => character == '/')).ThenBy(item => item, StringComparer.Ordinal))
        {
            if (entrypoints.TryGetValue(folder, out var existing))
            {
                if (existing.Length > 1)
                    boundaries.Add(new(RouteScaffoldBoundaryKind.EntrypointAmbiguous, folder));
                continue;
            }

            if (roots.Contains(folder, StringComparer.Ordinal))
                continue;
            var id = SourceLogicalPath.ReadSourceCoordinate(catalogue.Layout, folder);
            var path = RouteScaffoldSourceBuilder.CanonicalPath(catalogue.Layout, id);
            if (catalogue.FindAllById(id).Count != 0 || catalogue.FindAllCandidatesById(id).Count != 0)
                boundaries.Add(new(RouteScaffoldBoundaryKind.IdentityOccupied, path));
            else if (catalogue.FindCandidateByPath(path) is not null || request.OccupiedPaths.Contains(path, StringComparer.Ordinal))
                boundaries.Add(new(RouteScaffoldBoundaryKind.PathOccupied, path));
            else
                gaps.Add(new(id, path));
        }

        return new(gaps.OrderBy(gap => gap.FolderId.Count(character => character == '/'))
            .ThenBy(gap => gap.CanonicalPath, StringComparer.Ordinal).ToImmutableArray(), boundaries.ToImmutable());
    }

    private static bool IsExcluded(string path, RouteScaffoldDiscoveryRequest request)
        => WorkspaceRemovals.IsPathRemoved(request.Catalogue.Layout, path, request.Settings) || request.Sharing?.Includes(path) == false;

    private static bool IsWithin(string path, string folder)
        => path == folder || path.StartsWith(folder + "/", StringComparison.Ordinal);

}
