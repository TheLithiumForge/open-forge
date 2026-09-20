using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Context.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Content;
using OpenForge.Cli.Core.Presentation.Shared.Content.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Context;

public sealed class ContextPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "context"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ContentWriterPreservesAuthoredBytesAndOnlyWritesSelectedFraming()
    {
        var block = new CliContentBlock
        {
            Path = ".agents/example.md",
            Id = "example",
            DelimiterLayer = "base",
            IncludedBecause = ["selected source"],
            Route = "docs/example",
            Order = 3,
            Layer = "base",
            Parts =
            [
                new CliContentPart
                {
                    Name = "frontmatter",
                    Kind = CliContentPartKind.Text,
                    AuthoredText = new CliAuthoredSpan("---\r\nname: example\r\n---"),
                },
                new CliContentPart
                {
                    Name = "body",
                    Kind = CliContentPartKind.Text,
                    AuthoredText = new CliAuthoredSpan("body\r\nwithout ending"),
                },
            ],
        };

        var document = ContentPartsTextRenderer.Render([block], CliTextStyle.Plain);

        Assert.Equal(
            "=== .agents/example.md (example) ===\n"
            + "included because selected source\n"
            + "route: docs/example\n"
            + "order: 3\n"
            + "layer: base\n"
            + "---\r\nname: example\r\n---\n"
            + "body\r\nwithout ending",
            document.Content);
        Assert.DoesNotContain("scope:", document.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("[body:", document.Content, StringComparison.Ordinal);
        Assert.Equal(
            ["---\r\nname: example\r\n---", "body\r\nwithout ending"],
            document.Spans.Where(span => span.Authored).Select(span => span.Content));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "context"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ContentWriterAddsOneGeneratedBlankLineBetweenNoEndingAuthoredBodies()
    {
        var blocks = new[]
        {
            new CliContentBlock
            {
                Path = ".agents/first.md",
                DelimiterLayer = "base",
                Parts =
                [
                    new CliContentPart
                    {
                        Name = "body",
                        Kind = CliContentPartKind.Text,
                        AuthoredText = new CliAuthoredSpan("first"),
                    },
                ],
            },
            new CliContentBlock
            {
                Path = ".agents/second.md",
                DelimiterLayer = "base",
                Parts =
                [
                    new CliContentPart
                    {
                        Name = "body",
                        Kind = CliContentPartKind.Text,
                        AuthoredText = new CliAuthoredSpan("second"),
                    },
                ],
            },
        };

        var document = ContentPartsTextRenderer.Render(blocks, CliTextStyle.Plain);

        Assert.Equal(
            "=== .agents/first.md ===\n"
            + "first\n\n"
            + "=== .agents/second.md ===\n"
            + "second",
            document.Content);
        Assert.Equal(
            ["first", "second"],
            document.Spans.Where(span => span.Authored).Select(span => span.Content));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "context"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ContextSelectorCarriesTypedCountsIntoReportAndData()
    {
        var counts = ContextCounts.Complete(2, 8, 32, 1, 2);
        var result = new ContextResult(
            workspace: null,
            selection: new ContextSelection(
                requestedSources: [],
                startupIncluded: true,
                additionsOnly: false,
                linkExpansion: ContextLinkExpansion.None,
                sourceCount: 2),
            presentation: new OpenForge.Cli.Core.Commands.Context.Models.Result.ContextPresentation(
                suppliedDetail: null,
                effectiveView: CliDetail.Standard,
                content: new ContextContentSelection([], [])),
            coverage: new ContextCoverage(
                state: ContextCoverageState.Complete,
                selection: ContextCoverageState.Complete,
                links: ContextOptionalCoverageState.NotRequested,
                projection: ContextCoverageState.Complete),
            paths: [],
            links: [],
            sources: [],
            findings: [],
            status: CliSemanticStatus.Complete,
            next: null,
            counts: counts);

        var report = ContextReportSelector.Select(result, new CliSelection(CliDetail.Standard));

        Assert.Equal(2, report.Data.SourceCount);
        Assert.Equal(8, report.Data.TokenCount);
        Assert.Equal(32, report.Counts.Single(count => count.Name == "bytes").Value);
        Assert.Equal(1, report.Counts.Single(count => count.Name == "linksFollowed").Value);
        Assert.Equal(2, report.Data.LinksNotFollowed);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "context"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ContextDataJsonRetainsLevelFieldsAndAuthoredText()
    {
        var data = new ContextData
        {
            Sources =
            [
                new ContextDataSource
                {
                    Path = ".agents/example.md",
                    Id = "example",
                    Layer = "base",
                    Standard = new ContextDataSourceStandard
                    {
                        IncludedBecause = ["selected source"],
                        Route = "docs/example",
                        Scope = null,
                    },
                    Full = new ContextDataSourceFull { Order = 2 },
                    Parts =
                    [
                        new ContextDataPart
                        {
                            Kind = ContextDataPartKind.Text,
                            Part = "body",
                            Text = "body\r\nwithout ending",
                        },
                    ],
                },
            ],
        };

        var json = JsonSerializer.Serialize(
            data,
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering.DataJsonTypeInfo);
        using var document = JsonDocument.Parse(json);
        var source = Assert.Single(document.RootElement.GetProperty("sources").EnumerateArray());
        Assert.Equal("example", source.GetProperty("id").GetString());
        Assert.Equal("docs/example", source.GetProperty("route").GetString());
        Assert.True(source.GetProperty("scope").ValueKind == JsonValueKind.Null);
        Assert.Equal("body\r\nwithout ending", source.GetProperty("parts")[0].GetProperty("text").GetString());
    }
}
