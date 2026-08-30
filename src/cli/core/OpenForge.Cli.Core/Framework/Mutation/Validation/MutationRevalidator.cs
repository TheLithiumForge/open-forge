using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal sealed class MutationRevalidator(FileExpectationValidator validator)
{
    private readonly FileExpectationValidator _validator = validator;

    internal ValueTask<MutationValidationResult> ValidateAsync(
        WorkspaceLockLease lease,
        IReadOnlyList<PlannedFileChange> changes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var workspace = lease.Request.Workspace;
        if (!lease.IsHeldFor(workspace))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock lease identity does not match its selected workspace."));
        }

        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "Mutation revalidation requires at least one planned change."));
        }

        return MutationValidationRunner.ValidateAsync(
            _validator,
            workspace,
            changes,
            cancellationToken);
    }

    internal ValueTask<MutationValidationResult> ValidateAsync(
        WorkspaceLockLease lease,
        IReadOnlyList<PlannedDirectoryCreation> directoryCreations,
        IReadOnlyList<PlannedFileChange> fileChanges,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var workspace = lease.Request.Workspace;
        if (!lease.IsHeldFor(workspace))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock lease identity does not match its selected workspace."));
        }

        ArgumentNullException.ThrowIfNull(directoryCreations);
        ArgumentNullException.ThrowIfNull(fileChanges);
        if (directoryCreations.Count == 0 && fileChanges.Count == 0)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "Mutation revalidation requires at least one planned effect."));
        }

        return MutationValidationRunner.ValidateAsync(
            _validator,
            workspace,
            directoryCreations,
            fileChanges,
            cancellationToken);
    }
}
