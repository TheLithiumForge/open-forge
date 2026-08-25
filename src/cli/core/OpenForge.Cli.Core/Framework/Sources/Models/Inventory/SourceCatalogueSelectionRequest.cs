using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogueSelectionRequest
{
    internal SourceCatalogueSelectionRequest(
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<SourceCatalogueSelectionScope> includedScopes,
        IEnumerable<SourceCatalogueSelectionScope> excludedScopes)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(includedScopes);
        ArgumentNullException.ThrowIfNull(excludedScopes);

        var materializedSources = sources.ToArray();
        if (materializedSources.Any(source => source is null)
            || materializedSources.Select(source => source.Identity.CanonicalBasePath)
                .Distinct(StringComparer.Ordinal)
                .Count() != materializedSources.Length)
        {
            throw new ArgumentException("Selected logical sources must be non-null and unique by canonical base path.", nameof(sources));
        }

        Sources = new ReadOnlyCollection<SourceLogicalSource>(
            materializedSources
                .OrderBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
                .ThenBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
                .ToArray());
        IncludedScopes = MaterializeScopes(includedScopes, nameof(includedScopes));
        ExcludedScopes = MaterializeScopes(excludedScopes, nameof(excludedScopes));
    }

    internal IReadOnlyList<SourceLogicalSource> Sources { get; }

    internal IReadOnlyList<SourceCatalogueSelectionScope> IncludedScopes { get; }

    internal IReadOnlyList<SourceCatalogueSelectionScope> ExcludedScopes { get; }

    private static IReadOnlyList<SourceCatalogueSelectionScope> MaterializeScopes(
        IEnumerable<SourceCatalogueSelectionScope> scopes,
        string parameterName)
    {
        var materialized = scopes.ToArray();
        if (materialized.Any(scope => scope is null)
            || materialized.Select(scope => scope.CanonicalDirectoryPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != materialized.Length)
        {
            throw new ArgumentException("Selection scopes must be non-null and unique by canonical directory path.", parameterName);
        }

        return new ReadOnlyCollection<SourceCatalogueSelectionScope>(
            materialized.OrderBy(scope => scope.CanonicalDirectoryPath, StringComparer.Ordinal).ToArray());
    }
}
