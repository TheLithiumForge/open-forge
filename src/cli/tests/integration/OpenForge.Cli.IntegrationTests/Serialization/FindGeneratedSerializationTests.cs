using System.Text.Json;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class FindGeneratedSerializationTests
{
    [Fact(DisplayName = "Find JSON document is registered as one concrete source-generated graph with reflection disabled"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void FindDocumentUsesGeneratedMetadata()
    {
        var document = RichIncompleteDocument();
        var metadata = OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindJsonContext.Default.FindJsonDocument;

        Assert.Equal(typeof(FindJsonDocument), metadata.Type);
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);

        var json = JsonSerializer.Serialize(document, metadata);

        using var parsed = JsonDocument.Parse(json);
        AssertPropertyOrder(parsed.RootElement, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal("find", parsed.RootElement.GetProperty("command").GetString());
        Assert.Equal("incomplete", parsed.RootElement.GetProperty("status").GetString());
        Assert.Equal("explicit-workspace", parsed.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
    }

    [Fact(DisplayName = "Find generated JSON preserves every concrete wire order with null, array, count, and next rules"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void FindDocumentPreservesEveryWireShapeAndNullRule()
    {
        using var rich = JsonDocument.Parse(Serialize(RichIncompleteDocument()));
        using var blocked = JsonDocument.Parse(Serialize(SelectorAmbiguousBlockedDocument()));

        AssertWireOrder(rich.RootElement, includeMatchGraph: true);
        AssertWireOrder(blocked.RootElement, includeMatchGraph: false);
        AssertRichNullAndArrayRules(rich.RootElement);
        AssertBlockedNullAndArrayRules(blocked.RootElement);
    }

    private static string Serialize(FindJsonDocument document)
        => JsonSerializer.Serialize(
            document,
            OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindJsonContext.Default.FindJsonDocument);

    private static void AssertWireOrder(JsonElement root, bool includeMatchGraph)
    {
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");

        if (root.GetProperty("workspace").ValueKind != JsonValueKind.Null)
        {
            AssertPropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        }

        var result = root.GetProperty("result");
        AssertPropertyOrder(result, "universe", "query", "presentation", "coverage", "findings", "matches");

        var universe = result.GetProperty("universe");
        AssertPropertyOrder(universe, "mode", "include", "exclude", "candidateCount", "inspectedCount", "matchedCount");
        var include = universe.GetProperty("include").EnumerateArray().ToArray();
        if (include.Length != 0)
        {
            AssertSelectorWireOrder(include[0]);
        }

        var exclude = universe.GetProperty("exclude").EnumerateArray().ToArray();
        if (exclude.Length != 0)
        {
            AssertSelectorWireOrder(exclude[0]);
        }

        var query = result.GetProperty("query");
        AssertPropertyOrder(query, "predicates", "effectivePredicates", "require", "within");
        var predicates = query.GetProperty("predicates").EnumerateArray().ToArray();
        if (predicates.Length != 0)
        {
            AssertPropertyOrder(predicates[0], "kind", "value");
        }

        AssertPropertyOrder(query.GetProperty("within"), "supplied", "tag", "heading");

        var presentation = result.GetProperty("presentation");
        AssertPropertyOrder(presentation, "view", "content");
        AssertPropertyOrder(presentation.GetProperty("view"), "supplied", "effective");
        AssertPropertyOrder(presentation.GetProperty("content"), "supplied", "effective");
        AssertPropertyOrder(result.GetProperty("coverage"), "state", "matching", "projection");

        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        AssertPropertyOrder(finding, "code", "status", "subject", "cause", "selectorRole", "selectorOccurrence", "source", "layer", "path", "region", "location", "candidates");
        if (finding.GetProperty("location").ValueKind != JsonValueKind.Null)
        {
            AssertPropertyOrder(finding.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        }

        AssertPropertyOrder(root.GetProperty("next"), "command", "reason");

        if (!includeMatchGraph)
        {
            return;
        }

        var match = Assert.Single(result.GetProperty("matches").EnumerateArray());
        AssertPropertyOrder(match, "position", "id", "path", "description", "evidence", "projections");
        var evidence = match.GetProperty("evidence").EnumerateArray().First();
        AssertPropertyOrder(evidence, "predicate", "kind", "query", "authored", "region", "layer", "path", "location", "occurrence", "heading");
        AssertPropertyOrder(evidence.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        var headingEvidence = match.GetProperty("evidence").EnumerateArray()
            .First(item => item.GetProperty("heading").ValueKind != JsonValueKind.Null);
        AssertPropertyOrder(headingEvidence.GetProperty("heading"), "level", "form", "canonical");

        var projections = match.GetProperty("projections").EnumerateArray().ToArray();
        var metadataProjection = projections[0];
        AssertPropertyOrder(metadataProjection, "part", "name", "layer", "path", "state", "metadata", "text", "headings", "location");
        AssertPropertyOrder(metadataProjection.GetProperty("metadata"), "position", "id", "path", "routeState", "route", "layers");
        AssertPropertyOrder(
            metadataProjection.GetProperty("metadata").GetProperty("layers").EnumerateArray().First(),
            "kind",
            "path");

        var headingsProjection = projections[2];
        var projectedHeading = Assert.Single(headingsProjection.GetProperty("headings").EnumerateArray());
        AssertPropertyOrder(projectedHeading, "text", "level", "form", "location", "canonical");
        AssertPropertyOrder(projectedHeading.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
    }

    private static void AssertSelectorWireOrder(JsonElement selector)
    {
        AssertPropertyOrder(selector, "value", "form", "resolution", "identity", "sourceKind", "expansion", "candidates");
        if (selector.GetProperty("identity").ValueKind != JsonValueKind.Null)
        {
            AssertPropertyOrder(selector.GetProperty("identity"), "id", "path");
        }

        var candidates = selector.GetProperty("candidates").EnumerateArray().ToArray();
        if (candidates.Length != 0)
        {
            AssertPropertyOrder(candidates[0], "id", "path");
        }
    }

    private static void AssertRichNullAndArrayRules(JsonElement root)
    {
        var result = root.GetProperty("result");
        var universe = result.GetProperty("universe");
        Assert.Empty(universe.GetProperty("exclude").EnumerateArray());
        Assert.Empty(Assert.Single(universe.GetProperty("include").EnumerateArray()).GetProperty("candidates").EnumerateArray());
        Assert.Empty(result.GetProperty("findings").EnumerateArray().Single().GetProperty("candidates").EnumerateArray());

        var match = Assert.Single(result.GetProperty("matches").EnumerateArray());
        var evidence = match.GetProperty("evidence").EnumerateArray().First();
        Assert.Equal(JsonValueKind.Null, evidence.GetProperty("heading").ValueKind);
        Assert.NotEmpty(match.GetProperty("projections").EnumerateArray());

        var metadata = match.GetProperty("projections").EnumerateArray().First().GetProperty("metadata");
        Assert.NotEmpty(metadata.GetProperty("layers").EnumerateArray());
        var metadataProjection = match.GetProperty("projections").EnumerateArray().First();
        Assert.Equal(JsonValueKind.Null, metadataProjection.GetProperty("name").ValueKind);
        Assert.Equal(JsonValueKind.Null, metadataProjection.GetProperty("layer").ValueKind);
        Assert.Equal(JsonValueKind.Null, metadataProjection.GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Null, metadataProjection.GetProperty("text").ValueKind);
        Assert.Empty(metadataProjection.GetProperty("headings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, metadataProjection.GetProperty("location").ValueKind);
    }

    private static void AssertBlockedNullAndArrayRules(JsonElement root)
    {
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var result = root.GetProperty("result");
        var universe = result.GetProperty("universe");
        Assert.Equal(JsonValueKind.Null, universe.GetProperty("candidateCount").ValueKind);
        Assert.Equal(JsonValueKind.Null, universe.GetProperty("inspectedCount").ValueKind);
        Assert.Equal(JsonValueKind.Null, universe.GetProperty("matchedCount").ValueKind);
        Assert.Empty(universe.GetProperty("exclude").EnumerateArray());

        var selector = Assert.Single(universe.GetProperty("include").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, selector.GetProperty("identity").ValueKind);
        Assert.Equal(JsonValueKind.Null, selector.GetProperty("sourceKind").ValueKind);
        Assert.Equal(JsonValueKind.Null, selector.GetProperty("expansion").ValueKind);
        Assert.Equal(2, selector.GetProperty("candidates").GetArrayLength());
        AssertPropertyOrder(selector.GetProperty("candidates")[0], "id", "path");

        var query = result.GetProperty("query");
        Assert.Empty(query.GetProperty("predicates").EnumerateArray());
        Assert.Empty(query.GetProperty("effectivePredicates").EnumerateArray());
        Assert.Empty(query.GetProperty("within").GetProperty("supplied").EnumerateArray());
        var content = result.GetProperty("presentation").GetProperty("content");
        Assert.Empty(content.GetProperty("supplied").EnumerateArray());
        Assert.Empty(content.GetProperty("effective").EnumerateArray());
        Assert.Empty(result.GetProperty("matches").EnumerateArray());

        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("source").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("layer").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("region").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("location").ValueKind);
        Assert.Equal(2, finding.GetProperty("candidates").GetArrayLength());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));

    private static FindJsonDocument RichIncompleteDocument()
        => new()
        {
            SchemaVersion = 1,
            Command = "find",
            Status = "incomplete",
            Workspace = new FindJsonWorkspace
            {
                Path = "C:/fixtures/find-workspace",
                SelectedBy = "explicit-workspace",
            },
            Result = new FindJsonResult
            {
                Universe = new FindJsonUniverse
                {
                    Mode = "filtered",
                    Include =
                    [
                        new FindJsonSelector
                        {
                            Value = "docs",
                            Form = "id",
                            Resolution = "resolved",
                            Identity = new FindJsonIdentity { Id = "docs", Path = ".agents/docs.md" },
                            SourceKind = "ordinary",
                            Expansion = "source",
                            Candidates = [],
                        },
                    ],
                    Exclude = [],
                    CandidateCount = 1,
                    InspectedCount = 1,
                    MatchedCount = 1,
                },
                Query = new FindJsonQuery
                {
                    Predicates =
                    [
                        new FindJsonPredicate { Kind = "tag", Value = "Architecture" },
                        new FindJsonPredicate { Kind = "heading", Value = "Target" },
                    ],
                    EffectivePredicates =
                    [
                        new FindJsonPredicate { Kind = "tag", Value = "Architecture" },
                        new FindJsonPredicate { Kind = "heading", Value = "Target" },
                    ],
                    Require = "all",
                    Within = new FindJsonWithin
                    {
                        Supplied = ["frontmatter", "body"],
                        Tag = ["frontmatter"],
                        Heading = ["body"],
                    },
                },
                Presentation = new FindJsonPresentation
                {
                    View = new FindJsonView { Supplied = "expanded", Effective = "expanded" },
                    Content = new FindJsonContent
                    {
                        Supplied = ["metadata", "frontmatter", "headings", "body", "section:Target"],
                        Effective = ["metadata", "frontmatter", "headings", "body", "section:Target"],
                    },
                },
                Coverage = new FindJsonCoverage
                {
                    State = "incomplete",
                    Matching = "complete",
                    Projection = "incomplete",
                },
                Findings =
                [
                    new FindJsonFinding
                    {
                        Code = "find.section-ambiguous",
                        Status = "incomplete",
                        Subject = "Target",
                        Cause = "Several Target headings prevent one exact section projection.",
                        SelectorRole = null,
                        SelectorOccurrence = null,
                        Source = new FindJsonIdentity { Id = "docs", Path = ".agents/docs.md" },
                        Layer = "base",
                        Path = ".agents/docs.md",
                        Region = "section:Target",
                        Location = new FindJsonLocation { Line = 12, Column = 1, ByteOffset = 120, ByteLength = 10 },
                        Candidates = [],
                    },
                ],
                Matches =
                [
                    new FindJsonMatch
                    {
                        Position = 1,
                        Id = "docs",
                        Path = ".agents/docs.md",
                        Description = "Docs",
                        Evidence =
                        [
                            new FindJsonEvidence
                            {
                                Predicate = 1,
                                Kind = "tag",
                                Query = "Architecture",
                                Authored = "Architecture",
                                Region = "frontmatter",
                                Layer = "base",
                                Path = ".agents/docs.md",
                                Location = new FindJsonLocation { Line = 2, Column = 4, ByteOffset = 12, ByteLength = 8 },
                                Occurrence = 1,
                                Heading = null,
                            },
                            new FindJsonEvidence
                            {
                                Predicate = 2,
                                Kind = "heading",
                                Query = "Target",
                                Authored = "Target",
                                Region = "body",
                                Layer = "base",
                                Path = ".agents/docs.md",
                                Location = new FindJsonLocation { Line = 8, Column = 1, ByteOffset = 80, ByteLength = 9 },
                                Occurrence = 1,
                                Heading = new FindJsonHeadingEvidence { Level = 2, Form = "atx", Canonical = true },
                            },
                        ],
                        Projections =
                        [
                            new FindJsonProjection
                            {
                                Part = "metadata",
                                Name = null,
                                Layer = null,
                                Path = null,
                                State = "available",
                                Metadata = new FindJsonMetadata
                                {
                                    Position = 1,
                                    Id = "docs",
                                    Path = ".agents/docs.md",
                                    RouteState = "routed",
                                    Route = "docs",
                                    Layers =
                                    [
                                        new FindJsonMetadataLayer { Kind = "base", Path = ".agents/docs.md" },
                                        new FindJsonMetadataLayer { Kind = "overwrite", Path = ".agents/docs.overwrite.md" },
                                    ],
                                },
                                Text = null,
                                Headings = [],
                                Location = null,
                            },
                            TextProjection("frontmatter", null, "base", ".agents/docs.md", "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---", new FindJsonLocation { Line = 1, Column = 1, ByteOffset = 0, ByteLength = 65 }),
                            new FindJsonProjection
                            {
                                Part = "headings",
                                Name = null,
                                Layer = "base",
                                Path = ".agents/docs.md",
                                State = "available",
                                Metadata = null,
                                Text = null,
                                Headings =
                                [
                                    new FindJsonProjectedHeading
                                    {
                                        Text = "Architecture",
                                        Level = 1,
                                        Form = "atx",
                                        Location = new FindJsonLocation { Line = 6, Column = 1, ByteOffset = 70, ByteLength = 14 },
                                        Canonical = true,
                                    },
                                ],
                                Location = null,
                            },
                            TextProjection("body", null, "base", ".agents/docs.md", "# Architecture\n\n## Target\n\nTarget body\n", new FindJsonLocation { Line = 6, Column = 1, ByteOffset = 70, ByteLength = 43 }),
                            new FindJsonProjection
                            {
                                Part = "section",
                                Name = "Target",
                                Layer = "base",
                                Path = ".agents/docs.md",
                                State = "ambiguous",
                                Metadata = null,
                                Text = null,
                                Headings = [],
                                Location = null,
                            },
                            TextProjection("frontmatter", null, "overwrite", ".agents/docs.overwrite.md", "---\nopen-forge:\n  tags: [Architecture]\n---", new FindJsonLocation { Line = 1, Column = 1, ByteOffset = 0, ByteLength = 45 }),
                            new FindJsonProjection
                            {
                                Part = "headings",
                                Name = null,
                                Layer = "overwrite",
                                Path = ".agents/docs.overwrite.md",
                                State = "available",
                                Metadata = null,
                                Text = null,
                                Headings =
                                [
                                    new FindJsonProjectedHeading
                                    {
                                        Text = "Architecture",
                                        Level = 1,
                                        Form = "atx",
                                        Location = new FindJsonLocation { Line = 5, Column = 1, ByteOffset = 50, ByteLength = 14 },
                                        Canonical = true,
                                    },
                                ],
                                Location = null,
                            },
                            TextProjection("body", null, "overwrite", ".agents/docs.overwrite.md", "# Architecture\n\n## Target\n\nOverwrite body\n", new FindJsonLocation { Line = 5, Column = 1, ByteOffset = 50, ByteLength = 47 }),
                            TextProjection("section", "Target", "overwrite", ".agents/docs.overwrite.md", "## Target\n\nOverwrite body\n", new FindJsonLocation { Line = 7, Column = 1, ByteOffset = 68, ByteLength = 29 }),
                        ],
                    },
                ],
            },
            Next = new FindJsonNext
            {
                Command = "open-forge doctor",
                Reason = "Inspect the unavailable source or projection facts before relying on this Find result.",
            },
        };

    private static FindJsonProjection TextProjection(
        string part,
        string? name,
        string layer,
        string path,
        string text,
        FindJsonLocation location)
        => new()
        {
            Part = part,
            Name = name,
            Layer = layer,
            Path = path,
            State = "available",
            Metadata = null,
            Text = text,
            Headings = [],
            Location = location,
        };

    private static FindJsonDocument SelectorAmbiguousBlockedDocument()
        => new()
        {
            SchemaVersion = 1,
            Command = "find",
            Status = "blocked",
            Workspace = null,
            Result = new FindJsonResult
            {
                Universe = new FindJsonUniverse
                {
                    Mode = "filtered",
                    Include =
                    [
                        new FindJsonSelector
                        {
                            Value = "docs",
                            Form = "id",
                            Resolution = "ambiguous",
                            Identity = null,
                            SourceKind = null,
                            Expansion = null,
                            Candidates =
                            [
                                new FindJsonIdentity { Id = "docs", Path = ".agents/docs.md" },
                                new FindJsonIdentity { Id = "docs", Path = ".agents/docs/_docs.md" },
                            ],
                        },
                    ],
                    Exclude = [],
                    CandidateCount = null,
                    InspectedCount = null,
                    MatchedCount = null,
                },
                Query = new FindJsonQuery
                {
                    Predicates = [],
                    EffectivePredicates = [],
                    Require = "all",
                    Within = new FindJsonWithin
                    {
                        Supplied = [],
                        Tag = ["frontmatter"],
                        Heading = ["body"],
                    },
                },
                Presentation = new FindJsonPresentation
                {
                    View = new FindJsonView { Supplied = null, Effective = "compact" },
                    Content = new FindJsonContent { Supplied = [], Effective = [] },
                },
                Coverage = new FindJsonCoverage
                {
                    State = "blocked",
                    Matching = "blocked",
                    Projection = "not-requested",
                },
                Findings =
                [
                    new FindJsonFinding
                    {
                        Code = "find.selector-ambiguous",
                        Status = "blocked",
                        Subject = "docs",
                        Cause = "The source selector identifies more than one source.",
                        SelectorRole = "include",
                        SelectorOccurrence = 1,
                        Source = null,
                        Layer = null,
                        Path = null,
                        Region = null,
                        Location = null,
                        Candidates =
                        [
                            new FindJsonIdentity { Id = "docs", Path = ".agents/docs.md" },
                            new FindJsonIdentity { Id = "docs", Path = ".agents/docs/_docs.md" },
                        ],
                    },
                ],
                Matches = [],
            },
            Next = new FindJsonNext
            {
                Command = "open-forge find",
                Reason = "Replace every ambiguous selector with one listed exact path, then rerun the same request.",
            },
        };
}
