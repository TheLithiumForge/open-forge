using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.IntegrationTests.Commands.Index;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class IndexGeneratedSerializationTests
{
    [Fact(DisplayName = "Index JSON is one source-generated schema-v1 graph with frozen order and explicit nulls"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task IndexDocumentUsesGeneratedMetadataAndFrozenOrder()
    {
        var metadata = IndexJsonContext.Default.IndexJsonDocument;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("IndexJsonDocument", metadata.Type.Name);
        using var workspace = IndexOperationWorkspace.Create("index-serialization");

        var result = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--json"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        var workspaceValue = root.GetProperty("workspace");
        AssertPropertyOrder(workspaceValue, "path", "selectedBy");
        var commandResult = root.GetProperty("result");
        AssertPropertyOrder(commandResult, "mode", "selection", "regions", "recovery", "findings", "counts");
        AssertPropertyOrder(commandResult.GetProperty("selection"), "origin", "scope", "sources");
        AssertPropertyOrder(commandResult.GetProperty("selection").GetProperty("sources")[0], "id", "path", "scope");
        var region = commandResult.GetProperty("regions")[0];
        AssertPropertyOrder(region, "source", "action", "beforeEntryCount", "expectedEntryCount", "change", "outcome");
        AssertPropertyOrder(region.GetProperty("change"), "beforeBody", "expectedBody");
        AssertPropertyOrder(commandResult.GetProperty("recovery"), "state", "residualPath");
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("findings").ValueKind);
        AssertPropertyOrder(commandResult.GetProperty("counts"), "regions", "updates", "unchanged", "applied", "verified");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Index blocked JSON retains exact finding null array and next-action coordinates"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task BlockedIndexDocumentRetainsFindingAndNextCoordinates()
    {
        using var workspace = IndexOperationWorkspace.Create("index-serialization-blocked");
        var missing = Path.Combine(workspace.Workspace.LexicalRoot, "missing");

        var result = await CliHostCapture.RunAsync(
            ["index", "--workspace", missing, "--json"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(root.GetProperty("result").GetProperty("findings").EnumerateArray());
        AssertPropertyOrder(finding, "code", "status", "sourceOccurrence", "source", "cause", "candidates");
        Assert.Equal("index.workspace-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("sourceOccurrence").ValueKind);
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("source").ValueKind);
        Assert.Empty(finding.GetProperty("candidates").EnumerateArray());
        var next = root.GetProperty("next");
        AssertPropertyOrder(next, "command", "reason");
        Assert.Equal("open-forge doctor", next.GetProperty("command").GetString());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
