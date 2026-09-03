using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryDeletionApplier(
    MutationRevalidator revalidator,
    FileExpectationValidator validator)
{
    private const string CancellationBeforeEffectCause =
        "Directory deletion was cancelled before its target effect.";
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly FileExpectationValidator _validator = validator;

    internal async ValueTask<DirectoryDeletionReceipt> ApplyAsync(
        WorkspaceLockLease lease,
        PlannedDirectoryDeletion deletion,
        FileExpectationValidationResult check,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(deletion);
        ArgumentNullException.ThrowIfNull(check);

        var before = ValidateMatchedCheck(deletion, check);
        var context = new ApplicationContext(
            lease,
            deletion,
            before,
            check.PhysicalPath
                ?? throw new InvalidOperationException(
                    "A matched directory deletion check requires its physical target."));
        if (!TryValidateLeaseAndTarget(context, out var leaseCause))
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.ContractRejected,
                leaseCause);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(context);
        }

        var revalidation = await RevalidateBeforeEffectAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        if (revalidation is not null)
        {
            return revalidation;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(context);
        }

        var emptiness = InspectEmptiness(context, cancellationToken);
        if (emptiness is not null)
        {
            return emptiness;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(context);
        }

        try
        {
            Directory.Delete(context.PhysicalPath, recursive: false);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return await ResolveEffectFailureAsync(context, exception).ConfigureAwait(false);
        }

        return await VerifyAsync(context).ConfigureAwait(false);
    }

    private sealed record ApplicationContext(
        WorkspaceLockLease Lease,
        PlannedDirectoryDeletion Deletion,
        FileStateSnapshot Before,
        string PhysicalPath);
}
