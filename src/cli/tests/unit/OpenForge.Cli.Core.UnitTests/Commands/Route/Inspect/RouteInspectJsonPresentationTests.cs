using OpenForge.Cli.TestSupport;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectJsonPresentationTests
{
    [Theory(DisplayName = "Route Inspect JSON emits one complete envelope for every semantic status")]
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
        var json = RouteInspectJsonRenderer.Render(
            RouteInspectPresentationTestData.Presentation(
                result,
                CliView.Expanded,
                CliOutputFormat.Json));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal(CliStatusDefinitions.Read(status).MachineName, root.GetProperty("status").GetString());
        Assert.True(root.GetProperty("result").TryGetProperty("selection", out _));
        Assert.True(root.GetProperty("result").TryGetProperty("observations", out _));
        Assert.True(root.GetProperty("result").TryGetProperty("conditions", out _));

        AssertTypedNext(root, result);
    }

    [Fact(DisplayName = "Route Inspect JSON projection retains complete typed identity profile facts and ordered arrays independent of view")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void JsonProjectionRetainsCompleteTypedGraphAcrossViews()
    {
        var result = RouteInspectPresentationTestData.CompleteResult();
        var compact = RouteInspectJsonRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Compact, CliOutputFormat.Json));
        var expanded = RouteInspectJsonRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Expanded, CliOutputFormat.Json));

        Assert.True(JsonViewComparison.RetainsResult(compact, expanded));
        using var document = JsonDocument.Parse(compact);
        var root = document.RootElement;
        var workspace = root.GetProperty("workspace");
        Assert.Equal(
            ["path", "selectedBy"],
            workspace.EnumerateObject().Select(property => property.Name));
        var workspaceValue = Assert.IsType<CliWorkspace>(result.Workspace);
        Assert.Equal(workspaceValue.LexicalRoot, workspace.GetProperty("path").GetString());
        Assert.Equal("current-directory", workspace.GetProperty("selectedBy").GetString());

        var selection = root.GetProperty("result").GetProperty("selection");
        Assert.Equal(
            ["referenceKind", "selectionMethod", "requestedReference", "candidatePaths"],
            selection.EnumerateObject().Select(property => property.Name));
        Assert.Equal("source-id", selection.GetProperty("referenceKind").GetString());
        Assert.Equal("automatic-id", selection.GetProperty("selectionMethod").GetString());
        Assert.Equal(result.Selection.RequestedReference, selection.GetProperty("requestedReference").GetString());
        Assert.Empty(selection.GetProperty("candidatePaths").EnumerateArray());

        var identity = root.GetProperty("result").GetProperty("identity");
        Assert.Equal(
            ["id", "path", "sourceKind", "sourceForm", "routeState", "physicalLayers"],
            identity.EnumerateObject().Select(property => property.Name));
        var identityValue = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(identityValue.Id, identity.GetProperty("id").GetString());
        Assert.Equal(identityValue.CanonicalWorkspaceRelativePath, identity.GetProperty("path").GetString());
        Assert.Equal("entrypoint", identity.GetProperty("sourceKind").GetString());
        Assert.Equal("canonical", identity.GetProperty("sourceForm").GetString());
        Assert.Equal("routed", identity.GetProperty("routeState").GetString());

        var layers = identity.GetProperty("physicalLayers").EnumerateArray().ToArray();
        Assert.Equal(2, layers.Length);
        Assert.Equal(
            ["workspaceRelativePath", "physicalPath", "role"],
            layers[0].EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["workspaceRelativePath", "physicalPath", "role"],
            layers[1].EnumerateObject().Select(property => property.Name));
        AssertPhysicalLayer(layers[0], identityValue.PhysicalLayers[0], "base");
        AssertPhysicalLayer(layers[1], identityValue.PhysicalLayers[1], "overwrite");

        var profile = root.GetProperty("result").GetProperty("profile");
        Assert.Equal(
            ["reading", "measurements", "topology", "axioms", "completeness", "safety"],
            profile.EnumerateObject().Select(property => property.Name));
        Assert.Equal("complete", profile.GetProperty("completeness").GetString());
        Assert.Equal("safe", profile.GetProperty("safety").GetString());

        var reading = profile.GetProperty("reading");
        Assert.Equal(
            ["taskStart", "automatic", "later"],
            reading.EnumerateObject().Select(property => property.Name));
        var taskStart = reading.GetProperty("taskStart");
        Assert.Equal("value", taskStart.GetProperty("state").GetString());
        Assert.True(taskStart.GetProperty("value").GetBoolean());
        Assert.Equal(JsonValueKind.Null, taskStart.GetProperty("reason").ValueKind);

        var automatic = reading.GetProperty("automatic");
        Assert.Equal("value", automatic.GetProperty("state").GetString());
        var automaticReason = Assert.Single(automatic.GetProperty("value").GetProperty("reasons").EnumerateArray());
        Assert.Equal("parent-load-now", automaticReason.GetProperty("kind").GetString());
        Assert.Equal("root", automaticReason.GetProperty("relatedSourceId").GetString());
        Assert.Equal(["exposing-parent-read"], automaticReason.GetProperty("events").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(JsonValueKind.Null, automatic.GetProperty("reason").ValueKind);

        var later = reading.GetProperty("later");
        Assert.Equal("value", later.GetProperty("state").GetString());
        Assert.True(later.GetProperty("value").GetProperty("mayBeReadAgain").GetBoolean());
        Assert.Equal(
            ["context-restoration", "handoff", "closeout", "followup-transition"],
            later.GetProperty("value").GetProperty("occasions").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(JsonValueKind.Null, later.GetProperty("reason").ValueKind);

        var measurements = profile.GetProperty("measurements");
        Assert.Equal(
            ["ownSource", "selectedClosure", "taskStartOverlap", "selectionAddition", "loadNowDescendants"],
            measurements.EnumerateObject().Select(property => property.Name));
        var profileValue = Assert.IsType<RouteInspectProfile>(result.Profile);
        AssertMeasurement(measurements.GetProperty("ownSource"), profileValue.Measurements.OwnSource);
        AssertMeasurement(measurements.GetProperty("selectedClosure"), profileValue.Measurements.SelectedClosure);
        AssertMeasurement(measurements.GetProperty("taskStartOverlap"), profileValue.Measurements.TaskStartOverlap);
        AssertMeasurement(measurements.GetProperty("selectionAddition"), profileValue.Measurements.SelectionAddition);
        AssertMeasurement(measurements.GetProperty("loadNowDescendants"), profileValue.Measurements.LoadNowDescendants);

        var topology = profile.GetProperty("topology");
        Assert.Equal("value", topology.GetProperty("state").GetString());
        var topologyValue = topology.GetProperty("value");
        Assert.Equal("root", topologyValue.GetProperty("rootRoute").GetString());
        Assert.Equal(["root", "item"], topologyValue.GetProperty("routeChain").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal("root", topologyValue.GetProperty("parentId").GetString());
        Assert.Equal(2, topologyValue.GetProperty("depth").GetInt32());
        var counts = topologyValue.GetProperty("counts");
        Assert.Equal("value", counts.GetProperty("state").GetString());
        Assert.Equal(2, counts.GetProperty("value").GetProperty("directRoutedFileCount").GetInt32());
        Assert.Equal(1, counts.GetProperty("value").GetProperty("directEntrypointCount").GetInt32());
        Assert.Equal(4, counts.GetProperty("value").GetProperty("descendantRoutedFileCount").GetInt32());
        Assert.Equal(2, counts.GetProperty("value").GetProperty("descendantEntrypointCount").GetInt32());
        Assert.Equal(JsonValueKind.Null, topology.GetProperty("reason").ValueKind);

        var axioms = profile.GetProperty("axioms");
        Assert.Equal("value", axioms.GetProperty("state").GetString());
        var inherited = axioms.GetProperty("value").GetProperty("inherited");
        Assert.Equal("value", inherited.GetProperty("state").GetString());
        Assert.Equal(["loader", "root"], inherited.GetProperty("value").GetProperty("sourceIds").EnumerateArray().Select(value => value.GetString()));
        var local = axioms.GetProperty("value").GetProperty("local");
        Assert.Equal("value", local.GetProperty("state").GetString());
        Assert.Equal("missing", local.GetProperty("value").GetString());
        Assert.Empty(root.GetProperty("result").GetProperty("observations").EnumerateArray());
        Assert.Empty(root.GetProperty("result").GetProperty("conditions").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Route Inspect JSON preserves unavailable and not-applicable tri-state facts with reasons")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesTriStateFacts()
    {
        var json = RouteInspectJsonRenderer.Render(
            RouteInspectPresentationTestData.Presentation(
                RouteInspectPresentationTestData.IncompleteResult(),
                CliView.Expanded,
                CliOutputFormat.Json));

        using var document = JsonDocument.Parse(json);
        var measurements = document.RootElement.GetProperty("result").GetProperty("profile")
            .GetProperty("measurements");
        var unavailable = measurements.GetProperty("ownSource");
        Assert.Equal("unavailable", unavailable.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, unavailable.GetProperty("value").ValueKind);
        var unavailableReason = Assert.IsType<string>(unavailable.GetProperty("reason").GetString());
        Assert.NotEmpty(unavailableReason);
        var notApplicable = measurements.GetProperty("loadNowDescendants");
        Assert.Equal("not-applicable", notApplicable.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, notApplicable.GetProperty("value").ValueKind);
        var notApplicableReason = Assert.IsType<string>(notApplicable.GetProperty("reason").GetString());
        Assert.NotEmpty(notApplicableReason);
    }

    [Theory(DisplayName = "Route Inspect JSON preserves typed next command and reason for every fixed action result")]
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
        var json = RouteInspectJsonRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Compact, CliOutputFormat.Json));
        using var document = JsonDocument.Parse(json);

        AssertTypedNext(document.RootElement, result);
        if (scenario == "blocked-fallback")
        {
            var selection = document.RootElement.GetProperty("result").GetProperty("selection");
            Assert.Equal("source-id", selection.GetProperty("referenceKind").GetString());
            Assert.Equal("root/unsafe", selection.GetProperty("requestedReference").GetString());
        }
    }

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

    private static void AssertPhysicalLayer(
        JsonElement json,
        OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution.RouteInspectPhysicalLayer expected,
        string role)
    {
        Assert.Equal(expected.WorkspaceRelativePath, json.GetProperty("workspaceRelativePath").GetString());
        Assert.Equal(expected.PhysicalPath, json.GetProperty("physicalPath").GetString());
        Assert.Equal(role, json.GetProperty("role").GetString());
    }

    private static void AssertMeasurement(
        JsonElement json,
        RouteInspectFact<RouteInspectMeasurement> expected)
    {
        Assert.Equal("value", json.GetProperty("state").GetString());
        var value = json.GetProperty("value");
        var expectedMeasurement = Assert.IsType<RouteInspectMeasurement>(expected.Value);
        Assert.Equal(expectedMeasurement.PhysicalFileCount, value.GetProperty("physicalFileCount").GetInt64());
        Assert.Equal(expectedMeasurement.UnicodeScalarCount, value.GetProperty("unicodeScalarCount").GetInt64());
        Assert.Equal(expectedMeasurement.Utf8ByteCount, value.GetProperty("utf8ByteCount").GetInt64());
        Assert.Equal(expectedMeasurement.EstimatedTokens, value.GetProperty("estimatedTokens").GetInt64());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("reason").ValueKind);
    }
}
