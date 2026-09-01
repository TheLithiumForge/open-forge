using System.Text.Json;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ReferencesGeneratedSerializationTests
{
    [Fact(DisplayName = "References JSON document is registered as one concrete source-generated graph with reflection disabled"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReferencesDocumentUsesGeneratedMetadata()
    {
        var metadata = ReferencesJsonContext.Default.ReferencesJsonDocument;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("ReferencesJsonDocument", metadata.Type.Name);

        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            ["references", "docs", "--direction=out", "--json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        using var parsed = JsonDocument.Parse(result.Output);
        Assert.Equal("references", parsed.RootElement.GetProperty("command").GetString());
        Assert.Equal("out", parsed.RootElement.GetProperty("result").GetProperty("requestedDirection").GetString());
    }

    [Fact(DisplayName = "References generated JSON preserves exact member order, nullable values, arrays, locations, and duplicate occurrence graph"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReferencesGeneratedDocumentPreservesWireGraph()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            ["references", "docs", "--direction=both", "--include=alpha", "--exclude=beta", "--json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        using var parsed = JsonDocument.Parse(result.Output);
        var root = parsed.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        var commandResult = root.GetProperty("result");
        AssertPropertyOrder(commandResult, "source", "requestedDirection", "incomingSelection", "incoming", "outgoing", "findings");
        AssertPropertyOrder(commandResult.GetProperty("source"), "id", "path", "layers");
        AssertPropertyOrder(commandResult.GetProperty("incomingSelection"), "mode", "supplied", "resolved", "effectiveSources", "inspectedSources");
        var supplied = commandResult.GetProperty("incomingSelection").GetProperty("supplied").EnumerateArray().ToArray();
        Assert.Equal(2, supplied.Length);
        Assert.Equal(["include", "exclude"], supplied.Select(value => value.GetProperty("role").GetString()));
        AssertPropertyOrder(commandResult.GetProperty("incoming"), "coverage", "status", "occurrenceCount", "occurrences");
        AssertPropertyOrder(commandResult.GetProperty("outgoing"), "coverage", "status", "occurrenceCount", "occurrences");

        var occurrence = commandResult.GetProperty("outgoing").GetProperty("occurrences").EnumerateArray().First();
        AssertPropertyOrder(
            occurrence,
            "direction",
            "level",
            "source",
            "location",
            "destinationLocation",
            "rawDestination",
            "fragment",
            "target",
            "provenance");
        AssertPropertyOrder(occurrence.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        AssertPropertyOrder(occurrence.GetProperty("target"), "kind", "id", "path", "layer", "resolution", "network");
        Assert.Equal(1, occurrence.GetProperty("level").GetInt32());
        Assert.Equal("selected-source", occurrence.GetProperty("provenance").GetString());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("references-serialization");
        workspace.WriteText(".agents/loader.md", "# Loader\n");
        workspace.WriteText(
            ".agents/docs.md",
            "# Docs\n\n[alpha](target.md#overview)\n[external](https://example.invalid/x)\n");
        workspace.WriteText(
            ".agents/docs.overwrite.md",
            "# Docs Override\n\n[duplicate](target.md#overview)\n");
        workspace.WriteText(".agents/target.md", "# Target\n\n## Overview\n");
        workspace.WriteText(".agents/alpha.md", "# Alpha\n\n[docs](docs.md)\n");
        workspace.WriteText(".agents/beta.md", "# Beta\n\n[docs](docs.md)\n");
        return workspace;
    }
}
