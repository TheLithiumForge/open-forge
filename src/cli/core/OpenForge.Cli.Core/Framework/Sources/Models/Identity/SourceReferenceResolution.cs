using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Identity;

internal enum SourceReferenceResolutionState
{
    Resolved,
    Invalid,
    Unknown,
    Unsupported,
    Ambiguous,
    Unsafe,
}

internal sealed class SourceReferenceResolution
{
    internal SourceReferenceResolution(
        string value,
        SourceReferenceKind form,
        SourceReferenceResolutionState state,
        SourceLogicalSource? source,
        IEnumerable<SourceLogicalSource> candidates,
        string? canonicalPath,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The source-reference form is not defined.");
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source-reference resolution state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(candidates);
        var materializedCandidates = candidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "Source-reference candidates cannot contain null members.",
                nameof(candidates)))
            .OrderBy(candidate => candidate.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (materializedCandidates
            .Select(candidate => candidate.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != materializedCandidates.Length)
        {
            throw new ArgumentException("Source-reference candidates must have unique logical paths.", nameof(candidates));
        }

        if (state == SourceReferenceResolutionState.Resolved
            && (source is null || materializedCandidates.Length != 0 || cause is not null))
        {
            throw new ArgumentException("A resolved source reference requires one source and no unresolved evidence.");
        }

        if (state == SourceReferenceResolutionState.Ambiguous
            && (source is not null || materializedCandidates.Length < 2 || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException("An ambiguous source reference requires at least two candidates and a cause.");
        }

        if (state is not (SourceReferenceResolutionState.Resolved or SourceReferenceResolutionState.Ambiguous)
            && (source is not null || materializedCandidates.Length != 0 || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException("An unresolved source reference requires only one cause.");
        }

        Value = value;
        Form = form;
        State = state;
        Source = source;
        Candidates = new ReadOnlyCollection<SourceLogicalSource>(materializedCandidates);
        CanonicalPath = canonicalPath;
        Cause = cause;
    }

    internal string Value { get; }

    internal SourceReferenceKind Form { get; }

    internal SourceReferenceResolutionState State { get; }

    internal SourceLogicalSource? Source { get; }

    internal IReadOnlyList<SourceLogicalSource> Candidates { get; }

    internal string? CanonicalPath { get; }

    internal string? Cause { get; }
}
