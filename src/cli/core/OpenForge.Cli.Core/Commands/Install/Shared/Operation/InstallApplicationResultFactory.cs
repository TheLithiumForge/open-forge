using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal static class InstallApplicationResultFactory
{
    internal static InstallApplicationOutcome Complete(
        InstallPlan plan,
        InstallApplicationProgress progress,
        InstallRecoveryOutcome recovery)
        => Create(plan, progress, recovery, findings: []);

    internal static InstallApplicationOutcome Failed(
        InstallPlan plan,
        InstallApplicationProgress progress,
        InstallFinding finding,
        InstallRecoveryOutcome? recovery = null)
        => Create(
            plan,
            progress,
            recovery ?? ReadFailureRecovery(plan, progress),
            [finding]);

    internal static InstallRecoveryOutcome Recovery(
        InstallRecoveryState state,
        string? residualPath = null)
        => new()
        {
            State = state,
            ResidualPath = residualPath,
        };

    private static InstallRecoveryOutcome ReadFailureRecovery(
        InstallPlan plan,
        InstallApplicationProgress progress)
    {
        if (progress.RecoveryPreparation is null)
        {
            return Recovery(
                plan.RequiresRecovery
                    ? InstallRecoveryState.NotCreated
                    : InstallRecoveryState.NotRequired);
        }

        return Recovery(
            InstallRecoveryState.Retained,
            progress.RecoveryPreparation.BundlePath);
    }

    private static InstallApplicationOutcome Create(
        InstallPlan plan,
        InstallApplicationProgress progress,
        InstallRecoveryOutcome recovery,
        IReadOnlyList<InstallFinding> findings)
        => new()
        {
            Findings = findings,
            Facts = InstallResultFactsFactory.FromApplication(plan, progress, recovery),
            Summary = new InstallOperationSummary
            {
                ManagementState = plan.ManagementState,
                PlannedDirectoryCount = plan.DirectoryCreations.Count,
                PlannedFileCount = plan.FileChanges.Count,
                AppliedDirectoryCount = progress.AppliedDirectoryCount,
                AppliedTargetFileCount = progress.AppliedTargetFileCount,
                LifecyclePublished = progress.LifecyclePublished,
                RecoveryState = recovery.State,
                RecoveryResidualPath = recovery.ResidualPath,
            },
        };
}
