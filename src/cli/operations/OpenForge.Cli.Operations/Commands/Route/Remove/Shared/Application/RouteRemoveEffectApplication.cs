using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveEffectApplication(
    FileChangeApplier fileChangeApplier,
    DirectoryDeletionApplier directoryDeletionApplier,
    FileExpectationValidator expectationValidator,
    PhysicalPathResolver physicalPathResolver)
{
    private readonly FileChangeApplier _fileChangeApplier = fileChangeApplier;
    private readonly DirectoryDeletionApplier _directoryDeletionApplier = directoryDeletionApplier;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RouteRemoveApplicationProgress> ApplyAsync(
        RouteRemoveEffectApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var preparation = input.RecoveryPreparation.Preparation
            ?? throw new ArgumentException(
                "Route Remove effect application requires a prepared recovery bundle.",
                nameof(input));
        var projection = input.Plan.Projection;
        var receiptCount = projection.FileChanges.Length + projection.DirectoryDeletions.Length;
        var progress = new ApplicationReceiptProgress(input.Plan, preparation, receiptCount);
        var stop = cancellationToken.IsCancellationRequested
            ? Interrupted(input.Plan, FilesystemNotStartedReason.Cancelled)
            : null;
        try
        {
            stop = await ApplyAndVerifySettingsAsync(input, progress, stop, preparation, cancellationToken)
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

    private async ValueTask<ApplicationStop?> ApplyFilesAsync(
        RouteRemoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        RecoveryBundlePreparation preparation,
        CancellationToken cancellationToken)
    {
        foreach (var change in input.Plan.Projection.FileChanges)
        {
            if (stop is not null)
            {
                progress.Add(new RouteRemoveFileChangeReceipt(
                    NotStarted(input.Plan, change, stop.Reason, stop.Cause)));
                continue;
            }

            progress.Enter(change);
            var receipt = await ApplyFileAsync(input, change, preparation, cancellationToken)
                .ConfigureAwait(false);
            progress.Complete(new RouteRemoveFileChangeReceipt(receipt));
            stop = ReadStop(input.Plan, receipt);
        }

        return stop;
    }

    private async ValueTask<ApplicationStop?> ApplyDeletionsAsync(
        RouteRemoveEffectApplicationInput input,
        ApplicationReceiptProgress progress,
        ApplicationStop? stop,
        CancellationToken cancellationToken)
    {
        foreach (var deletion in input.Plan.Projection.DirectoryDeletions)
        {
            if (stop is not null)
            {
                progress.Add(new RouteRemoveDirectoryDeletionReceipt(
                    NotStarted(input.Plan, deletion, stop.Reason, stop.Cause)));
                continue;
            }

            progress.Enter(deletion);
            var receipt = await ApplyDeletionAsync(input, deletion, cancellationToken)
                .ConfigureAwait(false);
            progress.Complete(new RouteRemoveDirectoryDeletionReceipt(receipt));
            stop = ReadStop(input.Plan, receipt);
        }

        return stop;
    }
}
