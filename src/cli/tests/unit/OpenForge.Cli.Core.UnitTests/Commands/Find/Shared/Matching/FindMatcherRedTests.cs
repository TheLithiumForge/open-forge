using System.Globalization;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Matching;

public sealed class FindMatcherRedTests
{
    [Theory(DisplayName = "Find all and any requirements aggregate evidence across ordered physical layers"),
        InlineData("all-distributed"),
        InlineData("any-base-only"),
        InlineData("all-missing")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void AllAndAnyAggregateEvidenceAcrossOrderedLayers(string scenario)
    {
        var source = CreateSource(withOverwrite: true);
        var request = CreateRequest(
            [
                new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic"),
                new FindPredicate(FindPredicateKind.Heading, "Finalize", "Finalize"),
            ],
            scenario == "any-base-only" ? FindRequirement.Any : FindRequirement.All,
            NaturalRegions());
        IReadOnlyList<FindLayerInspectionFacts> inspections = scenario switch
        {
            "all-distributed" =>
            [
                Inspection(source, source.Base, ["Topic"], [Heading("Other", 1, 0)], "base"),
                Inspection(source, source.Overwrite!, [], [Heading("Finalize", 1, 0)], "overwrite"),
            ],
            "any-base-only" =>
            [
                Inspection(source, source.Base, ["Topic"], [Heading("Other", 1, 0)], "base"),
                Inspection(source, source.Overwrite!, [], [Heading("Other", 1, 0)], "overwrite"),
            ],
            "all-missing" =>
            [
                Inspection(source, source.Base, [], [Heading("Other", 1, 0)], "base"),
                Inspection(source, source.Overwrite!, [], [Heading("Other", 1, 0)], "overwrite"),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The matcher scenario is not defined."),
        };

        var facts = new FindMatcher().Match(new FindMatchingInput(
            request,
            Universe(source),
            inspections));

        switch (scenario)
        {
            case "all-distributed":
                var match = Assert.Single(facts.Matches);
                Assert.Equal(
                    [FindPredicateKind.Tag, FindPredicateKind.Heading],
                    match.Evidence.Select(evidence => evidence.Kind));
                Assert.Equal(
                    [SourceLayerKind.Base, SourceLayerKind.Overwrite],
                    match.Evidence.Select(evidence => evidence.Layer));
                break;
            case "any-base-only":
                var evidence = Assert.Single(Assert.Single(facts.Matches).Evidence);
                Assert.Equal(FindPredicateKind.Tag, evidence.Kind);
                Assert.Equal(SourceLayerKind.Base, evidence.Layer);
                break;
            case "all-missing":
                Assert.Empty(facts.Matches);
                Assert.Empty(facts.Findings);
                Assert.Equal(FindCoverageState.Complete, facts.Coverage);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The matcher assertion scenario is not defined.");
        }
    }

    [Theory(DisplayName = "Find natural and explicit regions select exact authored tag, body, document, and section evidence"),
        InlineData("natural"),
        InlineData("explicit")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void NaturalExplicitDocumentBodyAndSectionRegionsAreExact(string scenario)
    {
        var source = CreateSource();
        var document = Document(
            new HeadingSpec("Intro", 1, 4, 5, MarkdownHeadingForm.Atx, true));
        var inspections = new[]
        {
            Inspection(
                source,
                source.Base,
                ["Topic"],
                [Heading("Intro", 1, 4)],
                "description",
                document,
                [new FindBodyTagOccurrence("BodyTopic", Location(12, 4))]),
        };

        switch (scenario)
        {
            case "natural":
                AssertSingleEvidence(
                    source,
                    inspections,
                    CreateRequest(
                        [new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic")],
                        FindRequirement.All,
                        NaturalRegions()),
                    FindPredicateKind.Tag,
                    "Topic",
                    FindRegionKind.Frontmatter,
                    null);
                AssertSingleEvidence(
                    source,
                    inspections,
                    CreateRequest(
                        [new FindPredicate(FindPredicateKind.Heading, "Intro", "Intro")],
                        FindRequirement.All,
                        NaturalRegions()),
                    FindPredicateKind.Heading,
                    "Intro",
                    FindRegionKind.Body,
                    null);
                break;
            case "explicit":
                AssertSingleEvidence(
                    source,
                    inspections,
                    CreateRequest(
                        [new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic")],
                        FindRequirement.All,
                        ExplicitRegions(
                            [new FindRegion(FindRegionKind.Document, null, "document")],
                            [new FindRegion(FindRegionKind.Document, null, "document")])),
                    FindPredicateKind.Tag,
                    "Topic",
                    FindRegionKind.Frontmatter,
                    null);
                AssertSingleEvidence(
                    source,
                    inspections,
                    CreateRequest(
                        [new FindPredicate(FindPredicateKind.Tag, "BodyTopic", "BodyTopic")],
                        FindRequirement.All,
                        ExplicitRegions(
                            [new FindRegion(FindRegionKind.Body, null, "body")],
                            [new FindRegion(FindRegionKind.Body, null, "body")])),
                    FindPredicateKind.Tag,
                    "BodyTopic",
                    FindRegionKind.Body,
                    null);
                AssertSingleEvidence(
                    source,
                    inspections,
                    CreateRequest(
                        [new FindPredicate(FindPredicateKind.Heading, "Intro", "Intro")],
                        FindRequirement.All,
                        ExplicitRegions(
                            [new FindRegion(FindRegionKind.Section, "Intro", "section:Intro")],
                            [new FindRegion(FindRegionKind.Section, "Intro", "section:Intro")])),
                    FindPredicateKind.Heading,
                    "Intro",
                    FindRegionKind.Section,
                    "Intro");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The matcher region scenario is not defined.");
        }
    }

    [Theory(DisplayName = "Find missing and ambiguous sections retain only independently safe evidence"),
        InlineData("missing-and-unavailable"),
        InlineData("ambiguous-with-safe-source")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void MissingAndAmbiguousSectionsRetainOnlySafeEvidence(string scenario)
    {
        var predicate = new FindPredicate(FindPredicateKind.Heading, "Needle", "Needle");
        var section = new FindRegion(FindRegionKind.Section, "Intro", "section:Intro");
        var request = CreateRequest(
            [predicate],
            FindRequirement.All,
            ExplicitRegions([section], [section]));

        if (scenario == "missing-and-unavailable")
        {
            var missing = CreateSource("docs/missing.md");
            var missingFacts = new FindMatcher().Match(new FindMatchingInput(
                request,
                Universe(missing),
                [
                    Inspection(
                        missing,
                        missing.Base,
                        [],
                        [Heading("Other", 1, 4)],
                        "missing",
                        Document(new HeadingSpec("Other", 1, 4, 5, MarkdownHeadingForm.Atx, true))),
                ]));
            Assert.Empty(missingFacts.Matches);
            Assert.Empty(missingFacts.Findings);
            Assert.Equal(FindCoverageState.Complete, missingFacts.Coverage);

            var unavailable = CreateSource("docs/unavailable.md");
            var unavailableFacts = new FindMatcher().Match(new FindMatchingInput(
                request,
                Universe(unavailable),
                [InspectionUnavailable(unavailable, unavailable.Base)]));
            Assert.Empty(unavailableFacts.Matches);
            Assert.Equal(FindCoverageState.Incomplete, unavailableFacts.Coverage);
            Assert.Contains(
                unavailableFacts.Findings,
                finding => finding.Code == FindFindingCode.InspectionUnavailable);
            return;
        }

        if (scenario != "ambiguous-with-safe-source")
        {
            throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The matcher section scenario is not defined.");
        }

        var first = CreateSource("docs/ambiguous.md");
        var second = CreateSource("docs/safe.md");
        var facts = new FindMatcher().Match(new FindMatchingInput(
            request,
            Universe(first, second),
            [
                Inspection(
                    first,
                    first.Base,
                    [],
                    [
                        Heading("Intro", 1, 4),
                        Heading("Needle", 2, 10),
                        Heading("Intro", 1, 20),
                        Heading("Needle", 2, 26),
                    ],
                    "ambiguous",
                    Document(
                        new HeadingSpec("Intro", 1, 4, 5, MarkdownHeadingForm.Atx, true),
                        new HeadingSpec("Needle", 2, 10, 6, MarkdownHeadingForm.Atx, true),
                        new HeadingSpec("Intro", 1, 20, 5, MarkdownHeadingForm.Atx, true),
                        new HeadingSpec("Needle", 2, 26, 6, MarkdownHeadingForm.Atx, true))),
                Inspection(
                    second,
                    second.Base,
                    [],
                    [Heading("Intro", 1, 4), Heading("Needle", 2, 10)],
                    "safe",
                    Document(
                        new HeadingSpec("Intro", 1, 4, 5, MarkdownHeadingForm.Atx, true),
                        new HeadingSpec("Needle", 2, 10, 6, MarkdownHeadingForm.Atx, true))),
            ]));

        Assert.Equal(FindCoverageState.Incomplete, facts.Coverage);
        Assert.Contains(facts.Findings, finding => finding.Code == FindFindingCode.SectionAmbiguous);
        var safeMatch = Assert.Single(facts.Matches);
        Assert.Equal(".agents/docs/safe.md", safeMatch.Path);
        Assert.Equal("Needle", Assert.Single(safeMatch.Evidence).Authored);
    }

    [Fact(DisplayName = "Find matching uses ordinal-ignore-case comparison without culture-sensitive or Unicode-normalizing matches")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void OrdinalIgnoreCaseMatchingIsCultureIndependentWithoutUnicodeNormalization()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            foreach (var cultureName in new[] { "tr-TR", "en-US" })
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                if (cultureName == "tr-TR")
                {
                    AssertCultureMatch("I", "i", true);
                    AssertCultureMatch("ı", "i", false);
                }
                else
                {
                    AssertCultureMatch("é", "e\u0301", false);
                    AssertCultureMatch("É", "é", true);
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact(DisplayName = "Find bare inventory keeps only safe predicate matches while retaining independently known sources")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void BareAndPredicateSearchesRetainOnlyIndependentlySafeMatches()
    {
        var safe = CreateSource("docs/safe.md");
        var unavailable = CreateSource("docs/unavailable.md");
        var bareRequest = CreateRequest([], FindRequirement.All, NaturalRegions());
        var bareFacts = new FindMatcher().Match(new FindMatchingInput(
            bareRequest,
            Universe(safe, unavailable),
            [
                Inspection(safe, safe.Base, [], [], "safe"),
                InspectionUnavailable(unavailable, unavailable.Base),
            ]));
        Assert.Equal(FindCoverageState.Incomplete, bareFacts.Coverage);
        Assert.Equal(
            [".agents/docs/safe.md", ".agents/docs/unavailable.md"],
            bareFacts.Matches.Select(match => match.Path));

        var unavailableDocument = new MarkdownDocumentFacts(
            "---\n"
                + "open-forge:\n"
                + "# Body\n",
            new MarkdownFrontmatterBoundary(
                MarkdownFrontmatterState.Unavailable,
                null,
                null,
                null),
            null,
            [],
            [],
            [],
            [],
            [],
            MarkdownGeneratedRegionFact.Unavailable("The body boundary is unavailable."));
        Assert.Equal(MarkdownFrontmatterState.Unavailable, unavailableDocument.Frontmatter.State);
        Assert.Null(unavailableDocument.BodySpan);

        var unavailableBoundaryFacts = new FindMatcher().Match(new FindMatchingInput(
            bareRequest,
            Universe(safe),
            [
                new FindLayerInspectionFacts(
                    safe,
                    safe.Base,
                    unavailableDocument,
                    new FindFrontmatterFacts(FindFrontmatterAvailability.Unavailable, null, []),
                    new FindBodyTagFacts(FindBodyTagAvailability.Unavailable, []),
                    null,
                    []),
            ]));
        Assert.Equal(FindCoverageState.Incomplete, unavailableBoundaryFacts.Coverage);
        Assert.Equal([".agents/docs/safe.md"], unavailableBoundaryFacts.Matches.Select(match => match.Path));
        var unavailableFinding = Assert.Single(unavailableBoundaryFacts.Findings);
        Assert.Equal(FindFindingCode.InspectionUnavailable, unavailableFinding.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, unavailableFinding.Status);
        Assert.Equal(SourceLayerKind.Base, unavailableFinding.Layer);
        Assert.Equal(safe.Base.CanonicalPath, unavailableFinding.Path);
        Assert.Null(unavailableFinding.Region);

        var predicate = new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic");
        var predicateRequest = CreateRequest([predicate], FindRequirement.All, NaturalRegions());
        var predicateFacts = new FindMatcher().Match(new FindMatchingInput(
            predicateRequest,
            Universe(safe, unavailable),
            [
                Inspection(safe, safe.Base, ["Topic"], [], "safe"),
                InspectionUnavailable(unavailable, unavailable.Base),
            ]));
        Assert.Equal(FindCoverageState.Incomplete, predicateFacts.Coverage);
        Assert.Equal([".agents/docs/safe.md"], predicateFacts.Matches.Select(match => match.Path));

        var frontmatterUnavailableFacts = new FindMatcher().Match(new FindMatchingInput(
            predicateRequest,
            Universe(safe),
            [
                Inspection(
                    safe,
                    safe.Base,
                    [],
                    [],
                    null,
                    Document(),
                    [],
                    FindFrontmatterAvailability.Unavailable),
            ]));
        Assert.Empty(frontmatterUnavailableFacts.Matches);
        Assert.Equal(FindCoverageState.Incomplete, frontmatterUnavailableFacts.Coverage);

        var completeBareFacts = new FindMatcher().Match(new FindMatchingInput(
            bareRequest,
            Universe(safe),
            [Inspection(safe, safe.Base, [], [], "safe")]));
        Assert.Equal(FindCoverageState.Complete, completeBareFacts.Coverage);
        Assert.Equal([".agents/docs/safe.md"], completeBareFacts.Matches.Select(match => match.Path));
    }

    private static void AssertSingleEvidence(
        SourceLogicalSource source,
        IReadOnlyList<FindLayerInspectionFacts> inspections,
        FindRequest request,
        FindPredicateKind expectedKind,
        string expectedAuthored,
        FindRegionKind expectedRegionKind,
        string? expectedRegionName)
    {
        var facts = new FindMatcher().Match(new FindMatchingInput(
            request,
            Universe(source),
            inspections));
        var evidence = Assert.Single(Assert.Single(facts.Matches).Evidence);

        Assert.Equal(expectedKind, evidence.Kind);
        Assert.Equal(expectedAuthored, evidence.Authored);
        Assert.Equal(SourceLayerKind.Base, evidence.Layer);
        Assert.Equal(expectedRegionKind, evidence.Region.Kind);
        Assert.Equal(expectedRegionName, evidence.Region.Name);
    }

    private static void AssertCultureMatch(
        string authored,
        string queryValue,
        bool expectedMatch)
    {
        var source = CreateSource();
        var predicate = new FindPredicate(FindPredicateKind.Tag, queryValue, queryValue);
        var request = CreateRequest([predicate], FindRequirement.All, NaturalRegions());
        var facts = new FindMatcher().Match(new FindMatchingInput(
            request,
            Universe(source),
            [Inspection(source, source.Base, [authored], [], "culture-test")]));

        Assert.Equal(expectedMatch, facts.Matches.Count == 1);
    }

    private static FindRequest CreateRequest(
        IEnumerable<FindPredicate> predicates,
        FindRequirement requirement,
        FindRegionSelection regions)
    {
        var workspace = CreateWorkspace();
        return new FindRequest(
            workspace,
            new FindUniverseFilter([], []),
            new FindQuery(predicates, predicates, requirement, regions),
            new FindPresentationSelection(
                null,
                CliView.Expanded,
                new FindContentSelection([], [])));
    }

    private static FindRegionSelection NaturalRegions()
        => new(
            [],
            [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
            [new FindRegion(FindRegionKind.Body, null, "body")]);

    private static FindRegionSelection ExplicitRegions(
        IEnumerable<FindRegion> tagRegions,
        IEnumerable<FindRegion> headingRegions)
        => new(
            tagRegions,
            tagRegions,
            headingRegions);

    private static FindUniverse Universe(params SourceLogicalSource[] sources)
        => new(
            FindUniverseMode.Default,
            [],
            [],
            sources.Length,
            sources.Length,
            null);

    private static FindLayerInspectionFacts Inspection(
        SourceLogicalSource source,
        SourceLayer layer,
        IEnumerable<string> frontmatterTags,
        IEnumerable<MarkdownHeadingFact> headings,
        string? description,
        MarkdownDocumentFacts? document = null,
        IEnumerable<FindBodyTagOccurrence>? bodyTags = null,
        FindFrontmatterAvailability frontmatterAvailability = FindFrontmatterAvailability.Complete)
    {
        document ??= DocumentFromHeadings(headings);
        var frontmatter = new FindFrontmatterFacts(
            frontmatterAvailability,
            frontmatterAvailability == FindFrontmatterAvailability.Complete ? description : null,
            frontmatterAvailability == FindFrontmatterAvailability.Complete
                ? frontmatterTags.Select((tag, index) => new FindFrontmatterTagOccurrence(tag, Location(index + 1, tag.Length)))
                : []);
        var body = new FindBodyTagFacts(
            FindBodyTagAvailability.Complete,
            bodyTags ?? []);
        return new FindLayerInspectionFacts(
            source,
            layer,
            document,
            frontmatter,
            body,
            description,
            []);
    }

    private static FindLayerInspectionFacts InspectionUnavailable(
        SourceLogicalSource source,
        SourceLayer layer)
        => new(source, layer, null, null, null, null, []);

    private static SourceLogicalSource CreateSource(
        string path = ".agents/docs/result.md",
        bool withOverwrite = false)
    {
        path = path.StartsWith(".agents/", StringComparison.Ordinal)
            ? path
            : $".agents/{path}";
        var root = CreateWorkspace().PhysicalRoot;
        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidOperationException("The matcher source must have a derived ID.");
        var baseLayer = new SourceLayer(
            path,
            Physical(root, path),
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (withOverwrite)
        {
            var overwritePath = path[..^".md".Length] + ".overwrite.md";
            overwrite = new SourceLayer(
                overwritePath,
                Physical(root, overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(new SourceLogicalIdentity(id, path), baseLayer, overwrite);
    }

    private static MarkdownDocumentFacts Document(params HeadingSpec[] specifications)
    {
        var sourceLength = specifications.Length == 0
            ? 20
            : specifications.Max(specification => specification.Start + specification.Length) + 20;
        var source = new string('x', sourceLength);
        var body = new MarkdownTextSpan(0, source.Length);
        var headings = specifications
            .Select(specification => new MarkdownHeadingFact(
                specification.Text,
                specification.Level,
                specification.Form,
                specification.Canonical,
                null,
                new MarkdownTextSpan(specification.Start, specification.Length)))
            .ToArray();
        return DocumentFromHeadings(source, body, headings);
    }

    private static MarkdownDocumentFacts DocumentFromHeadings(
        string source,
        MarkdownTextSpan body,
        IReadOnlyList<MarkdownHeadingFact> headings)
    {
        var sections = headings
            .Select((heading, index) =>
            {
                var end = source.Length;
                for (var next = index + 1; next < headings.Count; next++)
                {
                    if (headings[next].Level <= heading.Level)
                    {
                        end = headings[next].Span.Start;
                        break;
                    }
                }

                return new MarkdownSectionFact(
                    heading,
                    new MarkdownTextSpan(heading.Span.Start, end - heading.Span.Start));
            })
            .ToArray();
        return new MarkdownDocumentFacts(
            source,
            new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0),
            body,
            headings,
            sections,
            [],
            [],
            [],
            MarkdownGeneratedRegionFact.Absent());
    }

    private static MarkdownDocumentFacts DocumentFromHeadings(
        IEnumerable<MarkdownHeadingFact> headings)
    {
        var materialized = headings.ToArray();
        var sourceLength = materialized.Length == 0
            ? 20
            : materialized.Max(heading => heading.Span.End) + 20;
        return DocumentFromHeadings(
            new string('x', sourceLength),
            new MarkdownTextSpan(0, sourceLength),
            materialized);
    }

    private static MarkdownHeadingFact Heading(
        string text,
        int level,
        int start)
        => new(
            text,
            level,
            MarkdownHeadingForm.Atx,
            true,
            null,
            new MarkdownTextSpan(start, Math.Max(1, text.Length)));

    private static SourceLocation Location(long offset, long length)
        => new(1, checked((int)Math.Min(offset + 1, int.MaxValue)), offset, length);

    private static CliWorkspace CreateWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-matcher-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private sealed record HeadingSpec(
        string Text,
        int Level,
        int Start,
        int Length,
        MarkdownHeadingForm Form,
        bool Canonical);
}
