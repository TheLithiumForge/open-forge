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
            ["status", "--detail=minimal"]);
        var expanded = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--detail=standard"]);

        foreach (var result in new[] { compact, expanded })
        {
            Assert.Equal(3, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
            Assert.Contains("Open Forge is installed", result.StandardOutput, StringComparison.Ordinal);

            // There is no `Status:` line any more. The incomplete draft is stated as a finding,
            // and its path is payload, so it is listed at every level.
            Assert.DoesNotContain("Status:", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Recovery draft is incomplete", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("A command did not finish.", result.StandardOutput, StringComparison.Ordinal);
            Assert.Contains(Path.GetFileName(working.RecoveryDraftPath), result.StandardOutput, StringComparison.Ordinal);
        }

        // Minimal answers the startup cost in one sentence; standard opens it into the block and
        // adds the workspace echo and the routed totals.
        Assert.Contains("Startup reads", compact.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Startup context", compact.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("All routed files:", compact.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Workspace:", compact.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Startup context", expanded.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Shipped by this CLI:", expanded.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("All routed files:", expanded.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Workspace:", expanded.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Status JSON journey emits one schema document and preserves workspace and recovery bytes"), Trait("Feature", "status-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedStatusJsonJourneyEmitsOneSchemaDocumentAndPreservesWorkspaceAndRecoveryBytes()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedStatusWorkspace.CreateIncompleteRecovery();
        var result = await RunWithoutWritesAsync(
            target,
            working,
            ["status", "--format=json", "--detail=full"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);

        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("status", root.GetProperty("command").GetString());
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var candidates = root.GetProperty("data").GetProperty("recovery").GetProperty("candidates");
        Assert.Equal(JsonValueKind.Array, candidates.ValueKind);
        var candidate = Assert.Single(candidates.EnumerateArray());
        Assert.Equal("draft", candidate.GetProperty("kind").GetString());
        Assert.Equal("incomplete", candidate.GetProperty("integrity").GetString());
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
