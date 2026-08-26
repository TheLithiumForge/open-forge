using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;

namespace OpenForge.Cli.Core.Framework.Sources.Selection;

internal sealed class SourceUniverseFilterResolver
{
    private readonly SourceReferenceResolver _referenceResolver;

    internal SourceUniverseFilterResolver(SourceReferenceResolver referenceResolver)
    {
        ArgumentNullException.ThrowIfNull(referenceResolver);
        _referenceResolver = referenceResolver;
    }

    internal SourceUniverseFilterResolution Resolve(SourceUniverseFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roleOccurrences = new Dictionary<SourceUniverseSelectorRole, int>();
        var selectors = new List<SourceUniverseSelectorResolution>(request.Occurrences.Count);
        foreach (var occurrence in request.Occurrences)
        {
            roleOccurrences.TryGetValue(occurrence.Role, out var currentRoleOccurrence);
            roleOccurrences[occurrence.Role] = ++currentRoleOccurrence;
            selectors.Add(ResolveSelector(
                occurrence,
                currentRoleOccurrence,
                request.Catalogue));
        }

        var selection = selectors.Any(
            selector => selector.Reference.State != SourceReferenceResolutionState.Resolved)
            ? SelectNothing(request.Catalogue)
            : SelectEffectiveSources(request, selectors);
        return new SourceUniverseFilterResolution(selectors, selection);
    }

    private SourceUniverseSelectorResolution ResolveSelector(
        SourceUniverseSelectorOccurrence occurrence,
        int roleOccurrence,
        SourceCatalogue catalogue)
    {
        var reference = _referenceResolver.Resolve(occurrence.Value, catalogue);
        if (reference.Source is not { } source)
        {
            return new SourceUniverseSelectorResolution(
                occurrence,
                roleOccurrence,
                reference,
                null,
                [],
                null);
        }

        var expansion = IsFolderSource(source.Base.Form)
            ? SourceUniverseSelectorExpansion.Folder
            : SourceUniverseSelectorExpansion.Source;
        if (expansion == SourceUniverseSelectorExpansion.Source)
        {
            return new SourceUniverseSelectorResolution(
                occurrence,
                roleOccurrence,
                reference,
                expansion,
                [source],
                null);
        }

        var scope = CreateScope(source);
        var expandedSources = catalogue.Sources
            .Where(candidate => PhysicalContainment.Contains(
                scope.PhysicalDirectoryPath,
                candidate.Base.PhysicalPath))
            .ToArray();
        return new SourceUniverseSelectorResolution(
            occurrence,
            roleOccurrence,
            reference,
            expansion,
            expandedSources,
            scope);
    }

    private static SourceCatalogueSelection SelectEffectiveSources(
        SourceUniverseFilterRequest request,
        IReadOnlyList<SourceUniverseSelectorResolution> selectors)
    {
        if (selectors.Count == 0)
        {
            return request.Catalogue.SelectAll();
        }

        var includes = selectors
            .Where(selector => selector.Occurrence.Role == SourceUniverseSelectorRole.Include)
            .ToArray();
        var excludes = selectors
            .Where(selector => selector.Occurrence.Role == SourceUniverseSelectorRole.Exclude)
            .ToArray();
        var excludedPaths = excludes
            .SelectMany(selector => selector.ExpandedSources)
            .Select(source => source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);
        var selectedSources = (includes.Length == 0
                ? request.Catalogue.Sources
                : includes.SelectMany(selector => selector.ExpandedSources))
            .Where(source => !excludedPaths.Contains(source.Identity.CanonicalBasePath))
            .GroupBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();

        var includedScopes = includes.Length == 0
            ? ReadDefaultScope(request.DefaultSelectionScope)
            : ReadScopes(includes);
        var excludedScopes = ReadScopes(excludes);
        return request.Catalogue.Select(new SourceCatalogueSelectionRequest(
            selectedSources,
            includedScopes,
            excludedScopes));
    }

    private static IReadOnlyList<SourceCatalogueSelectionScope> ReadDefaultScope(
        SourceCatalogueSelectionScope? scope)
        => scope is null ? [] : [scope];

    private static IReadOnlyList<SourceCatalogueSelectionScope> ReadScopes(
        IEnumerable<SourceUniverseSelectorResolution> selectors)
        => selectors
            .Select(selector => selector.Scope)
            .Where(scope => scope is not null)
            .Cast<SourceCatalogueSelectionScope>()
            .GroupBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal)
            .ToArray();

    private static SourceCatalogueSelection SelectNothing(SourceCatalogue catalogue)
        => catalogue.Select(new SourceCatalogueSelectionRequest([], [], []));

    private static bool IsFolderSource(SourceDocumentForm form)
        => form is SourceDocumentForm.Loader or SourceDocumentForm.Skill
            || SourceFormClassifier.IsEntrypoint(form);

    private static SourceCatalogueSelectionScope CreateScope(SourceLogicalSource source)
    {
        var physicalDirectory = Path.GetDirectoryName(source.Base.PhysicalPath)
            ?? throw new InvalidOperationException("A source layer must have a physical parent directory.");
        return new SourceCatalogueSelectionScope(
            SourceLogicalPath.ReadParent(source.Base.CanonicalPath),
            physicalDirectory);
    }
}
