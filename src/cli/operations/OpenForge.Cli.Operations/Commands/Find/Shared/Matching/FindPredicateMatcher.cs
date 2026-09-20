using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindPredicateMatcher
{
    internal FindPredicateMatchFacts Match(FindPredicateMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inspection = input.Layer.Inspection
            ?? throw new InvalidOperationException("A Find predicate requires an established layer inspection.");
        var document = inspection.Document
            ?? throw new InvalidOperationException("A Find predicate requires an established Markdown document.");
        var evidence = new List<FindEvidenceCandidate>();
        var findings = new List<FindFinding>();
        var unknown = false;

        foreach (var region in ReadPredicateRegions(input))
        {
            switch (region.Kind)
            {
                case FindRegionKind.Document:
                    if (input.Predicate.Kind == FindPredicateKind.Tag)
                    {
                        Merge(MatchFrontmatter(new FindRegionalMatchInput(
                            input.Predicate,
                            input.PredicateIndex,
                            input.Layer,
                            document,
                            new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                            null)));
                        Merge(MatchBodyTags(new FindRegionalMatchInput(
                            input.Predicate,
                            input.PredicateIndex,
                            input.Layer,
                            document,
                            new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                            null)));
                    }
                    else
                    {
                        Merge(MatchHeadings(new FindRegionalMatchInput(
                            input.Predicate,
                            input.PredicateIndex,
                            input.Layer,
                            document,
                            new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                            null)));
                    }

                    break;
                case FindRegionKind.Frontmatter:
                    Merge(MatchFrontmatter(new FindRegionalMatchInput(
                        input.Predicate,
                        input.PredicateIndex,
                        input.Layer,
                        document,
                        region,
                        null)));
                    break;
                case FindRegionKind.Body:
                    Merge(input.Predicate.Kind == FindPredicateKind.Tag
                        ? MatchBodyTags(new FindRegionalMatchInput(
                            input.Predicate,
                            input.PredicateIndex,
                            input.Layer,
                            document,
                            region,
                            null))
                        : MatchHeadings(new FindRegionalMatchInput(
                            input.Predicate,
                            input.PredicateIndex,
                            input.Layer,
                            document,
                            region,
                            null)));
                    break;
                case FindRegionKind.Section:
                    Merge(MatchSection(new FindRegionalMatchInput(
                        input.Predicate,
                        input.PredicateIndex,
                        input.Layer,
                        document,
                        region,
                        null)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(region),
                        region.Kind,
                        "The Find region kind is not defined.");
            }
        }

        return new FindPredicateMatchFacts(evidence, findings, unknown);

        void Merge(FindPredicateMatchFacts result)
        {
            evidence.AddRange(result.Evidence);
            foreach (var finding in result.Findings)
            {
                FindMatchingFindingPolicy.Add(findings, finding);
            }

            unknown |= result.Unknown;
        }
    }

    private static IEnumerable<FindRegion> ReadPredicateRegions(FindPredicateMatchInput input)
        => input.Predicate.Kind == FindPredicateKind.Tag
            ? input.Regions.Tag
            : input.Regions.Heading;

    private static FindPredicateMatchFacts MatchFrontmatter(FindRegionalMatchInput input)
    {
        var frontmatter = input.Layer.Inspection?.Frontmatter;
        if (frontmatter is null
            || frontmatter.Availability == FindFrontmatterAvailability.Unavailable)
        {
            var finding = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                FindFindingCode.FrontmatterUnavailable,
                input.Layer.Source,
                input.Layer.Layer,
                input.Region,
                "The semantic frontmatter facts are unavailable for the selected region."));
            return new FindPredicateMatchFacts([], [finding], true);
        }

        var evidence = new List<FindEvidenceCandidate>();
        var occurrence = 0;
        foreach (var tag in frontmatter.Tags
                     .OrderBy(value => value.Location.ByteOffset)
                     .ThenBy(value => value.Authored, StringComparer.Ordinal))
        {
            if (!string.Equals(
                    input.Predicate.ComparisonValue,
                    tag.Authored,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new FindEvidenceCandidate(
                input.PredicateIndex,
                input.Predicate.Kind,
                input.Predicate.SuppliedValue,
                tag.Authored,
                input.Region,
                input.Layer.Layer,
                tag.Location,
                null,
                occurrence));
        }

        return new FindPredicateMatchFacts(evidence, [], false);
    }

    private static FindPredicateMatchFacts MatchBodyTags(FindRegionalMatchInput input)
    {
        if (input.Document.BodySpan is null)
        {
            var finding = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                FindFindingCode.InspectionUnavailable,
                input.Layer.Source,
                input.Layer.Layer,
                input.Region,
                "The Markdown body boundary is unavailable for the selected region."));
            return new FindPredicateMatchFacts([], [finding], true);
        }

        var bodyTags = input.Layer.Inspection?.BodyTags;
        if (bodyTags is null || bodyTags.Availability == FindBodyTagAvailability.Unavailable)
        {
            var finding = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                FindFindingCode.InspectionUnavailable,
                input.Layer.Source,
                input.Layer.Layer,
                input.Region,
                "The visible body-tag facts are unavailable for the selected region."));
            return new FindPredicateMatchFacts([], [finding], true);
        }

        var evidence = new List<FindEvidenceCandidate>();
        var occurrence = 0;
        foreach (var tag in bodyTags.Occurrences
                     .OrderBy(value => value.Location.ByteOffset)
                     .ThenBy(value => value.Authored, StringComparer.Ordinal))
        {
            if (!string.Equals(
                    input.Predicate.ComparisonValue,
                    tag.Authored,
                    StringComparison.OrdinalIgnoreCase)
                || !IsByteSpanWithin(input.Document.Source, input.Document.BodySpan, tag.Location)
                || input.SectionSpan is not null
                    && !IsByteSpanWithin(input.Document.Source, input.SectionSpan, tag.Location))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new FindEvidenceCandidate(
                input.PredicateIndex,
                input.Predicate.Kind,
                input.Predicate.SuppliedValue,
                tag.Authored,
                input.Region,
                input.Layer.Layer,
                tag.Location,
                null,
                occurrence));
        }

        return new FindPredicateMatchFacts(evidence, [], false);
    }

    private static FindPredicateMatchFacts MatchHeadings(FindRegionalMatchInput input)
    {
        if (input.Document.BodySpan is null)
        {
            return new FindPredicateMatchFacts([], [], true);
        }

        var evidence = new List<FindEvidenceCandidate>();
        var occurrence = 0;
        foreach (var heading in input.Document.Headings)
        {
            if (input.SectionSpan is not null
                && (heading.Span.Start < input.SectionSpan.Start
                    || heading.Span.End > input.SectionSpan.End))
            {
                continue;
            }

            if (!string.Equals(
                    input.Predicate.ComparisonValue,
                    heading.VisibleText,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new FindEvidenceCandidate(
                input.PredicateIndex,
                input.Predicate.Kind,
                input.Predicate.SuppliedValue,
                heading.VisibleText,
                input.Region,
                input.Layer.Layer,
                MapLocation(input.Document.Source, heading.Span),
                new FindHeadingEvidence(
                    heading.Level,
                    heading.Form,
                    heading.IsCanonical),
                occurrence));
        }

        return new FindPredicateMatchFacts(evidence, [], false);
    }

    private static FindPredicateMatchFacts MatchSection(FindRegionalMatchInput input)
    {
        if (input.Document.BodySpan is null)
        {
            var finding = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                FindFindingCode.InspectionUnavailable,
                input.Layer.Source,
                input.Layer.Layer,
                input.Region,
                "The Markdown body boundary is unavailable for the requested section."));
            return new FindPredicateMatchFacts([], [finding], true);
        }

        var sections = input.Document.Sections
            .Where(section => string.Equals(
                section.Heading.VisibleText,
                input.Region.Name,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (sections.Length > 1)
        {
            var finding = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                FindFindingCode.SectionAmbiguous,
                input.Layer.Source,
                input.Layer.Layer,
                input.Region,
                "The requested section has more than one matching heading in the source layer."));
            return new FindPredicateMatchFacts([], [finding], true);
        }

        if (sections.Length == 0)
        {
            return new FindPredicateMatchFacts([], [], false);
        }

        var sectionInput = input with { SectionSpan = sections[0].Span };
        return input.Predicate.Kind == FindPredicateKind.Tag
            ? MatchBodyTags(sectionInput)
            : MatchHeadings(sectionInput);
    }

    private static bool IsByteSpanWithin(
        string source,
        MarkdownTextSpan container,
        SourceLocation location)
    {
        var containerStart = Encoding.UTF8.GetByteCount(source.AsSpan(0, container.Start));
        var containerEnd = Encoding.UTF8.GetByteCount(source.AsSpan(0, container.End));
        var locationEnd = checked(location.ByteOffset + location.ByteLength);
        return location.ByteOffset >= containerStart && locationEnd <= containerEnd;
    }

    private static SourceLocation MapLocation(string source, MarkdownTextSpan span)
        => new Utf8SourceMap(source).Map(span.Start, span.Length);
}
