using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedUpdateProcessTests
{
    [Fact(DisplayName = "Published Update help, invalid singleton repetition, and repeated Booleans are truthful and write-free"), Trait("Feature", "update-command"), Trait("Evidence", "EndToEnd")]
    public async Task HelpInvalidSingletonAndBooleanRepetitionJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedUpdateWorkspace.Create();

        var help = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--help"]);

        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Contains(
            "open-forge update [--force] [--prune] [--automatic] [--dry-run] [global options]",
            string.Join(" ", help.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Contains("--force", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--prune", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--automatic", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--dry-run", help.StandardOutput, StringComparison.Ordinal);

        var invalid = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "update",
                "--workspace", workspace.Path,
                "--workspace", workspace.Path,
                "--format=json",
            ]);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("--workspace", invalid.StandardError, StringComparison.Ordinal);

        var installation = await workspace.EstablishTrustedFrameworkAsync(target);
        Assert.Equal(0, installation.ExitCode);
        Assert.Equal(string.Empty, installation.StandardError);

        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "update",
                "--automatic",
                "--automatic",
                "--dry-run",
                "--dry-run",
                "--format=json",
            ]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        var repeatedRoot = ReadJson(repeated);
        AssertUpdateEnvelope(repeatedRoot, workspace.Path, "completed");
        var repeatedResult = repeatedRoot.GetProperty("data");
        Assert.Equal("dry-run", repeatedResult.GetProperty("mode").GetString());
        Assert.False(repeatedResult.GetProperty("force").GetBoolean());
        Assert.False(repeatedResult.GetProperty("prune").GetBoolean());
        Assert.True(repeatedResult.GetProperty("automatic").GetBoolean());
        Assert.False(repeatedResult.TryGetProperty("effects", out _));
        Assert.Equal(JsonValueKind.Null, repeatedRoot.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published redirected Update confirmation is required before automatic dry-run, apply, and repeat"), Trait("Feature", "update-command"), Trait("Evidence", "EndToEnd")]
    public async Task RedirectedConfirmationAutomaticDryRunApplyRepeatJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedUpdateWorkspace.Create();
        var installation = await workspace.EstablishTrustedFrameworkAsync(target);
        Assert.Equal(0, installation.ExitCode);
        Assert.Equal(string.Empty, installation.StandardError);
        workspace.MutateManagedContent();

        var redirected = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force"]);

        Assert.Equal(4, redirected.ExitCode);
        Assert.Equal(string.Empty, redirected.StandardOutput);
        Assert.Contains(
            "Update needs confirmation, and this session cannot ask.",
            redirected.StandardError,
            StringComparison.Ordinal);
        Assert.Contains(
            "open-forge update --force --automatic",
            redirected.StandardError,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Apply this Update plan?",
            redirected.StandardError,
            StringComparison.Ordinal);

        var dryRun = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force", "--automatic", "--dry-run", "--detail", "full", "--format=json"]);

        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(string.Empty, dryRun.StandardError);
        var dryRunRoot = ReadJson(dryRun);
        AssertUpdateEnvelope(dryRunRoot, workspace.Path, "completed");
        var dryRunResult = dryRunRoot.GetProperty("data");
        Assert.Equal("dry-run", dryRunResult.GetProperty("mode").GetString());
        Assert.True(dryRunResult.GetProperty("force").GetBoolean());
        Assert.True(dryRunResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(dryRunRoot.GetProperty("effects").EnumerateArray());
        Assert.All(
            dryRunRoot.GetProperty("effects").EnumerateArray(),
            effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["update", "--force", "--automatic", "--detail", "full", "--format=json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        var appliedRoot = ReadJson(applied);
        AssertUpdateEnvelope(appliedRoot, workspace.Path, "completed");
        var appliedResult = appliedRoot.GetProperty("data");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.True(appliedResult.GetProperty("force").GetBoolean());
        Assert.True(appliedResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(appliedRoot.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", appliedResult.GetProperty("verification").GetString());
        var afterApply = workspace.SnapshotState();

        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force", "--automatic", "--detail", "full", "--format=json"]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        var repeatedRoot = ReadJson(repeated);
        AssertUpdateEnvelope(repeatedRoot, workspace.Path, "completed");
        var repeatedResult = repeatedRoot.GetProperty("data");
        Assert.Equal("apply", repeatedResult.GetProperty("mode").GetString());
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", repeatedResult.GetProperty("verification").GetString());
        Assert.Equal(afterApply, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published Update diagnoses edits then overwrites normally and prunes retired ownership"), Trait("Feature", "update-command"), Trait("Evidence", "EndToEnd")]
    public async Task OrdinaryUpdateAndPruneRetainReviewableRecovery()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedUpdateWorkspace.Create();
        var installation = await workspace.EstablishTrustedFrameworkAsync(target);
        Assert.Equal(0, installation.ExitCode);
        Assert.Equal(string.Empty, installation.StandardError);
        workspace.MutateManagedContent();

        var status = await RunWithoutWritesAsync(
            target,
            workspace,
            ["status", "--format=json"]);

        Assert.Equal(2, status.ExitCode);
        Assert.Equal(string.Empty, status.StandardError);
        using var statusDocument = JsonDocument.Parse(status.StandardOutput);
        var statusRoot = statusDocument.RootElement;
        Assert.Equal("status", statusRoot.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", statusRoot.GetProperty("status").GetString());
        Assert.Equal(
            "installed",
            statusRoot.GetProperty("data").GetProperty("installation").GetProperty("state").GetString());
        Assert.Contains(
            statusRoot.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "status.framework-target-changed");

        var doctor = await RunWithoutWritesAsync(
            target,
            workspace,
            ["doctor", "--format=json", "--detail=standard"]);

        Assert.Equal(2, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        using var doctorDocument = JsonDocument.Parse(doctor.StandardOutput);
        var doctorRoot = doctorDocument.RootElement;
        Assert.Equal("doctor", doctorRoot.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", doctorRoot.GetProperty("status").GetString());
        var categories = doctorRoot
            .GetProperty("data")
            .GetProperty("categories")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(6, categories.Length);
        Assert.Equal(
            [
                "Workspace",
                "Recovery data",
                "Routes and Entries",
                "Links",
                "Framework files",
                "Extensions",
            ],
            categories.Select(category => category.GetProperty("name").GetString()));
        var frameworkCategory = Assert.Single(
            categories,
            category => category.GetProperty("name").GetString() == "Framework files");
        Assert.Equal("complete", frameworkCategory.GetProperty("coverage").GetString());
        Assert.Contains(
            doctorRoot.GetProperty("findings")
                .EnumerateArray()
                .SelectMany(finding => finding.GetProperty("actions").EnumerateArray())
                .Select(action => action.GetProperty("command").GetString()),
            command => command == "open-forge update");

        var normal = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
            ["update", "--automatic", "--detail", "full", "--format=json"], workspace.ProcessEnvironment);
        Assert.Equal(0, normal.ExitCode);
        Assert.Equal(string.Empty, normal.StandardError);
        var normalRoot = ReadJson(normal);
        AssertUpdateEnvelope(normalRoot, workspace.Path, "completed");
        var normalResult = normalRoot.GetProperty("data");
        Assert.False(normalResult.GetProperty("force").GetBoolean());
        Assert.NotEmpty(normalRoot.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", normalResult.GetProperty("verification").GetString());
        Assert.Equal("retained", normalRoot.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.True(File.Exists(normalRoot.GetProperty("recovery").GetProperty("path").GetString()));

        workspace.SeedHistoricalRetiredTarget();
        var forced = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["update", "--force", "--prune", "--automatic", "--detail", "full", "--format=json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, forced.ExitCode);
        Assert.Equal(string.Empty, forced.StandardError);
        var forcedRoot = ReadJson(forced);
        AssertUpdateEnvelope(forcedRoot, workspace.Path, "completed");
        var forcedResult = forcedRoot.GetProperty("data");
        Assert.Equal("apply", forcedResult.GetProperty("mode").GetString());
        Assert.True(forcedResult.GetProperty("force").GetBoolean());
        Assert.True(forcedResult.GetProperty("prune").GetBoolean());
        Assert.True(forcedResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(forcedRoot.GetProperty("effects").EnumerateArray());
        Assert.Contains(
            forcedRoot.GetProperty("effects").EnumerateArray()
                .Select(effect => effect.GetProperty("action").GetString()),
            action => action == "deleted");
        Assert.Equal("verified", forcedResult.GetProperty("verification").GetString());
        Assert.False(workspace.HistoricalTargetExists());
        var afterForcePrune = workspace.SnapshotState();

        var repeat = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force", "--prune", "--automatic", "--detail", "full", "--format=json"]);

        Assert.Equal(0, repeat.ExitCode);
        Assert.Equal(string.Empty, repeat.StandardError);
        var repeatRoot = ReadJson(repeat);
        AssertUpdateEnvelope(repeatRoot, workspace.Path, "completed");
        var repeatResult = repeatRoot.GetProperty("data");
        Assert.Empty(repeatResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", repeatResult.GetProperty("verification").GetString());
        Assert.Equal(afterForcePrune, workspace.SnapshotState());
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedUpdateWorkspace workspace,
        IReadOnlyList<string> arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static JsonElement ReadJson(ProcessRunResult result)
    {
        using var document = JsonDocument.Parse(result.StandardOutput);
        return document.RootElement.Clone();
    }

    private static void AssertUpdateEnvelope(
        JsonElement root,
        string workspacePath,
        string status)
    {
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("update", root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(
            workspacePath,
            root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Object, root.GetProperty("data").ValueKind);
    }
}
