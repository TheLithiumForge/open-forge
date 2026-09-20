using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class Schema3CommandIntegrationTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Every command emits one schema-3 JSON document at every detail without writes")]
    [Trait("Feature", "cli-serialization"), Trait("Evidence", "Integration")]
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
    public async Task EveryCommandSupportsSchema3AtEveryDetail(string command, int expectedExit, string expectedStatus)
    {
        using var workspace = TemporaryWorkspace.Create("schema3-json-command");
        var before = workspace.SnapshotHashes();
        var missingCatalogue = workspace.Combine("missing-catalogue");
        string[] arguments = [.. Arguments(command, missingCatalogue), "--workspace", workspace.Combine("missing"), "--format", "json"];
        JsonElement? baseline = null;
        int? exitCode = null;
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var output = await CliHostCapture.RunAsync([.. arguments, "--detail", detail], workspace.Path);
            Assert.Equal(expectedExit, output.ExitCode);
            Assert.DoesNotContain("\n", output.Output.TrimEnd(), StringComparison.Ordinal);
            Assert.DoesNotContain("\r", output.Output.TrimEnd(), StringComparison.Ordinal);
            if (detail != "debug")
            {
                Assert.Empty(output.Error);
            }

            using var document = JsonDocument.Parse(output.Output);
            var root = document.RootElement;
            Schema3Assertions.Envelope(root, command, expectedStatus, detail);
            if (baseline is { } first)
            {
                Assert.Equal(exitCode, output.ExitCode);
                foreach (var coordinate in new[] { "command", "status", "workspace", "counts", "next" })
                {
                    Assert.True(JsonElement.DeepEquals(first.GetProperty(coordinate), root.GetProperty(coordinate)), coordinate);
                }
            }
            else
            {
                baseline = root.Clone();
                exitCode = output.ExitCode;
            }
        }

        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine("missing")));
        Assert.False(Directory.Exists(missingCatalogue));
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
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, "The command case is not defined."),
        };
        return [.. command.Split(' '), .. operands];
    }
}
