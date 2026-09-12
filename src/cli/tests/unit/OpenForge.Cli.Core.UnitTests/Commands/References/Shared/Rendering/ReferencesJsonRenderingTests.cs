using OpenForge.Cli.TestSupport;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesJsonRenderingTests
{
    [Fact(DisplayName = "References JSON projection preserves the exact schema-v1 envelope and command-local member order"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesExactEnvelopeAndResultOrder()
    {
        var json = ReferencesJsonRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.CompleteResult(),
                CliOutputFormat.Json,
                CliView.Expanded));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("references", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        AssertPropertyOrder(
            result,
            "source",
            "requestedDirection",
            "incomingSelection",
            "incoming",
            "outgoing",
            "findings");
        AssertPropertyOrder(result.GetProperty("source"), "id", "path", "layers");
        Assert.Equal(["base", "overwrite"], result.GetProperty("source").GetProperty("layers").EnumerateArray().Select(value => value.GetString()));
        AssertPropertyOrder(
            result.GetProperty("incomingSelection"),
            "mode",
            "supplied",
            "resolved",
            "effectiveSources",
            "inspectedSources");
        AssertPropertyOrder(result.GetProperty("incoming"), "coverage", "status", "occurrenceCount", "occurrences");
        AssertPropertyOrder(result.GetProperty("outgoing"), "coverage", "status", "occurrenceCount", "occurrences");
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "References JSON projection preserves nullable members, arrays, locations, finite values, external facts, and one-hop occurrence identity"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesRichGraphAndNullRules()
    {
        using var document = JsonDocument.Parse(
            ReferencesJsonRenderer.Render(
                ReferencesPresentationTestData.Presentation(
                    ReferencesPresentationTestData.AttentionResult(),
                    CliOutputFormat.Json,
                    CliView.Expanded)));
        var root = document.RootElement;
        var result = root.GetProperty("result");
        Assert.Equal("out", result.GetProperty("requestedDirection").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("incomingSelection").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("incoming").ValueKind);

        var outgoing = result.GetProperty("outgoing");
        var occurrences = outgoing.GetProperty("occurrences").EnumerateArray().ToArray();
        Assert.Equal(2, outgoing.GetProperty("occurrenceCount").GetInt32());
        AssertPropertyOrder(
            occurrences[0],
            "direction",
            "level",
            "source",
            "location",
            "destinationLocation",
            "rawDestination",
            "fragment",
            "target",
            "provenance");
        AssertPropertyOrder(occurrences[0].GetProperty("source"), "id", "path", "layer");
        AssertPropertyOrder(occurrences[0].GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        AssertPropertyOrder(occurrences[0].GetProperty("destinationLocation"), "line", "column", "byteOffset", "byteLength");
        Assert.Equal(1, occurrences[0].GetProperty("level").GetInt32());
        Assert.Equal("selected-source", occurrences[0].GetProperty("provenance").GetString());

        var external = occurrences[1].GetProperty("target");
        AssertPropertyOrder(external, "kind", "id", "path", "layer", "resolution", "network");
        Assert.Equal("external", external.GetProperty("kind").GetString());
        Assert.Equal("external-unchecked", external.GetProperty("resolution").GetString());
        Assert.Equal("network-not-attempted", external.GetProperty("network").GetString());
        Assert.Equal(JsonValueKind.Null, external.GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, external.GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Null, external.GetProperty("layer").ValueKind);

        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        AssertPropertyOrder(
            finding,
            "code",
            "status",
            "direction",
            "subject",
            "cause",
            "selectorRole",
            "selectorOccurrence",
            "source",
            "layer",
            "path",
            "location",
            "destinationLocation",
            "candidates");
        Assert.Equal("references.destination-unsupported", finding.GetProperty("code").GetString());
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("selectorRole").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("selectorOccurrence").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("destinationLocation").ValueKind);
        Assert.Empty(finding.GetProperty("candidates").EnumerateArray());
        AssertPropertyOrder(root.GetProperty("next"), "command", "reason");
    }

    [Fact(DisplayName = "References JSON views omit only supporting occurrence detail and invalid or blocked results retain their nullable section rules"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void JsonViewsRetainOccurrencesAndInvalidBlockedShapesRemainTyped()
    {
        var compact = ReferencesJsonRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.CompleteResult(),
                CliOutputFormat.Json,
                CliView.Compact));
        var expanded = ReferencesJsonRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.CompleteResult(),
                CliOutputFormat.Json,
                CliView.Expanded));
        Assert.True(JsonViewComparison.RetainsResult(compact, expanded, ["incoming.occurrences.*.provenance", "incoming.occurrences.*.destinationLocation", "incoming.occurrences.*.location.byteOffset", "incoming.occurrences.*.location.byteLength", "outgoing.occurrences.*.provenance", "outgoing.occurrences.*.destinationLocation", "outgoing.occurrences.*.location.byteOffset", "outgoing.occurrences.*.location.byteLength"]));

        using var invalid = JsonDocument.Parse(
            ReferencesJsonRenderer.Render(
                ReferencesPresentationTestData.Presentation(
                    ReferencesPresentationTestData.InvalidResult(),
                    CliOutputFormat.Json)));
        Assert.Equal("invalid", invalid.RootElement.GetProperty("status").GetString());
        var invalidResult = invalid.RootElement.GetProperty("result");
        Assert.Equal(JsonValueKind.Null, invalidResult.GetProperty("source").ValueKind);
        Assert.Equal(JsonValueKind.Null, invalidResult.GetProperty("requestedDirection").ValueKind);
        Assert.Equal(JsonValueKind.Null, invalidResult.GetProperty("incomingSelection").ValueKind);
        Assert.Equal(JsonValueKind.Null, invalidResult.GetProperty("incoming").ValueKind);
        Assert.Equal(JsonValueKind.Null, invalidResult.GetProperty("outgoing").ValueKind);
        AssertPropertyOrder(invalid.RootElement.GetProperty("next"), "command", "reason");

        using var blocked = JsonDocument.Parse(
            ReferencesJsonRenderer.Render(
                ReferencesPresentationTestData.Presentation(
                    ReferencesPresentationTestData.BlockedResult(),
                    CliOutputFormat.Json)));
        Assert.Equal("blocked", blocked.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, blocked.RootElement.GetProperty("workspace").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, blocked.RootElement.GetProperty("result").GetProperty("incoming").ValueKind);
        Assert.Equal("blocked", blocked.RootElement.GetProperty("result").GetProperty("incoming").GetProperty("coverage").GetString());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
