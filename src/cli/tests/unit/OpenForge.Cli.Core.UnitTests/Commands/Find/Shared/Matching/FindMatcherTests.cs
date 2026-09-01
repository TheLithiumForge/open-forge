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

public sealed class FindMatcherTests
{
    public static TheoryData<AggregationScenario> AggregationScenarios => new()
    {
        new AggregationScenario(
            "all-distributed",
            FindRequirement.All,
            [
                new LayerScenario(SourceLayerKind.Base, ["Topic"], ["Other"]),
                new LayerScenario(SourceLayerKind.Overwrite, [], ["Finalize"]),
            ],
            [FindPredicateKind.Tag, FindPredicateKind.Heading],
            [SourceLayerKind.Base, SourceLayerKind.Overwrite],
            false,
            null),
        new AggregationScenario(
            "any-base-only",
            FindRequirement.Any,
            [
                new LayerScenario(SourceLayerKind.Base, ["Topic"], ["Other"]),
                new LayerScenario(SourceLayerKind.Overwrite, [], ["Other"]),
            ],
            [FindPredicateKind.Tag],
            [SourceLayerKind.Base],
            false,
            null),
        new AggregationScenario(
            "all-missing",
            FindRequirement.All,
            [
                new LayerScenario(SourceLayerKind.Base, [], ["Other"]),
                new LayerScenario(SourceLayerKind.Overwrite, [], ["Other"]),
            ],
            [],
            [],
            true,
            FindCoverageState.Complete),
    };

    public static TheoryData<RegionScenario> RegionScenarios => new()
    {
        new RegionScenario(
            "natural",
            [
                new RegionEvidenceScenario(
                    new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic"),
                    NaturalRegions(),
                    FindPredicateKind.Tag,
                    "Topic",
                    FindRegionKind.Frontmatter,
                    null),
                new RegionEvidenceScenario(
                    new FindPredicate(FindPredicateKind.Heading, "Intro", "Intro"),
                    NaturalRegions(),
                    FindPredicateKind.Heading,
                    "Intro",
                    FindRegionKind.Body,
                    null),
            ]),
        new RegionScenario(
            "explicit",
            [
                new RegionEvidenceScenario(
                    new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic"),
                    ExplicitRegions(
                        [new FindRegion(FindRegionKind.Document, null, "document")],
                        [new FindRegion(FindRegionKind.Document, null, "document")]),
                    FindPredicateKind.Tag,
                    "Topic",
                    FindRegionKind.Frontmatter,
                    null),
                new RegionEvidenceScenario(
                    new FindPredicate(FindPredicateKind.Tag, "BodyTopic", "BodyTopic"),
                    ExplicitRegions(
                        [new FindRegion(FindRegionKind.Body, null, "body")],
                        [new FindRegion(FindRegionKind.Body, null, "body")]),
                    FindPredicateKind.Tag,
                    "BodyTopic",
                    FindRegionKind.Body,
                    null),
                new RegionEvidenceScenario(
                    new FindPredicate(FindPredicateKind.Heading, "Intro", "Intro"),
                    ExplicitRegions(
                        [new FindRegion(FindRegionKind.Section, "Intro", "section:Intro")],
                        [new FindRegion(FindRegionKind.Section, "Intro", "section:Intro")]),
                    FindPredicateKind.Heading,
                    "Intro",
                    FindRegionKind.Section,
                    "Intro"),
            ]),
    };

    public static TheoryData<SectionScenario> SectionScenarios => new()
    {
        new SectionScenario(
            "missing-and-unavailable",
            [
                new SectionCase(
                    [
                        new AvailableSectionSourceScenario(
                            "docs/missing.md",
                            [Heading("Other", 1, 4)],
                            "missing",
                            [new HeadingSpec("Other", 1, 4, 5, MarkdownHeadingForm.Atx, true)]),
                    ],
                    FindCoverageState.Complete,
                    true,
                    null,
                    null,
                    null),
                new SectionCase(
                    [new UnavailableSectionSourceScenario("docs/unavailable.md")],
                    FindCoverageState.Incomplete,
                    false,
                    FindFindingCode.InspectionUnavailable,
                    null,
                    null),
            ]),
        new SectionScenario(
            "ambiguous-with-safe-source",
            [
                new SectionCase(
                    [
                        new AvailableSectionSourceScenario(
                            "docs/ambiguous.md",
                            [
                                Heading("Intro", 1, 4),
                                Heading("Needle", 2, 10),
                                Heading("Intro", 1, 20),
                                Heading("Needle", 2, 26),
                            ],
                            "ambiguous",
                            [
                                new HeadingSpec("Intro", 1, 4, 5, MarkdownHeadingForm.Atx, true),
                                new HeadingSpec("Needle", 2, 10, 6, MarkdownHeadingForm.Atx, true),
                                new HeadingSpec("Intro", 1, 20, 5, MarkdownHeadingForm.Atx, true),
                                new HeadingSpec("Needle", 2, 26, 6, MarkdownHeadingForm.Atx, true),
                            ]),
                        new AvailableSectionSourceScenario(
                            "docs/safe.md",
                            [Heading("Intro", 1, 4), Heading("Needle", 2, 10)],
                            "safe",
                            [
                                new HeadingSpec("Intro", 1, 4, 5, MarkdownHeadingForm.Atx, true),
                                new HeadingSpec("Needle", 2, 10, 6, MarkdownHeadingForm.Atx, true),
                            ]),
                    ],
                    FindCoverageState.Incomplete,
                    false,
                    FindFindingCode.SectionAmbiguous,
                    ".agents/docs/safe.md",
                    "Needle"),
            ]),
    };

    [Theory(DisplayName = "Find all and any requirements aggregate evidence across ordered physical layers"),
        MemberData(nameof(AggregationScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void AllAndAnyAggregateEvidenceAcrossOrderedLayers(AggregationScenario scenario)
    {
        var source = CreateSource(withOverwrite: true);
        var request = CreateRequest(
            [
                new FindPredicate(FindPredicateKind.Tag, "Topic", "Topic"),
                new FindPredicate(FindPredicateKind.Heading, "Finalize", "Finalize"),
            ],
            scenario.Requirement,
            NaturalRegions());
        var inspections = scenario.Layers
            .Select(layer => Inspection(
                source,
                layer.Kind == SourceLayerKind.Base ? source.Base : source.Overwrite!,
                layer.Tags,
                layer.Headings.Select(heading => Heading(heading, 1, 0)),
                layer.Kind == SourceLayerKind.Base ? "base" : "overwrite"))
            .ToArray();

        var facts = new FindMatcher().Match(new FindMatchingInput(
            request,
            Universe(source),
            inspections));

        if (scenario.ExpectedEvidenceKinds.Count == 0)
        {
            Assert.Empty(facts.Matches);
        }
        else
        {
            var evidence = Assert.Single(facts.Matches).Evidence;
            Assert.Equal(scenario.ExpectedEvidenceKinds, evidence.Select(value => value.Kind));
            Assert.Equal(scenario.ExpectedEvidenceLayers, evidence.Select(value => value.Layer));
        }

        if (scenario.ExpectNoFindings)
        {
            Assert.Empty(facts.Findings);
        }

        if (scenario.ExpectedCoverage is { } expectedCoverage)
        {
            Assert.Equal(expectedCoverage, facts.Coverage);
        }
    }

    [Theory(DisplayName = "Find natural and explicit regions select exact authored tag, body, document, and section evidence"),
        MemberData(nameof(RegionScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void NaturalExplicitDocumentBodyAndSectionRegionsAreExact(RegionScenario scenario)
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

        foreach (var evidence in scenario.Evidence)
        {
            AssertSingleEvidence(
                source,
                inspections,
                CreateRequest(
                    [evidence.Predicate],
                    FindRequirement.All,
                    evidence.Regions),
                evidence.ExpectedKind,
                evidence.ExpectedAuthored,
                evidence.ExpectedRegionKind,
                evidence.ExpectedRegionName);
        }
    }

    [Theory(DisplayName = "Find missing and ambiguous sections retain only independently safe evidence"),
        MemberData(nameof(SectionScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void MissingAndAmbiguousSectionsRetainOnlySafeEvidence(SectionScenario scenario)
    {
        var predicate = new FindPredicate(FindPredicateKind.Heading, "Needle", "Needle");
        var section = new FindRegion(FindRegionKind.Section, "Intro", "section:Intro");
        var request = CreateRequest(
            [predicate],
            FindRequirement.All,
            ExplicitRegions([section], [section]));

        foreach (var sectionCase in scenario.Cases)
        {
            var sources = sectionCase.Sources
                .Select(source => CreateSource(source.Path))
                .ToArray();
            var inspections = sectionCase.Sources
                .Zip(sources, (source, logicalSource) => source.CreateInspection(logicalSource))
                .ToArray();
            var facts = new FindMatcher().Match(new FindMatchingInput(
                request,
                Universe(sources),
                inspections));

            Assert.Equal(sectionCase.ExpectedCoverage, facts.Coverage);
            if (sectionCase.ExpectedMatchPath is null)
            {
                Assert.Empty(facts.Matches);
            }
            else
            {
                var match = Assert.Single(facts.Matches);
                Assert.Equal(sectionCase.ExpectedMatchPath, match.Path);
                Assert.Equal(
                    sectionCase.ExpectedEvidenceAuthored,
                    Assert.Single(match.Evidence).Authored);
            }

            if (sectionCase.ExpectNoFindings)
            {
                Assert.Empty(facts.Findings);
            }

            if (sectionCase.RequiredFindingCode is { } requiredFindingCode)
            {
                Assert.Contains(
                    facts.Findings,
                    finding => finding.Code == requiredFindingCode);
            }
        }
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

    public sealed class AggregationScenario
    {
        internal AggregationScenario(
            string name,
            FindRequirement requirement,
            IReadOnlyList<LayerScenario> layers,
            IReadOnlyList<FindPredicateKind> expectedEvidenceKinds,
            IReadOnlyList<SourceLayerKind> expectedEvidenceLayers,
            bool expectNoFindings,
            FindCoverageState? expectedCoverage)
        {
            Name = name;
            Requirement = requirement;
            Layers = layers;
            ExpectedEvidenceKinds = expectedEvidenceKinds;
            ExpectedEvidenceLayers = expectedEvidenceLayers;
            ExpectNoFindings = expectNoFindings;
            ExpectedCoverage = expectedCoverage;
        }

        private string Name { get; }

        internal FindRequirement Requirement { get; }

        internal IReadOnlyList<LayerScenario> Layers { get; }

        internal IReadOnlyList<FindPredicateKind> ExpectedEvidenceKinds { get; }

        internal IReadOnlyList<SourceLayerKind> ExpectedEvidenceLayers { get; }

        internal bool ExpectNoFindings { get; }

        internal FindCoverageState? ExpectedCoverage { get; }

        public override string ToString() => Name;
    }

    public sealed class RegionScenario
    {
        internal RegionScenario(string name, IReadOnlyList<RegionEvidenceScenario> evidence)
        {
            Name = name;
            Evidence = evidence;
        }

        private string Name { get; }

        internal IReadOnlyList<RegionEvidenceScenario> Evidence { get; }

        public override string ToString() => Name;
    }

    public sealed class SectionScenario
    {
        internal SectionScenario(string name, IReadOnlyList<SectionCase> cases)
        {
            Name = name;
            Cases = cases;
        }

        private string Name { get; }

        internal IReadOnlyList<SectionCase> Cases { get; }

        public override string ToString() => Name;
    }

    internal sealed record LayerScenario(
        SourceLayerKind Kind,
        IReadOnlyList<string> Tags,
        IReadOnlyList<string> Headings);

    internal sealed record RegionEvidenceScenario(
        FindPredicate Predicate,
        FindRegionSelection Regions,
        FindPredicateKind ExpectedKind,
        string ExpectedAuthored,
        FindRegionKind ExpectedRegionKind,
        string? ExpectedRegionName);

    internal sealed record SectionCase(
        IReadOnlyList<SectionSourceScenario> Sources,
        FindCoverageState ExpectedCoverage,
        bool ExpectNoFindings,
        FindFindingCode? RequiredFindingCode,
        string? ExpectedMatchPath,
        string? ExpectedEvidenceAuthored);

    internal abstract class SectionSourceScenario(string path)
    {
        internal string Path { get; } = path;

        internal abstract FindLayerInspectionFacts CreateInspection(SourceLogicalSource source);
    }

    internal sealed class AvailableSectionSourceScenario(
        string path,
        IReadOnlyList<MarkdownHeadingFact> headings,
        string description,
        IReadOnlyList<HeadingSpec> documentHeadings)
        : SectionSourceScenario(path)
    {
        internal override FindLayerInspectionFacts CreateInspection(SourceLogicalSource source)
            => Inspection(
                source,
                source.Base,
                [],
                headings,
                description,
                Document(documentHeadings.ToArray()));
    }

    internal sealed class UnavailableSectionSourceScenario(string path)
        : SectionSourceScenario(path)
    {
        internal override FindLayerInspectionFacts CreateInspection(SourceLogicalSource source)
            => InspectionUnavailable(source, source.Base);
    }

    internal sealed record HeadingSpec(
        string Text,
        int Level,
        int Start,
        int Length,
        MarkdownHeadingForm Form,
        bool Canonical);
}
