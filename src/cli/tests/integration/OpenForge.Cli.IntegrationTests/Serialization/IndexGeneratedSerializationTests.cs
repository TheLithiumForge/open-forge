using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.IntegrationTests.Commands.Index;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class IndexGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index JSON uses generated data metadata inside the schema-3 envelope with frozen order"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task IndexDocumentUsesGeneratedMetadataAndFrozenOrder()
    {
        var metadata = IndexPresentation.Rendering.DataJsonTypeInfo;
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("IndexData", metadata.Type.Name);
        using var workspace = IndexOperationWorkspace.Create("index-serialization");

        var result = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--format", "json", "--detail", "full"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        var workspaceValue = root.GetProperty("workspace");
        AssertPropertyOrder(workspaceValue, "path", "selectedBy");
        var data = root.GetProperty("data");
        AssertPropertyOrder(data, "mode", "changes", "unchanged", "selection", "regions");
        AssertPropertyOrder(data.GetProperty("selection"), "origin", "scope", "sources");
        AssertPropertyOrder(data.GetProperty("selection").GetProperty("sources")[0], "id", "path");
        AssertPropertyOrder(data.GetProperty("regions")[0], "path", "action", "outcome");
        AssertPropertyOrder(root.GetProperty("recovery"), "path", "disposition");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("findings").ValueKind);
        AssertPropertyOrder(root.GetProperty("counts"), "filesChecked", "filesUpdated", "filesCurrent");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index blocked JSON retains exact finding null array and next-action coordinates"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task BlockedIndexDocumentRetainsFindingAndNextCoordinates()
    {
        using var workspace = IndexOperationWorkspace.Create("index-serialization-blocked");
        var missing = Path.Combine(workspace.Workspace.LexicalRoot, "missing");

        var result = await CliHostCapture.RunAsync(
            ["index", "--workspace", missing, "--format", "json", "--detail", "full"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        AssertPropertyOrder(finding, "severity", "code", "title", "message", "subject", "category", "resolution", "actions", "candidates", "evidence", "provenance");
        Assert.Equal("index.workspace-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal("error", finding.GetProperty("severity").GetString());
        Assert.Equal(JsonValueKind.Null, finding.GetProperty("subject").GetProperty("location").ValueKind);
        Assert.Empty(finding.GetProperty("candidates").EnumerateArray());
        var next = root.GetProperty("next");
        AssertPropertyOrder(next, "kind", "command", "reason");
        Assert.Equal("open-forge doctor", next.GetProperty("command").GetString());
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
