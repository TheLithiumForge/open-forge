using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Result;

internal static class UpdateResultBuilder
{
    internal static UpdateResult Create(
        UpdatePlanExecution execution,
        IReadOnlyList<UpdatePhysicalEffect> effects,
        UpdateLifecycle lifecycle,
        UpdateRecovery recovery,
        UpdateVerificationState verification,
        IReadOnlyList<UpdateFinding> findings)
    {
        var preview = execution.Build.Preview;
        return new UpdateResult(new UpdateResultFormation
        {
            Workspace = execution.Request.Workspace,
            Mode = execution.Request.Mode,
            Force = execution.Request.Force,
            Prune = execution.Request.Prune,
            Automatic = execution.Request.Automatic,
            Source = preview.Source,
            Comparisons = preview.Comparisons,
            GeneratedNavigation = preview.GeneratedNavigation,
            Effects = effects,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
            Findings = findings,
        });
    }

    internal static UpdateResult BeforeEffects(
        UpdatePlanExecution execution,
        UpdateFinding finding)
        => Create(
            execution,
            effects: [],
            new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = execution.LifecycleChange is null
                    ? UpdateLifecycleAction.Preserve
                    : UpdateLifecycleAction.Publish,
                Outcome = execution.LifecycleChange is null
                    ? UpdateLifecycleOutcome.AlreadyCurrent
                    : UpdateLifecycleOutcome.NotStarted,
            },
            new UpdateRecovery
            {
                State = UpdateRecoveryState.NotCreated,
                ProtectedPaths = ProtectedPaths(execution),
                ResidualPath = null,
            },
            UpdateVerificationState.NotRequested,
            [finding]);

    internal static UpdateResult ChangedPlan(
        UpdatePlanExecution execution,
        string cause)
        => Create(
            execution,
            effects: [],
            new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = execution.LifecycleChange is null
                    ? UpdateLifecycleAction.Preserve
                    : UpdateLifecycleAction.Publish,
                Outcome = execution.LifecycleChange is null
                    ? UpdateLifecycleOutcome.AlreadyCurrent
                    : UpdateLifecycleOutcome.NotStarted,
            },
            new UpdateRecovery
            {
                State = UpdateRecoveryState.NotCreated,
                ProtectedPaths = ProtectedPaths(execution),
                ResidualPath = null,
            },
            UpdateVerificationState.NotRequested,
            [
                new UpdateFinding(UpdateFindingCode.TargetUnsafe, target: null, cause),
                new UpdateFinding(
                    UpdateFindingCode.PlanBlocked,
                    target: null,
                    "The complete Update plan changed before application."),
            ]);

    internal static IReadOnlyList<string> ProtectedPaths(UpdatePlanExecution execution)
    {
        var paths = execution.Effects
            .Where(effect => effect.FileChange.Kind != Framework.Mutation.Models.Filesystem.PlannedFileChangeKind.Create)
            .Select(effect => effect.ResultEffect.Path)
            .ToList();
        if (execution.LifecycleChange is not null)
        {
            paths.Add(Framework.Lifecycle.LifecycleSchema.RelativePath);
        }

        return paths;
    }
}
