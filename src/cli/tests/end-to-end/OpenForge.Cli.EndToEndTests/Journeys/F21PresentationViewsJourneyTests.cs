using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F21PresentationViewsJourneyTests
{
    private static readonly string[] DetailNames = ["minimal", "standard", "full", "debug"];
    private static readonly string[] FilterNames = ["error", "warning", "info", "all"];

    private const string DecisionNotePath = ".agents/decision-note.md";
    private const string LockedDecisionPath = ".agents/locked-decision.md";

    [Fact(DisplayName = "F21 Status views keep a healthy W1 result stable across detail and JSON variants"), Trait("Feature", "presentation-views"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F21")]
    public async Task StatusViewsKeepHealthyResultStableAcrossDetailAndJsonVariants()
    {
        using var workspace = await CreateInstalledWorkspaceAsync("f21-status");
        var before = SnapshotState(workspace);
        var text = new Dictionary<string, ProcessRunResult>(StringComparer.Ordinal)
        {
            ["default"] = await RunReadOnlyAsync(workspace, before, ["status"]),
        };

        foreach (var detail in DetailNames)
        {
            text[detail] = await RunReadOnlyAsync(
                workspace,
                before,
                ["status", $"--detail={detail}"]);
        }

        Assert.Equal(
            NormalizeRunCoordinates(text["default"].StandardOutput, workspace.Path),
            NormalizeRunCoordinates(text["minimal"].StandardOutput, workspace.Path));
        Assert.Equal(text["default"].StandardError, text["minimal"].StandardError);
        Assert.Equal(text["default"].ExitCode, text["minimal"].ExitCode);

        foreach (var capture in text)
        {
            Assert.Equal(0, capture.Value.ExitCode);
            Assert.NotEmpty(capture.Value.StandardOutput);
            if (!string.Equals(capture.Key, "debug", StringComparison.Ordinal))
            {
                Assert.Equal(string.Empty, capture.Value.StandardError);
            }
            else
            {
                AssertDebugDiagnostics(capture.Value.StandardError);
            }
        }

        Assert.Equal(text["full"].StandardOutput, text["debug"].StandardOutput);

        var json = new List<JsonCapture>();
        foreach (var detail in DetailNames)
        {
            var result = await RunReadOnlyAsync(
                workspace,
                before,
                ["status", "--format=json", $"--detail={detail}"]);
            json.Add(ReadJsonCapture(result, "status", "completed", 0, detail, null, workspace.Path));
        }

        foreach (var filter in FilterNames)
        {
            var result = await RunReadOnlyAsync(
                workspace,
                before,
                ["status", "--format=json", $"--detail-filter={filter}"]);
            json.Add(ReadJsonCapture(
                result,
                "status",
                "completed",
                0,
                "minimal",
                ExpectedFilter(filter),
                workspace.Path));
        }

        AssertEquivalentJsonSemantics(json);
    }

    [Fact(DisplayName = "F21 incomplete Find views retain the readable match, locked path, counts and action"), Trait("Feature", "presentation-views"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F21")]
    public async Task IncompleteFindViewsRetainMatchLockedPathCountsAndAction()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F21 incomplete find requires the Windows FileShare.None read-denial capability.");
            return;
        }

        using var workspace = await CreateInstalledWorkspaceAsync("f21-find");
        workspace.WriteText(
            DecisionNotePath,
            OpenForgeDocumentSeed.Metadata(
                "Decision note",
                ["Decision"],
                "\n# Decision note\n\nReadable decision evidence.\n"));
        workspace.WriteText(
            LockedDecisionPath,
            OpenForgeDocumentSeed.Metadata(
                "Locked decision",
                ["Decision"],
                "\n# Locked decision\n\nThis candidate is intentionally unavailable.\n"));

        var before = SnapshotState(workspace);
        var text = new Dictionary<string, ProcessRunResult>(StringComparer.Ordinal)
        {
            ["default"] = await RunFindWithLockedCandidateAsync(
                workspace,
                before,
                ["find", "--tag=Decision"],
                LockedDecisionPath),
        };

        foreach (var detail in DetailNames)
        {
            text[detail] = await RunFindWithLockedCandidateAsync(
                workspace,
                before,
                ["find", "--tag=Decision", $"--detail={detail}"],
                LockedDecisionPath);
        }

        Assert.Equal(
            NormalizeRunCoordinates(text["default"].StandardOutput, workspace.Path),
            NormalizeRunCoordinates(text["minimal"].StandardOutput, workspace.Path));
        Assert.Equal(text["default"].StandardError, text["minimal"].StandardError);
        Assert.Equal(text["default"].ExitCode, text["minimal"].ExitCode);

        foreach (var capture in text)
        {
            Assert.Equal(3, capture.Value.ExitCode);
            Assert.NotEmpty(capture.Value.StandardOutput);
            if (!string.Equals(capture.Key, "debug", StringComparison.Ordinal))
            {
                Assert.Equal(string.Empty, capture.Value.StandardError);
            }
            else
            {
                AssertDebugDiagnostics(capture.Value.StandardError);
            }
        }

        Assert.Equal(text["full"].StandardOutput, text["debug"].StandardOutput);

        foreach (var capture in text.Values)
        {
            Assert.Contains(DecisionNotePath, capture.StandardOutput, StringComparison.Ordinal);
            Assert.Contains(LockedDecisionPath, capture.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Next:", capture.StandardOutput, StringComparison.Ordinal);
        }

        var json = new List<JsonCapture>();
        foreach (var detail in DetailNames)
        {
            var result = await RunFindWithLockedCandidateAsync(
                workspace,
                before,
                ["find", "--tag=Decision", "--format=json", $"--detail={detail}"],
                LockedDecisionPath);
            json.Add(ReadJsonCapture(result, "find", "incomplete", 3, detail, null, workspace.Path));
        }

        foreach (var filter in FilterNames)
        {
            var result = await RunFindWithLockedCandidateAsync(
                workspace,
                before,
                ["find", "--tag=Decision", "--format=json", $"--detail-filter={filter}"],
                LockedDecisionPath);
            json.Add(ReadJsonCapture(
                result,
                "find",
                "incomplete",
                3,
                "minimal",
                ExpectedFilter(filter),
                workspace.Path));
        }

        AssertEquivalentJsonSemantics(json);
    }

    private static async Task<PublishedJourneyWorkspace> CreateInstalledWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.WriteText("README.md", "# F21 W1 fixture\n\nPreserve this authored file.\n");
            var installed = await workspace.RunAsync("install", "--automatic");
            Assert.Equal(0, installed.ExitCode);
            Assert.Equal(string.Empty, installed.StandardError);
            Assert.True(File.Exists(workspace.Combine("AGENTS.md")));
            Assert.True(File.Exists(workspace.Combine(".agents/guidance/_guidance.md")));
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> expectedState,
        IReadOnlyList<string> arguments)
        => RunReadOnlyCoreAsync(workspace, expectedState, arguments);

    private static async Task<ProcessRunResult> RunReadOnlyCoreAsync(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> expectedState,
        IReadOnlyList<string> arguments)
    {
        var result = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotState(workspace),
            arguments,
            workspace.ProcessEnvironment);
        Assert.Equal(expectedState, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        return result;
    }

    private static async Task<ProcessRunResult> RunFindWithLockedCandidateAsync(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> expectedState,
        IReadOnlyList<string> arguments,
        string lockedRelativePath)
    {
        var lockedPath = workspace.Combine(lockedRelativePath);
        using var locked = new FileStream(
            lockedPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None);
        _ = Assert.Throws<IOException>(() => File.ReadAllBytes(lockedPath));

        var result = await workspace.RunAsync(arguments.ToArray());
        locked.Dispose();

        Assert.Equal(expectedState, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        return result;
    }

    private static JsonCapture ReadJsonCapture(
        ProcessRunResult result,
        string expectedCommand,
        string expectedStatus,
        int expectedExitCode,
        string detail,
        IReadOnlyList<string>? expectedFilter,
        string workspacePath)
    {
        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        if (!string.Equals(detail, "debug", StringComparison.Ordinal))
        {
            Assert.Equal(string.Empty, result.StandardError);
        }
        else
        {
            AssertDebugDiagnostics(result.StandardError);
        }

        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(expectedCommand, root.GetProperty("command").GetString());
        Assert.Equal(expectedStatus, root.GetProperty("status").GetString());
        Assert.Equal(detail, root.GetProperty("detail").GetString());
        AssertFilter(root, expectedFilter);
        AssertDetailGates(root, detail);

        var workspaceElement = root.GetProperty("workspace");
        Assert.Equal(JsonValueKind.Object, workspaceElement.ValueKind);
        Assert.Equal(
            Path.GetFullPath(workspacePath),
            workspaceElement.GetProperty("path").GetString());
        Assert.Equal("current-directory", workspaceElement.GetProperty("selectedBy").GetString());

        if (string.Equals(expectedCommand, "find", StringComparison.Ordinal))
        {
            var matches = root.GetProperty("data").GetProperty("matches").EnumerateArray().ToArray();
            var match = Assert.Single(matches);
            Assert.Equal(DecisionNotePath, match.GetProperty("path").GetString());
            var findings = root.GetProperty("findings").GetRawText();
            var includesWarning = expectedFilter is null
                || expectedFilter.Any(filter => string.Equals(filter, "warning", StringComparison.Ordinal));
            if (includesWarning)
            {
                Assert.Contains(LockedDecisionPath, findings, StringComparison.Ordinal);
            }
            else
            {
                Assert.DoesNotContain(LockedDecisionPath, findings, StringComparison.Ordinal);
            }
        }

        return new JsonCapture(
            Semantic: JsonSemanticProjection(root, expectedCommand));
    }

    private static IReadOnlyList<string> ExpectedFilter(string filter)
        => string.Equals(filter, "all", StringComparison.Ordinal)
            ? ["error", "warning", "info"]
            : [filter];

    private static void AssertFilter(JsonElement root, IReadOnlyList<string>? expectedFilter)
    {
        var filter = root.GetProperty("filter");
        if (expectedFilter is null)
        {
            Assert.Equal(JsonValueKind.Null, filter.ValueKind);
            return;
        }

        Assert.Equal(
            expectedFilter.ToArray(),
            filter.EnumerateArray().Select(value => value.GetString()).ToArray());
    }

    private static void AssertEquivalentJsonSemantics(IReadOnlyList<JsonCapture> captures)
    {
        Assert.NotEmpty(captures);
        var first = captures[0];
        Assert.All(captures, capture => Assert.Equal(first.Semantic, capture.Semantic));
    }

    private static string JsonSemanticProjection(JsonElement root, string command)
    {
        var workspace = root.GetProperty("workspace");
        var effects = root.GetProperty("effects")
            .EnumerateArray()
            .Select(effect => string.Join(
                "\u001f",
                effect.GetProperty("path").GetString() ?? "<null>",
                effect.GetProperty("kind").GetString() ?? "<null>",
                effect.GetProperty("action").GetString() ?? "<null>",
                effect.GetProperty("outcome").GetString() ?? "<null>",
                effect.GetProperty("reason").GetString() ?? "<null>",
                effect.GetProperty("owner").GetString() ?? "<null>"))
            .ToArray();
        var matches = string.Empty;
        if (string.Equals(command, "find", StringComparison.Ordinal))
        {
            matches = string.Join(
                "\u001f",
                root.GetProperty("data")
                    .GetProperty("matches")
                    .EnumerateArray()
                    .Select(match => string.Join(
                        "\u001e",
                        match.GetProperty("id").GetString() ?? "<null>",
                        match.GetProperty("path").GetString() ?? "<null>")));
        }

        return string.Join(
            "\n",
            root.GetProperty("command").GetString(),
            root.GetProperty("status").GetString(),
            workspace.GetProperty("path").GetString(),
            workspace.GetProperty("selectedBy").GetString(),
            string.Join("\u001e", effects),
            root.GetProperty("counts").GetRawText(),
            matches);
    }

    private static void AssertDetailGates(JsonElement root, string detail)
    {
        var full = detail is "full" or "debug";
        var standard = detail is "standard" or "full" or "debug";
        foreach (var finding in root.GetProperty("findings").EnumerateArray())
        {
            Assert.Equal(standard, finding.TryGetProperty("resolution", out _));
            Assert.Equal(full, finding.TryGetProperty("candidates", out _));
            Assert.Equal(full, finding.TryGetProperty("evidence", out _));
            Assert.Equal(full, finding.TryGetProperty("provenance", out _));
        }
    }

    private static void AssertDebugDiagnostics(string stderr)
        => Assert.InRange(stderr.Length, 0, 4096);

    private static string NormalizeRunCoordinates(string output, string workspacePath)
    {
        var fullPath = Path.GetFullPath(workspacePath);
        return output
            .Replace(fullPath, "<workspace>", StringComparison.Ordinal)
            .Replace(fullPath.Replace('\\', '/'), "<workspace>", StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in workspace.SnapshotState())
        {
            state[$"workspace/{entry.Key}"] = entry.Value;
        }

        AddDirectorySnapshot(
            state,
            "data-home",
            workspace.LockStore.LocalApplicationDataDirectory);
        AddDirectorySnapshot(
            state,
            "recovery-workspace",
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void AddDirectorySnapshot(
        IDictionary<string, string> state,
        string prefix,
        string path)
    {
        if (!Directory.Exists(path))
        {
            state[prefix] = File.Exists(path)
                ? FileState(path)
                : "absent";
            return;
        }

        foreach (var entry in PublishedWorkspaceTreeSnapshot.Capture(path))
        {
            state[$"{prefix}/{entry.Key}"] = entry.Value;
        }
    }

    private static string FileState(string path)
        => $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";

    private sealed record JsonCapture(string Semantic);
}
