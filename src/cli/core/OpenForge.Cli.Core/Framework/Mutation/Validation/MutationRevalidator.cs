using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal sealed class MutationRevalidator(
    FileExpectationValidator validator,
    PhysicalPathResolver physicalPathResolver)
{
    private readonly FileExpectationValidator _validator = validator;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal ValueTask<MutationValidationResult> ValidateAsync(
        WorkspaceLockLease lease,
        IReadOnlyList<PlannedFileChange> changes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeld)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "Mutation revalidation requires a live workspace lock lease."));
        }

        var workspace = lease.Request.Workspace;
        if (!LockPathMatchesWorkspace(lease, workspace))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock lease identity does not match its selected workspace."));
        }

        var lockResolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lease.Request.LogicalPath);
        if (lockResolution.State != PhysicalPathState.Contained)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock path cannot be revalidated as a contained resolved physical path."));
        }

        var resolvedPhysicalPath = lockResolution.GetContainedPhysicalPath();
        if (!string.Equals(
                resolvedPhysicalPath,
                lease.PhysicalPath,
                PathComparison()))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock path changed its resolved physical path before revalidation."));
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
        if (!lease.IsHeld)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "Mutation revalidation requires a live workspace lock lease."));
        }

        var workspace = lease.Request.Workspace;
        if (!LockPathMatchesWorkspace(lease, workspace))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock lease identity does not match its selected workspace."));
        }

        var lockResolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lease.Request.LogicalPath);
        if (lockResolution.State != PhysicalPathState.Contained)
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock path cannot be revalidated as a contained resolved physical path."));
        }

        var resolvedPhysicalPath = lockResolution.GetContainedPhysicalPath();
        if (!string.Equals(
                resolvedPhysicalPath,
                lease.PhysicalPath,
                PathComparison()))
        {
            return ValueTask.FromResult(
                MutationValidationResult.Blocked(
                    "The workspace lock path changed its resolved physical path before revalidation."));
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

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private static bool LockPathMatchesWorkspace(
        WorkspaceLockLease lease,
        CliWorkspace workspace)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(
            lease.LogicalPath,
            Path.Combine(workspace.LexicalRoot, WorkspaceLockRequest.RelativePath),
            comparison);
    }
}
