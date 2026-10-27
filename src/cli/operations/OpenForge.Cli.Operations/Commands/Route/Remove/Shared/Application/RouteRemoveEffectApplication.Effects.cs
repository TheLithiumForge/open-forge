using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication
{
    private async ValueTask<FileChangeReceipt> ApplyFileAsync(
        RouteRemoveEffectApplicationInput input,
        PlannedFileChange change,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        try
        {
            var check = await ValidateAsync(input.Plan, change.Expectation, cancellationToken)
                .ConfigureAwait(false);
            RecoveryBundlePreparation? changePreparation = null;
            if (change.Kind != PlannedFileChangeKind.Create
                || Equals(change, input.Plan.Projection.SettingsChange))
            {
                changePreparation = preparation;
            }

            return check.State == FileExpectationValidationState.Matched
                ? await _fileChangeApplier.ApplyAsync(
                    input.Lease, change, check, changePreparation, cancellationToken)
                    .ConfigureAwait(false)
                : NotStarted(input.Plan, change, Reason(check.State), Cause(check));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CompletionUnknown(input.Plan, change);
        }
    }

    private async ValueTask<DirectoryDeletionReceipt> ApplyDeletionAsync(
        RouteRemoveEffectApplicationInput input,
        PlannedDirectoryDeletion deletion,
        CancellationToken cancellationToken)
    {
        try
        {
            var check = await ValidateAsync(input.Plan, deletion.Expectation, cancellationToken)
                .ConfigureAwait(false);
            return check.State == FileExpectationValidationState.Matched
                ? await _directoryDeletionApplier.ApplyAsync(
                    input.Lease, deletion, check, cancellationToken).ConfigureAwait(false)
                : NotStarted(input.Plan, deletion, Reason(check.State), Cause(check));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CompletionUnknown(input.Plan, deletion);
        }
    }

    private async ValueTask<FileExpectationValidationResult> ValidateAsync(
        RouteRemovePlan plan,
        FileExpectation expectation,
        CancellationToken cancellationToken)
    {
        if (plan.Projection.Subject.OutsideRoute is not null)
        {
            var cause = RouteRemoveOutsideBoundaryInspector.ValidateCurrentAnchors(plan, cancellationToken);
            if (cause is null && plan.Projection.Ownership.Snapshot is { } ownership)
            {
                var currentOwnership = await _expectationValidator.ValidateAsync(plan.Request.Workspace, ownership.Expectation, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (currentOwnership.State != FileExpectationValidationState.Matched)
                    cause = "The retained ownership observation changed before an outside removal effect.";
            }
            if (cause is null && expectation.LogicalPath != plan.Projection.SettingsChange?.LogicalPath)
                cause = await ReadSettingsProblemAsync(plan, plan.Projection.SettingsChange, cancellationToken).ConfigureAwait(false);
            var control = expectation.LogicalPath == plan.Projection.SettingsChange?.LogicalPath
                || expectation.LogicalPath == plan.Projection.OwnershipChange?.LogicalPath;
            if (cause is null && !control)
            {
                var path = Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, expectation.LogicalPath).Replace('\\', '/');
                cause = RouteRemoveOutsideBoundaryInspector.ValidateEffect(plan.Projection.Subject, path, cancellationToken);
            }
            if (cause is not null) return FileExpectationValidationResult.Blocked(expectation, cause);
            foreach (var snapshot in plan.Projection.References.Documents.Select(document => document.Snapshot)
                .Concat(plan.Projection.Navigation.ReadonlyInputs)
                .Where(snapshot => plan.Projection.FileChanges.All(change => change.LogicalPath != snapshot.LogicalPath))
                .DistinctBy(snapshot => snapshot.LogicalPath))
            {
                var input = await _expectationValidator.ValidateAsync(plan.Request.Workspace, snapshot.Expectation, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (input.State != FileExpectationValidationState.Matched)
                    return FileExpectationValidationResult.Blocked(expectation, "A retained outside removal input changed before an effect.");
            }
        }
        return await _expectationValidator.ValidateAsync(plan.Request.Workspace, expectation, cancellationToken).ConfigureAwait(false);
    }
}
