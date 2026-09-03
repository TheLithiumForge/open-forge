using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveEffectApplication(
    DirectoryCreationApplier directoryCreationApplier,
    FileChangeApplier fileChangeApplier,
    DirectoryDeletionApplier directoryDeletionApplier,
    FileExpectationValidator expectationValidator)
{
    private readonly DirectoryCreationApplier _directoryCreationApplier = directoryCreationApplier;
    private readonly FileChangeApplier _fileChangeApplier = fileChangeApplier;
    private readonly DirectoryDeletionApplier _directoryDeletionApplier = directoryDeletionApplier;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteMoveApplicationProgress> ApplyAsync(
        RouteMoveEffectApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var preparation = input.RecoveryPreparation.Preparation
            ?? throw new ArgumentException(
                "Route Move effect application requires a prepared recovery bundle.",
                nameof(input));
        var projection = input.Plan.Projection;
        var receiptCount = projection.DirectoryCreations.Length
            + projection.FileChanges.Length
            + projection.DirectoryDeletions.Length;
        var progress = new ApplicationReceiptProgress(input.Plan, preparation, receiptCount);
        var stop = cancellationToken.IsCancellationRequested
            ? Interrupted(input.Plan, FilesystemNotStartedReason.Cancelled)
            : null;
        try
        {
            stop = await ApplyCreationsAsync(input, progress, stop, cancellationToken)
                .ConfigureAwait(false);
            stop = await ApplyFilesAsync(input, progress, stop, preparation, cancellationToken)
                .ConfigureAwait(false);
            stop = await ApplyDeletionsAsync(input, progress, stop, cancellationToken)
                .ConfigureAwait(false);
            return progress.Form(stop);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return progress.CloseUnexpected(interrupted: true);
        }
        catch (Exception)
        {
            return progress.CloseUnexpected(interrupted: false);
        }
    }

    private async ValueTask<ApplicationStop?> ApplyCreationsAsync(
        RouteMoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        CancellationToken cancellationToken)
    {
        foreach (var creation in input.Plan.Projection.DirectoryCreations)
        {
            if (stop is not null)
            {
                progress.Add(new RouteMoveDirectoryCreationReceipt(
                    NotStarted(input.Plan, creation, stop.Reason, stop.Cause)));
                continue;
            }

            progress.Enter(creation);
            var receipt = await ApplyCreationAsync(input, creation, cancellationToken)
                .ConfigureAwait(false);
            progress.Complete(new RouteMoveDirectoryCreationReceipt(receipt));
            stop = ReadStop(input.Plan, receipt);
        }

        return stop;
    }

    private async ValueTask<ApplicationStop?> ApplyFilesAsync(
        RouteMoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        foreach (var change in input.Plan.Projection.FileChanges)
        {
            if (stop is not null)
            {
                progress.Add(new RouteMoveFileChangeReceipt(
                    NotStarted(input.Plan, change, stop.Reason, stop.Cause)));
                continue;
            }

            progress.Enter(change);
            var receipt = await ApplyFileAsync(input, change, preparation, cancellationToken)
                .ConfigureAwait(false);
            progress.Complete(new RouteMoveFileChangeReceipt(receipt));
            stop = ReadStop(input.Plan, receipt);
        }

        return stop;
    }

    private async ValueTask<ApplicationStop?> ApplyDeletionsAsync(
        RouteMoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        CancellationToken cancellationToken)
    {
        foreach (var deletion in input.Plan.Projection.DirectoryDeletions)
        {
            if (stop is not null)
            {
                progress.Add(new RouteMoveDirectoryDeletionReceipt(
                    NotStarted(input.Plan, deletion, stop.Reason, stop.Cause)));
                continue;
            }

            progress.Enter(deletion);
            var receipt = await ApplyDeletionAsync(input, deletion, cancellationToken)
                .ConfigureAwait(false);
            progress.Complete(new RouteMoveDirectoryDeletionReceipt(receipt));
            stop = ReadStop(input.Plan, receipt);
        }

        return stop;
    }

}
