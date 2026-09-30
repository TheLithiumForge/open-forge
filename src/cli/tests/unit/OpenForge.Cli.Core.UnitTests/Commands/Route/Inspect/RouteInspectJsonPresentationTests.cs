using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Selection;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectJsonPresentationTests
{
    [Fact(DisplayName = "All-files JSON preserves its complete unmeasured answer and every field"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void AllFilesJsonKeepsNullCount()
    {
        var data = new RouteInspectMatchingFilesData
        {
            Scope = RouteInspectReportSelector.MatchingScope(RouteInspectMatchingFilesScope.AllFiles).Name,
            Complete = true,
            Count = null,
            Paths = [],
            PathLimit = 100,
            Truncated = false,
            Reason = null,
            Note = "No effective applyTo restriction. Every file applies. No scan was run.",
        };
        var json = JsonSerializer.Serialize(data, RouteInspectDataJsonContext.Default.RouteInspectMatchingFilesData);
        using var document = JsonDocument.Parse(json);
        var files = document.RootElement;
        Assert.Equal(["scope", "complete", "count", "paths", "pathLimit", "truncated", "reason", "note"],
            files.EnumerateObject().Select(property => property.Name));
        Assert.Equal("all-files", files.GetProperty("scope").GetString());
        Assert.True(files.GetProperty("complete").GetBoolean());
        Assert.Equal(JsonValueKind.Null, files.GetProperty("count").ValueKind);
        Assert.Empty(files.GetProperty("paths").EnumerateArray());
        Assert.Equal(100, files.GetProperty("pathLimit").GetInt32());
        Assert.False(files.GetProperty("truncated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, files.GetProperty("reason").ValueKind);
        Assert.Equal("No effective applyTo restriction. Every file applies. No scan was run.", files.GetProperty("note").GetString());
    }

    [Theory(DisplayName = "Matching-files JSON retains all facts at every detail and severity filter"),
     InlineData((int)CliDetail.Minimal), InlineData((int)CliDetail.Standard), InlineData((int)CliDetail.Full), InlineData((int)CliDetail.Debug),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MatchingFilesAreIndependentOfDetailAndFilter(int detail)
    {
        var basis = RouteInspectPresentationTestData.CompleteResult();
        foreach (var count in new[] { 0, 2, 143 })
        {
            var matching = RouteInspectMatchingFiles.Success(RouteInspectMatchingFilesScope.WorkspaceFiles,
                Enumerable.Range(0, count).Select(index => FormattableString.Invariant($"docs/{index:D3}.md")));
            var result = RouteInspectResult.Create(basis.Status, basis.Workspace, basis.Selection, basis.Identity, basis.Profile,
                basis.Observations, basis.Conditions, basis.Next, matchingFiles: matching);
            foreach (IReadOnlySet<CliSeverity>? filter in new IReadOnlySet<CliSeverity>?[] { null, new HashSet<CliSeverity> { CliSeverity.Error } })
            {
                var request = new CliPresentationRequest<RouteInspectResult>(result, new(CliFormat.Json, (CliDetail)detail, filter));
                var rendered = CliRenderingStage.Render(request, RouteInspectPresentation.Rendering);
                using var document = JsonDocument.Parse(rendered.PrimaryContent);
                var files = document.RootElement.GetProperty("data").GetProperty("matchingFiles");
                Assert.Equal(["scope", "complete", "count", "paths", "pathLimit", "truncated", "reason", "note"],
                    files.EnumerateObject().Select(property => property.Name));
                Assert.Equal("workspace-files", files.GetProperty("scope").GetString());
                Assert.True(files.GetProperty("complete").GetBoolean());
                Assert.Equal(count, files.GetProperty("count").GetInt32());
                Assert.Equal(matching.Paths, files.GetProperty("paths").EnumerateArray().Select(path => path.GetString()));
                Assert.Equal(100, files.GetProperty("pathLimit").GetInt32());
                Assert.Equal(count > 100, files.GetProperty("truncated").GetBoolean());
                Assert.Equal(JsonValueKind.Null, files.GetProperty("reason").ValueKind);
                Assert.Equal(count == 0 ? "No current files match this condition. Planned files may still match." : null,
                    files.GetProperty("note").GetString());
                var textRequest = new CliPresentationRequest<RouteInspectResult>(result, new(CliFormat.Text, (CliDetail)detail, filter));
                var text = CliRenderingStage.Render(textRequest, RouteInspectPresentation.Rendering).PrimaryContent;
                Assert.Contains(FormattableString.Invariant($"Matching files: {count}"), text, StringComparison.Ordinal);
                foreach (var path in matching.Paths)
                {
                    Assert.Contains($"  {path}", text, StringComparison.Ordinal);
                }
            }
        }
    }

    [Fact(DisplayName = "Matching-files mappings reject undefined enum values"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void UndefinedMatchingValuesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInspectReportSelector.MatchingScope((RouteInspectMatchingFilesScope)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteInspectReportSelector.MatchingReason((RouteInspectMatchingFilesReason)int.MaxValue));
        Assert.Null(RouteInspectReportSelector.ProjectMatchingFiles(null));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect JSON emits one schema-version-3 envelope for every semantic status")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete)]
    [InlineData((int)CliSemanticStatus.Attention)]
    [InlineData((int)CliSemanticStatus.Incomplete)]
    [InlineData((int)CliSemanticStatus.Invalid)]
    [InlineData((int)CliSemanticStatus.Blocked)]
    [InlineData((int)CliSemanticStatus.Failed)]
    [InlineData((int)CliSemanticStatus.Interrupted)]
    public void JsonStatusProjectionIsComplete(int statusValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var result = RouteInspectPresentationTestData.ForStatus(status);
        var json = RenderJson(result, CliDetail.Standard);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal(CliStatusDefinitions.Read(status).MachineName, root.GetProperty("status").GetString());
        Assert.True(root.GetProperty("data").TryGetProperty("belongs", out _));
        Assert.True(root.GetProperty("data").TryGetProperty("read", out _));
        Assert.True(root.GetProperty("data").TryGetProperty("size", out _));
        Assert.False(root.GetProperty("data").TryGetProperty("selection", out _));
        Assert.False(root.GetProperty("data").TryGetProperty("layers", out _));

        AssertTypedNext(root, result);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect JSON keeps the catalogue data members at minimal standard and full levels")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void JsonDataFollowsDetailLevels()
    {
        var result = RouteInspectPresentationTestData.CompleteResult();
        using var minimal = JsonDocument.Parse(RenderJson(result, CliDetail.Minimal));
        using var standard = JsonDocument.Parse(RenderJson(result, CliDetail.Standard));
        using var full = JsonDocument.Parse(RenderJson(result, CliDetail.Full));

        var minimalData = minimal.RootElement.GetProperty("data");
        var standardData = standard.RootElement.GetProperty("data");
        var fullData = full.RootElement.GetProperty("data");
        Assert.Equal(
            ["id", "path", "kind", "entrypointForm", "overwritePath", "belongs", "read", "size"],
            minimalData.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["id", "path", "kind", "entrypointForm", "overwritePath", "belongs", "read", "size", "axioms", "tags"],
            standardData.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["id", "path", "kind", "entrypointForm", "overwritePath", "belongs", "read", "size", "axioms", "tags", "selected", "selection", "layers", "statusReason"],
            fullData.EnumerateObject().Select(property => property.Name));

        Assert.Equal("root/item", minimalData.GetProperty("id").GetString());
        Assert.Equal(".agents/root/item/_item.md", minimalData.GetProperty("path").GetString());
        Assert.Equal("entrypoint", minimalData.GetProperty("kind").GetString());
        Assert.Equal("canonical", minimalData.GetProperty("entrypointForm").GetString());
        Assert.Equal(".agents/root/item/_item.overwrite.md", minimalData.GetProperty("overwritePath").GetString());

        var belongs = minimalData.GetProperty("belongs");
        Assert.Equal(["root", "item"], belongs.GetProperty("routeChain").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal("root", belongs.GetProperty("parent").GetString());
        Assert.Equal(2, belongs.GetProperty("directChildren").GetProperty("files").GetInt32());
        Assert.Equal(1, belongs.GetProperty("directChildren").GetProperty("entrypoints").GetInt32());
        Assert.Equal(4, belongs.GetProperty("descendants").GetProperty("files").GetInt32());
        Assert.Equal(2, belongs.GetProperty("descendants").GetProperty("entrypoints").GetInt32());

        var read = minimalData.GetProperty("read");
        Assert.True(read.GetProperty("atStart").GetBoolean());
        Assert.Equal(["root is read"], read.GetProperty("automaticallyWhen").EnumerateArray().Select(value => value.GetString()));
        Assert.True(read.GetProperty("mayReadAgain").GetBoolean());

        var own = minimalData.GetProperty("size").GetProperty("own");
        Assert.Equal(2, own.GetProperty("files").GetInt64());
        Assert.Equal(24, own.GetProperty("bytes").GetInt64());
        Assert.Equal(5, own.GetProperty("tokens").GetInt64());
        Assert.Equal(["loader", "root"], standardData.GetProperty("axioms").GetProperty("inheritedFrom").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal("no local Axioms section", standardData.GetProperty("axioms").GetProperty("local").GetString());
        Assert.Empty(standardData.GetProperty("tags").EnumerateArray());

        var selected = fullData.GetProperty("selected");
        Assert.Equal(4, selected.GetProperty("closure").GetProperty("files").GetInt64());
        Assert.Equal(64, selected.GetProperty("closure").GetProperty("bytes").GetInt64());
        Assert.Equal(12, selected.GetProperty("closure").GetProperty("tokens").GetInt64());
        Assert.Equal("source-id", fullData.GetProperty("selection").GetProperty("kind").GetString());
        Assert.Equal("automatic-id", fullData.GetProperty("selection").GetProperty("method").GetString());
        Assert.Equal("root/item", fullData.GetProperty("selection").GetProperty("requested").GetString());
        Assert.Equal(
            [".agents/root/item/_item.md", ".agents/root/item/_item.overwrite.md"],
            fullData.GetProperty("layers").EnumerateArray().Select(layer => layer.GetProperty("path").GetString()));
        Assert.Equal(["base", "overwrite"], fullData.GetProperty("layers").EnumerateArray().Select(layer => layer.GetProperty("kind").GetString()));
        Assert.Equal("all applicable route facts are available", fullData.GetProperty("statusReason").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect JSON preserves unavailable measurements as null data while retaining their warning")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void JsonDataPreservesUnavailableMeasurement()
    {
        var result = RouteInspectPresentationTestData.IncompleteResult();
        using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Standard));
        var root = document.RootElement;
        var own = root.GetProperty("data").GetProperty("size").GetProperty("own");

        Assert.Equal(JsonValueKind.Null, own.ValueKind);
        var warning = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("route-inspect.unavailable-fact", warning.GetProperty("code").GetString());
        Assert.Contains("could not be measured", warning.GetProperty("message").GetString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect JSON preserves the typed next action for every fixed action result")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData("interactive-attention")]
    [InlineData("incomplete")]
    [InlineData("blocked-collision")]
    [InlineData("blocked-fallback")]
    [InlineData("blocked-direct")]
    [InlineData("invalid")]
    [InlineData("failed")]
    [InlineData("interrupted")]
    public void JsonProjectionPreservesTypedNextAction(string scenario)
    {
        var result = scenario switch
        {
            "interactive-attention" => RouteInspectPresentationTestData.InteractiveAttentionResult(),
            "incomplete" => RouteInspectPresentationTestData.IncompleteResult(),
            "blocked-collision" => RouteInspectPresentationTestData.BlockedCollisionResult(),
            "blocked-fallback" => RouteInspectPresentationTestData.FallbackBlockedResult(),
            "blocked-direct" => RouteInspectPresentationTestData.DirectCorrectionBlockedResult(),
            "invalid" => RouteInspectPresentationTestData.InvalidResult(),
            "failed" => RouteInspectPresentationTestData.FailedResult(),
            "interrupted" => RouteInspectPresentationTestData.InterruptedResult(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The fixed next-action scenario is unknown."),
        };
        using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Minimal));

        AssertTypedNext(document.RootElement, result);
    }

    private static string RenderJson(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail, CliFormat.Json),
            RouteInspectPresentation.Rendering).PrimaryContent;

    private static void AssertTypedNext(JsonElement root, RouteInspectResult result)
    {
        var expected = result.Next;
        if (expected is null)
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
            return;
        }

        var next = root.GetProperty("next");
        Assert.Equal(expected.Command, next.GetProperty("command").GetString());
        Assert.Equal(expected.Reason, next.GetProperty("reason").GetString());
    }
}
