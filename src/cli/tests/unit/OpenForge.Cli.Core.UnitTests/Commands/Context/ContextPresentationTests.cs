using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

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
        var rows = result.Sources
            .SelectMany(source => source.Layers.Select(layer => string.Create(
                CultureInfo.InvariantCulture,
                $"{layer.PathPosition} {source.Id ?? "none"} {layer.Path} {LayerName(layer.Kind)}{Environment.NewLine}")));
        Assert.Equal(ExpectedSourceCount, result.Sources.Count);
        var expected = $"context complete coverage=complete sources={ExpectedSourceCount}{Environment.NewLine}" + string.Concat(rows);

        Assert.Equal(expected, rendered);
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
        Assert.Contains("--follow-links=<positive-depth|all>", text, StringComparison.Ordinal);
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

    private static string LayerName(ContextSourceLayerKind kind)
        => kind == ContextSourceLayerKind.Base ? "base" : "overwrite";
}
