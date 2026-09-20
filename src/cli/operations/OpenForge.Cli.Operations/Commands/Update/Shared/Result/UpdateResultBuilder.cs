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
        var result = new UpdateResult(new UpdateResultFormation
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
            PreviousContentAvailable = UpdatePreviousContentObserver.Read(execution.Request.Workspace, effects),
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
            Findings = preview.Findings.Concat(findings).Distinct().ToArray(),
        });
        if (result.Next is null && verification == UpdateVerificationState.Verified
            && recovery.State == UpdateRecoveryState.Retained && recovery.ResidualPath is { } bundlePath)
        {
            var gitPath = Path.Combine(execution.Request.Workspace.LexicalRoot, ".git");
            var hasGitDirectory = Directory.Exists(gitPath) && (File.GetAttributes(gitPath) & FileAttributes.ReparsePoint) == 0;
            return result with
            {
                Next = new(hasGitDirectory ? "git diff" : "open-forge doctor",
                    hasGitDirectory ? $"Review the changes with git diff. Previous content remains in {bundlePath}."
                        : $"Review previous content in the recovery bundle at {bundlePath}."),
            };
        }
        return result;
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
                Action = execution.OwnershipChange is null
                    ? execution.Build.Preview.Lifecycle.Action
                    : UpdateLifecycleAction.Publish,
                Outcome = execution.OwnershipChange is null
                    ? execution.Build.Preview.Lifecycle.Outcome
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
                Action = execution.OwnershipChange is null
                    ? execution.Build.Preview.Lifecycle.Action
                    : UpdateLifecycleAction.Publish,
                Outcome = execution.OwnershipChange is null
                    ? execution.Build.Preview.Lifecycle.Outcome
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
            .Where(effect => effect.FileChange.Kind != Framework.Mutation.Models.Filesystem.Files.PlannedFileChangeKind.Create)
            .Select(effect => effect.ResultEffect.Path)
            .ToList();
        if (execution.OwnershipChange?.Kind == Framework.Mutation.Models.Filesystem.Files.PlannedFileChangeKind.Replace)
        {
            paths.Add(Framework.Ownership.WorkspaceOwnershipDefinitions.RelativePath);
        }

        return paths;
    }
}
