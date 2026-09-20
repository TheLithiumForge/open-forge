using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectJsonPresentationTests
{
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
