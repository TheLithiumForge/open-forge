using System.Collections.ObjectModel;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedStatusProcessTests
{
    private static readonly string[] ExpectedRootLeafOrder =
        ["find", "index", "status", "context", "references", "install"];

    [Fact(DisplayName = "Published root and Status help expose the direct leaf in implemented order without workspace inspection"), Trait("Feature", "status-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootAndStatusHelpExposeTheDirectLeafInImplementedOrderWithoutWorkspaceInspection()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedStatusWorkspace.Create();
        var missingWorkspace = working.Combine("missing-help-workspace");
        var root = await RunWithoutWritesAsync(
            target,
            working,
            ["--help", "--workspace", missingWorkspace]);
        var status = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--help", "--workspace", missingWorkspace]);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Equal(ExpectedRootLeafOrder, ReadImplementedRootLeaves(root.StandardOutput));

        Assert.Equal(0, status.ExitCode);
        Assert.Equal(string.Empty, status.StandardError);
        Assert.Contains("open-forge status", status.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published Status human journey uses semantic stream and exit and preserves workspace and recovery bytes"), Trait("Feature", "status-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedStatusHumanJourneyUsesSemanticStreamExitAndPreservesWorkspaceAndRecoveryBytes()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedStatusWorkspace.CreateIncompleteRecovery();
        var compact = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--view=compact"]);
        var expanded = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--view=expanded"]);

        foreach (var result in new[] { compact, expanded })
        {
            Assert.Equal(3, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
            Assert.Contains("Open Forge status", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Result: incomplete", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Incomplete recovery drafts: 1", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains(Path.GetFileName(working.RecoveryDraftPath), result.StandardOutput, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("Total available context", compact.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Largest continuity sources", compact.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Total available context", expanded.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Largest continuity sources", expanded.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Status JSON journey emits one schema document and preserves workspace and recovery bytes"), Trait("Feature", "status-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedStatusJsonJourneyEmitsOneSchemaDocumentAndPreservesWorkspaceAndRecoveryBytes()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedStatusWorkspace.CreateIncompleteRecovery();
        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--json"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);

        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("status", root.GetProperty("command").GetString());
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var recovery = root.GetProperty("result").GetProperty("recovery");
        Assert.Equal(JsonValueKind.Object, recovery.ValueKind);
        Assert.Equal(1, recovery.GetProperty("incompleteDrafts").GetProperty("value").GetInt64());
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedStatusWorkspace working,
        IReadOnlyList<string> arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            () => SnapshotState(working),
            arguments,
            working.ProcessEnvironment);

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedStatusWorkspace working)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["infrastructure/workspace-lock"] = PathState(working.WorkspaceLockPath),
        };

        foreach (var file in working.SnapshotWorkspaceFiles())
        {
            state[$"workspace/{file.Key}"] = file.Value;
        }

        var relativeWorkspace = System.IO.Path.GetRelativePath(
            working.RecoveryStoreRoot,
            working.RecoveryWorkspaceDirectory).Replace('\\', '/');
        var bucketKey = $"recovery/{relativeWorkspace}";
        state[bucketKey] = PathState(working.RecoveryWorkspaceDirectory);
        if (Directory.Exists(working.RecoveryWorkspaceDirectory))
        {
            foreach (var file in Directory
                         .EnumerateFiles(working.RecoveryWorkspaceDirectory, "*", SearchOption.TopDirectoryOnly)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                var name = System.IO.Path.GetFileName(file);
                state[$"{bucketKey}/{name}"] = PathState(file);
            }
        }

        return new ReadOnlyDictionary<string, string>(state);
    }

    private static string PathState(string path)
    {
        if (File.Exists(path))
        {
            return $"file:{Convert.ToBase64String(File.ReadAllBytes(path))}";
        }

        if (Directory.Exists(path))
        {
            return "directory";
        }

        return "absent";
    }

    private static IReadOnlyList<string> ReadImplementedRootLeaves(string help)
    {
        return help.Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => line != "Commands:")
            .Skip(1)
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.TrimStart().Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .Where(token => token is not null && ExpectedRootLeafOrder.Contains(token, StringComparer.Ordinal))
            .OfType<string>()
            .ToArray();
    }
}
