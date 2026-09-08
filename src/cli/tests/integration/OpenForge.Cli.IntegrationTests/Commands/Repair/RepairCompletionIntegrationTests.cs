using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairCompletionIntegrationTests
{
    [Fact(DisplayName = "Repair wizard default No leaves the workspace and write infrastructure unchanged"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task WizardDefaultNoHasNoEffects()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-wizard-no", includeGuided: false);
        var before = workspace.SnapshotState();
        using var output = new StringWriter();
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            InteractiveSession = new CliInteractiveSession(new StringReader("\n\n"), output, canPrompt: true),
        };
        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Fact(DisplayName = "Repair post-verification requires the addressed occurrence rather than an equivalent other link"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task AnotherOccurrenceDoesNotVerifyTheSelectedOccurrence()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-exact-occurrence", includeGuided: false);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var plan = await ReadPlanAsync(workspace, components);
        File.WriteAllText(workspace.Combine(RepairIntegrationWorkspace.SourcePath),
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath) + "\n[Other](guide.md)\n");
        var verified = await new RepairPostVerifier(components.DiagnosisReader).VerifyAsync(
            plan, [], TestContext.Current.CancellationToken);
        Assert.Equal(RepairVerificationState.Failed, verified.Verification.Targets);
    }

    [Fact(DisplayName = "Repair post-verification rejects a now-missing required Markdown fragment"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task MissingRequiredFragmentFailsVerification()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-required-fragment", includeGuided: false);
        File.WriteAllText(workspace.Combine(RepairIntegrationWorkspace.SourcePath),
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath).Replace("./guide.md", "./guide.md#Guide", StringComparison.Ordinal));
        var components = RepairOperationFactory.CreateDefaultComponents();
        var plan = await ReadPlanAsync(workspace, components);
        var effect = Assert.Single(plan.Effects);
        File.WriteAllBytes(workspace.Combine(RepairIntegrationWorkspace.SourcePath), [.. effect.IntendedState.Bytes]);
        File.WriteAllText(workspace.Combine(RepairIntegrationWorkspace.SafeTargetPath), "# Changed\n");
        var verified = await new RepairPostVerifier(components.DiagnosisReader).VerifyAsync(
            plan, [], TestContext.Current.CancellationToken);
        Assert.Equal(RepairVerificationState.Failed, verified.Verification.Targets);
    }

    [Theory(DisplayName = "Repair revalidates source and target facts changed during final confirmation"),
        InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public static async Task ConfirmationDoesNotAuthorizeChangedFacts(bool changeTarget)
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-confirmation-drift", includeGuided: false);
        var original = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var changed = original + "\nAn independently authored edit.\n";
        using var output = new StringWriter();
        using var input = new ConfirmationEditReader(() =>
        {
            if (changeTarget)
            {
                File.Move(workspace.Combine(RepairIntegrationWorkspace.SafeTargetPath),
                    workspace.Combine(".agents/docs/moved.md"));
            }
            else
            {
                File.WriteAllText(workspace.Combine(RepairIntegrationWorkspace.SourcePath), changed);
            }
        });
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            InteractiveSession = new CliInteractiveSession(input, output, canPrompt: true),
        };
        try
        {
            var result = await new RepairOperation(components).ExecuteAsync(
                workspace.Request(allowInteraction: true), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(changeTarget ? original : changed, workspace.ReadText(RepairIntegrationWorkspace.SourcePath));
            workspace.AssertNoRecoveryArtifacts();
        }
        finally
        {
            var movedPath = workspace.Combine(".agents/docs/moved.md");
            if (File.Exists(movedPath))
            {
                File.Move(movedPath, workspace.Combine(RepairIntegrationWorkspace.SafeTargetPath));
            }
        }
    }

    private sealed class ConfirmationEditReader(Action edit) : StringReader("select\nyes\n")
    {
        private int _reads;

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            if (++_reads == 2)
            {
                edit();
            }

            return base.ReadLineAsync(cancellationToken);
        }
    }

    [Fact(DisplayName = "Repair cancellation at post-diagnosis preserves applied bytes and its verified recovery final"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task PostDiagnosisCancellationRetainsPreparedRecovery()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-completion-cancellation", includeGuided: false);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var plan = await ReadPlanAsync(workspace, components);
        var operationId = Guid.NewGuid();
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, RepairDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await RepairRecoveryLifecycle.PrepareAsync(plan, operationId, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        var validation = await revalidator.ValidateAsync(lease,
            [.. plan.Effects.Select(effect => effect.FileChange)], TestContext.Current.CancellationToken);
        var receipt = await new FileChangeApplier(revalidator, validator).ApplyAsync(
            lease, plan.Effects[0].FileChange, validation.Checks[0], preparation, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await new RepairApplicationCompletion(new RepairPostVerifier(components.DiagnosisReader))
            .CompleteAsync(new RepairPreparedApplication(plan, lease, preparation, [receipt]), cancellation.Token);
        Assert.Equal(RepairRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(preparation.BundlePath, result.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(plan.Effects[0].IntendedState.Bytes.ToArray(), workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
        Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.Interrupted);
    }

    private static async Task<RepairPlan> ReadPlanAsync(
        RepairIntegrationWorkspace workspace,
        RepairOperationComponents components)
    {
        var diagnosis = await components.DiagnosisReader.ReadAsync(
            new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var catalogue = await components.CatalogueReader.ReadAsync(
            workspace.Workspace, diagnosis.Observation.LocalReferences, TestContext.Current.CancellationToken);
        return RepairPlanner.Build(workspace.Request(automatic: true), catalogue.Proposals);
    }
}
