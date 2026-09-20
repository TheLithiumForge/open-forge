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
        IEnumerable<SourceLogicalSource> intendedSources,
        IEnumerable<SourceCandidate> observedCandidates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        var orderedSources = MaterializeSources(intendedSources);
        var orderedCandidates = MaterializeCandidates(observedCandidates);
        Validate(kind, subject, orderedSources, orderedCandidates);

        Kind = kind;
        Subject = subject;
        IntendedSources = new ReadOnlyCollection<SourceLogicalSource>(orderedSources);
        Candidates = new ReadOnlyCollection<SourceCandidate>(orderedCandidates);
    }

    internal GeneratedNavigationFormationAmbiguityKind Kind { get; }

    internal string Subject { get; }

    internal IReadOnlyList<SourceLogicalSource> IntendedSources { get; }

    internal IReadOnlyList<SourceCandidate> Candidates { get; }

    private static SourceLogicalSource[] MaterializeSources(
        IEnumerable<SourceLogicalSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var ordered = sources
            .Select(source => source ?? throw new ArgumentException(
                "A generated navigation ambiguity cannot contain a null intended source.",
                nameof(sources)))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(source => source.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A generated navigation ambiguity requires unique intended sources.",
                nameof(sources));
        }

        return ordered;
    }

    private static SourceCandidate[] MaterializeCandidates(
        IEnumerable<SourceCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var ordered = candidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "A generated navigation ambiguity cannot contain a null observed candidate.",
                nameof(candidates)))
            .OrderBy(candidate => candidate.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(candidate => candidate.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .Count() != ordered.Length)
        {
            throw new ArgumentException(
                "A generated navigation ambiguity requires unique observed candidates.",
                nameof(candidates));
        }

        return ordered;
    }

    private static void Validate(
        GeneratedNavigationFormationAmbiguityKind kind,
        string subject,
        IReadOnlyList<SourceLogicalSource> intendedSources,
        IReadOnlyList<SourceCandidate> observedCandidates)
    {
        var validationError = kind switch
        {
            GeneratedNavigationFormationAmbiguityKind.RootEntrypoint => ReadRootEntrypointValidationError(
                subject,
                intendedSources),
            GeneratedNavigationFormationAmbiguityKind.RouteParent => ReadRouteParentValidationError(
                subject,
                intendedSources),
            GeneratedNavigationFormationAmbiguityKind.PhysicalAlias => ReadPhysicalAliasValidationError(
                subject,
                observedCandidates),
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The generated navigation ambiguity kind is not defined."),
        };
        if (validationError is not null)
        {
            throw new ArgumentException(validationError, nameof(subject));
        }
    }

    private static string? ReadRootEntrypointValidationError(
        string subject,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        if (intendedSources.Count >= 2
            && SourceLogicalPath.IsCanonicalRoot(subject)
            && intendedSources.All(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && string.Equals(
                    SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                    subject,
                    StringComparison.Ordinal)))
        {
            return null;
        }

        return "A root-entrypoint ambiguity requires one represented folder and at least two intended entrypoints.";
    }

    private static string? ReadRouteParentValidationError(
        string subject,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        return intendedSources.Count >= 2 && SourceLogicalPath.IsCanonicalSource(subject)
            ? null
            : "A route-parent ambiguity requires a canonical subject and at least two intended parents.";
    }

    private static string? ReadPhysicalAliasValidationError(
        string subject,
        IReadOnlyList<SourceCandidate> observedCandidates)
    {
        if (observedCandidates.Count >= 2
            && string.Equals(subject, observedCandidates[0].CanonicalPath, StringComparison.Ordinal))
        {
            return null;
        }

        return "A physical-alias ambiguity requires at least two observed candidates and their canonical representative.";
    }
}
