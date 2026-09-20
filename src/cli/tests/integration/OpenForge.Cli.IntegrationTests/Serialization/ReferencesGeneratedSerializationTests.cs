using System.Text.Json;
using OpenForge.Cli.Core.Presentation.References.Shared.Rendering;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ReferencesGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References JSON document is registered as one concrete source-generated graph with reflection disabled"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReferencesDocumentUsesGeneratedMetadata()
    {
        var metadata = ReferencesDataJsonContext.Default.ReferencesData;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("ReferencesData", metadata.Type.Name);

        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            ["references", "docs", "--direction=out", "--format", "json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        using var parsed = JsonDocument.Parse(result.Output);
        Schema3Assertions.Envelope(parsed.RootElement, "references", "completed", "minimal");
        Assert.Equal("out", parsed.RootElement.GetProperty("data").GetProperty("direction").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "References generated JSON preserves exact member order, nullable values, arrays, locations, and duplicate occurrence graph"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task ReferencesGeneratedDocumentPreservesWireGraph()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            ["references", "docs", "--direction=both", "--include=alpha", "--exclude=beta", "--format", "json", "--detail", "full"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        using var parsed = JsonDocument.Parse(result.Output);
        var root = parsed.RootElement;
        Schema3Assertions.Envelope(root, "references", "completed", "full");
        var commandResult = root.GetProperty("data");

        // The command data is written in declaration order and omits what the level does not
        // select. `full` selects every member, so this is the complete graph.
        AssertPropertyOrder(commandResult, "source", "direction", "incoming", "outgoing", "coverage", "filters", "scanned");
        AssertPropertyOrder(commandResult.GetProperty("source"), "id", "path");
        AssertPropertyOrder(commandResult.GetProperty("coverage"), "incoming", "outgoing");
        AssertPropertyOrder(commandResult.GetProperty("filters"), "include", "exclude");
        Assert.Equal(["alpha"], commandResult.GetProperty("filters").GetProperty("include").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["beta"], commandResult.GetProperty("filters").GetProperty("exclude").EnumerateArray().Select(value => value.GetString()));

        var incoming = commandResult.GetProperty("incoming").EnumerateArray().First();
        AssertPropertyOrder(incoming, "path", "location", "layer");
        Assert.Equal(".agents/alpha.md", incoming.GetProperty("path").GetString());
        Assert.Equal("base", incoming.GetProperty("layer").GetString());

        var outgoing = commandResult.GetProperty("outgoing").EnumerateArray().First();
        AssertPropertyOrder(outgoing, "location", "destination", "resolvedPath", "state", "layer");
        Assert.Equal("target.md#overview", outgoing.GetProperty("destination").GetString());
        Assert.Equal(".agents/target.md", outgoing.GetProperty("resolvedPath").GetString());
        Assert.Equal("complete", outgoing.GetProperty("state").GetString());

        var scanned = commandResult.GetProperty("scanned").EnumerateArray().First();
        AssertPropertyOrder(scanned, "id", "path", "layer");

        // An external destination resolves to nothing, so the nullable member is omitted rather
        // than written as null.
        var external = commandResult.GetProperty("outgoing").EnumerateArray()
            .Single(value => value.GetProperty("state").GetString() == "external-unchecked");
        Assert.False(external.TryGetProperty("resolvedPath", out _));
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
