using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Find.Models.Selection;

internal enum FindUniverseMode
{
    Default,
    Filtered,
}

internal enum FindSelectorResolution
{
    Resolved,
    Invalid,
    Unknown,
    Unsupported,
    Ambiguous,
    Unsafe,
}

internal enum FindSourceKind
{
    Loader,
    Entrypoint,
    Skill,
    Ordinary,
}

internal enum FindSelectorExpansion
{
    Folder,
    Source,
}

internal sealed record FindSourceIdentity
{
    internal FindSourceIdentity(string id, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !path.EndsWith(".md", StringComparison.Ordinal)
            || path.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("A Find source identity requires a canonical base source path.", nameof(path));
        }

        if (!string.Equals(SourceIdentity.DeriveId(path), id, StringComparison.Ordinal))
        {
            throw new ArgumentException("A Find source identity ID must be derived from its canonical base path.", nameof(id));
        }

        Id = id;
        Path = path;
    }

    internal string Id { get; }

    internal string Path { get; }
}

internal sealed record FindSelector
{
    internal FindSelector(
        string value,
        SourceReferenceKind? form,
        FindSelectorResolution resolution,
        FindSourceIdentity? identity,
        FindSourceKind? sourceKind,
        FindSelectorExpansion? expansion,
        IEnumerable<FindSourceIdentity> candidates)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (form is { } suppliedForm && !Enum.IsDefined(suppliedForm))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Find selector form is not defined.");
        }

        if (!Enum.IsDefined(resolution))
        {
            throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The Find selector resolution is not defined.");
        }

        if (sourceKind is { } kind && !Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(sourceKind), sourceKind, "The Find source kind is not defined.");
        }

        if (expansion is { } selectorExpansion && !Enum.IsDefined(selectorExpansion))
        {
            throw new ArgumentOutOfRangeException(nameof(expansion), expansion, "The Find selector expansion is not defined.");
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var materializedCandidates = candidates.ToArray();
        if (materializedCandidates.Any(candidate => candidate is null)
            || materializedCandidates
                .Select(candidate => (candidate.Id, candidate.Path))
                .Distinct()
                .Count() != materializedCandidates.Length)
        {
            throw new ArgumentException("Find selector candidates must be non-null and unique.", nameof(candidates));
        }

        ValidateIdentityOrder(materializedCandidates, nameof(candidates));

        if (resolution == FindSelectorResolution.Resolved
            && (identity is null || sourceKind is null || expansion is null || materializedCandidates.Length != 0))
        {
            throw new ArgumentException("A resolved Find selector requires one identity and expansion facts only.");
        }

        if (resolution == FindSelectorResolution.Ambiguous
            && (identity is not null || sourceKind is not null || expansion is not null || materializedCandidates.Length < 2))
        {
            throw new ArgumentException("An ambiguous Find selector requires candidate identities only.");
        }

        if (resolution != FindSelectorResolution.Resolved
            && resolution != FindSelectorResolution.Ambiguous
            && (identity is not null || sourceKind is not null || expansion is not null || materializedCandidates.Length != 0))
        {
            throw new ArgumentException("An unresolved Find selector cannot carry resolved identity facts.");
        }

        Value = value;
        Form = form;
        Resolution = resolution;
        Identity = identity;
        SourceKind = sourceKind;
        Expansion = expansion;
        Candidates = Array.AsReadOnly(materializedCandidates);
    }

    internal string Value { get; }

    internal SourceReferenceKind? Form { get; }

    internal FindSelectorResolution Resolution { get; }

    internal FindSourceIdentity? Identity { get; }

    internal FindSourceKind? SourceKind { get; }

    internal FindSelectorExpansion? Expansion { get; }

    internal IReadOnlyList<FindSourceIdentity> Candidates { get; }

    private static void ValidateIdentityOrder(
        IReadOnlyList<FindSourceIdentity> identities,
        string parameterName)
    {
        for (var index = 1; index < identities.Count; index++)
        {
            var idOrder = string.CompareOrdinal(identities[index - 1].Id, identities[index].Id);
            if (idOrder > 0
                || idOrder == 0
                    && string.CompareOrdinal(identities[index - 1].Path, identities[index].Path) >= 0)
            {
                throw new ArgumentException("Find source identities must use ordinal ID and path order.", parameterName);
            }
        }
    }
}

internal sealed record FindUniverse
{
    internal FindUniverse(
        FindUniverseMode mode,
        IEnumerable<FindSelector> include,
        IEnumerable<FindSelector> exclude,
        int? candidateCount,
        int? inspectedCount,
        int? matchedCount)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Find universe mode is not defined.");
        }

        Include = Snapshot(include, nameof(include));
        Exclude = Snapshot(exclude, nameof(exclude));
        if ((mode == FindUniverseMode.Default) != (Include.Count == 0 && Exclude.Count == 0))
        {
            throw new ArgumentException("The Find universe mode must reflect whether selectors were supplied.", nameof(mode));
        }

        ValidateCount(candidateCount, nameof(candidateCount));
        ValidateCount(inspectedCount, nameof(inspectedCount));
        ValidateCount(matchedCount, nameof(matchedCount));
        if (inspectedCount is { } establishedInspected
            && candidateCount is { } establishedCandidates
            && establishedInspected > establishedCandidates)
        {
            throw new ArgumentException("The inspected Find count cannot exceed the candidate count.", nameof(inspectedCount));
        }

        if (matchedCount is { } establishedMatched
            && candidateCount is { } establishedCandidatesForMatches
            && establishedMatched > establishedCandidatesForMatches)
        {
            throw new ArgumentException("The matched Find count cannot exceed the candidate count.", nameof(matchedCount));
        }

        Mode = mode;
        CandidateCount = candidateCount;
        InspectedCount = inspectedCount;
        MatchedCount = matchedCount;
    }

    internal FindUniverseMode Mode { get; }

    internal IReadOnlyList<FindSelector> Include { get; }

    internal IReadOnlyList<FindSelector> Exclude { get; }

    internal int? CandidateCount { get; }

    internal int? InspectedCount { get; }

    internal int? MatchedCount { get; }

    private static IReadOnlyList<FindSelector> Snapshot(
        IEnumerable<FindSelector> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find selector lists cannot contain null values.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static void ValidateCount(int? count, string parameterName)
    {
        if (count is < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, count, "Find counts cannot be negative.");
        }
    }
}
