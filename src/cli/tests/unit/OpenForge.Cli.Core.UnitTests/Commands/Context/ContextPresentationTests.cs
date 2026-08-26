using System.Text.Json;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

public sealed class ContextPresentationTests
{
    [Fact(DisplayName = "Context human views preserve selected authored text and canonical overwrite framing")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task HumanViewsPreserveAuthoredTextAndLayerFraming()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(
            workspace,
            Content(ContextContentPartKind.Frontmatter, ContextContentPartKind.Body));

        var compact = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = ContextHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.Contains(ContextOperationWorkspace.GuideFrontmatter.TrimEnd('\n'), compact, StringComparison.Ordinal);
        Assert.Contains(ContextOperationWorkspace.GuideBody, compact, StringComparison.Ordinal);
        Assert.Contains(ContextOperationWorkspace.GuideFrontmatter.TrimEnd('\n'), expanded, StringComparison.Ordinal);
        Assert.Contains(ContextOperationWorkspace.GuideBody, expanded, StringComparison.Ordinal);
        Assert.Contains("=== Overwrite ===", compact, StringComparison.Ordinal);
        Assert.Contains("=== Overwrite ===", expanded, StringComparison.Ordinal);
        Assert.Contains("Included because: selected source for projects/guide", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context paths-only human projection emits ordered paths without authored bodies")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task PathsOnlyReplacesSourceContentFraming()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, Content(ContextContentPartKind.Paths));

        var compact = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = ContextHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.Contains(".agents/projects/guide.md", compact, StringComparison.Ordinal);
        Assert.Contains("Ordered paths", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Base rule.", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Route: projects/guide", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context compact fixed rows preserve byte-exact separators, culture, ordering, and final newline"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task CompactFixedRowsAreByteExact()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, Content(ContextContentPartKind.Metadata));

        var rendered = ContextHumanRenderer.Render(Presentation(result, CliView.Compact));
        var rows = result.Sources
            .SelectMany(source => source.Layers.Select(layer => string.Create(
                CultureInfo.InvariantCulture,
                $"{layer.PathPosition} {source.Id ?? "none"} {layer.Path} {LayerName(layer.Kind)}{Environment.NewLine}")));
        var expected = $"context complete coverage=complete sources=2{Environment.NewLine}" + string.Concat(rows);

        Assert.Equal(expected, rendered);
    }

    [Fact(DisplayName = "Context JSON uses the frozen ordered schema from the same typed result")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task JsonUsesFrozenOrderedSchema()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(
            workspace,
            Content(ContextContentPartKind.Metadata),
            ContextLinkExpansion.All);

        var json = ContextJsonRenderer.Render(new CliPresentationRequest<ContextResult>(
            result,
            new CliPresentation(CliOutputFormat.Json, CliView.Compact, CliVerbosity.Normal)));
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

    [Fact(DisplayName = "Context help and diagnostics retain public grammar, streams, and bounded evidence")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task HelpAndDiagnosticsRetainPublicFacts()
    {
        var help = ContextHelpSections.Create();
        var text = string.Join('\n', help.Sections.Select(section => $"{section.Heading}\n{section.Body}"));
        Assert.Contains("--additions-only", text, StringComparison.Ordinal);
        Assert.Contains("--follow-links=<positive-depth|all>", text, StringComparison.Ordinal);
        Assert.Contains("Human complete, attention, and incomplete results use stdout", text, StringComparison.Ordinal);
        Assert.Contains("deterministic, stateless, and read-only", text, StringComparison.Ordinal);

        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, Content(ContextContentPartKind.Metadata));
        var diagnostics = ContextDiagnosticRenderer.Render(Presentation(
            result,
            CliView.Expanded,
            CliVerbosity.Verbose));
        Assert.NotNull(diagnostics);
        Assert.InRange(diagnostics.Length, 1, 4095);
        Assert.Contains("status=complete", diagnostics, StringComparison.Ordinal);
        Assert.Contains("links=0", diagnostics, StringComparison.Ordinal);
    }

    private static ValueTask<ContextResult> ExecuteAsync(
        ContextOperationWorkspace workspace,
        ContextContentSelection content,
        ContextLinkExpansion? links = null)
        => ContextOperationFactory.Create().ExecuteAsync(
            new ContextRequest(
                workspace: workspace.Workspace,
                sourceReferences: ["projects/guide"],
                additionsOnly: true,
                content: content,
                linkExpansion: links ?? ContextLinkExpansion.None,
                suppliedView: null,
                effectiveView: CliView.Expanded),
            TestContext.Current.CancellationToken);

    private static ContextContentSelection Content(params ContextContentPartKind[] kinds)
    {
        var values = kinds.Select(kind => new ContextContentPart(
            kind: kind,
            name: null,
            canonicalValue: kind.ToString().ToLowerInvariant())).ToArray();
        return new ContextContentSelection(
            supplied: values,
            effective: values.OrderBy(value => value.Kind));
    }

    private static CliPresentationRequest<ContextResult> Presentation(
        ContextResult result,
        CliView view,
        CliVerbosity verbosity = CliVerbosity.Normal)
        => new(result, new CliPresentation(CliOutputFormat.Human, view, verbosity));

    private static string LayerName(ContextSourceLayerKind kind)
        => kind == ContextSourceLayerKind.Base ? "base" : "overwrite";
}
