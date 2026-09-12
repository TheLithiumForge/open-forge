using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

public sealed class ContextPresentationTests
{
    private const int ExpectedSourceCount = 2;
    private const int MaximumDiagnosticLength = 4095;
    private const string OverwriteHeading = "=== Overwrite ===";

    [Fact(DisplayName = "Context human views preserve selected authored text and canonical overwrite framing")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void HumanViewsPreserveAuthoredTextAndLayerFraming()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(
                ContextContentPartKind.Frontmatter,
                ContextContentPartKind.Body));

        var compact = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = ContextHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.Contains(ContextPresentationTestData.GuideFrontmatter.TrimEnd('\n'), compact, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideBody, compact, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideFrontmatter.TrimEnd('\n'), expanded, StringComparison.Ordinal);
        Assert.Contains(ContextPresentationTestData.GuideBody, expanded, StringComparison.Ordinal);
        Assert.Contains(OverwriteHeading, compact, StringComparison.Ordinal);
        Assert.Contains(OverwriteHeading, expanded, StringComparison.Ordinal);
        Assert.Contains("Included because: selected source for projects/guide", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context paths-only human projection emits ordered paths without authored bodies")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void PathsOnlyReplacesSourceContentFraming()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Paths));

        var compact = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = ContextHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.Contains(".agents/projects/guide.md", compact, StringComparison.Ordinal);
        Assert.Contains("Ordered paths", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Route: projects/guide", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context compact fixed rows preserve byte-exact separators, culture, ordering, and final newline"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void CompactFixedRowsAreByteExact()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata));

        var rendered = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        Assert.Equal(ExpectedSourceCount, result.Sources.Count);
        var expected = $"""
            Context
            Status: complete
            Workspace: {result.Workspace?.LexicalRoot}
            Selected by: current directory
            Sources: 2; coverage complete
            Content: metadata
            Startup context included: no; additions only: yes
            Follow links: none
            Requested: projects/guide -> resolved; projects/guide; .agents/projects/guide.md
            Source: .agents/docs/topic.md; docs/topic; base; order 1
            Source: .agents/projects/guide.md; projects/guide; base; order 2
            Source: .agents/projects/guide.overwrite.md; projects/guide; overwrite; order 3
            """.ReplaceLineEndings(Environment.NewLine) + Environment.NewLine;

        Assert.Equal(expected, rendered);
    }

    [Fact(DisplayName = "Context compact findings name distinct known source paths")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void CompactFindingsNameDistinctKnownSourcePaths()
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

        var rendered = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));

        Assert.Contains(
            $"  {firstPath}",
            rendered,
            StringComparison.Ordinal);
        Assert.Contains(
            $"  {secondPath}",
            rendered,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context compact finding subjects are escaped without truncation")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void CompactFindingSubjectsAreEscapedWithoutTruncation()
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

        var rendered = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var findingLine = Assert.Single(
            rendered.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.StartsWith("  Subject:", StringComparison.Ordinal));

        Assert.DoesNotContain(hostileSubject, findingLine, StringComparison.Ordinal);
        Assert.Contains("Subject: line\\u000a", findingLine, StringComparison.Ordinal);
        Assert.EndsWith(new string('x', 300), findingLine, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context JSON uses the frozen ordered schema from the same typed result")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void JsonUsesFrozenOrderedSchema()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata),
            ContextLinkExpansion.All);

        var json = ContextJsonRenderer.Render(new CliPresentationRequest<ContextResult>(
            result,
            new CliPresentation(
                Format: CliOutputFormat.Json,
                View: CliView.Compact,
                Verbosity: CliVerbosity.Normal)));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        var commandResult = root.GetProperty("result");
        Assert.Equal(
            ["selection", "presentation", "coverage", "paths", "links", "sources", "findings"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("all", commandResult.GetProperty("selection").GetProperty("linkExpansion").GetProperty("mode").GetString());
        Assert.Equal("network-not-attempted", commandResult.GetProperty("links")[1].GetProperty("target").GetProperty("network").GetString());
    }

    [Fact(DisplayName = "Context help retains public grammar and stream guidance")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void HelpRetainsPublicGrammarAndStreamGuidance()
    {
        var help = ContextHelpSections.Create();
        var text = string.Join('\n', help.Sections.Select(section => $"{section.Heading}\n{section.Body}"));

        Assert.Contains("--additions-only", text, StringComparison.Ordinal);
        Assert.Contains("--follow-links <positive-depth|all>", text, StringComparison.Ordinal);
        Assert.Contains("Human complete, attention, and incomplete results use stdout", text, StringComparison.Ordinal);
        Assert.Contains("deterministic, stateless, and read-only", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context diagnostics retain bounded typed-result evidence")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void DiagnosticsRetainBoundedTypedResultEvidence()
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Metadata));

        var diagnostics = ContextDiagnosticRenderer.Render(Presentation(
            result,
            CliView.Expanded,
            CliVerbosity.Verbose));

        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics.Length, 1, MaximumDiagnosticLength);
        Assert.Contains("status=complete", diagnostics, StringComparison.Ordinal);
        Assert.Contains("links=0", diagnostics, StringComparison.Ordinal);
    }

    private static CliPresentationRequest<ContextResult> Presentation(
        ContextResult result,
        CliView view,
        CliVerbosity verbosity = CliVerbosity.Normal)
        => new(
            result,
            new CliPresentation(
                Format: CliOutputFormat.Human,
                View: view,
                Verbosity: verbosity));

    [Theory(DisplayName = "Context views retain exact content and place incomplete findings before source blocks"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void ViewsKeepContentAndPartialFacts(int viewValue)
    {
        var result = ContextPresentationTestData.Create(
            ContextPresentationTestData.Content(ContextContentPartKind.Paths, ContextContentPartKind.Body),
            findings: [ClosureFinding(".agents/missing.md")]);
        var jsonRequest = new CliPresentationRequest<ContextResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal));
        var json = ContextJsonRenderer.Render(jsonRequest);
        var rendered = ContextHumanRenderer.Render(Presentation(result, (CliView)viewValue));

        Assert.Contains(ContextPresentationTestData.GuideBody, rendered, StringComparison.Ordinal);
        Assert.Contains("Status: incomplete", rendered, StringComparison.Ordinal);
        Assert.Contains(".agents/missing.md", rendered, StringComparison.Ordinal);
        Assert.True(rendered.IndexOf("context.closure-unavailable", StringComparison.Ordinal) < rendered.IndexOf(ContextPresentationTestData.GuideBody, StringComparison.Ordinal));
        Assert.DoesNotContain("Ordered paths", rendered, StringComparison.Ordinal);
        var sourceRows = rendered.Split(Environment.NewLine).Where(line =>
            line.StartsWith("Source: .agents/docs/topic.md", StringComparison.Ordinal)
            || line.StartsWith("Path: .agents/docs/topic.md", StringComparison.Ordinal));
        Assert.Single(sourceRows);
        Assert.Equal(json, ContextJsonRenderer.Render(jsonRequest));
    }

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
