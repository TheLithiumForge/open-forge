using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Models.Query;

internal enum FindPredicateKind
{
    Tag,
    Heading,
}

internal enum FindRequirement
{
    All,
    Any,
}

internal enum FindRegionKind
{
    Document,
    Frontmatter,
    Body,
    Section,
}

internal sealed record FindPredicate
{
    internal FindPredicate(FindPredicateKind kind, string suppliedValue, string comparisonValue)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find predicate kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(suppliedValue);
        ArgumentNullException.ThrowIfNull(comparisonValue);
        Kind = kind;
        SuppliedValue = suppliedValue;
        ComparisonValue = comparisonValue;
    }

    internal FindPredicateKind Kind { get; }

    internal string SuppliedValue { get; }

    internal string ComparisonValue { get; }
}

internal sealed record FindPredicateOccurrence
{
    internal FindPredicateOccurrence(FindPredicateKind kind, string value, int position)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find predicate kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(value);
        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "A Find predicate position must be positive.");
        }

        Kind = kind;
        Value = value;
        Position = position;
    }

    internal FindPredicateKind Kind { get; }

    internal string Value { get; }

    internal int Position { get; }
}

internal sealed record FindRegion
{
    internal FindRegion(FindRegionKind kind, string? name, string canonicalValue)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find region kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(canonicalValue);
        if (kind == FindRegionKind.Section)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (!string.Equals(
                    canonicalValue,
                    $"{FindDefinitions.SectionPrefix}{name}",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException("A section region must use its canonical section: value.", nameof(canonicalValue));
            }
        }
        else if (name is not null)
        {
            throw new ArgumentException("Only a section region can carry a name.", nameof(name));
        }

        var expected = kind switch
        {
            FindRegionKind.Document => FindDefinitions.Document,
            FindRegionKind.Frontmatter => FindDefinitions.Frontmatter,
            FindRegionKind.Body => FindDefinitions.Body,
            FindRegionKind.Section => canonicalValue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find region kind is not defined."),
        };
        if (!string.Equals(expected, canonicalValue, StringComparison.Ordinal))
        {
            throw new ArgumentException("The Find region canonical value does not match its kind.", nameof(canonicalValue));
        }

        Kind = kind;
        Name = name;
        CanonicalValue = canonicalValue;
    }

    internal FindRegionKind Kind { get; }

    internal string? Name { get; }

    internal string CanonicalValue { get; }
}

internal sealed record FindRegionSelection
{
    internal FindRegionSelection(
        IEnumerable<FindRegion> supplied,
        IEnumerable<FindRegion> tag,
        IEnumerable<FindRegion> heading)
    {
        Supplied = Snapshot(supplied, nameof(supplied));
        Tag = Snapshot(tag, nameof(tag));
        Heading = Snapshot(heading, nameof(heading));
        ValidateEffectiveRegions(Tag, allowFrontmatter: true, nameof(tag));
        ValidateEffectiveRegions(Heading, allowFrontmatter: false, nameof(heading));
        if (Supplied.Count == 0
            && (Tag.Count != 1
                || Tag[0].Kind != FindRegionKind.Frontmatter
                || Heading.Count != 1
                || Heading[0].Kind != FindRegionKind.Body))
        {
            throw new ArgumentException("Omitted Find regions must establish the natural predicate defaults.");
        }
    }

    internal IReadOnlyList<FindRegion> Supplied { get; }

    internal IReadOnlyList<FindRegion> Tag { get; }

    internal IReadOnlyList<FindRegion> Heading { get; }

    private static IReadOnlyList<FindRegion> Snapshot(
        IEnumerable<FindRegion> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find region selections cannot contain null values.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static void ValidateEffectiveRegions(
        IReadOnlyList<FindRegion> values,
        bool allowFrontmatter,
        string parameterName)
    {
        if (values.Count == 0
            || values.Select(value => value.CanonicalValue)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != values.Count)
        {
            throw new ArgumentException("Effective Find regions must be nonempty and unique.", parameterName);
        }

        if (!allowFrontmatter && values.Any(value => value.Kind == FindRegionKind.Frontmatter))
        {
            throw new ArgumentException("Heading predicates cannot use frontmatter regions.", parameterName);
        }

        if (values.Any(value => value.Kind == FindRegionKind.Document) && values.Count != 1
            || values.Any(value => value.Kind == FindRegionKind.Body)
                && values.Any(value => value.Kind == FindRegionKind.Section))
        {
            throw new ArgumentException("Effective Find regions cannot retain subsumed members.", parameterName);
        }
    }
}

internal sealed record FindQuery
{
    internal FindQuery(
        IEnumerable<FindPredicate> predicates,
        IEnumerable<FindPredicate> effectivePredicates,
        FindRequirement requirement,
        FindRegionSelection within)
    {
        Predicates = Snapshot(predicates, nameof(predicates));
        EffectivePredicates = Snapshot(effectivePredicates, nameof(effectivePredicates));
        if (!Enum.IsDefined(requirement))
        {
            throw new ArgumentOutOfRangeException(nameof(requirement), requirement, "The Find requirement is not defined.");
        }

        ArgumentNullException.ThrowIfNull(within);
        ValidateEffectivePredicates(Predicates, EffectivePredicates);
        Requirement = requirement;
        Within = within;
    }

    internal IReadOnlyList<FindPredicate> Predicates { get; }

    internal IReadOnlyList<FindPredicate> EffectivePredicates { get; }

    internal FindRequirement Requirement { get; }

    internal FindRegionSelection Within { get; }

    private static IReadOnlyList<FindPredicate> Snapshot(
        IEnumerable<FindPredicate> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find predicates cannot contain null values.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static void ValidateEffectivePredicates(
        IReadOnlyList<FindPredicate> supplied,
        IReadOnlyList<FindPredicate> effective)
    {
        var firstOccurrences = new List<FindPredicate>();
        foreach (var predicate in supplied)
        {
            if (!firstOccurrences.Any(first => AreEquivalent(first, predicate)))
            {
                firstOccurrences.Add(predicate);
            }
        }

        if (effective.Count != firstOccurrences.Count
            || effective.Where((predicate, index) => !AreEquivalent(predicate, firstOccurrences[index])
                || !string.Equals(predicate.SuppliedValue, firstOccurrences[index].SuppliedValue, StringComparison.Ordinal))
                .Any())
        {
            throw new ArgumentException("Effective Find predicates must contain every first supplied occurrence in order.", nameof(effective));
        }
    }

    private static bool AreEquivalent(FindPredicate left, FindPredicate right)
        => left.Kind == right.Kind
            && string.Equals(left.ComparisonValue, right.ComparisonValue, StringComparison.OrdinalIgnoreCase);
}

internal sealed record FindQueryInput
{
    internal FindQueryInput(
        IEnumerable<string> includeValues,
        IEnumerable<string> excludeValues,
        IEnumerable<string> tagValues,
        IEnumerable<string> headingValues,
        string? requireValue,
        string? withinValue,
        string? contentValue,
        IEnumerable<FindPredicateOccurrence> predicateOccurrences,
        CliOptionResultFacts includeFacts,
        CliOptionResultFacts excludeFacts,
        CliOptionResultFacts tagFacts,
        CliOptionResultFacts headingFacts,
        CliOptionResultFacts requireFacts,
        CliOptionResultFacts withinFacts,
        CliOptionResultFacts contentFacts,
        string? requireSpelling,
        string? withinSpelling,
        string? contentSpelling,
        CliView? suppliedView,
        CliView effectiveView)
    {
        IncludeValues = Snapshot(includeValues, nameof(includeValues));
        ExcludeValues = Snapshot(excludeValues, nameof(excludeValues));
        TagValues = Snapshot(tagValues, nameof(tagValues));
        HeadingValues = Snapshot(headingValues, nameof(headingValues));
        PredicateOccurrences = Snapshot(predicateOccurrences, nameof(predicateOccurrences));
        IncludeFacts = ValidateFacts(includeFacts, nameof(includeFacts));
        ExcludeFacts = ValidateFacts(excludeFacts, nameof(excludeFacts));
        TagFacts = ValidateFacts(tagFacts, nameof(tagFacts));
        HeadingFacts = ValidateFacts(headingFacts, nameof(headingFacts));
        RequireFacts = ValidateFacts(requireFacts, nameof(requireFacts));
        WithinFacts = ValidateFacts(withinFacts, nameof(withinFacts));
        ContentFacts = ValidateFacts(contentFacts, nameof(contentFacts));
        if (suppliedView is { } requestedView && !Enum.IsDefined(requestedView)
            || !Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The Find view is not defined.");
        }

        RequireValue = requireValue;
        WithinValue = withinValue;
        ContentValue = contentValue;
        RequireSpelling = requireSpelling;
        WithinSpelling = withinSpelling;
        ContentSpelling = contentSpelling;
        SuppliedView = suppliedView;
        EffectiveView = effectiveView;
    }

    internal IReadOnlyList<string> IncludeValues { get; }

    internal IReadOnlyList<string> ExcludeValues { get; }

    internal IReadOnlyList<string> TagValues { get; }

    internal IReadOnlyList<string> HeadingValues { get; }

    internal IReadOnlyList<FindPredicateOccurrence> PredicateOccurrences { get; }

    internal CliOptionResultFacts IncludeFacts { get; }

    internal CliOptionResultFacts ExcludeFacts { get; }

    internal CliOptionResultFacts TagFacts { get; }

    internal CliOptionResultFacts HeadingFacts { get; }

    internal CliOptionResultFacts RequireFacts { get; }

    internal CliOptionResultFacts WithinFacts { get; }

    internal CliOptionResultFacts ContentFacts { get; }

    internal string? RequireValue { get; }

    internal string? WithinValue { get; }

    internal string? ContentValue { get; }

    internal string? RequireSpelling { get; }

    internal string? WithinSpelling { get; }

    internal string? ContentSpelling { get; }

    internal CliView? SuppliedView { get; }

    internal CliView EffectiveView { get; }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Find query input values cannot contain null members.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static CliOptionResultFacts ValidateFacts(CliOptionResultFacts facts, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(facts, parameterName);
        return facts;
    }
}
