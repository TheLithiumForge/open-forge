using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal static partial class MutationValidationRunner
{
    internal static async ValueTask<MutationValidationResult> ValidateAsync(
        FileExpectationValidator validator,
        CliWorkspace workspace,
        IReadOnlyList<PlannedDirectoryCreation> directoryCreations,
        IReadOnlyList<PlannedFileChange> fileChanges,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(directoryCreations);
        ArgumentNullException.ThrowIfNull(fileChanges);
        if (cancellationToken.IsCancellationRequested)
        {
            return MutationValidationResult.Cancelled();
        }

        if (directoryCreations.Count == 0 && fileChanges.Count == 0)
        {
            return MutationValidationResult.Valid();
        }

        var structure = new CombinedPlanStructure(validator, workspace);

        foreach (var creation in directoryCreations)
        {
            if (creation is null)
            {
                return MutationValidationResult.Blocked(
                    "Planned directory creations cannot contain null members.");
            }

            if (!structure.TryRegisterTarget(
                creation.LogicalPath,
                expectedPhysicalPath: null,
                out var targetCause))
            {
                return MutationValidationResult.Blocked(targetCause);
            }

            if (!structure.HasPermittedDirectParent(
                creation.LogicalPath,
                out var parentCause))
            {
                return MutationValidationResult.Blocked(parentCause);
            }

            structure.AddPlannedDirectory(creation.LogicalPath);
        }

        foreach (var change in fileChanges)
        {
            if (change is null)
            {
                return MutationValidationResult.Blocked(
                    "Planned file changes cannot contain null members.");
            }

            if (!structure.TryRegisterTarget(
                change.LogicalPath,
                change.Expectation.PhysicalPath,
                out var targetCause))
            {
                return MutationValidationResult.Blocked(targetCause);
            }

            if (!structure.HasPermittedDirectParent(
                change.LogicalPath,
                out var parentCause))
            {
                return MutationValidationResult.Blocked(parentCause);
            }
        }

        var checks = new List<FileExpectationValidationResult>(
            directoryCreations.Count + fileChanges.Count);
        var actualPhysicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        foreach (var creation in directoryCreations)
        {
            var result = await ValidateTargetAsync(
                validator,
                workspace,
                creation.Expectation,
                checks,
                actualPhysicalPaths,
                cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }

        foreach (var change in fileChanges)
        {
            var result = await ValidateTargetAsync(
                validator,
                workspace,
                change.Expectation,
                checks,
                actualPhysicalPaths,
                cancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                return result;
            }
        }

        return MutationValidationResult.FromChecks(checks);
    }

    private static async ValueTask<MutationValidationResult?> ValidateTargetAsync(
        FileExpectationValidator validator,
        CliWorkspace workspace,
        FileExpectation expectation,
        ICollection<FileExpectationValidationResult> checks,
        ISet<string> actualPhysicalPaths,
        CancellationToken cancellationToken)
    {
        var check = await validator
            .ValidateAsync(workspace, expectation, cancellationToken)
            .ConfigureAwait(false);
        checks.Add(check);
        if (check.State != FileExpectationValidationState.Matched)
        {
            return MutationValidationResult.FromChecks(checks);
        }

        if (check.PhysicalPath is { } physicalPath
            && !actualPhysicalPaths.Add(physicalPath))
        {
            return MutationValidationResult.Blocked(
                "Directory and file effects cannot resolve to one physical target more than once.",
                checks);
        }

        return null;
    }

}
