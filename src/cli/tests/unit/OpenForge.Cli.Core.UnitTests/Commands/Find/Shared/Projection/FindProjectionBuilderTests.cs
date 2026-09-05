using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Projection;

public sealed class FindProjectionBuilderTests
{
    [Theory(DisplayName = "Find requested parts project exact logical metadata and ordered physical-layer payloads"),
        InlineData("metadata", "metadata", 1, 0),
        InlineData("all", "metadata,frontmatter,headings,body,section:Intro", 9, 2)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void RequestedPartsProjectExactLogicalAndLayerPayloads(
        string scenario,
        string parts,
        int expectedProjectionCount,
        int expectedFrontmatterProjectionCount)
    {
        var fixture = CreateFixture(withOverwrite: true);
        var input = CreateInput(
            fixture,
            parts,
            parts,
            CreateRouteFacts(fixture.Source, SourceRouteState.Routed, true));

        var facts = new FindProjectionBuilder().Build(input);

        Assert.Equal(expectedProjectionCount, facts.Projections.Count);
        Assert.Equal(FindProjectionCoverageState.Complete, facts.Coverage);
        var actualParts = facts.Projections.Select(projection => projection.Part).ToArray();
        if (scenario == "metadata")
        {
            Assert.Equal([FindContentPartKind.Metadata], actualParts);
        }
        else
        {
            Assert.Equal(
                [
                    FindContentPartKind.Metadata,
                    FindContentPartKind.Frontmatter,
                    FindContentPartKind.Headings,
                    FindContentPartKind.Body,
                    FindContentPartKind.Section,
                    FindContentPartKind.Frontmatter,
                    FindContentPartKind.Headings,
                    FindContentPartKind.Body,
                    FindContentPartKind.Section,
                ],
                actualParts);
        }

        Assert.All(
            facts.Projections,
            projection =>
            {
                Assert.Equal(FindProjectionState.Available, projection.State);
                if (projection.Part == FindContentPartKind.Metadata)
                {
                    Assert.NotNull(projection.Metadata);
                }
                else if (projection.Part == FindContentPartKind.Headings)
                {
                    Assert.NotEmpty(projection.Headings);
                }
                else
                {
                    Assert.NotNull(projection.Text);
                    Assert.NotNull(projection.Location);
                }
            });

        var physical = facts.Projections
            .Where(projection => projection.Part != FindContentPartKind.Metadata)
            .ToArray();
        Assert.Equal(
            expectedFrontmatterProjectionCount,
            physical.Count(projection => projection.Part == FindContentPartKind.Frontmatter));
        if (expectedFrontmatterProjectionCount == 0)
        {
            Assert.Empty(physical);
        }
        else
        {
            Assert.Equal(
                [SourceLayerKind.Base, SourceLayerKind.Overwrite],
                physical
                    .Where(projection => projection.Part == FindContentPartKind.Frontmatter)
                    .Select(projection => projection.Layer));
        }

        var metadata = Assert.Single(
            facts.Projections,
            projection => projection.Part == FindContentPartKind.Metadata);
        var metadataPayload = Assert.IsType<FindMetadata>(metadata.Metadata);
        Assert.Equal(fixture.Source.Identity.AutomaticId, metadataPayload.Id);
        Assert.Equal(
            [SourceLayerKind.Base, SourceLayerKind.Overwrite],
            metadataPayload.Layers.Select(layer => layer.Kind));
    }

    [Theory(DisplayName = "Find section projections distinguish available, missing, ambiguous, and unavailable states"),
        InlineData("available", "Available", null, "Complete"),
        InlineData("missing", "Missing", "ProjectionMissing", "Complete"),
        InlineData("ambiguous", "Ambiguous", "SectionAmbiguous", "Incomplete"),
        InlineData("unavailable", "Unavailable", "ProjectionUnavailable", "Incomplete")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void SectionsDistinguishAvailableMissingAmbiguousAndUnavailable(
        string scenario,
        string expectedStateValue,
        string? expectedFindingValue,
        string expectedCoverageValue)
    {
        var expectedState = Enum.Parse<FindProjectionState>(expectedStateValue);
        var expectedFinding = expectedFindingValue is null
            ? (FindFindingCode?)null
            : Enum.Parse<FindFindingCode>(expectedFindingValue);
        var expectedCoverage = Enum.Parse<FindProjectionCoverageState>(expectedCoverageValue);
        var fixture = CreateFixture();
        var inspection = scenario switch
        {
            "available" => Inspection(fixture.Source, fixture.Source.Base, ["Intro"]),
            "missing" => Inspection(
                fixture.Source,
                fixture.Source.Base,
                ["Other"],
                Array.Empty<MarkdownHeadingFact>()),
            "ambiguous" => Inspection(
                fixture.Source,
                fixture.Source.Base,
                ["Intro", "Intro"],
                [
                    Heading("Intro", 1, 22),
                    Heading("Intro", 1, 30),
                ]),
            "unavailable" => InspectionUnavailable(fixture.Source, fixture.Source.Base),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The projection section scenario is not defined."),
        };
        if (scenario == "missing")
        {
            var document = Assert.IsType<MarkdownDocumentFacts>(inspection.Document);
            Assert.Empty(document.Headings);
        }

        var input = CreateInput(
            fixture,
            "section:Intro",
            "section:Intro",
            null,
            [inspection]);

        var facts = new FindProjectionBuilder().Build(input);
        var projection = Assert.Single(facts.Projections);

        Assert.Equal(FindContentPartKind.Section, projection.Part);
        Assert.Equal("Intro", projection.Name);
        Assert.Equal(expectedState, projection.State);
        Assert.Equal(expectedCoverage, facts.Coverage);
        if (expectedFinding is { } finding)
        {
            Assert.Contains(facts.Findings, value => value.Code == finding);
        }
        else
        {
            Assert.Empty(facts.Findings);
            Assert.NotNull(projection.Text);
            Assert.NotNull(projection.Location);
        }
    }

    [Theory(DisplayName = "Find metadata uses only effective route facts and never infers scope or route identity"),
        InlineData("routed", "Available", "Routed", true),
        InlineData("outside-filter", "Unavailable", null, false)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void MetadataUsesOnlyEffectiveRouteFactsAndDoesNotInferScope(
        string routeScenario,
        string expectedStateValue,
        string? expectedRouteStateValue,
        bool expectedRouted)
    {
        var expectedState = Enum.Parse<FindProjectionState>(expectedStateValue);
        var expectedRouteState = expectedRouteStateValue is null
            ? (FindRouteState?)null
            : Enum.Parse<FindRouteState>(expectedRouteStateValue);
        var fixture = CreateFixture();
        SourceRouteFacts? routeFacts = routeScenario switch
        {
            "routed" => CreateRouteFacts(fixture.Source, SourceRouteState.Routed, true),
            "outside-filter" => CreateRouteFacts(
                CreateSource(".agents/other/source.md"),
                SourceRouteState.Routed,
                true),
            _ => throw new ArgumentOutOfRangeException(nameof(routeScenario), routeScenario, "The projection route scenario is not defined."),
        };
        var input = routeScenario == "outside-filter"
            ? CreateInput(
                fixture,
                "metadata",
                "metadata",
                routeFacts,
                universe: CreateFilteredUniverse(fixture.Source))
            : CreateInput(fixture, "metadata", "metadata", routeFacts);

        var facts = new FindProjectionBuilder().Build(input);
        var projection = Assert.Single(facts.Projections);

        Assert.Equal(expectedState, projection.State);
        if (expectedState == FindProjectionState.Available)
        {
            var metadata = Assert.IsType<FindMetadata>(projection.Metadata);
            Assert.Equal(expectedRouted ? FindRouteState.Routed : FindRouteState.Unrouted, metadata.RouteState);
            Assert.Equal(expectedRouteState, metadata.RouteState);
            Assert.Equal(expectedRouted ? fixture.Source.Identity.AutomaticId : null, metadata.Route);
            Assert.Equal(fixture.Source.Identity.AutomaticId, metadata.Id);
        }
        else
        {
            Assert.Null(projection.Metadata);
            Assert.Contains(facts.Findings, finding => finding.Code == FindFindingCode.ProjectionUnavailable);
        }
    }

    [Theory(DisplayName = "Find projection order is canonical and independent of matching evidence order"),
        InlineData("body,metadata,frontmatter", "metadata,frontmatter,body")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void ProjectionOrderIsCanonicalAndIndependentOfMatching(
        string suppliedParts,
        string effectiveParts)
    {
        var fixture = CreateFixture(withOverwrite: true);
        var input = CreateInput(
            fixture,
            suppliedParts,
            effectiveParts,
            CreateRouteFacts(fixture.Source, SourceRouteState.Unrouted, true),
            [fixture.Inspections[1], fixture.Inspections[0]]);

        var facts = new FindProjectionBuilder().Build(input);

        Assert.Equal(5, facts.Projections.Count);
        Assert.Equal(
            [
                FindContentPartKind.Metadata,
                FindContentPartKind.Frontmatter,
                FindContentPartKind.Body,
                FindContentPartKind.Frontmatter,
                FindContentPartKind.Body,
            ],
            facts.Projections.Select(projection => projection.Part));
        Assert.DoesNotContain(
            facts.Findings,
            finding => finding.Code == FindFindingCode.ProjectionUnavailable);
        var metadata = Assert.Single(
            facts.Projections,
            projection => projection.Part == FindContentPartKind.Metadata);
        var metadataPayload = Assert.IsType<FindMetadata>(metadata.Metadata);
        Assert.Equal(FindRouteState.Unrouted, metadataPayload.RouteState);
        Assert.Null(metadataPayload.Route);
    }

    private static FindProjectionInput CreateInput(
        ProjectionFixture fixture,
        string suppliedParts,
        string effectiveParts,
        SourceRouteFacts? routeFacts,
        IEnumerable<FindLayerInspectionFacts>? inspections = null,
        FindUniverse? universe = null)
    {
        var contentParts = ReadParts(suppliedParts);
        var effectiveContentParts = ReadParts(effectiveParts);
        var effectiveUniverse = universe
            ?? new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, null);
        var universeFilter = effectiveUniverse.Mode == FindUniverseMode.Filtered
            ? new FindUniverseFilter([fixture.Source.Identity.CanonicalBasePath], [])
            : new FindUniverseFilter([], []);
        var query = new FindQuery(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
                [new FindRegion(FindRegionKind.Body, null, "body")]));
        var request = new FindRequest(
            fixture.Workspace,
            universeFilter,
            query,
            new FindPresentationSelection(
                null,
                CliView.Expanded,
                new FindContentSelection(contentParts, effectiveContentParts)));
        var match = new FindMatch(
            1,
            fixture.Source.Identity.AutomaticId,
            fixture.Source.Identity.CanonicalBasePath,
            "Description",
            [],
            []);
        return new FindProjectionInput(
            request,
            effectiveUniverse,
            inspections ?? fixture.Inspections,
            [match],
            routeFacts);
    }

    private static FindUniverse CreateFilteredUniverse(SourceLogicalSource source)
    {
        var identity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        var selector = new FindSelector(
            source.Identity.CanonicalBasePath,
            SourceReferenceKind.SourcePath,
            FindSelectorResolution.Resolved,
            identity,
            FindSourceKind.Ordinary,
            FindSelectorExpansion.Source,
            []);
        return new FindUniverse(
            FindUniverseMode.Filtered,
            [selector],
            [],
            1,
            1,
            null);
    }

    private static ProjectionFixture CreateFixture(bool withOverwrite = false)
    {
        var workspace = CreateWorkspace();
        var source = CreateSource(".agents/docs/result.md", withOverwrite);
        var inspections = new List<FindLayerInspectionFacts>
        {
            Inspection(source, source.Base, ["Intro"]),
        };
        if (source.Overwrite is { } overwrite)
        {
            inspections.Add(Inspection(source, overwrite, ["Intro"]));
        }

        return new ProjectionFixture(workspace, source, inspections);
    }

    private static FindLayerInspectionFacts Inspection(
        SourceLogicalSource source,
        SourceLayer layer,
        IEnumerable<string> tags,
        IEnumerable<MarkdownHeadingFact>? headings = null)
    {
        var document = CreateDocument(headings?.ToArray() ?? [Heading("Intro", 1, 22)]);
        var frontmatter = new FindFrontmatterFacts(
            FindFrontmatterAvailability.Complete,
            "Description",
            tags.Select((tag, index) => new FindFrontmatterTagOccurrence(
                tag,
                new SourceLocation(1, index + 1, index, tag.Length))));
        return new FindLayerInspectionFacts(
            source,
            layer,
            document,
            frontmatter,
            new FindBodyTagFacts(FindBodyTagAvailability.Complete, []),
            "Description",
            []);
    }

    private static FindLayerInspectionFacts InspectionUnavailable(
        SourceLogicalSource source,
        SourceLayer layer)
        => new(source, layer, null, null, null, null, []);

    private static SourceRouteFacts CreateRouteFacts(
        SourceLogicalSource source,
        SourceRouteState state,
        bool identityUnique)
    {
        var node = new SourceRouteNode(source.Identity, SourceRouteParentState.None, [], []);
        var topology = new SourceRouteTopology(
            [node],
            state == SourceRouteState.Routed ? [source.Identity.CanonicalBasePath] : []);
        return new SourceRouteFacts(
            topology,
            [new SourceRouteFact(source.Identity, state, identityUnique)],
            [],
            true,
            false);
    }

    private static SourceLogicalSource CreateSource(
        string path,
        bool withOverwrite = false)
    {
        var root = CreateWorkspace().PhysicalRoot;
        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidOperationException("The projection source must have a derived ID.");
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

        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            baseLayer,
            overwrite);
    }

    private static MarkdownDocumentFacts CreateDocument(
        IReadOnlyList<MarkdownHeadingFact> headings)
    {
        const string sourceWithHeading = "---\ntags: [Intro]\n---\n# Intro\nBody\n";
        const string sourceWithTwoHeadings = "---\ntags: [Intro]\n---\n# Intro\n# Intro\nBody\n";
        const string sourceWithoutHeading = "---\ntags: [Intro]\n---\nBody\n";
        var source = headings.Count switch
        {
            0 => sourceWithoutHeading,
            1 => sourceWithHeading,
            _ => sourceWithTwoHeadings,
        };
        const int blockLength = 21;
        const int yamlStart = 4;
        const int yamlLength = 14;
        const int bodyStart = 22;
        var bodySpan = new MarkdownTextSpan(bodyStart, source.Length - bodyStart);
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
            new MarkdownDocumentStructure(
                new MarkdownFrontmatterBoundary(
                    MarkdownFrontmatterState.Complete,
                    new MarkdownTextSpan(0, blockLength),
                    new MarkdownTextSpan(yamlStart, yamlLength),
                    bodyStart),
                bodySpan,
                headings,
                sections,
                MarkdownGeneratedRegionFact.Absent()),
            new MarkdownInlineFacts([], [], [], []));
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
            new MarkdownTextSpan(start, level + 1 + text.Length));

    private static FindContentPart[] ReadParts(string value)
        => value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ReadContentPart)
            .ToArray();

    private static FindContentPart ReadContentPart(string value)
        => value switch
        {
            "metadata" => new FindContentPart(FindContentPartKind.Metadata, null, "metadata"),
            "frontmatter" => new FindContentPart(FindContentPartKind.Frontmatter, null, "frontmatter"),
            "headings" => new FindContentPart(FindContentPartKind.Headings, null, "headings"),
            "body" => new FindContentPart(FindContentPartKind.Body, null, "body"),
            _ when value.StartsWith("section:", StringComparison.Ordinal)
                => new FindContentPart(FindContentPartKind.Section, value["section:".Length..], value),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The projection part is not defined."),
        };

    private static CliWorkspace CreateWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-projection-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private sealed record ProjectionFixture(
        CliWorkspace Workspace,
        SourceLogicalSource Source,
        IReadOnlyList<FindLayerInspectionFacts> Inspections);
}
