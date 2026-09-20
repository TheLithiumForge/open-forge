using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F07FindAndReadJourneyTests
{
    private const string NotesPath = ".agents/guidance/notes.md";
    private const string DecisionOnlyPath = ".agents/guidance/decision-only.md";
    private const string ArchitectureOnlyPath = ".agents/patterns/architecture-only.md";
    private const string BothPath = ".agents/patterns/both.md";
    private const string UnrelatedPath = ".agents/guidance/unrelated.md";
    private const string HeldPath = ".agents/guidance/held.md";

    [Fact(
        DisplayName = "F07 carries one installed workspace from route discovery through exact find and context selection"),
        Trait("Feature", "find-guidance-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F07"),
        Trait("Scenarios", "C11-03,C09-03,C09-06,C09-07,C08-02")]
    public async Task FindIntersectionProjectionScopeAndContextCarryOneRealState()
    {
        using var workspace = await CreateWorkspaceAsync("f07-main");

        var routeList = await RunReadOnlyAsync(workspace, "route", "list", "--depth=all");
        AssertCompleteText(routeList);
        Assert.Contains("guidance/notes", routeList.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guidance/decision-only", routeList.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("patterns/architecture-only", routeList.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("patterns/both", routeList.StandardOutput, StringComparison.Ordinal);

        using (var routeJson = await RunJsonReadOnlyAsync(workspace, "route", "list", "--depth=all"))
        {
            Assert.Equal("completed", routeJson.RootElement.GetProperty("status").GetString());
            var rows = routeJson.RootElement.GetProperty("data").GetProperty("rows");
            Assert.Contains(rows.EnumerateArray(), row => row.GetProperty("path").GetString() == NotesPath);
            Assert.Contains(rows.EnumerateArray(), row => row.GetProperty("path").GetString() == DecisionOnlyPath);
            Assert.Contains(rows.EnumerateArray(), row => row.GetProperty("path").GetString() == ArchitectureOnlyPath);
            Assert.Contains(rows.EnumerateArray(), row => row.GetProperty("path").GetString() == BothPath);
        }

        var intersection = await RunReadOnlyAsync(
            workspace,
            "find",
            "--tag=Decision",
            "--tag=Architecture");
        AssertCompleteText(intersection);
        Assert.Contains("guidance/notes", intersection.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("patterns/both", intersection.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("decision-only", intersection.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("architecture-only", intersection.StandardOutput, StringComparison.Ordinal);

        using (var intersectionJson = await RunJsonReadOnlyAsync(
                   workspace,
                   "find",
                   "--tag=Decision",
                   "--tag=Architecture",
                   "--format=json",
                   "--detail=full"))
        {
            Assert.Equal(
                [NotesPath, BothPath],
                FindPaths(intersectionJson.RootElement).Order(StringComparer.Ordinal));
        }

        var headings = await RunReadOnlyAsync(
            workspace,
            "find",
            "--tag=Decision",
            "--tag=Architecture",
            "--content=headings");
        AssertCompleteText(headings);
        Assert.Contains("guidance/notes", headings.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("patterns/both", headings.StandardOutput, StringComparison.Ordinal);

        using (var headingsJson = await RunJsonReadOnlyAsync(
                   workspace,
                   "find",
                   "--tag=Decision",
                   "--tag=Architecture",
                   "--content=headings",
                   "--format=json",
                   "--detail=full"))
        {
            Assert.Equal(
                [NotesPath, BothPath],
                FindPaths(headingsJson.RootElement).Order(StringComparer.Ordinal));
            var notes = Assert.Single(
                headingsJson.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray(),
                match => match.GetProperty("path").GetString() == NotesPath);
            var headingPart = Assert.Single(
                notes.GetProperty("parts").EnumerateArray(),
                part => part.GetProperty("part").GetString() == "headings");
            var headingTexts = headingPart.GetProperty("headings")
                .EnumerateArray()
                .Select(heading => heading.GetProperty("text").GetString()
                    ?? throw new InvalidOperationException("Find heading text was null."))
                .ToArray();
            Assert.Contains("Overview", headingTexts);
            Assert.Contains("Decisions", headingTexts);
        }

        var guidanceOnly = await RunReadOnlyAsync(
            workspace,
            "find",
            "--tag=Decision",
            "--include=guidance");
        AssertCompleteText(guidanceOnly);
        Assert.Contains("guidance/notes", guidanceOnly.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guidance/decision-only", guidanceOnly.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("patterns/both", guidanceOnly.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("patterns/architecture-only", guidanceOnly.StandardOutput, StringComparison.Ordinal);

        using (var guidanceJson = await RunJsonReadOnlyAsync(
                   workspace,
                   "find",
                   "--tag=Decision",
                   "--include=guidance",
                   "--format=json",
                   "--detail=full"))
        {
            Assert.Equal(
                [DecisionOnlyPath, NotesPath],
                FindPaths(guidanceJson.RootElement).Order(StringComparer.Ordinal));
        }

        using var contextMatch = await RunJsonReadOnlyAsync(
            workspace,
            "find",
            "--tag=Decision",
            "--tag=Architecture",
            "--format=json",
            "--detail=full");
        var selected = Assert.Single(
            contextMatch.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray(),
            match => match.GetProperty("path").GetString() == NotesPath);
        var selectedIdentity = selected.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(selectedIdentity));

        var context = await RunReadOnlyAsync(workspace, "context", selectedIdentity!);
        Assert.Equal(0, context.ExitCode);
        Assert.Equal(string.Empty, context.StandardError);
        Assert.Contains(selectedIdentity!, context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Overview", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Decisions", context.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(
        DisplayName = "F07 any-match returns the independently known union without widening to unrelated material"),
        Trait("Feature", "find-guidance-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F07"),
        Trait("Scenarios", "X28")]
    public async Task AnyMatchUnionRetainsEachMatchingTagAlternative()
    {
        using var workspace = await CreateWorkspaceAsync("f07-any");

        var text = await RunReadOnlyAsync(
            workspace,
            "find",
            "--require=any",
            "--tag=Decision",
            "--tag=Architecture");
        AssertCompleteText(text);
        foreach (var path in new[] { NotesPath, DecisionOnlyPath, ArchitectureOnlyPath, BothPath })
        {
            Assert.Contains(path[8..^3], text.StandardOutput, StringComparison.Ordinal);
        }

        using var json = await RunJsonReadOnlyAsync(
            workspace,
            "find",
            "--require=any",
            "--tag=Decision",
            "--tag=Architecture",
            "--format=json",
            "--detail=full");
        Assert.Equal(
            [DecisionOnlyPath, NotesPath, ArchitectureOnlyPath, BothPath],
            FindPaths(json.RootElement).Order(StringComparer.Ordinal));
        Assert.DoesNotContain(UnrelatedPath, FindPaths(json.RootElement));
    }

    [Fact(
        DisplayName = "F07 no-match is a completed zero-result query rather than an empty-workspace or incomplete claim"),
        Trait("Feature", "find-guidance-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F07"),
        Trait("Scenarios", "C09-05")]
    public async Task NoMatchRemainsCompleteAndExplicit()
    {
        using var workspace = await CreateWorkspaceAsync("f07-no-match");

        var text = await RunReadOnlyAsync(workspace, "find", "--tag=NonexistentTag");
        Assert.Equal(0, text.ExitCode);
        Assert.Equal(string.Empty, text.StandardError);
        Assert.Contains("NonexistentTag", text.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("empty workspace", text.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("incomplete", text.StandardOutput, StringComparison.OrdinalIgnoreCase);

        using var json = await RunJsonReadOnlyAsync(
            workspace,
            "find",
            "--tag=NonexistentTag",
            "--format=json",
            "--detail=full");
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.Empty(json.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
        Assert.Empty(json.RootElement.GetProperty("findings").EnumerateArray());
    }

    [Fact(
        DisplayName = "F07 Windows read denial retains known find matches and marks coverage incomplete"),
        Trait("Feature", "find-guidance-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F07"),
        Trait("Scenarios", "C09-09,X28")]
    public async Task WindowsHeldCandidateIsUnavailableButNotMalformed()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F07 C09-09/X28 requires Windows FileShare.None read-denial semantics; this is not a product verdict on another OS.");
        }

        using var workspace = await CreateWorkspaceAsync("f07-unreadable", writeHeldCandidateAfterIndex: true);
        var heldPath = workspace.Combine(HeldPath);
        var before = workspace.SnapshotState();
        AssertOrdinaryFile(heldPath);

        using var held = new FileStream(heldPath, FileMode.Open, FileAccess.Read, FileShare.None);
        Exception? readFailure = null;
        try
        {
            _ = File.ReadAllBytes(heldPath);
        }
        catch (Exception exception)
        {
            readFailure = exception;
        }

        Assert.NotNull(readFailure);
        Assert.True(
            readFailure is IOException or UnauthorizedAccessException,
            $"The held ordinary candidate failed with an unexpected exception type: {readFailure!.GetType().FullName}.");

        var text = await workspace.RunAsync("find", "--tag=Decision", "--detail=standard");
        Assert.Equal(3, text.ExitCode);
        Assert.Equal(string.Empty, text.StandardError);
        Assert.Contains("guidance/notes", text.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guidance/decision-only", text.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("incomplete", text.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("No sources match", text.StandardOutput, StringComparison.Ordinal);

        var jsonResult = await workspace.RunAsync(
            "find",
            "--tag=Decision",
            "--format=json",
            "--detail=full");
        Assert.Equal(3, jsonResult.ExitCode);
        Assert.Equal(string.Empty, jsonResult.StandardError);
        using var json = JsonDocument.Parse(jsonResult.StandardOutput);
        Assert.Equal("incomplete", json.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            [DecisionOnlyPath, NotesPath, BothPath],
            FindPaths(json.RootElement).Order(StringComparer.Ordinal));
        Assert.Contains(
            json.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "find.inspection-unavailable");

        held.Dispose();
        Assert.Equal(before, workspace.SnapshotState());
    }

    private static async Task<PublishedJourneyWorkspace> CreateWorkspaceAsync(
        string purpose,
        bool writeHeldCandidateAfterIndex = false)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                NotesPath,
                DecisionOnlyPath,
                ArchitectureOnlyPath,
                BothPath,
                UnrelatedPath,
                HeldPath);

            var install = await workspace.RunAsync("install", "--automatic");
            AssertSetupSuccess(install, "install");
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));

            workspace.WriteText(
                NotesPath,
                OpenForgeDocumentSeed.Metadata(
                    "Notes",
                    ["Decision", "Architecture"],
                    "\n# Notes\n\n## Overview\nSelected overview.\n\n## Decisions\nSelected decisions.\n"));
            workspace.WriteText(
                DecisionOnlyPath,
                OpenForgeDocumentSeed.Metadata(
                    "Decision only",
                    ["Decision"],
                    "\n# Decision only\n\n## Decision\nDecision-only body.\n"));
            workspace.WriteText(
                ArchitectureOnlyPath,
                OpenForgeDocumentSeed.Metadata(
                    "Architecture only",
                    ["Architecture"],
                    "\n# Architecture only\n\n## Architecture\nArchitecture-only body.\n"));
            workspace.WriteText(
                BothPath,
                OpenForgeDocumentSeed.Metadata(
                    "Both",
                    ["Decision", "Architecture"],
                    "\n# Both\n\n## Both\nBoth-tag body.\n"));
            workspace.WriteText(
                UnrelatedPath,
                OpenForgeDocumentSeed.Metadata(
                    "Unrelated",
                    ["Other"],
                    "\n# Unrelated\n\n## Noise\nUnrelated body.\n"));

            var indexed = await workspace.RunAsync("index");
            AssertSetupSuccess(indexed, "index");

            if (writeHeldCandidateAfterIndex)
            {
                workspace.WriteText(
                    HeldPath,
                    OpenForgeDocumentSeed.Metadata(
                        "Held candidate",
                        ["Decision"],
                        "\n# Held candidate\n\n## Held\nThis source is readable only before the OS handle is held.\n"));
            }

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
        params string[] arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static async Task<JsonDocument> RunJsonReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
    {
        var commandArguments = arguments
            .Where(argument => !string.Equals(argument, "--format=json", StringComparison.Ordinal)
                && !argument.StartsWith("--detail=", StringComparison.Ordinal))
            .Append("--format=json")
            .Append("--detail=full")
            .ToArray();
        var result = await RunReadOnlyAsync(workspace, commandArguments);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        return JsonDocument.Parse(result.StandardOutput);
    }

    private static string[] FindPaths(JsonElement root)
        => root.GetProperty("data")
            .GetProperty("matches")
            .EnumerateArray()
            .Select(match => match.GetProperty("path").GetString()
                ?? throw new InvalidOperationException("Find returned a null source path."))
            .ToArray();

    private static void AssertCompleteText(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static void AssertSetupSuccess(ProcessRunResult result, string operation)
    {
        Assert.True(
            result.ExitCode == 0,
            $"F07 {operation} setup failed with exit {result.ExitCode}.\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        Assert.Equal(
            0,
            (int)(attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)));
    }
}
