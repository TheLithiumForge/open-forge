using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryCreationApplier(
    MutationRevalidator revalidator,
    FileExpectationValidator validator)
{
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly FileExpectationValidator _validator = validator;

    private const string CancellationBeforeEffectCause =
        "Directory creation was cancelled before its target effect.";

    internal async ValueTask<DirectoryCreationReceipt> ApplyAsync(
        WorkspaceLockLease lease,
        PlannedDirectoryCreation creation,
        FileExpectationValidationResult check,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(creation);
        ArgumentNullException.ThrowIfNull(check);

        var before = ValidateMatchedCheck(creation, check);
        var context = new ApplicationContext(
            lease,
            creation,
            before,
            check.PhysicalPath
                ?? throw new InvalidOperationException(
                    "A matched missing-directory check requires its prospective physical target."));
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

        try
        {
            _ = Directory.CreateDirectory(context.IntendedPhysicalPath);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return await ResolveEffectFailureAsync(
                context,
                exception).ConfigureAwait(false);
        }

        return await VerifyAsync(context).ConfigureAwait(false);
    }

    private sealed record ApplicationContext(
        WorkspaceLockLease Lease,
        PlannedDirectoryCreation Creation,
        FileStateSnapshot Before,
        string IntendedPhysicalPath);
}
