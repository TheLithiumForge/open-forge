using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Selection;

internal enum SourceUniverseSelectorExpansion
{
    Source,
    Folder,
}

internal sealed class SourceUniverseSelectorResolution
{
    internal SourceUniverseSelectorResolution(
        SourceUniverseSelectorOccurrence occurrence,
        int roleOccurrence,
        SourceReferenceResolution reference,
        SourceUniverseSelectorExpansion? expansion,
        IEnumerable<SourceLogicalSource> expandedSources,
        SourceCatalogueSelectionScope? scope)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        if (roleOccurrence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(roleOccurrence), roleOccurrence, "A role occurrence must be positive.");
        }

        ArgumentNullException.ThrowIfNull(reference);
        if (!string.Equals(reference.Value, occurrence.Value, StringComparison.Ordinal))
        {
            throw new ArgumentException("A selector occurrence and its reference resolution must preserve the same value.", nameof(reference));
        }

        if (expansion is { } establishedExpansion && !Enum.IsDefined(establishedExpansion))
        {
            throw new ArgumentOutOfRangeException(nameof(expansion), expansion, "The selector expansion is not defined.");
        }

        ArgumentNullException.ThrowIfNull(expandedSources);
        var materializedSources = expandedSources
            .Select(source => source ?? throw new ArgumentException(
                "Expanded selector sources cannot contain null members.",
                nameof(expandedSources)))
            .OrderBy(source => source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (materializedSources
            .Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != materializedSources.Length)
        {
            throw new ArgumentException("Expanded selector sources must have unique logical paths.", nameof(expandedSources));
        }

        var isResolved = reference.State == SourceReferenceResolutionState.Resolved;
        if (isResolved != (expansion is not null)
            || !isResolved && (materializedSources.Length != 0 || scope is not null)
            || expansion == SourceUniverseSelectorExpansion.Source
                && (materializedSources.Length != 1 || scope is not null)
            || expansion == SourceUniverseSelectorExpansion.Folder && scope is null)
        {
            throw new ArgumentException("Selector expansion facts must match the source-reference resolution.");
        }

        Occurrence = occurrence;
        RoleOccurrence = roleOccurrence;
        Reference = reference;
        Expansion = expansion;
        ExpandedSources = new ReadOnlyCollection<SourceLogicalSource>(materializedSources);
        Scope = scope;
    }

    internal SourceUniverseSelectorOccurrence Occurrence { get; }

    internal int RoleOccurrence { get; }

    internal SourceReferenceResolution Reference { get; }

    internal SourceUniverseSelectorExpansion? Expansion { get; }

    internal IReadOnlyList<SourceLogicalSource> ExpandedSources { get; }

    internal SourceCatalogueSelectionScope? Scope { get; }
}
