using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Presentation.Invariants;

public sealed class Task32WorkspacePolicyTests
{
    private static readonly WorkspacePolicyFamily[] CorrectedFamilies =
    [
        new("Cleanup", "Commands/Cleanup"),
        new("Repair", "Commands/Repair"),
        new("Extension Install", "Commands/Extension/Install"),
        new("Extension Update", "Commands/Extension/Update"),
        new("Extension Remove", "Commands/Extension/Remove"),
        new("Library Sync", "Commands/Library/Sync"),
        new("Route Remove", "Commands/Route/Remove"),
    ];

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Task 32 workspace echoes match JSON facts in corrected command captures"), Trait("Feature", "task32-presentation"), Trait("Evidence", "Integration")]
    public void MinimalWorkspaceEchoMatchesCapturedWorkspaceFacts()
    {
        var root = IntegrationProjectRoot();
        var snapshotDirectories = Directory
            .EnumerateDirectories(root, "__snapshots__", SearchOption.AllDirectories)
            .ToArray();

        foreach (var family in CorrectedFamilies)
        {
            var capturedPairs = 0;
            foreach (var snapshotDirectory in snapshotDirectories)
            {
                var relativeDirectory = Path.GetRelativePath(root, snapshotDirectory).Replace('\\', '/');
                if (!string.Equals(
                    relativeDirectory,
                    $"{family.RelativeDirectory}/__snapshots__",
                    StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var textPath in Directory.EnumerateFiles(snapshotDirectory, "*.minimal.txt", SearchOption.AllDirectories))
                {
                    var textName = Path.GetFileName(textPath);
                    if (textName.EndsWith(".json.minimal.txt", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var jsonPath = Path.Combine(
                        Path.GetDirectoryName(textPath)!,
                        textName.Replace(".minimal.txt", ".json.minimal.txt", StringComparison.Ordinal));
                    if (!File.Exists(jsonPath))
                    {
                        continue;
                    }

                    var jsonContent = File.ReadAllText(jsonPath);
                    JsonDocument document;
                    try
                    {
                        document = JsonDocument.Parse(jsonContent);
                    }
                    catch (JsonException)
                    {
                        Assert.NotEmpty(jsonContent);
                        Assert.DoesNotContain("schemaVersion", jsonContent, StringComparison.Ordinal);
                        var trimmedContent = jsonContent.TrimStart();
                        Assert.False(
                            trimmedContent.StartsWith('{') || trimmedContent.StartsWith('['),
                            $"Malformed JSON-shaped capture must fail report-envelope validation: {jsonPath}");
                        continue;
                    }

                    using (document)
                    {
                        AssertWorkspaceEchoMatchesJson(textPath, document.RootElement);
                        capturedPairs++;
                    }
                }
            }

            Assert.True(capturedPairs > 0, $"No paired minimal text and JSON captures were found for {family.Name}.");
        }
    }

    private static void AssertWorkspaceEchoMatchesJson(string textPath, JsonElement report)
    {
        Assert.Equal(JsonValueKind.Object, report.ValueKind);
        Assert.Equal(3, report.GetProperty("schemaVersion").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(report.GetProperty("command").GetString()));
        Assert.Equal("minimal", report.GetProperty("detail").GetString());
        Assert.False(string.IsNullOrWhiteSpace(report.GetProperty("status").GetString()));
        Assert.Equal(JsonValueKind.Array, report.GetProperty("findings").ValueKind);
        Assert.Equal(JsonValueKind.Array, report.GetProperty("effects").ValueKind);
        Assert.Equal(JsonValueKind.Object, report.GetProperty("counts").ValueKind);
        Assert.Equal(JsonValueKind.Array, report.GetProperty("limitations").ValueKind);
        var workspace = report.GetProperty("workspace");
        string? workspacePath = null;
        string? selectedBy = null;
        if (workspace.ValueKind == JsonValueKind.Object
            && workspace.TryGetProperty("path", out var path)
            && path.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(path.GetString()))
        {
            workspacePath = path.GetString();
            selectedBy = workspace.TryGetProperty("selectedBy", out var selectedByValue)
                && selectedByValue.ValueKind == JsonValueKind.String
                    ? selectedByValue.GetString()
                    : null;
        }

        var status = report.GetProperty("status").GetString();
        var shouldShowWorkspace = workspacePath is not null
            && (selectedBy == "explicit-workspace"
                || status is "blocked" or "failed" or "cancelled");
        var text = File.ReadAllText(textPath).Replace("\r\n", "\n", StringComparison.Ordinal);
        var workspaceLines = text.Split('\n', StringSplitOptions.None)
            .Where(line => line.StartsWith("Workspace: ", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(shouldShowWorkspace ? 1 : 0, workspaceLines.Length);
        if (shouldShowWorkspace)
        {
            Assert.Equal($"Workspace: {workspacePath}", Assert.Single(workspaceLines));
        }
    }

    private static string IntegrationProjectRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx")))
                {
                    return Path.Combine(
                        directory.FullName,
                        "src",
                        "cli",
                        "tests",
                        "integration",
                        "OpenForge.Cli.IntegrationTests");
                }
            }
        }

        throw new DirectoryNotFoundException("The Open Forge repository root could not be located.");
    }

    private sealed record WorkspacePolicyFamily(string Name, string RelativeDirectory);
}
