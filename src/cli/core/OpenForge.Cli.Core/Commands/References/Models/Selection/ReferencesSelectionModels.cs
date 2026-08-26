using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;

namespace OpenForge.Cli.Core.Commands.References.Models.Selection;

internal enum ReferencesSelectionMode
{
    Default,
    Filtered,
}

internal sealed record ReferencesSelectorOccurrence
{
    internal ReferencesSelectorOccurrence(SourceUniverseSelectorRole role, string value)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "The References selector role is not defined.");
        }

        ArgumentNullException.ThrowIfNull(value);
        Role = role;
        Value = value;
    }

    internal SourceUniverseSelectorRole Role { get; }

    internal string Value { get; }
}

internal sealed record ReferencesSelectorResolution
{
    internal ReferencesSelectorResolution(
        SourceUniverseSelectorRole role,
        int occurrence,
        string supplied,
        SourceReferenceKind form,
        SourceReferenceResolutionState resolution,
        ReferencesSourceIdentity? source,
        SourceUniverseSelectorExpansion? expansion,
        IEnumerable<ReferencesSourceIdentity> candidates)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "The References selector role is not defined.");
        }

        if (occurrence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(occurrence), occurrence, "A References selector occurrence must be positive.");
        }

        ArgumentNullException.ThrowIfNull(supplied);
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The References selector form is not defined.");
        }

        if (!Enum.IsDefined(resolution))
        {
            throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The References selector resolution is not defined.");
        }

        if (resolution == SourceReferenceResolutionState.Resolved && source is null)
        {
            throw new ArgumentException("A resolved selector requires a source identity.", nameof(source));
        }

        if (resolution != SourceReferenceResolutionState.Resolved && source is not null)
        {
            throw new ArgumentException("An unresolved selector cannot carry a source identity.", nameof(source));
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var materialized = candidates
            .Select(candidate => candidate ?? throw new ArgumentException("Selector candidates cannot contain null members.", nameof(candidates)))
            .Distinct()
            .OrderBy(candidate => candidate.Id, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
            .ToArray();
        if (resolution != SourceReferenceResolutionState.Ambiguous && materialized.Length != 0)
        {
            throw new ArgumentException("Only ambiguous selector resolutions carry candidates.", nameof(candidates));
        }

        Role = role;
        Occurrence = occurrence;
        Supplied = supplied;
        Form = form;
        Resolution = resolution;
        Source = source;
        Expansion = expansion;
        Candidates = new ReadOnlyCollection<ReferencesSourceIdentity>(materialized);
    }

    internal SourceUniverseSelectorRole Role { get; }

    internal int Occurrence { get; }

    internal string Supplied { get; }

    internal SourceReferenceKind Form { get; }

    internal SourceReferenceResolutionState Resolution { get; }

    internal ReferencesSourceIdentity? Source { get; }

    internal SourceUniverseSelectorExpansion? Expansion { get; }

    internal IReadOnlyList<ReferencesSourceIdentity> Candidates { get; }
}

internal sealed record ReferencesSourceLayerEvidence
{
    internal ReferencesSourceLayerEvidence(ReferencesSourceIdentity source, SourceLayerKind layer, string path)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(layer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The References source layer is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Source = source;
        Layer = layer;
        Path = path;
    }

    internal ReferencesSourceIdentity Source { get; }

    internal SourceLayerKind Layer { get; }

    internal string Path { get; }
}

internal sealed record ReferencesIncomingSelection
{
    internal ReferencesIncomingSelection(
        ReferencesSelectionMode mode,
        IEnumerable<ReferencesSelectorOccurrence> supplied,
        IEnumerable<ReferencesSelectorResolution> resolved,
        IEnumerable<ReferencesSourceIdentity> effectiveSources,
        IEnumerable<ReferencesSourceLayerEvidence> inspectedSources)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The References selection mode is not defined.");
        }

        ArgumentNullException.ThrowIfNull(supplied);
        ArgumentNullException.ThrowIfNull(resolved);
        ArgumentNullException.ThrowIfNull(effectiveSources);
        ArgumentNullException.ThrowIfNull(inspectedSources);
        var suppliedValues = supplied
            .Select(value => value ?? throw new ArgumentException("Supplied selectors cannot contain null members.", nameof(supplied)))
            .ToArray();
        var resolvedValues = resolved
            .Select(value => value ?? throw new ArgumentException("Selector resolutions cannot contain null members.", nameof(resolved)))
            .ToArray();
        if (resolvedValues.Length != suppliedValues.Length)
        {
            throw new ArgumentException("Incoming selection must retain one resolution per supplied selector.", nameof(resolved));
        }

        if (mode == ReferencesSelectionMode.Default && suppliedValues.Length != 0)
        {
            throw new ArgumentException("Default incoming selection has no selectors.", nameof(mode));
        }

        var roleOccurrences = new Dictionary<SourceUniverseSelectorRole, int>();
        for (var index = 0; index < suppliedValues.Length; index++)
        {
            var suppliedValue = suppliedValues[index];
            var resolvedValue = resolvedValues[index];
            roleOccurrences.TryGetValue(suppliedValue.Role, out var priorOccurrence);
            var expectedOccurrence = priorOccurrence + 1;
            roleOccurrences[suppliedValue.Role] = expectedOccurrence;
            if (resolvedValue.Role != suppliedValue.Role
                || resolvedValue.Occurrence != expectedOccurrence
                || !string.Equals(resolvedValue.Supplied, suppliedValue.Value, StringComparison.Ordinal))
            {
                throw new ArgumentException("Resolved selectors must preserve supplied global order and role-local occurrence.", nameof(resolved));
            }
        }

        var effectiveValues = effectiveSources
            .Select(value => value ?? throw new ArgumentException("Effective sources cannot contain null members.", nameof(effectiveSources)))
            .Distinct()
            .OrderBy(value => value.Id, StringComparer.Ordinal)
            .ThenBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
        var inspectedValues = inspectedSources
            .Select(value => value ?? throw new ArgumentException("Inspected source evidence cannot contain null members.", nameof(inspectedSources)))
            .ToArray();
        Mode = mode;
        Supplied = new ReadOnlyCollection<ReferencesSelectorOccurrence>(suppliedValues);
        Resolved = new ReadOnlyCollection<ReferencesSelectorResolution>(resolvedValues);
        EffectiveSources = new ReadOnlyCollection<ReferencesSourceIdentity>(effectiveValues);
        InspectedSources = new ReadOnlyCollection<ReferencesSourceLayerEvidence>(inspectedValues);
    }

    internal ReferencesSelectionMode Mode { get; }

    internal IReadOnlyList<ReferencesSelectorOccurrence> Supplied { get; }

    internal IReadOnlyList<ReferencesSelectorResolution> Resolved { get; }

    internal IReadOnlyList<ReferencesSourceIdentity> EffectiveSources { get; }

    internal IReadOnlyList<ReferencesSourceLayerEvidence> InspectedSources { get; }
}
