using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Presentation.Context.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

public sealed class ContextPresentationTests
{
    private const int ExpectedSourceCount = 2;
    private const int MaximumDiagnosticLength = 4095;
    private const string OverwriteHeading = "=== .agents/projects/guide.overwrite.md (projects/guide, overwrite) ===";

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context human views preserve selected authored text and canonical overwrite framing")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void HumanViewsPreserveAuthoredTextAndLayerFraming()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(
                ContextContentPartKind.Frontmatter,
                ContextContentPartKind.Body));

        var compact = RenderText(result, CliDetail.Minimal);
        var expanded = RenderText(result, CliDetail.Standard);

        Assert.Contains(ContextPresentationTestData.GuideFrontmatter.TrimEnd('\n'), compact, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideBody, compact, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideFrontmatter.TrimEnd('\n'), expanded, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideBody, expanded, StringComparison.Ordinal);
        Assert.Contains(OverwriteHeading, compact, StringComparison.Ordinal);
        Assert.Contains(OverwriteHeading, expanded, StringComparison.Ordinal);
        Assert.Contains("included because selected source for projects/guide", expanded, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context paths-only human projection emits ordered paths without authored bodies")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void PathsOnlyReplacesSourceContentFraming()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Paths));

        var compact = RenderText(result, CliDetail.Minimal);
        var expanded = RenderText(result, CliDetail.Standard);

        Assert.Contains(".agents/projects/guide.md", compact, StringComparison.Ordinal);
        Assert.Contains(".agents/projects/guide.md", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("route:", expanded, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context minimal text fixed rows preserve byte-exact separators, culture, ordering, and final newline"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void MinimalTextFixedRowsAreByteExact()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            counts: ContextCounts.Complete(2, 0, 0, 0, 0));

        var rendered = RenderText(result, CliDetail.Minimal);
        Assert.Equal(ExpectedSourceCount, result.Sources.Count);
        var expected =
            "=== .agents/docs/topic.md (docs/topic) ===\n"
            + "id: docs/topic\n"
            + "route: docs/topic\n\n"
            + "=== .agents/projects/guide.md (projects/guide) ===\n"
            + "id: projects/guide\n"
            + "route: projects/guide\n\n"
            + OverwriteHeading + "\n"
            + "id: projects/guide\n"
            + "route: projects/guide\n";

        Assert.Equal(expected, rendered);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context standard text findings name distinct known source paths")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void StandardTextFindingsNameDistinctKnownSourcePaths()
    {
        const string firstPath = ".agents/skills/first/SKILL.md";
        const string secondPath = ".agents/skills/second/SKILL.md";
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            findings:
            [
                ClosureFinding(firstPath),
                ClosureFinding(secondPath),
            ]);

        var rendered = RenderText(result, CliDetail.Standard);

        Assert.Contains(
            $"  {firstPath}",
            rendered,
            StringComparison.Ordinal);
        Assert.Contains(
            $"  {secondPath}",
            rendered,
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context standard text finding subjects are escaped without truncation")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void StandardTextFindingSubjectsAreEscapedWithoutTruncation()
    {
        var hostileSubject = $"line\n{new string('x', 300)}";
        var finding = new ContextFinding(
            code: ContextFindingCode.ClosureUnavailable,
            subject: hostileSubject,
            cause: "Loading metadata is unavailable.",
            reference: null,
            source: null,
            layer: null,
            path: null,
            part: null,
            location: null,
            destinationLocation: null,
            candidates: []);
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            findings: [finding]);

        var rendered = RenderText(result, CliDetail.Standard);
        var findingLine = Assert.Single(
            rendered.Split('\n', StringSplitOptions.RemoveEmptyEntries),
            line => line.Contains("line\\n", StringComparison.Ordinal)
                && line.Contains(new string('x', 300), StringComparison.Ordinal));

        Assert.DoesNotContain(hostileSubject, findingLine, StringComparison.Ordinal);
        Assert.Contains("line\\n", findingLine, StringComparison.Ordinal);
        Assert.Contains(new string('x', 300), findingLine, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context JSON uses the frozen ordered schema from the same typed result")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void JsonUsesFrozenOrderedSchema()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            ContextLinkExpansion.All);

        var json = RenderJson(result, CliDetail.Full);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        var commandResult = root.GetProperty("data");
        Assert.Equal(
            ["sources", "links"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("external-unchecked", commandResult.GetProperty("links")[1].GetProperty("resolution").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context help retains public grammar and stream guidance")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void HelpRetainsPublicGrammarAndStreamGuidance()
    {
        var help = ContextHelpSections.Create();
        var text = string.Join('\n', help.Sections.Select(section => $"{section.Heading}\n{section.Body}"));

        Assert.Contains("--additions-only", text, StringComparison.Ordinal);
        Assert.Contains("--follow-links <positive-depth|all>", text, StringComparison.Ordinal);
        Assert.Contains("Text completed, completed-with-warnings, and incomplete results use stdout", text, StringComparison.Ordinal);
        Assert.Contains("preserves authored source bytes", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context diagnostics retain bounded typed-result evidence")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void DiagnosticsRetainBoundedTypedResultEvidence()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            findings: [ClosureFinding(".agents/missing.md")]);

        var diagnostics = CliRenderingStage.Render(
            Presentation(result, CliDetail.Debug),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).DiagnosticContent;

        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics.Length, 1, MaximumDiagnosticLength);
        Assert.Contains("Loading metadata is unavailable.", diagnostics, StringComparison.Ordinal);
    }

    private static CliPresentationRequest<ContextResult> Presentation(
        ContextResult result,
        CliDetail view,
        CliFormat format = CliFormat.Text)
        => new(
            result,
            new CliPresentation(
                Format: format,
                Detail: view,
                Filter: null));

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Context views retain exact content and place incomplete findings before source blocks"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void ViewsKeepContentAndPartialFacts(int viewValue)
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Paths, ContextContentPartKind.Body),
            findings: [ClosureFinding(".agents/missing.md")]);
        var json = RenderJson(result, CliDetail.Standard);
        var rendered = RenderText(result, (CliDetail)viewValue);

        Assert.Contains(ContextPresentationTestData.GuideBody, rendered, StringComparison.Ordinal);
        Assert.Contains("Startup context is unavailable", rendered, StringComparison.Ordinal);
        Assert.True(rendered.IndexOf("Startup context is unavailable", StringComparison.Ordinal) < rendered.IndexOf(ContextPresentationTestData.GuideBody, StringComparison.Ordinal));

        Assert.DoesNotContain("Ordered paths", rendered, StringComparison.Ordinal);
        var sourceRows = rendered.Split('\n').Where(line =>
            line.StartsWith("=== .agents/docs/topic.md", StringComparison.Ordinal));
        Assert.Single(sourceRows);
        Assert.Equal(json, RenderJson(result, CliDetail.Standard));
    }

    private static string RenderText(ContextResult result, CliDetail detail)
        => CliRenderingStage.Render(
            Presentation(result, detail),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).PrimaryContent;

    private static string RenderJson(ContextResult result, CliDetail detail)
        => CliRenderingStage.Render(
            Presentation(result, detail, CliFormat.Json),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).PrimaryContent;

    private static ContextFinding ClosureFinding(string path)
        => new(
            code: ContextFindingCode.ClosureUnavailable,
            subject: null,
            cause: "Loading metadata is unavailable.",
            reference: null,
            source: new ContextSourceIdentity(id: path, path: path),
            layer: null,
            path: path,
            part: null,
            location: null,
            destinationLocation: null,
            candidates: []);
}
