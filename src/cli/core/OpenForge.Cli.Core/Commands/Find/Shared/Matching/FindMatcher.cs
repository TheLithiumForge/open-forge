using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindMatcher
{
    internal FindMatchingFacts Match(FindMatchingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var findings = new List<FindFinding>();
        var sourceMatches = new List<SourceMatch>();
        var coverage = FindCoverageState.Complete;
        var representedSources = 0;

        var sourceGroups = input.Inspections
            .GroupBy(inspection => inspection.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .OrderBy(group => group.First().Source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(group => group.Key, StringComparer.Ordinal);

        foreach (var sourceGroup in sourceGroups)
        {
            var source = sourceGroup.First().Source;
            representedSources++;
            var layerFacts = ReadLayerFacts(source, sourceGroup, findings, ref coverage);
            var result = MatchSource(input.Request, layerFacts);
            findings.AddRange(result.Findings);
            coverage = MergeCoverage(coverage, result.Coverage);

            if (result.Match)
            {
                sourceMatches.Add(new SourceMatch(
                    source,
                    ReadBaseDescription(source, layerFacts),
                    result.Evidence));
            }
        }

        if (input.Universe.CandidateCount is { } candidateCount
            && candidateCount != representedSources)
        {
            coverage = MergeCoverage(coverage, FindCoverageState.Incomplete);
        }

        var matches = sourceMatches
            .OrderBy(match => match.Source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(match => match.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select((match, index) => new FindMatch(
                index + 1,
                match.Source.Identity.AutomaticId,
                match.Source.Identity.CanonicalBasePath,
                match.Description,
                match.Evidence,
                []))
            .ToArray();

        return new FindMatchingFacts(matches, OrderFindings(findings), coverage);
    }

    private static IReadOnlyList<LayerFacts> ReadLayerFacts(
        SourceLogicalSource source,
        IEnumerable<FindLayerInspectionFacts> sourceInspections,
        ICollection<FindFinding> findings,
        ref FindCoverageState coverage)
    {
        var inspections = sourceInspections
            .OrderBy(inspection => ReadLayerRank(inspection.Layer.Kind))
            .ThenBy(inspection => inspection.Layer.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var layers = new List<LayerFacts>(source.Overwrite is null ? 1 : 2);

        ReadLayerFacts(source, source.Base, inspections, layers, findings, ref coverage);
        if (source.Overwrite is { } overwrite)
        {
            ReadLayerFacts(source, overwrite, inspections, layers, findings, ref coverage);
        }

        return layers;
    }

    private static void ReadLayerFacts(
        SourceLogicalSource source,
        SourceLayer layer,
        IReadOnlyList<FindLayerInspectionFacts> inspections,
        ICollection<LayerFacts> layers,
        ICollection<FindFinding> findings,
        ref FindCoverageState coverage)
    {
        var inspection = inspections.FirstOrDefault(value =>
            value.Layer.Kind == layer.Kind
            && string.Equals(value.Layer.CanonicalPath, layer.CanonicalPath, StringComparison.Ordinal));

        if (inspection is null)
        {
            var unavailable = CreateFinding(
                FindFindingCode.InspectionUnavailable,
                source,
                layer,
                null,
                "The source layer could not be inspected.");
            AddFinding(findings, unavailable);
            coverage = MergeCoverage(coverage, FindCoverageState.Incomplete);
        }
        else
        {
            foreach (var finding in inspection.Findings)
            {
                findings.Add(finding);
                coverage = MergeCoverage(coverage, ReadCoverage(finding));
            }

            if (inspection.Document?.BodySpan is null)
            {
                coverage = MergeCoverage(coverage, FindCoverageState.Incomplete);
                if (inspection.Findings.Count == 0)
                {
                    var unavailable = CreateFinding(
                        FindFindingCode.InspectionUnavailable,
                        source,
                        layer,
                        null,
                        "The source layer could not be inspected.");
                    AddFinding(findings, unavailable);
                }
            }
        }

        layers.Add(new LayerFacts(source, layer, inspection));
    }

    private static string? ReadBaseDescription(
        SourceLogicalSource source,
        IReadOnlyList<LayerFacts> layers)
        => layers.FirstOrDefault(layer => ReferenceEquals(layer.Layer, source.Base))?.Inspection?.Description;

    private static SourceMatchResult MatchSource(
        FindRequest request,
        IReadOnlyList<LayerFacts> layers)
    {
        var predicates = request.Query.EffectivePredicates;
        if (predicates.Count == 0)
        {
            var hasUnavailableLayer = layers.Any(layer => layer.Inspection?.Document?.BodySpan is null);
            return new SourceMatchResult(
                true,
                [],
                hasUnavailableLayer ? FindCoverageState.Incomplete : FindCoverageState.Complete,
                []);
        }

        var evidence = Enumerable.Range(0, predicates.Count)
            .Select(_ => new List<EvidenceCandidate>())
            .ToArray();
        var unknown = new bool[predicates.Count];
        var findings = new List<FindFinding>();
        foreach (var layer in layers)
        {
            if (layer.Inspection?.Document?.BodySpan is null)
            {
                Array.Fill(unknown, true);
                continue;
            }

            for (var predicateIndex = 0; predicateIndex < predicates.Count; predicateIndex++)
            {
                var predicate = predicates[predicateIndex];
                var layerResult = MatchPredicate(
                    predicate,
                    predicateIndex + 1,
                    request.Query.Within,
                    layer,
                    findings);
                unknown[predicateIndex] |= layerResult.Unknown;
                evidence[predicateIndex].AddRange(layerResult.Evidence);
            }
        }

        var predicateMatches = evidence.Select(values => values.Count > 0).ToArray();
        var matches = request.Query.Requirement == FindRequirement.All
            ? predicateMatches.All(value => value)
            : predicateMatches.Any(value => value);
        var coverage = unknown.Any(value => value)
            ? FindCoverageState.Incomplete
            : FindCoverageState.Complete;
        var orderedEvidence = evidence
            .SelectMany(values => values)
            .OrderBy(value => value.Predicate)
            .ThenBy(value => ReadRegionRank(value.Region))
            .ThenBy(value => ReadLayerRank(value.Layer.Kind))
            .ThenBy(value => value.Location.ByteOffset)
            .ThenBy(value => value.Occurrence)
            .ThenBy(value => value.Region.CanonicalValue, StringComparer.Ordinal)
            .Select(value => value.Create())
            .ToArray();

        return new SourceMatchResult(matches, orderedEvidence, coverage, findings);
    }

    private static PredicateMatchResult MatchPredicate(
        FindPredicate predicate,
        int predicateIndex,
        FindRegionSelection regions,
        LayerFacts layer,
        ICollection<FindFinding> findings)
    {
        var inspection = layer.Inspection
            ?? throw new InvalidOperationException("A Find predicate requires an established layer inspection.");
        var document = inspection.Document
            ?? throw new InvalidOperationException("A Find predicate requires an established Markdown document.");
        var evidence = new List<EvidenceCandidate>();
        var unknown = false;

        foreach (var region in ReadPredicateRegions(predicate.Kind, regions))
        {
            switch (region.Kind)
            {
                case FindRegionKind.Document:
                    if (predicate.Kind == FindPredicateKind.Tag)
                    {
                        MatchFrontmatter(
                            predicate,
                            predicateIndex,
                            layer,
                            new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                            evidence,
                            findings,
                            ref unknown);
                        MatchBodyTags(
                            predicate,
                            predicateIndex,
                            layer,
                            document,
                            new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                            null,
                            evidence,
                            findings,
                            ref unknown);
                    }
                    else
                    {
                        MatchHeadings(
                            predicate,
                            predicateIndex,
                            layer,
                            document,
                            new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                            null,
                            evidence,
                            ref unknown);
                    }

                    break;
                case FindRegionKind.Frontmatter:
                    MatchFrontmatter(predicate, predicateIndex, layer, region, evidence, findings, ref unknown);
                    break;
                case FindRegionKind.Body:
                    if (predicate.Kind == FindPredicateKind.Tag)
                    {
                        MatchBodyTags(predicate, predicateIndex, layer, document, region, null, evidence, findings, ref unknown);
                    }
                    else
                    {
                        MatchHeadings(predicate, predicateIndex, layer, document, region, null, evidence, ref unknown);
                    }

                    break;
                case FindRegionKind.Section:
                    MatchSection(
                        predicate,
                        predicateIndex,
                        layer,
                        document,
                        region,
                        evidence,
                        findings,
                        ref unknown);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined.");
            }
        }

        return new PredicateMatchResult(evidence, unknown);
    }

    private static IEnumerable<FindRegion> ReadPredicateRegions(
        FindPredicateKind kind,
        FindRegionSelection regions)
        => kind == FindPredicateKind.Tag ? regions.Tag : regions.Heading;

    private static void MatchFrontmatter(
        FindPredicate predicate,
        int predicateIndex,
        LayerFacts layer,
        FindRegion region,
        ICollection<EvidenceCandidate> evidence,
        ICollection<FindFinding> findings,
        ref bool unknown)
    {
        var frontmatter = layer.Inspection?.Frontmatter;
        if (frontmatter is null || frontmatter.Availability == FindFrontmatterAvailability.Unavailable)
        {
            unknown = true;
            AddFinding(
                findings,
                CreateFinding(
                    FindFindingCode.FrontmatterUnavailable,
                    layer.Source,
                    layer.Layer,
                    region,
                    "The semantic frontmatter facts are unavailable for the selected region."));
            return;
        }

        var occurrence = 0;
        foreach (var tag in frontmatter.Tags
                     .OrderBy(value => value.Location.ByteOffset)
                     .ThenBy(value => value.Authored, StringComparer.Ordinal))
        {
            if (!string.Equals(predicate.ComparisonValue, tag.Authored, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new EvidenceCandidate(
                predicateIndex,
                predicate.Kind,
                predicate.SuppliedValue,
                tag.Authored,
                region,
                layer.Layer,
                tag.Location,
                null,
                occurrence));
        }
    }

    private static void MatchBodyTags(
        FindPredicate predicate,
        int predicateIndex,
        LayerFacts layer,
        MarkdownDocumentFacts document,
        FindRegion region,
        MarkdownTextSpan? sectionSpan,
        ICollection<EvidenceCandidate> evidence,
        ICollection<FindFinding> findings,
        ref bool unknown)
    {
        if (document.BodySpan is null)
        {
            unknown = true;
            AddFinding(
                findings,
                CreateFinding(
                    FindFindingCode.InspectionUnavailable,
                    layer.Source,
                    layer.Layer,
                    region,
                    "The Markdown body boundary is unavailable for the selected region."));
            return;
        }

        var bodyTags = layer.Inspection?.BodyTags;
        if (bodyTags is null || bodyTags.Availability == FindBodyTagAvailability.Unavailable)
        {
            unknown = true;
            AddFinding(
                findings,
                CreateFinding(
                    FindFindingCode.InspectionUnavailable,
                    layer.Source,
                    layer.Layer,
                    region,
                    "The visible body-tag facts are unavailable for the selected region."));
            return;
        }

        var occurrence = 0;
        foreach (var tag in bodyTags.Occurrences
                     .OrderBy(value => value.Location.ByteOffset)
                     .ThenBy(value => value.Authored, StringComparer.Ordinal))
        {
            if (!string.Equals(predicate.ComparisonValue, tag.Authored, StringComparison.OrdinalIgnoreCase)
                || !IsByteSpanWithin(document.Source, document.BodySpan, tag.Location)
                || sectionSpan is not null
                    && !IsByteSpanWithin(document.Source, sectionSpan, tag.Location))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new EvidenceCandidate(
                predicateIndex,
                predicate.Kind,
                predicate.SuppliedValue,
                tag.Authored,
                region,
                layer.Layer,
                tag.Location,
                null,
                occurrence));
        }
    }

    private static void MatchHeadings(
        FindPredicate predicate,
        int predicateIndex,
        LayerFacts layer,
        MarkdownDocumentFacts document,
        FindRegion region,
        MarkdownTextSpan? sectionSpan,
        ICollection<EvidenceCandidate> evidence,
        ref bool unknown)
    {
        if (document.BodySpan is null)
        {
            unknown = true;
            return;
        }

        var occurrence = 0;
        foreach (var heading in document.Headings)
        {
            if (sectionSpan is not null
                && (heading.Span.Start < sectionSpan.Start || heading.Span.End > sectionSpan.End))
            {
                continue;
            }

            if (!string.Equals(predicate.ComparisonValue, heading.VisibleText, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            occurrence++;
            evidence.Add(new EvidenceCandidate(
                predicateIndex,
                predicate.Kind,
                predicate.SuppliedValue,
                heading.VisibleText,
                region,
                layer.Layer,
                MapLocation(document.Source, heading.Span),
                new FindHeadingEvidence(heading.Level, heading.Form, heading.IsCanonical),
                occurrence));
        }
    }

    private static void MatchSection(
        FindPredicate predicate,
        int predicateIndex,
        LayerFacts layer,
        MarkdownDocumentFacts document,
        FindRegion region,
        ICollection<EvidenceCandidate> evidence,
        ICollection<FindFinding> findings,
        ref bool unknown)
    {
        if (document.BodySpan is null)
        {
            unknown = true;
            AddFinding(
                findings,
                CreateFinding(
                    FindFindingCode.InspectionUnavailable,
                    layer.Source,
                    layer.Layer,
                    region,
                    "The Markdown body boundary is unavailable for the requested section."));
            return;
        }

        var sections = document.Sections
            .Where(section => string.Equals(
                section.Heading.VisibleText,
                region.Name,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (sections.Length > 1)
        {
            unknown = true;
            AddFinding(
                findings,
                CreateFinding(
                    FindFindingCode.SectionAmbiguous,
                    layer.Source,
                    layer.Layer,
                    region,
                    "The requested section has more than one matching heading in the source layer."));
            return;
        }

        if (sections.Length == 0)
        {
            return;
        }

        if (predicate.Kind == FindPredicateKind.Tag)
        {
            MatchBodyTags(
                predicate,
                predicateIndex,
                layer,
                document,
                region,
                sections[0].Span,
                evidence,
                findings,
                ref unknown);
        }
        else
        {
            MatchHeadings(
                predicate,
                predicateIndex,
                layer,
                document,
                region,
                sections[0].Span,
                evidence,
                ref unknown);
        }
    }

    private static bool IsByteSpanWithin(
        string source,
        MarkdownTextSpan container,
        FindSourceLocation location)
    {
        var containerStart = Encoding.UTF8.GetByteCount(source.AsSpan(0, container.Start));
        var containerEnd = Encoding.UTF8.GetByteCount(source.AsSpan(0, container.End));
        var locationEnd = checked(location.ByteOffset + location.ByteLength);
        return location.ByteOffset >= containerStart && locationEnd <= containerEnd;
    }

    private static FindSourceLocation MapLocation(string source, MarkdownTextSpan span)
    {
        var line = 1;
        var column = 1;
        for (var index = 0; index < span.Start;)
        {
            if (source[index] == '\r')
            {
                index += index + 1 < source.Length && source[index + 1] == '\n' ? 2 : 1;
                line++;
                column = 1;
                continue;
            }

            if (source[index] == '\n')
            {
                index++;
                line++;
                column = 1;
                continue;
            }

            index += char.IsHighSurrogate(source[index]) ? 2 : 1;
            column++;
        }

        return new FindSourceLocation(
            line,
            column,
            Encoding.UTF8.GetByteCount(source.AsSpan(0, span.Start)),
            Encoding.UTF8.GetByteCount(source.AsSpan(span.Start, span.Length)));
    }

    private static FindFinding CreateFinding(
        FindFindingCode code,
        SourceLogicalSource source,
        SourceLayer layer,
        FindRegion? region,
        string cause)
        => new(
            code,
            FindDefinitions.ReadFindingStatus(code),
            region?.Name,
            cause,
            null,
            null,
            new FindSourceIdentity(source.Identity.AutomaticId, source.Identity.CanonicalBasePath),
            layer.Kind,
            layer.CanonicalPath,
            region,
            null,
            []);

    private static void AddFinding(ICollection<FindFinding> findings, FindFinding finding)
    {
        if (findings.Any(existing =>
                existing.Code == finding.Code
                && existing.Layer == finding.Layer
                && string.Equals(existing.Path, finding.Path, StringComparison.Ordinal)
                && string.Equals(existing.Region?.CanonicalValue, finding.Region?.CanonicalValue, StringComparison.Ordinal)
                && string.Equals(existing.Source?.Id, finding.Source?.Id, StringComparison.Ordinal)))
        {
            return;
        }

        findings.Add(finding);
    }

    private static IReadOnlyList<FindFinding> OrderFindings(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => (int)finding.Code)
            .ThenBy(finding => finding.SelectorRole is FindSelectorRole.Include ? 0 : 1)
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadNullableLayerRank(finding.Layer))
            .ThenBy(finding => ReadNullableRegionRank(finding.Region))
            .ThenBy(finding => finding.Region?.CanonicalValue, StringComparer.Ordinal)
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

    private static FindCoverageState ReadCoverage(FindFinding finding)
        => finding.Code switch
        {
            FindFindingCode.OperationFailed => FindCoverageState.Failed,
            FindFindingCode.Interrupted => FindCoverageState.Interrupted,
            FindFindingCode.WorkspaceUnavailable
                or FindFindingCode.WorkspaceUnsafe
                or FindFindingCode.SelectorAmbiguous
                or FindFindingCode.SelectorUnsafe => FindCoverageState.Blocked,
            FindFindingCode.CandidateUnsafe
                or FindFindingCode.LayerUnresolved
                or FindFindingCode.InspectionUnavailable
                or FindFindingCode.InvalidEncoding
                or FindFindingCode.FrontmatterUnavailable
                or FindFindingCode.SectionAmbiguous
                or FindFindingCode.ProjectionUnavailable => FindCoverageState.Incomplete,
            _ => FindCoverageState.Complete,
        };

    private static FindCoverageState MergeCoverage(
        FindCoverageState current,
        FindCoverageState candidate)
        => ReadCoverageRank(candidate) > ReadCoverageRank(current) ? candidate : current;

    private static int ReadCoverageRank(FindCoverageState state)
        => state switch
        {
            FindCoverageState.Complete => 0,
            FindCoverageState.Incomplete => 1,
            FindCoverageState.Blocked => 2,
            FindCoverageState.Failed => 3,
            FindCoverageState.Interrupted => 4,
            FindCoverageState.NotStarted => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined."),
        };

    private static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body => 1,
            FindRegionKind.Section => 2,
            FindRegionKind.Document => throw new ArgumentException("A Find evidence region cannot be document.", nameof(region)),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined."),
        };

    private static int ReadNullableRegionRank(FindRegion? region)
        => region is null ? int.MaxValue : ReadRegionRank(region);

    private static int ReadLayerRank(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static int ReadNullableLayerRank(SourceLayerKind? layer)
        => layer is { } value ? ReadLayerRank(value) : int.MaxValue;

    private sealed record LayerFacts(
        SourceLogicalSource Source,
        SourceLayer Layer,
        FindLayerInspectionFacts? Inspection);

    private sealed record SourceMatch(
        SourceLogicalSource Source,
        string? Description,
        IReadOnlyList<FindEvidence> Evidence);

    private sealed record SourceMatchResult(
        bool Match,
        IReadOnlyList<FindEvidence> Evidence,
        FindCoverageState Coverage,
        IReadOnlyList<FindFinding> Findings);

    private sealed record PredicateMatchResult(
        IReadOnlyList<EvidenceCandidate> Evidence,
        bool Unknown);

    private sealed record EvidenceCandidate(
        int Predicate,
        FindPredicateKind Kind,
        string Query,
        string Authored,
        FindRegion Region,
        SourceLayer Layer,
        FindSourceLocation Location,
        FindHeadingEvidence? Heading,
        int Occurrence)
    {
        internal FindEvidence Create()
            => new(
                Predicate,
                Kind,
                Query,
                Authored,
                Region,
                Layer.Kind,
                Layer.CanonicalPath,
                Location,
                Occurrence,
                Heading);
    }
}
