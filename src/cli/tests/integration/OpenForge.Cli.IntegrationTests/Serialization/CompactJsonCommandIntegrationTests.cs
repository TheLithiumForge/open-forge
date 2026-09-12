using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class CompactJsonCommandIntegrationTests
{
    [Theory(DisplayName = "Every command emits identified compact JSON without writes at an unavailable workspace")]
    [Trait("Feature", "compact-json"), Trait("Evidence", "Integration")]
    [InlineData("install", null)]
    [InlineData("update", null)]
    [InlineData("index", null)]
    [InlineData("repair", null)]
    [InlineData("cleanup", null)]
    [InlineData("doctor", null)]
    [InlineData("status", null)]
    [InlineData("find", null)]
    [InlineData("context", null)]
    [InlineData("references", null)]
    [InlineData("route", "list")]
    [InlineData("route", "inspect")]
    [InlineData("route", "create")]
    [InlineData("route", "init")]
    [InlineData("route", "update")]
    [InlineData("route", "move")]
    [InlineData("route", "remove")]
    [InlineData("extension", "list")]
    [InlineData("extension", "inspect")]
    [InlineData("extension", "create")]
    [InlineData("extension", "install")]
    [InlineData("extension", "update")]
    [InlineData("extension", "remove")]
    [InlineData("library", "list")]
    [InlineData("library", "inspect")]
    [InlineData("library", "attach")]
    [InlineData("library", "detach")]
    [InlineData("library", "sync")]
    public async Task EveryCommandSupportsCompactJson(string command, string? leaf)
    {
        using var workspace = TemporaryWorkspace.Create("compact-json-command");
        var before = workspace.SnapshotHashes();
        string[] path = leaf is null ? [command] : [command, leaf];
        string[] arguments = [.. path, "--workspace", workspace.Combine("missing"), "--json"];
        var compact = await CliHostCapture.RunAsync([.. arguments, "--view=compact"], workspace.Path);
        var expanded = await CliHostCapture.RunAsync([.. arguments, "--view=expanded"], workspace.Path);

        Assert.Equal(expanded.ExitCode, compact.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.DoesNotContain("\n", compact.Output.TrimEnd(), StringComparison.Ordinal);
        using var shortDocument = JsonDocument.Parse(compact.Output);
        using var fullDocument = JsonDocument.Parse(expanded.Output);
        var shortRoot = shortDocument.RootElement;
        var fullRoot = fullDocument.RootElement;
        Assert.Equal(2, shortRoot.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("compact", shortRoot.GetProperty("view").GetString());
        Assert.Equal(1, fullRoot.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(string.Join(' ', path), shortRoot.GetProperty("command").GetString());
        foreach (var coordinate in new[] { "command", "status", "workspace", "next" })
        {
            Assert.True(JsonElement.DeepEquals(shortRoot.GetProperty(coordinate), fullRoot.GetProperty(coordinate)), coordinate);
        }

        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
