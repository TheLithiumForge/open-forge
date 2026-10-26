using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
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
    private const string ConfirmationSentence = "Apply these changes? [y/N]";

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Install dry-run serializes real safe-absence facts without persistent effects"),
     Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task DryRunSerializesRealSafeAbsenceFactsWithoutEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-dry-run-packet");
        var before = workspace.SnapshotHashes();
        var result = await RunAsync(
            ["install", "--dry-run", "--automatic", "--format", "json"], workspace.PhysicalPath,
            "unused", standardInputRedirected: true, promptOutputRedirected: true);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal("unused", result.RemainingInput);
        Assert.DoesNotContain(ConfirmationSentence, result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"], root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(workspace.PhysicalPath, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var facts = root.GetProperty("data");
        Assert.Equal(
            ["mode", "force", "automatic", "classification", "footprint", "lockPath", "frontmatter"],
            facts.EnumerateObject().Select(property => property.Name));
        Assert.Equal("root", facts.GetProperty("frontmatter").GetProperty("form").GetString());
        Assert.Equal("dry-run", facts.GetProperty("mode").GetString());
        Assert.False(facts.GetProperty("force").GetBoolean());
        Assert.True(facts.GetProperty("automatic").GetBoolean());
        Assert.Equal("safe-absence", facts.GetProperty("classification").GetString());
        var footprint = facts.GetProperty("footprint");
        Assert.Equal(["files", "directories", "sections"], footprint.EnumerateObject().Select(property => property.Name));
        Assert.True(footprint.GetProperty("files").GetInt32() > 0);
        Assert.True(footprint.GetProperty("directories").GetInt32() > 0);
        Assert.True(footprint.GetProperty("sections").GetInt32() > 0);
        Assert.Equal(".agents/open-forge.lock.json", facts.GetProperty("lockPath").GetString());
        var firstEffect = root.GetProperty("effects")[0];
        Assert.Equal(".agents", firstEffect.GetProperty("path").GetString());
        Assert.Equal("directory", firstEffect.GetProperty("kind").GetString());
        Assert.Equal("created", firstEffect.GetProperty("action").GetString());
        Assert.Equal("planned", firstEffect.GetProperty("outcome").GetString());
        var recovery = root.GetProperty("recovery");
        Assert.Equal("not-required", recovery.GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, recovery.GetProperty("path").ValueKind);
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(result.LockInfrastructureExists);
        Assert.False(workspace.RecoveryDirectoryExists());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root-composed Install prompts once for human apply and never prompts for its verified no-op"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionOwnsOneEligibleConfirmation()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-confirmation");
        var applied = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"{Environment.NewLine}3{Environment.NewLine}2{Environment.NewLine}y{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.EndsWith(
            ConfirmationSentence + Environment.NewLine,
            applied.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Would install the Open Forge Framework", applied.StandardError, StringComparison.Ordinal);
        Assert.StartsWith("Installed the Open Forge Framework", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Created 18 files and 13 directories under .agents", applied.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("Open Forge is already installed and current. Nothing to do.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("unused", noOp.RemainingInput);
        Assert.Equal(afterApply, workspace.SnapshotHashes());
        Assert.False(noOp.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root-composed Install refusal is interrupted without writes and preserves remaining input"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionRefusalPreservesNoWriteBoundary()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-refusal");
        var before = workspace.SnapshotHashes();
        var run = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"{Environment.NewLine}2{Environment.NewLine}2{Environment.NewLine}n{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains(ConfirmationSentence, run.StandardError, StringComparison.Ordinal);
        Assert.Contains("Install was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.AgentsDirectoryExists());
        Assert.False(run.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root-composed Install end of input cancels without writes"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionEndOfInputCancelsBeforeWrites()
    {
        using var workspace = InstallOperationWorkspace.Create("install-composed-end-of-input");
        var before = workspace.SnapshotHashes();
        var run = await RunAsync(
            ["install"],
            workspace.PhysicalPath,
            $"{Environment.NewLine}2{Environment.NewLine}2{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains("Install was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Status: interrupted", run.StandardError, StringComparison.Ordinal);
        var previewPosition = run.StandardError.IndexOf(
            "Would install the Open Forge Framework",
            StringComparison.Ordinal);
        var questionPosition = run.StandardError.IndexOf(ConfirmationSentence, StringComparison.Ordinal);
        Assert.True(previewPosition >= 0 && previewPosition < questionPosition);
        Assert.Null(run.RemainingInput);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.AgentsDirectoryExists());
        Assert.False(run.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
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

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root-composed Install JSON and redirected human writes never prompt or consume input"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task NonPromptCapableWritesRequireAutomaticWithoutInputConsumption()
    {
        using var jsonWorkspace = InstallOperationWorkspace.Create("install-composed-json-nonprompt");
        var jsonBefore = jsonWorkspace.SnapshotHashes();
        var json = await RunAsync(
            ["install", "--format", "json"],
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
            Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "install.confirmation-required",
                document.RootElement.GetProperty("findings")[0]
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
        Assert.DoesNotContain(ConfirmationSentence, redirected.StandardError, StringComparison.Ordinal);
        Assert.Contains("Install needs confirmation, and this session cannot ask.", redirected.StandardError, StringComparison.Ordinal);
        Assert.Equal("unused", redirected.RemainingInput);
        Assert.Equal(redirectedBefore, redirectedWorkspace.SnapshotHashes());
        Assert.False(redirected.LockInfrastructureExists);
    }

    [Trait("Boundary", "Host")]
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
        Assert.Contains("Already installed?", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("open-forge install --preset minimal", root.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Contains(
            "open-forge install [options]",
            string.Join(" ", leaf.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Contains("Custom routes", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Remove keeps existing files and routes.",
            string.Join(" ", leaf.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)), StringComparison.Ordinal);
        Assert.Contains("ask once after selection and checks", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Automation", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Empty(workspace.SnapshotHashes());
        Assert.False(root.LockInfrastructureExists);
        Assert.False(leaf.LockInfrastructureExists);
    }

    [Fact(DisplayName = "Install next commands retain the selected workspace and explicit Custom choices")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task NextRetainsTwoWorkspaceSelection()
    {
        using var current = InstallOperationWorkspace.Create("install-current-workspace");
        using var selected = InstallOperationWorkspace.Create("install-selected-workspace");
        var beforeCurrent = current.SnapshotHashes();
        var beforeSelected = selected.SnapshotHashes();
        string[] arguments = ["install", "--workspace", selected.PhysicalPath, "--preset", "custom", "--route", "guidance=git-ignore",
            "--route", "maps=remove", "--frontmatter", "scoped", "--force", "--detail", "standard", "--detail-filter", "warning"];
        var json = await RunAsync([.. arguments, "--format", "json"], current.PhysicalPath, "unused", true, true);
        Assert.Equal(4, json.ExitCode);
        using var document = JsonDocument.Parse(json.StandardOutput);
        var next = document.RootElement.GetProperty("next").GetProperty("command").GetString() ?? throw new InvalidOperationException("Retry was not supplied.");
        Assert.Contains($"--workspace {CliCommandText.Quote(selected.PhysicalPath)}", next, StringComparison.Ordinal);
        Assert.Contains("--preset custom", next, StringComparison.Ordinal);
        Assert.Contains("--route guidance=git-ignore", next, StringComparison.Ordinal);
        Assert.Contains("--route maps=remove", next, StringComparison.Ordinal);
        Assert.Equal(10, next.Split("--route ", StringSplitOptions.None).Length - 1);
        Assert.Contains("--frontmatter scoped --force --automatic", next, StringComparison.Ordinal);
        Assert.EndsWith("--format json --detail standard --detail-filter warning", next, StringComparison.Ordinal);
        var text = await RunAsync(arguments, current.PhysicalPath, "unused", true, true);
        Assert.Equal(130, text.ExitCode);
        Assert.Contains(CliRetryCommand.Build(arguments), text.StandardError, StringComparison.Ordinal);
        Assert.Equal(beforeCurrent, current.SnapshotHashes());
        Assert.Equal(beforeSelected, selected.SnapshotHashes());
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
