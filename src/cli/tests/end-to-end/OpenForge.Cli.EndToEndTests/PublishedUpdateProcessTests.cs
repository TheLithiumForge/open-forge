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
            "open-forge update [--force] [--prune] [--automatic] [--dry-run] [global flags]",
            help.StandardOutput,
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
                "--json",
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
                "--json",
            ]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        var repeatedRoot = ReadJson(repeated);
        AssertUpdateEnvelope(repeatedRoot, workspace.Path, "complete");
        var repeatedResult = repeatedRoot.GetProperty("result");
        Assert.Equal("dry-run", repeatedResult.GetProperty("mode").GetString());
        Assert.False(repeatedResult.GetProperty("force").GetBoolean());
        Assert.False(repeatedResult.GetProperty("prune").GetBoolean());
        Assert.True(repeatedResult.GetProperty("automatic").GetBoolean());
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
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
        Assert.Contains("Status: invalid", redirected.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "update.confirmation-required",
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
            ["update", "--force", "--automatic", "--dry-run", "--json"]);

        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(string.Empty, dryRun.StandardError);
        var dryRunRoot = ReadJson(dryRun);
        AssertUpdateEnvelope(dryRunRoot, workspace.Path, "complete");
        var dryRunResult = dryRunRoot.GetProperty("result");
        Assert.Equal("dry-run", dryRunResult.GetProperty("mode").GetString());
        Assert.True(dryRunResult.GetProperty("force").GetBoolean());
        Assert.True(dryRunResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(dryRunResult.GetProperty("effects").EnumerateArray());
        Assert.All(
            dryRunResult.GetProperty("effects").EnumerateArray(),
            effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["update", "--force", "--automatic", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        var appliedRoot = ReadJson(applied);
        AssertUpdateEnvelope(appliedRoot, workspace.Path, "complete");
        var appliedResult = appliedRoot.GetProperty("result");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.True(appliedResult.GetProperty("force").GetBoolean());
        Assert.True(appliedResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(appliedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", appliedResult.GetProperty("verification").GetString());
        var afterApply = workspace.SnapshotState();

        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force", "--automatic", "--json"]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        var repeatedRoot = ReadJson(repeated);
        AssertUpdateEnvelope(repeatedRoot, workspace.Path, "complete");
        var repeatedResult = repeatedRoot.GetProperty("result");
        Assert.Equal("apply", repeatedResult.GetProperty("mode").GetString());
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("verified", repeatedResult.GetProperty("verification").GetString());
        Assert.Equal(afterApply, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published Update attributes normal divergence and force-prune JSON repeat state through Status and Doctor"), Trait("Feature", "update-command"), Trait("Evidence", "EndToEnd")]
    public async Task DivergenceForcePruneJsonRepeatStatusDoctorAttributionJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedUpdateWorkspace.Create();
        var installation = await workspace.EstablishTrustedFrameworkAsync(target);
        Assert.Equal(0, installation.ExitCode);
        Assert.Equal(string.Empty, installation.StandardError);
        workspace.MutateManagedContent();

        var normal = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--automatic", "--json"]);

        Assert.Equal(2, normal.ExitCode);
        Assert.Equal(string.Empty, normal.StandardError);
        var normalRoot = ReadJson(normal);
        AssertUpdateEnvelope(normalRoot, workspace.Path, "attention");
        var normalResult = normalRoot.GetProperty("result");
        Assert.False(normalResult.GetProperty("force").GetBoolean());
        Assert.False(normalResult.GetProperty("prune").GetBoolean());
        Assert.True(normalResult.GetProperty("automatic").GetBoolean());
        Assert.Contains(
            normalResult.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "update.managed-divergence");
        Assert.Empty(normalResult.GetProperty("effects").EnumerateArray());

        var status = await RunWithoutWritesAsync(
            target,
            workspace,
            ["status", "--json"]);

        Assert.Equal(2, status.ExitCode);
        Assert.Equal(string.Empty, status.StandardError);
        using var statusDocument = JsonDocument.Parse(status.StandardOutput);
        var statusRoot = statusDocument.RootElement;
        Assert.Equal("status", statusRoot.GetProperty("command").GetString());
        Assert.Equal("attention", statusRoot.GetProperty("status").GetString());
        var statusFramework = statusRoot
            .GetProperty("result")
            .GetProperty("lifecycle")
            .GetProperty("framework");
        Assert.Equal("trusted", statusFramework.GetProperty("state").GetString());
        Assert.Contains(
            statusRoot.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "framework-target-changed");

        var doctor = await RunWithoutWritesAsync(
            target,
            workspace,
            ["doctor", "--json"]);

        Assert.Equal(3, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        using var doctorDocument = JsonDocument.Parse(doctor.StandardOutput);
        var doctorRoot = doctorDocument.RootElement;
        Assert.Equal("doctor", doctorRoot.GetProperty("command").GetString());
        Assert.Equal("incomplete", doctorRoot.GetProperty("status").GetString());
        var domains = doctorRoot
            .GetProperty("result")
            .GetProperty("domains")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(6, domains.Length);
        Assert.Equal(
            [
                "workspace-entry",
                "recovery-residuals",
                "routes-metadata-overwrites-generated-navigation",
                "local-references",
                "framework-lifecycle",
                "extension-lifecycle",
            ],
            domains.Select(domain => domain.GetProperty("domain").GetString()));
        var frameworkDomain = Assert.Single(
            domains,
            domain => domain.GetProperty("domain").GetString() == "framework-lifecycle");
        Assert.Contains(
            frameworkDomain.GetProperty("findings")
                .EnumerateArray()
                .SelectMany(finding => finding.GetProperty("actions").EnumerateArray())
                .Select(action => action.GetProperty("command").GetString()),
            command => command == "open-forge update");

        workspace.SeedHistoricalRetiredTarget();
        var forced = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["update", "--force", "--prune", "--automatic", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, forced.ExitCode);
        Assert.Equal(string.Empty, forced.StandardError);
        var forcedRoot = ReadJson(forced);
        AssertUpdateEnvelope(forcedRoot, workspace.Path, "complete");
        var forcedResult = forcedRoot.GetProperty("result");
        Assert.Equal("apply", forcedResult.GetProperty("mode").GetString());
        Assert.True(forcedResult.GetProperty("force").GetBoolean());
        Assert.True(forcedResult.GetProperty("prune").GetBoolean());
        Assert.True(forcedResult.GetProperty("automatic").GetBoolean());
        Assert.NotEmpty(forcedResult.GetProperty("effects").EnumerateArray());
        Assert.Contains(
            forcedResult.GetProperty("effects").EnumerateArray()
                .SelectMany(effect => effect.GetProperty("changes").EnumerateArray())
                .Select(change => change.GetProperty("action").GetString()),
            action => action == "delete");
        Assert.Equal("verified", forcedResult.GetProperty("verification").GetString());
        Assert.False(workspace.HistoricalTargetExists());
        var afterForcePrune = workspace.SnapshotState();

        var repeat = await RunWithoutWritesAsync(
            target,
            workspace,
            ["update", "--force", "--prune", "--automatic", "--json"]);

        Assert.Equal(0, repeat.ExitCode);
        Assert.Equal(string.Empty, repeat.StandardError);
        var repeatRoot = ReadJson(repeat);
        AssertUpdateEnvelope(repeatRoot, workspace.Path, "complete");
        var repeatResult = repeatRoot.GetProperty("result");
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
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("update", root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(
            workspacePath,
            root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Object, root.GetProperty("result").ValueKind);
    }
}
