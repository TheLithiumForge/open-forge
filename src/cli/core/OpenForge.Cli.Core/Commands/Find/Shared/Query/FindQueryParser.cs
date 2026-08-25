using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Shared.Tags;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Query;

internal sealed class FindQueryParser
{
    internal FindQuery Parse(FindQueryInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateOptionFacts(input);

        var occurrences = ReadPredicateOccurrences(input);
        var predicates = occurrences
            .Select(occurrence => CreatePredicate(occurrence.Kind, occurrence.Value))
            .ToArray();
        var requirement = ReadRequirement(input.RequireValue ?? input.RequireSpelling, predicates.Length);
        var within = ReadRegions(input.WithinValue ?? input.WithinSpelling, predicates);

        _ = ParseContentSelection(input.ContentValue ?? input.ContentSpelling);
        return new FindQuery(predicates, ReadEffectivePredicates(predicates), requirement, within);
    }

    internal FindContentSelection ParseContentSelection(string? value)
    {
        if (value is null)
        {
            return new FindContentSelection([], []);
        }

        var supplied = ParseParts<FindContentPart>(value, ReadContentPart);
        var effective = supplied
            .DistinctBy(part => part.CanonicalValue, StringComparer.OrdinalIgnoreCase)
            .OrderBy(ReadContentRank)
            .ToArray();
        return new FindContentSelection(supplied, effective);
    }

    internal FindQuery Recover(FindQueryInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var occurrences = ReadRecoverableOccurrences(input);
        var predicates = occurrences
            .Select(occurrence => CreatePredicate(occurrence.Kind, occurrence.Value))
            .ToArray();
        var effective = ReadEffectivePredicates(predicates);
        var requirement = string.Equals(
                input.RequireValue ?? input.RequireSpelling,
                FindDefinitions.Any,
                StringComparison.Ordinal)
            ? FindRequirement.Any
            : FindRequirement.All;
        FindRegionSelection within;
        try
        {
            within = ReadRegions(input.WithinValue ?? input.WithinSpelling, predicates);
        }
        catch (ArgumentException)
        {
            within = DefaultRegions();
        }

        return new FindQuery(predicates, effective, requirement, within);
    }

    private static void ValidateOptionFacts(FindQueryInput input)
    {
        ValidateRepeatableFacts(input.IncludeFacts, input.IncludeValues, FindDefinitions.Include.Name);
        ValidateRepeatableFacts(input.ExcludeFacts, input.ExcludeValues, FindDefinitions.Exclude.Name);
        ValidateRepeatableFacts(input.TagFacts, input.TagValues, FindDefinitions.Tag.Name);
        ValidateRepeatableFacts(input.HeadingFacts, input.HeadingValues, FindDefinitions.Heading.Name);
        ValidateSingletonFacts(input.RequireFacts, input.RequireValue, FindDefinitions.Require.Name);
        ValidateSingletonFacts(input.WithinFacts, input.WithinValue, FindDefinitions.Within.Name);
        ValidateSingletonFacts(input.ContentFacts, input.ContentValue, FindDefinitions.Content.Name);
    }

    private static void ValidateRepeatableFacts(
        CliOptionResultFacts facts,
        IReadOnlyList<string> values,
        string optionName)
    {
        if (facts.IsExplicit != (facts.IdentifierCount != 0)
            || facts.ValueCount != values.Count
            || facts.IdentifierCount != facts.ValueCount
            || values.Any(value => value.Length == 0))
        {
            throw new ArgumentException(
                $"{optionName} requires one non-empty value for every option occurrence.",
                optionName);
        }
    }

    private static void ValidateSingletonFacts(
        CliOptionResultFacts facts,
        string? value,
        string optionName)
    {
        if (!facts.IsExplicit)
        {
            if (facts.IdentifierCount != 0 || facts.ValueCount != 0 || value is not null)
            {
                throw new ArgumentException($"{optionName} has inconsistent parser facts.", optionName);
            }

            return;
        }

        if (facts.IdentifierCount != 1 || facts.ValueCount != 1 || value is null)
        {
            throw new ArgumentException($"{optionName} requires exactly one value.", optionName);
        }
    }

    private static IReadOnlyList<FindPredicateOccurrence> ReadPredicateOccurrences(FindQueryInput input)
    {
        var occurrences = input.PredicateOccurrences
            .OrderBy(occurrence => occurrence.Position)
            .ToArray();
        if (occurrences.Select((occurrence, index) => occurrence.Position == index + 1).Any(value => !value))
        {
            throw new ArgumentException("Find predicate occurrences must use contiguous command-line positions.", nameof(input));
        }

        var tags = occurrences
            .Where(occurrence => occurrence.Kind == FindPredicateKind.Tag)
            .Select(occurrence => occurrence.Value)
            .ToArray();
        var headings = occurrences
            .Where(occurrence => occurrence.Kind == FindPredicateKind.Heading)
            .Select(occurrence => occurrence.Value)
            .ToArray();
        if (!tags.SequenceEqual(input.TagValues, StringComparer.Ordinal)
            || !headings.SequenceEqual(input.HeadingValues, StringComparer.Ordinal))
        {
            throw new ArgumentException("Find predicate occurrences must agree with typed option values.", nameof(input));
        }

        return occurrences;
    }

    private static IReadOnlyList<FindPredicateOccurrence> ReadRecoverableOccurrences(FindQueryInput input)
    {
        if (input.PredicateOccurrences.Count != 0)
        {
            return input.PredicateOccurrences
                .OrderBy(occurrence => occurrence.Position)
                .ToArray();
        }

        var values = input.TagValues
            .Select(value => (Kind: FindPredicateKind.Tag, Value: value))
            .Concat(input.HeadingValues.Select(value => (Kind: FindPredicateKind.Heading, Value: value)))
            .Select((value, index) => new FindPredicateOccurrence(value.Kind, value.Value, index + 1))
            .ToArray();
        return values;
    }

    private static FindPredicate CreatePredicate(FindPredicateKind kind, string suppliedValue)
    {
        var comparisonValue = kind == FindPredicateKind.Tag
            && suppliedValue.StartsWith('#')
            ? suppliedValue[1..]
            : suppliedValue;
        if (kind == FindPredicateKind.Tag && !FindTagGrammar.IsValid(comparisonValue))
        {
            throw new ArgumentException(
                "A Find tag must start with a Unicode letter and contain only letters, numbers, and single internal ASCII hyphens.",
                nameof(suppliedValue));
        }

        if (kind == FindPredicateKind.Heading && suppliedValue.Length == 0)
        {
            throw new ArgumentException("A Find heading cannot be empty.", nameof(suppliedValue));
        }

        return new FindPredicate(kind, suppliedValue, comparisonValue);
    }

    private static IReadOnlyList<FindPredicate> ReadEffectivePredicates(
        IReadOnlyList<FindPredicate> predicates)
    {
        var effective = new List<FindPredicate>();
        foreach (var predicate in predicates)
        {
            if (!effective.Any(value => AreEquivalent(value, predicate)))
            {
                effective.Add(predicate);
            }
        }

        return effective;
    }

    private static bool AreEquivalent(FindPredicate left, FindPredicate right)
        => left.Kind == right.Kind
            && string.Equals(left.ComparisonValue, right.ComparisonValue, StringComparison.OrdinalIgnoreCase);

    private static FindRequirement ReadRequirement(string? value, int predicateCount)
    {
        if (value is not null && predicateCount == 0)
        {
            throw new ArgumentException("--require requires at least one tag or heading predicate.", nameof(value));
        }

        return value switch
        {
            null => FindRequirement.All,
            FindDefinitions.All => FindRequirement.All,
            FindDefinitions.Any => FindRequirement.Any,
            _ => throw new ArgumentException("--require accepts only all or any.", nameof(value)),
        };
    }

    private static FindRegionSelection ReadRegions(
        string? value,
        IReadOnlyList<FindPredicate> predicates)
    {
        var hasTag = predicates.Any(predicate => predicate.Kind == FindPredicateKind.Tag);
        var hasHeading = predicates.Any(predicate => predicate.Kind == FindPredicateKind.Heading);
        if (value is null)
        {
            return DefaultRegions();
        }

        if (predicates.Count == 0)
        {
            throw new ArgumentException("--within requires at least one tag or heading predicate.", nameof(value));
        }

        var supplied = ParseParts<FindRegion>(value, ReadRegion).ToArray();
        var tag = hasTag
            ? NormalizeRegions(supplied, allowFrontmatter: true)
            : [Frontmatter()];
        var heading = hasHeading
            ? NormalizeRegions(supplied, allowFrontmatter: false)
            : [Body()];
        return new FindRegionSelection(supplied, tag, heading);
    }

    private static FindRegionSelection DefaultRegions()
    {
        return new FindRegionSelection([], [Frontmatter()], [Body()]);
    }

    private static IReadOnlyList<FindRegion> NormalizeRegions(
        IReadOnlyList<FindRegion> supplied,
        bool allowFrontmatter)
    {
        if (supplied.Any(region => region.Kind == FindRegionKind.Document))
        {
            return [Document()];
        }

        var filtered = supplied
            .Where(region => allowFrontmatter || region.Kind != FindRegionKind.Frontmatter)
            .DistinctBy(region => region.CanonicalValue, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (filtered.Length == 0)
        {
            throw new ArgumentException("The selected Find regions do not apply to the requested predicate.", nameof(supplied));
        }

        if (filtered.Any(region => region.Kind == FindRegionKind.Body))
        {
            filtered = filtered
                .Where(region => region.Kind != FindRegionKind.Section)
                .ToArray();
        }

        return filtered
            .OrderBy(ReadRegionRank)
            .ToArray();
    }

    private static IEnumerable<T> ParseParts<T>(
        string value,
        Func<string, T> parse)
    {
        if (value.Length == 0)
        {
            throw new ArgumentException("A Find list cannot be empty.", nameof(value));
        }

        var parts = new List<string>();
        var part = new System.Text.StringBuilder();
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '\\')
            {
                if (++index == value.Length || value[index] is not (',' or '\\'))
                {
                    throw new ArgumentException("A Find list contains an unsupported or trailing escape.", nameof(value));
                }

                part.Append(value[index]);
                continue;
            }

            if (character == ',')
            {
                AddPart(parts, part, value);
                continue;
            }

            part.Append(character);
        }

        AddPart(parts, part, value);
        return parts.Select(parse).ToArray();
    }

    private static void AddPart(
        ICollection<string> parts,
        System.Text.StringBuilder part,
        string value)
    {
        var trimmed = part.ToString().Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("A Find list cannot contain an empty part.", nameof(value));
        }

        parts.Add(trimmed);
        part.Clear();
    }

    private static FindRegion ReadRegion(string value)
    {
        return value switch
        {
            FindDefinitions.Document => Document(),
            FindDefinitions.Frontmatter => Frontmatter(),
            FindDefinitions.Body => Body(),
            _ when value.StartsWith(FindDefinitions.SectionPrefix, StringComparison.Ordinal)
                => new FindRegion(
                    FindRegionKind.Section,
                    value[FindDefinitions.SectionPrefix.Length..],
                    value),
            _ => throw new ArgumentException($"Unknown Find region '{value}'.", nameof(value)),
        };
    }

    private static FindContentPart ReadContentPart(string value)
    {
        return value switch
        {
            FindDefinitions.Metadata => new FindContentPart(FindContentPartKind.Metadata, null, value),
            FindDefinitions.Frontmatter => new FindContentPart(FindContentPartKind.Frontmatter, null, value),
            FindDefinitions.Headings => new FindContentPart(FindContentPartKind.Headings, null, value),
            FindDefinitions.Body => new FindContentPart(FindContentPartKind.Body, null, value),
            _ when value.StartsWith(FindDefinitions.SectionPrefix, StringComparison.Ordinal)
                => new FindContentPart(
                    FindContentPartKind.Section,
                    value[FindDefinitions.SectionPrefix.Length..],
                    value),
            _ => throw new ArgumentException($"Unknown Find content part '{value}'.", nameof(value)),
        };
    }

    private static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Document => 0,
            FindRegionKind.Frontmatter => 1,
            FindRegionKind.Body => 2,
            FindRegionKind.Section => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined."),
        };

    private static int ReadContentRank(FindContentPart part)
        => part.Kind switch
        {
            FindContentPartKind.Metadata => 0,
            FindContentPartKind.Frontmatter => 1,
            FindContentPartKind.Headings => 2,
            FindContentPartKind.Body => 3,
            FindContentPartKind.Section => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The Find content part kind is not defined."),
        };

    private static FindRegion Document()
        => new(FindRegionKind.Document, null, FindDefinitions.Document);

    private static FindRegion Frontmatter()
        => new(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter);

    private static FindRegion Body()
        => new(FindRegionKind.Body, null, FindDefinitions.Body);
}
