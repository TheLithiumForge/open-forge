using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryDeletionApplier
{
    private static FileStateSnapshot ValidateMatchedCheck(
        PlannedDirectoryDeletion deletion,
        FileExpectationValidationResult check)
    {
        if (check.State != FileExpectationValidationState.Matched
            || check.Expectation != deletion.Expectation
            || check.Actual is not { } actual
            || actual.Expectation != deletion.Expectation
            || actual.Kind != FileExpectationKind.Directory
            || check.PhysicalPath is null)
        {
            throw new ArgumentException(
                "Directory deletion requires one matched directory check for its planned target.",
                nameof(check));
        }

        return actual;
    }

    private static bool TryValidateLeaseAndTarget(
        ApplicationContext context,
        out string cause)
    {
        cause = string.Empty;
        var workspace = context.Lease.Request.Workspace;
        if (!context.Lease.IsHeldFor(workspace))
        {
            cause = "The workspace lock lease identity does not match its selected workspace.";
            return false;
        }

        if (!PhysicalContainment.Contains(workspace.LexicalRoot, context.Deletion.LogicalPath)
            || !PhysicalContainment.Contains(workspace.PhysicalRoot, context.PhysicalPath)
            || context.Deletion.Expectation.PhysicalPath is not { } expectedPhysicalPath
            || !PhysicalIdentityTracker.PathComparer.Equals(
                expectedPhysicalPath,
                context.PhysicalPath))
        {
            cause = "The planned directory deletion target is outside the selected workspace or changed identity.";
            return false;
        }

        return true;
    }

    private static bool IsSameMatchedDirectoryCheck(
        ApplicationContext context,
        FileExpectationValidationResult check)
        => check.State == FileExpectationValidationState.Matched
            && check.Expectation == context.Deletion.Expectation
            && check.Actual?.Expectation == context.Deletion.Expectation
            && check.Actual.Kind == FileExpectationKind.Directory
            && check.PhysicalPath is { } physicalPath
            && PhysicalIdentityTracker.PathComparer.Equals(
                context.PhysicalPath,
                physicalPath);
}
