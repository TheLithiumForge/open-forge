using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Context;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ContextGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context JSON is one concrete source-generated ordered graph with reflection disabled")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ContextDocumentUsesGeneratedMetadataAndFrozenOrder()
    {
        var metadata = ContextPresentation.Rendering.DataJsonTypeInfo;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("ContextData", metadata.Type.Name);
        using var workspace = CreateWorkspace();

        var result = await CliHostCapture.RunAsync(
            [
                "context",
                "--content=paths,headings,section:Absent",
                "--follow-links=1",
                "--format", "json",
            ],
            workspace.Path);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Schema3Assertions.Envelope(root, "context", "completed-with-warnings", "minimal");
        var commandResult = root.GetProperty("data");
        AssertPropertyOrder(commandResult, "sources");
        AssertPropertyOrder(
            commandResult.GetProperty("sources")[0],
            "path",
            "id",
            "layer",
            "parts");
        var source = commandResult.GetProperty("sources")[0];
        Assert.True(source.GetProperty("parts").GetArrayLength() > 0);
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
        Assert.False(commandResult.TryGetProperty("links", out _));
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
            + "- [Docs](docs/_docs.md) - #LoadNow\n"
            );
        workspace.WriteText(
            ".agents/docs/_docs.md",
            Document(
                "Docs",
                "LoadNow",
                "# Docs\n\n[Target](target.md) [External](https://example.invalid).\n\n"
                + "## Entries\n\n"
                    + "- [Target](target.md) - #LoadNow\n"
                + ""));
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
