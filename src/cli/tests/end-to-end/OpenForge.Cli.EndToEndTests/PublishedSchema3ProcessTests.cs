using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedSchema3ProcessTests
{
    [Theory(DisplayName = "Every published command preserves its unavailable-workspace result across formats and detail levels"),
     Trait("Feature", "cli-serialization"), Trait("Evidence", "EndToEnd")]
    [InlineData("install", 5, "blocked")]
    [InlineData("update", 5, "blocked")]
    [InlineData("index", 5, "blocked")]
    [InlineData("repair", 4, "invalid-input")]
    [InlineData("cleanup", 5, "blocked")]
    [InlineData("doctor", 5, "blocked")]
    [InlineData("status", 5, "blocked")]
    [InlineData("find", 5, "blocked")]
    [InlineData("context", 5, "blocked")]
    [InlineData("references", 5, "blocked")]
    [InlineData("route list", 4, "invalid-input")]
    [InlineData("route inspect", 4, "invalid-input")]
    [InlineData("route create", 4, "invalid-input")]
    [InlineData("route init", 4, "invalid-input")]
    [InlineData("route update", 4, "invalid-input")]
    [InlineData("route move", 4, "invalid-input")]
    [InlineData("route remove", 4, "invalid-input")]
    [InlineData("extension list", 5, "blocked")]
    [InlineData("extension inspect", 5, "blocked")]
    [InlineData("extension create", 4, "invalid-input")]
    [InlineData("extension install", 4, "invalid-input")]
    [InlineData("extension update", 4, "invalid-input")]
    [InlineData("extension remove", 4, "invalid-input")]
    [InlineData("library list", 3, "incomplete")]
    [InlineData("library inspect", 3, "incomplete")]
    [InlineData("library attach", 3, "incomplete")]
    [InlineData("library detach", 3, "incomplete")]
    [InlineData("library sync", 3, "incomplete")]
    public async Task EveryPublishedBindingSupportsBothFormatsAtEveryDetail(string command, int expectedExit, string expectedStatus)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = TemporaryWorkspace.Create("published-schema3");
        using var lockStore = PublishedWorkspaceLockStore.Create("published-schema3-store");
        var missingWorkspace = workspace.Combine("missing-workspace");
        var missingCatalogue = workspace.Combine("missing-catalogue");
        _ = lockStore.Track(missingWorkspace);
        workspace.WriteText("keep.txt", "Unchanged process evidence.\r\n");
        var ownershipMarker = Path.Combine(workspace.Path, ".open-forge-test-workspace-owner");
        var ownershipBytes = File.ReadAllBytes(ownershipMarker);
        var arguments = Arguments(command, missingCatalogue);
        JsonElement? baseline = null;
        ProcessRunResult? fullText = null;

        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var json = await PublishedProcessTestSupport.RunWithoutWritesAsync(
                target, workspace.Path, workspace.SnapshotHashes,
                [.. arguments, "--workspace", missingWorkspace, "--format", "json", "--detail", detail],
                lockStore.EnvironmentVariables);
            Assert.Equal(expectedExit, json.ExitCode);
            if (detail != "debug") Assert.Equal(string.Empty, json.StandardError);
            var jsonBody = json.StandardOutput.TrimEnd('\r', '\n');
            Assert.DoesNotContain('\r', jsonBody);
            Assert.DoesNotContain('\n', jsonBody);
            using var document = JsonDocument.Parse(json.StandardOutput);
            var root = document.RootElement;
            AssertEnvelope(root, command, expectedStatus, detail);
            if (baseline is { } first)
            {
                foreach (var coordinate in new[] { "command", "status", "workspace", "counts", "next" })
                {
                    Assert.True(JsonElement.DeepEquals(first.GetProperty(coordinate), root.GetProperty(coordinate)), coordinate);
                }
            }
            else
            {
                baseline = root.Clone();
            }

            var text = await PublishedProcessTestSupport.RunWithoutWritesAsync(
                target, workspace.Path, workspace.SnapshotHashes,
                [.. arguments, "--workspace", missingWorkspace, "--format", "text", "--detail", detail],
                lockStore.EnvironmentVariables);
            Assert.Equal(expectedExit, text.ExitCode);
            if (expectedExit == 3)
            {
                Assert.NotEmpty(text.StandardOutput);
                Assert.Equal(json.StandardError, text.StandardError);
            }
            else
            {
                Assert.Equal(string.Empty, text.StandardOutput);
                Assert.NotEmpty(text.StandardError);
            }

            if (detail == "full") fullText = text;
            if (detail == "debug")
            {
                Assert.NotNull(fullText);
                Assert.Equal(fullText.StandardOutput, text.StandardOutput);
                Assert.Equal(fullText.StandardError + json.StandardError, text.StandardError);
            }

            Assert.False(Directory.Exists(missingWorkspace));
            Assert.False(File.Exists(missingWorkspace));
            Assert.False(Directory.Exists(missingCatalogue));
            Assert.False(File.Exists(missingCatalogue));
            Assert.Equal(
                [".open-forge-test-workspace-owner", "keep.txt"],
                Directory.EnumerateFileSystemEntries(workspace.Path).Select(Path.GetFileName).Order(StringComparer.Ordinal));
            Assert.Equal(ownershipBytes, File.ReadAllBytes(ownershipMarker));
            lockStore.AssertNoInfrastructure();
        }
    }

    private static string[] Arguments(string command, string missingCatalogue)
    {
        string[] operands = command switch
        {
            "references" or "route list" or "route inspect" or "route remove" => ["root"],
            "route create" => ["docs/example.md", "--description", "Example", "--tag", "Docs"],
            "route init" => ["docs"],
            "route update" => ["root", "--description", "Updated description"],
            "route move" => ["root", "docs/moved.md"],
            "extension inspect" or "extension install" or "extension update" or "extension remove" => ["sample-package"],
            "extension create" => ["sample-package", "--path", missingCatalogue, "--automatic", "--dry-run"],
            "library inspect" or "library detach" or "library sync" => ["sample-library"],
            "library attach" => ["sample-library", "library-source"],
            "install" or "update" or "index" or "repair" or "cleanup" or "doctor" or "status" or "find" or "context"
                or "extension list" or "library list" => [],
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, "The published command case is not defined."),
        };
        return [.. command.Split(' '), .. operands];
    }

    private static void AssertEnvelope(JsonElement root, string command, string status, string detail)
    {
        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(detail, root.GetProperty("detail").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("filter").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("summary").ValueKind);
        Assert.NotEmpty(root.GetProperty("summary").GetProperty("headline").GetString() ?? string.Empty);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("findings").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("effects").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("counts").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("limitations").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("data").ValueKind);
    }
}
