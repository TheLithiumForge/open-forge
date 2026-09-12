using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairC1LifecycleIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public static async Task CancellationBeforeLeaseProducesAnInterruptedCompleteResult(bool atLock)
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-c1-before-lease", includeGuided: false);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var plan = await ReadPlanAsync(workspace, components);
        var before = workspace.SnapshotState();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        RepairApplicationOutcome outcome;
        if (atLock)
        {
            var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
                new WorkspaceLockRequest(workspace.Workspace, RepairDefinitions.CommandIdentity, Guid.NewGuid()), cancellation.Token);
            Assert.Equal(WorkspaceLockState.Cancelled, acquired.State);
            Assert.Null(acquired.Lease);
            outcome = RepairApplicationOutcomeFactory.LockBoundary(acquired);
        }
        else
        {
            outcome = await components.Application.ExecuteAsync(plan, [], cancellation.Token);
        }

        var diagnosis = await new RepairBoundaryDiagnosisReader(components.DiagnosisReader).ReadAsync(plan.Request);
        var result = FormResult(plan, outcome with { PostDiagnosis = diagnosis });

        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
        Assert.Equal(0, result.Counts.AppliedEffects);
        Assert.Equal(0, result.Counts.VerifiedEffects);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.DoesNotContain(Assert.IsType<RepairPlan>(result.Plan).Steps, step => step.Outcome == RepairStepOutcome.Verified);
    }

    [Theory, InlineData(0), InlineData(1), Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public static async Task PreparedCancellationCountsOnlyActuallyAppliedAndVerifiedEffects(int appliedCount)
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-c1-prepared", includeGuided: false);
        File.AppendAllText(workspace.Combine(RepairIntegrationWorkspace.GuidedTargetPath), "\n[Second source](./guide.md)\n");
        var components = RepairOperationFactory.CreateDefaultComponents();
        var plan = await ReadPlanAsync(workspace, components);
        Assert.Equal(2, plan.Effects.Count);
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
        var receipts = new List<FileChangeReceipt>();
        for (var index = 0; index < appliedCount; index++)
        {
            var receipt = await new FileChangeApplier(revalidator, validator).ApplyAsync(
                lease, plan.Effects[index].FileChange, validation.Checks[index], preparation, TestContext.Current.CancellationToken);
            Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
            Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
            receipts.Add(receipt);
        }

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var outcome = await new RepairApplicationCompletion(new RepairPostVerifier(components.DiagnosisReader))
            .CompleteAsync(new RepairPreparedApplication(plan, lease, preparation, receipts), cancellation.Token);
        var result = FormResult(plan, outcome);

        Assert.Equal(RepairRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(preparation.BundlePath, result.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        for (var index = 0; index < plan.Effects.Count; index++)
        {
            var effect = plan.Effects[index];
            var expected = index < appliedCount ? effect.IntendedState : effect.ExpectedState;
            Assert.Equal(expected.Bytes.ToArray(), workspace.ReadBytes(effect.SourceCanonicalPath));
        }

        Assert.Equal(appliedCount, result.Counts.AppliedEffects);
        Assert.Equal(appliedCount, result.Counts.VerifiedEffects);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        var steps = Assert.IsType<RepairPlan>(result.Plan).Steps;
        Assert.Equal(appliedCount, steps.Count(step => step.Outcome == RepairStepOutcome.Verified));
        Assert.DoesNotContain(steps.Skip(appliedCount), step => step.Outcome is RepairStepOutcome.Applied or RepairStepOutcome.Verified);
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public static async Task CoalescedStepOutcomesDescribePreviewOrCompletedApplication(bool apply)
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-c1-step-lifecycle", includeGuided: false);
        File.AppendAllText(workspace.Combine(RepairIntegrationWorkspace.SourcePath), "\n[Second occurrence](./guide.md)\n");
        var before = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
        var result = await new RepairOperation(RepairOperationFactory.CreateDefaultComponents()).ExecuteAsync(
            workspace.Request(mode: apply ? RepairMode.Apply : RepairMode.DryRun, automatic: true), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RepairPlan>(result.Plan);
        Assert.Single(plan.Effects);
        Assert.Equal(2, plan.Steps.Count);
        Assert.All(plan.Steps, step => Assert.Equal(apply ? RepairStepOutcome.Verified : RepairStepOutcome.Planned, step.Outcome));
        Assert.Equal(apply ? 1 : 0, result.Counts.AppliedEffects);
        Assert.Equal(apply ? 1 : 0, result.Counts.VerifiedEffects);
        if (!apply)
        {
            Assert.Equal(before, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
            workspace.AssertNoWriteInfrastructure();
        }
    }

    private static RepairResult FormResult(RepairPlan plan, RepairApplicationOutcome outcome)
        => RepairResultBuilder.Build(new RepairResultInput
        {
            Request = plan.Request,
            Diagnosis = outcome.PostDiagnosis.Coverage,
            Plan = plan,
            Findings = outcome.Findings,
            InitialFindings = [],
            Preflight = outcome.Preflight,
            Application = outcome.Application,
            Verification = outcome.Verification,
            Recovery = outcome.Recovery,
            PostDiagnosis = outcome.PostDiagnosis,
        });

    private static async Task<RepairPlan> ReadPlanAsync(RepairIntegrationWorkspace workspace, RepairOperationComponents components)
    {
        var diagnosis = await components.DiagnosisReader.ReadAsync(new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var catalogue = await components.CatalogueReader.ReadAsync(
            workspace.Workspace, diagnosis.Observation.LocalReferences, TestContext.Current.CancellationToken);
        return RepairPlanner.Build(workspace.Request(automatic: true), catalogue.Proposals);
    }
}
