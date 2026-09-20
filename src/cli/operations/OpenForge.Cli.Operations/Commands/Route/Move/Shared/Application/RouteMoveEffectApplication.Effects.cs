using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveEffectApplication
{
    private async ValueTask<DirectoryCreationReceipt> ApplyCreationAsync(
        RouteMoveEffectApplicationInput input,
        PlannedDirectoryCreation creation,
        CancellationToken cancellationToken)
    {
        try
        {
            var check = await ValidateAsync(input.Plan, creation.Expectation, cancellationToken)
                .ConfigureAwait(false);
            return check.State == FileExpectationValidationState.Matched
                ? await _directoryCreationApplier.ApplyAsync(
                    input.Lease, creation, check, cancellationToken).ConfigureAwait(false)
                : NotStarted(input.Plan, creation, Reason(check.State), Cause(check));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return CompletionUnknown(input.Plan, creation);
        }
    }

    private async ValueTask<FileChangeReceipt> ApplyFileAsync(
        RouteMoveEffectApplicationInput input,
        PlannedFileChange change,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        try
        {
            var check = await ValidateAsync(input.Plan, change.Expectation, cancellationToken)
                .ConfigureAwait(false);
            RecoveryBundlePreparation? changePreparation = null;
            if (change.Kind != PlannedFileChangeKind.Create)
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
        RouteMoveEffectApplicationInput input,
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
        RouteMovePlan plan,
        FileExpectation expectation,
        CancellationToken cancellationToken)
        => await _expectationValidator.ValidateAsync(
            plan.Request.Workspace,
            expectation,
            cancellationToken).ConfigureAwait(false);
}
