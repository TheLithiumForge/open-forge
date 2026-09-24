using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication
{
    private async ValueTask<ApplicationStop?> ApplyAndVerifySettingsAsync(
        RouteRemoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        var change = input.Plan.Projection.SettingsChange;
        if (stop is not null)
        {
            if (change is not null)
            {
                var receipt = NotStarted(input.Plan, change, stop.Reason, stop.Cause);
                progress.SetSettingsReceipt(receipt);
                progress.SetSettingsOutcome(RouteRemovePersistenceOutcome.NotStarted);
            }

            return stop;
        }

        if (change is not null)
        {
            progress.SetSettingsOutcome(RouteRemovePersistenceOutcome.Unknown);
            var receipt = await ApplyFileAsync(input, change, preparation, cancellationToken)
                .ConfigureAwait(false);
            progress.SetSettingsReceipt(receipt);
            var receiptStop = ReadStop(input.Plan, receipt);
            if (receiptStop is not null)
            {
                progress.SetSettingsOutcome(ReadReceiptOutcome(receipt));
                return receiptStop;
            }

            var problem = await ReadSettingsProblemAsync(input.Plan, change, cancellationToken)
                .ConfigureAwait(false);
            if (problem is not null)
            {
                return new ApplicationStop(
                    new RouteRemoveFinding(
                        RouteRemoveFindingCode.VerificationFailed,
                        CliSemanticStatus.Failed,
                        WorkspaceSettingsDefinitions.RelativePath,
                        problem),
                    FilesystemNotStartedReason.ContractRejected,
                    problem);
            }

            progress.SetSettingsOutcome(RouteRemovePersistenceOutcome.Applied);
            return null;
        }

        var unchangedProblem = await ReadSettingsProblemAsync(input.Plan, appliedChange: null, cancellationToken)
            .ConfigureAwait(false);
        if (unchangedProblem is null)
        {
            progress.SetSettingsOutcome(RouteRemovePersistenceOutcome.Unchanged);
            return null;
        }

        var observed = await ReadCurrentSettingsAsync(input.Plan, cancellationToken).ConfigureAwait(false);
        var unavailable = observed.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observed.Snapshot is null;
        return new ApplicationStop(
            new RouteRemoveFinding(
                unavailable ? RouteRemoveFindingCode.SettingsUnavailable : RouteRemoveFindingCode.TargetChanged,
                CliSemanticStatus.Blocked,
                WorkspaceSettingsDefinitions.RelativePath,
                unchangedProblem),
            unavailable
                ? FilesystemNotStartedReason.ContractRejected
                : FilesystemNotStartedReason.TargetChanged,
            unchangedProblem);
    }

    private async ValueTask<string?> ReadSettingsProblemAsync(
        RouteRemovePlan plan,
        PlannedFileChange? appliedChange,
        CancellationToken cancellationToken)
    {
        var current = await ReadCurrentSettingsAsync(plan, cancellationToken).ConfigureAwait(false);
        if (current.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || current.Snapshot is null)
        {
            return current.Cause
                ?? "The workspace settings became unavailable or invalid before content removal.";
        }

        if (appliedChange is null)
        {
            if (!plan.Projection.Settings.MatchesObservation(current))
            {
                return "The workspace settings changed after the Route Remove plan was accepted.";
            }
        }
        else if (current.State != WorkspaceSettingsReadState.Complete
            || !current.Snapshot.Bytes.AsSpan().SequenceEqual(appliedChange.IntendedBytes.AsSpan()))
        {
            return "The written workspace settings do not match the exact planned removal bytes.";
        }

        var selection = plan.Projection.RemovalSelection;
        var paths = selection.Categories.Select(category => $".agents/{category}")
            .Concat(selection.Files)
            .Concat(selection.Directories);
        if (!paths.All(path => WorkspaceRemovals.IsPathRemoved(path, current.Document)))
        {
            return "The workspace settings do not establish every selected Route Remove exclusion.";
        }

        return null;
    }

    private ValueTask<WorkspaceSettingsRead> ReadCurrentSettingsAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
        => WorkspaceSettingsReader.ReadAsync(
            _physicalPathResolver,
            plan.Request.Workspace,
            cancellationToken);

    private static RouteRemovePersistenceOutcome ReadReceiptOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => RouteRemovePersistenceOutcome.Unknown,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) => RouteRemovePersistenceOutcome.Failed,
            (FilesystemEffectState.Unknown, _) => RouteRemovePersistenceOutcome.Unknown,
            (FilesystemEffectState.NotStarted, _) => RouteRemovePersistenceOutcome.NotStarted,
            _ => RouteRemovePersistenceOutcome.Unknown,
        };
}
