using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallRevalidationIntegrationTests
{
    private const string AuthoredSourcePath = ".agents/local-guidance.md";
    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private const string RecoveryInputPath = "recovery-race-input.md";

    [Fact(DisplayName = "Install blocks when an authored source is added between confirmation and lease acquisition"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task AddedAuthoredSourceInvalidatesTheConfirmedPlan()
    {
        await AssertAuthoredSourceRaceBlockedAsync(
            beforePrompt: null,
            duringPrompt: path => File.WriteAllText(
                path,
                AuthoredSource("Added source", "Added"),
                Encoding.UTF8));
    }

    [Fact(DisplayName = "Install blocks when an authored source changes between confirmation and lease acquisition"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ChangedAuthoredSourceInvalidatesTheConfirmedPlan()
    {
        await AssertAuthoredSourceRaceBlockedAsync(
            beforePrompt: AuthoredSource("Initial source", "Initial"),
            duringPrompt: path => File.WriteAllText(
                path,
                AuthoredSource("Changed source", "Changed"),
                Encoding.UTF8));
    }

    [Fact(DisplayName = "Install blocks when an authored source is removed between confirmation and lease acquisition"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RemovedAuthoredSourceInvalidatesTheConfirmedPlan()
    {
        await AssertAuthoredSourceRaceBlockedAsync(
            beforePrompt: AuthoredSource("Removed source", "Removed"),
            duringPrompt: File.Delete);
    }

    [Fact(DisplayName = "Install preserves incomplete status when an authored source becomes unavailable after confirmation"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task UnavailableAuthoredSourcePreservesIncompleteResultMeaning()
    {
        using var workspace = InstallOperationWorkspace.Create("install-authored-source-unavailable");
        Directory.CreateDirectory(workspace.Combine(".agents"));
        var sourcePath = workspace.Combine(AuthoredSourcePath);
        File.WriteAllText(
            sourcePath,
            AuthoredSource("Unavailable source", "Unavailable"),
            Encoding.UTF8);
        FileStream? contendingSource = null;
        try
        {
            var result = await RunPromptedAsync(
                workspace,
                () => contendingSource = new FileStream(
                    sourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None));

            AssertPreEffectBoundary(
                result,
                CliSemanticStatus.Incomplete,
                InstallFindingCode.ProjectionUnavailable);
            var disposition = CliStatusDefinitions.Read(result.Status).Disposition;
            Assert.Equal(3, disposition.ExitCode);
            Assert.Equal(CliOutputTarget.StandardOutput, disposition.HumanOutputTarget);
            AssertNoInstallTargets(workspace);
        }
        finally
        {
            contendingSource?.Dispose();
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    [Fact(DisplayName = "Install blocks and preserves a recognized recovery candidate that appears after confirmation"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RecognizedRecoveryAppearanceInvalidatesTheConfirmedPlan()
    {
        using var workspace = InstallOperationWorkspace.Create("install-recovery-appearance");
        var inputPath = workspace.Combine(RecoveryInputPath);
        var priorBytes = "recovery race prior bytes\n"u8.ToArray();
        workspace.WriteText(RecoveryInputPath, Encoding.UTF8.GetString(priorBytes));
        RecoveryBundlePreparation? preparation = null;
        try
        {
            var before = FileStateSnapshot.File(inputPath, inputPath, priorBytes);
            var recoveryInput = RecoveryBundleInput.Create(
                workspace.Workspace,
                command: IndexDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    workspace.Workspace),
                operationId: Guid.NewGuid(),
                targets:
                [
                    RecoveryBundleTarget.Create(
                        PlannedFileChange.Replace(
                            before.Expectation,
                            "recovery race intended bytes\n"u8),
                        before),
                ]);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                recoveryInput,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var bundleBytes = await File.ReadAllBytesAsync(
                preparation.BundlePath,
                TestContext.Current.CancellationToken);
            File.Delete(preparation.BundlePath);
            File.Delete(inputPath);
            Directory.CreateDirectory(workspace.Combine(".agents"));

            var result = await RunPromptedAsync(
                workspace,
                () => File.WriteAllBytes(preparation.BundlePath, bundleBytes));

            AssertPreEffectBoundary(
                result,
                CliSemanticStatus.Blocked,
                InstallFindingCode.RecoveryConflict);
            Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(
                preparation.BundlePath,
                TestContext.Current.CancellationToken));
            Assert.Equal(1, await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
            AssertNoInstallTargets(workspace);
        }
        finally
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }

            if (preparation is not null && File.Exists(preparation.BundlePath))
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }

    [Fact(DisplayName = "Install blocks when planned-missing .agents appears after confirmation"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RacedAgentsDirectoryCannotSatisfyThePlannedDirectoryEffect()
    {
        using var workspace = InstallOperationWorkspace.Create("install-raced-agents");
        var result = await RunPromptedAsync(
            workspace,
            () => Directory.CreateDirectory(workspace.Combine(".agents")));

        AssertPreEffectBoundary(
            result,
            CliSemanticStatus.Blocked,
            InstallFindingCode.TargetUnsafe);
        Assert.True(workspace.AgentsDirectoryExists());
        AssertNoInstallTargets(workspace);
    }

    [Fact(DisplayName = "Install reports external lock contention before planned .agents creation"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ExternalLockContentionStartsNoWorkspaceEffect()
    {
        using var workspace = InstallOperationWorkspace.Create("install-raced-agents-lock");
        FileStream? contendingLease = null;
        try
        {
            var result = await RunPromptedAsync(
                workspace,
                () =>
                {
                    contendingLease = workspace.HoldExternalLock();
                });

            AssertPreEffectBoundary(
                result,
                CliSemanticStatus.Blocked,
                InstallFindingCode.WorkspaceUnsafe);
            Assert.NotNull(contendingLease);
            Assert.Equal(0, contendingLease.Length);
            Assert.False(workspace.AgentsDirectoryExists());
            AssertNoInstallTargets(workspace);
        }
        finally
        {
            contendingLease?.Dispose();
        }
    }

    private static async Task AssertAuthoredSourceRaceBlockedAsync(
        string? beforePrompt,
        Action<string> duringPrompt)
    {
        using var workspace = InstallOperationWorkspace.Create("install-authored-source-race");
        Directory.CreateDirectory(workspace.Combine(".agents"));
        var sourcePath = workspace.Combine(AuthoredSourcePath);
        if (beforePrompt is not null)
        {
            File.WriteAllText(sourcePath, beforePrompt, Encoding.UTF8);
        }

        try
        {
            var result = await RunPromptedAsync(
                workspace,
                () => duringPrompt(sourcePath));

            AssertPreEffectBoundary(
                result,
                CliSemanticStatus.Blocked,
                InstallFindingCode.TargetUnsafe);
            AssertNoInstallTargets(workspace);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    private static async Task<InstallResult> RunPromptedAsync(
        InstallOperationWorkspace workspace,
        Action mutation)
    {
        using var input = new MutatingAnswerReader(mutation);
        using var promptOutput = new StringWriter();
        var result = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    input,
                    promptOutput,
                    canPrompt: true),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    automatic: false,
                    allowsInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal("Apply this Install plan? [y/N] ", promptOutput.ToString());
        return result;
    }

    private static void AssertPreEffectBoundary(
        InstallResult result,
        CliSemanticStatus expectedStatus,
        InstallFindingCode expectedFindingCode)
    {
        Assert.Equal(expectedStatus, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(expectedFindingCode, finding.Code);
        Assert.NotEmpty(result.Facts.Effects);
        Assert.All(
            result.Facts.Effects,
            effect =>
            {
                Assert.Equal(InstallEffectOutcome.NotStarted, effect.Outcome);
                Assert.Equal(InstallEffectResidual.None, effect.Residual);
            });
        Assert.Equal(InstallLifecycleOutcome.NotStarted, result.Facts.Lifecycle.Outcome);
        Assert.Equal(
            InstallResultVerificationState.NotRequested,
            result.Facts.Verification.State);
    }

    private static void AssertNoInstallTargets(InstallOperationWorkspace workspace)
    {
        Assert.False(workspace.Exists("AGENTS.md"));
        Assert.False(workspace.Exists("CLAUDE.md"));
        Assert.False(workspace.Exists(".agents/loader.md"));
        Assert.False(workspace.Exists(LifecyclePath));
    }

    private static string AuthoredSource(string description, string tag)
        => OpenForgeDocumentSeed.Metadata(
            description,
            tags: [tag],
            body: $"# {description}\n");
}

internal sealed class MutatingAnswerReader(Action mutation) : TextReader
{
    private readonly Action _mutation = mutation;
    private bool _read;

    public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_read)
        {
            return ValueTask.FromResult<string?>(null);
        }

        _read = true;
        _mutation();
        return ValueTask.FromResult<string?>("y");
    }
}
