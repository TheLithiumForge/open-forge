using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallCompositionIntegrationTests
{
    private const string ConfirmationPrompt = "Apply this Install plan? [y/N] ";

    [Fact(DisplayName = "Composed Install dry-run serializes real safe-absence facts without persistent effects"),
     Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task DryRunSerializesRealSafeAbsenceFactsWithoutEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-dry-run-packet");
        var before = workspace.SnapshotHashes();
        var result = await RunAsync(
            ["install", "--dry-run", "--automatic", "--json"], workspace.PhysicalPath,
            "unused", standardInputRedirected: true, promptOutputRedirected: true);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal("unused", result.RemainingInput);
        Assert.DoesNotContain("Apply this Install plan?", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(["schemaVersion", "command", "status", "workspace", "result", "next"], root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.PhysicalPath, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var facts = root.GetProperty("result");
        Assert.Equal(
            ["mode", "force", "automatic", "source", "classification", "footprint", "effects", "lifecycle", "recovery", "verification", "findings"],
            facts.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", facts.GetProperty("mode").GetString());
        Assert.False(facts.GetProperty("force").GetBoolean());
        Assert.True(facts.GetProperty("automatic").GetBoolean());
        var source = facts.GetProperty("source");
        Assert.Equal(["inventoryFingerprint", "assetCount"], source.EnumerateObject().Select(property => property.Name));
        Assert.False(string.IsNullOrWhiteSpace(source.GetProperty("inventoryFingerprint").GetString()));
        Assert.Equal(InstallOperationWorkspace.EmbeddedPayloadPaths.Count + 2, source.GetProperty("assetCount").GetInt32());
        Assert.Equal("safe-absence", facts.GetProperty("classification").GetString());
        var footprint = facts.GetProperty("footprint");
        Assert.Equal(["payloadFiles", "managedRegions", "generatedRegions"], footprint.EnumerateObject().Select(property => property.Name));
        Assert.True(footprint.GetProperty("payloadFiles").GetInt32() > 0);
        Assert.True(footprint.GetProperty("managedRegions").GetInt32() > 0);
        Assert.True(footprint.GetProperty("generatedRegions").GetInt32() > 0);
        var firstEffect = facts.GetProperty("effects")[0];
        Assert.Equal(["path", "kind", "action", "sourceAssetPath", "outcome", "residual"], firstEffect.EnumerateObject().Select(property => property.Name));
        Assert.Equal(".agents", firstEffect.GetProperty("path").GetString());
        Assert.Equal("directory", firstEffect.GetProperty("kind").GetString());
        Assert.Equal("create", firstEffect.GetProperty("action").GetString());
        Assert.Equal(JsonValueKind.Null, firstEffect.GetProperty("sourceAssetPath").ValueKind);
        Assert.Equal("planned", firstEffect.GetProperty("outcome").GetString());
        Assert.Equal("none", firstEffect.GetProperty("residual").GetString());
        Assert.Equal("publish", facts.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("planned", facts.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("not-required", facts.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("not-requested", facts.GetProperty("verification").GetString());
        Assert.Empty(facts.GetProperty("findings").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(result.LockInfrastructureExists);
        Assert.False(workspace.RecoveryDirectoryExists());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Root-composed Install prompts once for human apply and never prompts for its verified no-op"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionOwnsOneEligibleConfirmation()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-confirmation");
        var applied = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"y{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(ConfirmationPrompt, applied.StandardError);
        Assert.StartsWith("Open Forge install", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Classification: safe-absence", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", applied.RemainingInput);
        Assert.True(workspace.AgentsDirectoryExists());
        Assert.True(applied.LockInfrastructureExists);
        var afterApply = workspace.SnapshotHashes();

        var noOp = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"unused{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, noOp.Status);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("Classification: trusted-exact", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("unused", noOp.RemainingInput);
        Assert.Equal(afterApply, workspace.SnapshotHashes());
        Assert.False(noOp.LockInfrastructureExists);
    }

    [Fact(DisplayName = "Root-composed Install refusal is interrupted without writes and preserves remaining input"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionRefusalPreservesNoWriteBoundary()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-refusal");
        var before = workspace.SnapshotHashes();
        var run = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"n{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.StartsWith(ConfirmationPrompt, run.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: interrupted", run.StandardError, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.AgentsDirectoryExists());
        Assert.False(run.LockInfrastructureExists);
    }

    [Fact(DisplayName = "Root-composed Install dry-run and automatic apply never prompt or consume terminal input"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ExplicitNonPromptModesPreserveTerminalInput()
    {
        using var dryRunWorkspace = InstallOperationWorkspace.Create("install-composed-dry-run");
        var dryRunBefore = dryRunWorkspace.SnapshotHashes();
        var dryRun = await RunAsync(
            ["install", "--dry-run"],
            dryRunWorkspace.PhysicalPath,
            $"unused{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Equal(string.Empty, dryRun.StandardError);
        Assert.Equal("unused", dryRun.RemainingInput);
        Assert.Equal(dryRunBefore, dryRunWorkspace.SnapshotHashes());
        Assert.False(dryRun.LockInfrastructureExists);

        using var automaticWorkspace = InstallOperationWorkspace.Create("install-composed-automatic");
        var automatic = await RunAsync(
            ["install", "--automatic"],
            automaticWorkspace.PhysicalPath,
            $"unused{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, automatic.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, automatic.Status);
        Assert.Equal(string.Empty, automatic.StandardError);
        Assert.Equal("unused", automatic.RemainingInput);
        Assert.True(automaticWorkspace.AgentsDirectoryExists());
        Assert.True(automatic.LockInfrastructureExists);
    }

    [Fact(DisplayName = "Root-composed Install JSON and redirected human writes never prompt or consume input"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task NonPromptCapableWritesRequireAutomaticWithoutInputConsumption()
    {
        using var jsonWorkspace = InstallOperationWorkspace.Create("install-composed-json-nonprompt");
        var jsonBefore = jsonWorkspace.SnapshotHashes();
        var json = await RunAsync(
            ["install", "--json"],
            jsonWorkspace.PhysicalPath,
            $"unused{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(4, json.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, json.Status);
        Assert.Equal(string.Empty, json.StandardError);
        Assert.Equal("unused", json.RemainingInput);
        using (var document = JsonDocument.Parse(json.StandardOutput))
        {
            Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "install.confirmation-required",
                document.RootElement.GetProperty("result").GetProperty("findings")[0]
                    .GetProperty("code").GetString());
        }

        Assert.Equal(jsonBefore, jsonWorkspace.SnapshotHashes());
        Assert.False(json.LockInfrastructureExists);

        using var redirectedWorkspace = InstallOperationWorkspace.Create("install-composed-redirected");
        var redirectedBefore = redirectedWorkspace.SnapshotHashes();
        var redirected = await RunAsync(
            ["install"],
            redirectedWorkspace.PhysicalPath,
            $"unused{Environment.NewLine}",
            standardInputRedirected: true,
            promptOutputRedirected: true);

        Assert.Equal(4, redirected.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, redirected.Status);
        Assert.Equal(string.Empty, redirected.StandardOutput);
        Assert.DoesNotContain(ConfirmationPrompt, redirected.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", redirected.StandardError, StringComparison.Ordinal);
        Assert.Equal("unused", redirected.RemainingInput);
        Assert.Equal(redirectedBefore, redirectedWorkspace.SnapshotHashes());
        Assert.False(redirected.LockInfrastructureExists);
    }

    [Fact(DisplayName = "Root and Install leaf help are truthful direct terminal modes without workspace effects"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ComposedHelpRegistersOneDirectInstallLeaf()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-help");
        var missing = workspace.Combine("missing-workspace");
        var root = await RunAsync(
            ["--help"],
            workspace.PhysicalPath,
            string.Empty,
            standardInputRedirected: true,
            promptOutputRedirected: true);
        var leaf = await RunAsync(
            ["install", "--help", "--workspace", missing],
            workspace.PhysicalPath,
            string.Empty,
            standardInputRedirected: true,
            promptOutputRedirected: true);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Single(
            root.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("install", StringComparison.Ordinal));
        Assert.Contains("Managed content", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Install establishes or verifies Framework management.", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "--force and --prune permit only their documented changes",
            string.Join(" ", root.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Contains(
            "open-forge install [--force] [--automatic] [--dry-run] [global options]",
            string.Join(" ", leaf.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Contains("Confirmation", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Empty(workspace.SnapshotHashes());
        Assert.False(root.LockInfrastructureExists);
        Assert.False(leaf.LockInfrastructureExists);
    }

    private static async Task<InstallCompositionRun> RunAsync(
        string[] arguments,
        string currentDirectory,
        string standardInput,
        bool standardInputRedirected,
        bool promptOutputRedirected)
    {
        using var input = new StringReader(standardInput);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        using var lockStore = WorkspaceLockTestStore.Create("install-composition-lock-store");
        _ = lockStore.Track(new CliWorkspace(
            lexicalRoot: currentDirectory,
            physicalRoot: currentDirectory,
            selectedBy: CliWorkspaceSelectionMethod.CurrentDirectory));
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = input,
                PromptOutput = standardError,
                StandardInputRedirected = standardInputRedirected,
                PromptOutputRedirected = promptOutputRedirected,
                LockStoreRoot = lockStore.StoreRoot,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new InstallCompositionRun
        {
            ExitCode = completion.ExitCode,
            Status = completion.Status,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            RemainingInput = await input.ReadLineAsync(TestContext.Current.CancellationToken),
            LockInfrastructureExists = lockStore.InfrastructureExists,
        };
    }
}

internal sealed record InstallCompositionRun
{
    public required int ExitCode { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string? RemainingInput { get; init; }

    public required bool LockInfrastructureExists { get; init; }
}
