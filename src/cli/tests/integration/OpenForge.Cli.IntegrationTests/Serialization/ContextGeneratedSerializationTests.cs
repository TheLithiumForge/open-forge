using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ContextGeneratedSerializationTests
{
    [Fact(DisplayName = "Context JSON is one concrete source-generated ordered graph with reflection disabled")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ContextDocumentUsesGeneratedMetadataAndFrozenOrder()
    {
        var metadata = ContextJsonContext.Default.ContextJsonDocument;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("ContextJsonDocument", metadata.Type.Name);
        using var workspace = CreateWorkspace();

        var result = await CliHostCapture.RunAsync(
            [
                "context",
                "--content=paths,headings,section:Absent",
                "--follow-links=1",
                "--json",
            ],
            workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        var commandResult = root.GetProperty("result");
        AssertPropertyOrder(commandResult, "selection", "presentation", "coverage", "paths", "links", "sources", "findings");
        AssertPropertyOrder(commandResult.GetProperty("selection"), "requestedSources", "startupIncluded", "additionsOnly", "linkExpansion", "sourceCount");
        AssertPropertyOrder(commandResult.GetProperty("presentation"), "view", "content");
        AssertPropertyOrder(commandResult.GetProperty("coverage"), "state", "selection", "links", "projection");
        AssertPropertyOrder(
            commandResult.GetProperty("paths")[0],
            "position",
            "sourcePosition",
            "id",
            "path",
            "layer",
            "inclusionReasons");
        AssertPropertyOrder(
            commandResult.GetProperty("links")[0],
            "depth",
            "source",
            "location",
            "destinationLocation",
            "rawDestination",
            "fragment",
            "target",
            "disposition");
        var source = commandResult.GetProperty("sources")[0];
        AssertPropertyOrder(source, "position", "id", "path", "routeState", "route", "scope", "inclusionReasons", "layers");
        AssertPropertyOrder(source.GetProperty("layers")[0], "pathPosition", "kind", "path", "inclusionReasons", "projections");
        var finding = commandResult.GetProperty("findings")[0];
        AssertPropertyOrder(
            finding,
            "code",
            "status",
            "subject",
            "cause",
            "reference",
            "source",
            "layer",
            "path",
            "part",
            "location",
            "destinationLocation",
            "candidates");
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("attention", root.GetProperty("status").GetString());
        Assert.Equal("complete", commandResult.GetProperty("coverage").GetProperty("links").GetString());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("context-serialization");
        workspace.WriteText("AGENTS.md", "# Workspace\n");
        workspace.WriteText(
            ".agents/loader.md",
            "# Loader\n\n## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- [Docs](docs/_docs.md) - #LoadNow\n"
            + "<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(
            ".agents/docs/_docs.md",
            Document(
                "Docs",
                "LoadNow",
                "# Docs\n\n[Target](target.md) [External](https://example.invalid).\n\n"
                + "## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Target](target.md) - #LoadNow\n"
                + "<!-- open-forge:generated-index:end -->\n"));
        workspace.WriteText(
            ".agents/docs/target.md",
            Document("Target", "LoadNow", "# Target\n"));
        return workspace;
    }

    private static string Document(string description, string tags, string body)
        => OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags.Split(", ", StringSplitOptions.None),
            body: $"\n{body}");
}
