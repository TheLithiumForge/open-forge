using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;

internal enum GeneratedNavigationFormationAmbiguityKind
{
    RootEntrypoint,
    RouteParent,
    PhysicalAlias,
}

internal sealed class GeneratedNavigationFormationAmbiguity
{
    internal GeneratedNavigationFormationAmbiguity(
        GeneratedNavigationFormationAmbiguityKind kind,
        string subject,
        IEnumerable<SourceCandidate> candidates)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The generated navigation ambiguity kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(candidates);
        var orderedCandidates = candidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "A generated navigation ambiguity cannot contain a null candidate.",
                nameof(candidates)))
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (orderedCandidates.Length < 2
            || orderedCandidates.Select(candidate => candidate.CanonicalPath)
                .Distinct(StringComparer.Ordinal)
                .Count() != orderedCandidates.Length)
        {
            throw new ArgumentException(
                "A generated navigation ambiguity requires at least two unique candidates.",
                nameof(candidates));
        }

        if (kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint
            && (!SourceLogicalPath.IsCanonicalRoot(subject)
                || orderedCandidates.Any(candidate => candidate.Form is not { } form
                    || !SourceFormClassifier.IsEntrypoint(form)
                    || !string.Equals(SourceLogicalPath.ReadParent(candidate.CanonicalPath), subject, StringComparison.Ordinal))))
        {
            throw new ArgumentException(
                "A root-entrypoint ambiguity requires one represented folder and its recognized entrypoint candidates.",
                nameof(subject));
        }

        if (kind == GeneratedNavigationFormationAmbiguityKind.RouteParent
            && !SourceLogicalPath.IsCanonicalSource(subject))
        {
            throw new ArgumentException("A route-parent ambiguity subject must be a canonical source path.", nameof(subject));
        }

        if (kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias
            && !string.Equals(subject, orderedCandidates[0].CanonicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("A physical-alias ambiguity subject must be its canonical representative.", nameof(subject));
        }

        Kind = kind;
        Subject = subject;
        Candidates = new ReadOnlyCollection<SourceCandidate>(orderedCandidates);
    }

    internal GeneratedNavigationFormationAmbiguityKind Kind { get; }

    internal string Subject { get; }

    internal IReadOnlyList<SourceCandidate> Candidates { get; }
}
