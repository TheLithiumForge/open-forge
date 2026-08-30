using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryCreationApplier
{
    private static FileStateSnapshot ValidateMatchedCheck(
        PlannedDirectoryCreation creation,
        FileExpectationValidationResult check)
    {
        if (check.State != FileExpectationValidationState.Matched
            || check.Expectation != creation.Expectation
            || check.Actual is null
            || check.Actual.Expectation != creation.Expectation
            || check.Actual.Kind != FileExpectationKind.Missing
            || check.PhysicalPath is null)
        {
            throw new ArgumentException(
                "Directory creation requires one matched missing check for its planned target.",
                nameof(check));
        }

        return check.Actual;
    }

    private bool TryValidateLeaseAndTarget(
        ApplicationContext context,
        out string cause)
    {
        cause = string.Empty;
        if (!context.Lease.IsHeld)
        {
            cause = "Directory creation requires a live workspace lock lease.";
            return false;
        }

        var workspace = context.Lease.Request.Workspace;
        var expectedLockPath = Path.Combine(
            workspace.LexicalRoot,
            WorkspaceLockRequest.RelativePath);
        if (!string.Equals(
            context.Lease.LogicalPath,
            expectedLockPath,
            PathComparison()))
        {
            cause = "The workspace lock lease identity does not match its selected workspace.";
            return false;
        }

        var lockResolution = _validator.ResolvePath(
            workspace,
            context.Lease.LogicalPath);
        if (lockResolution.State != PhysicalPathState.Contained
            || !string.Equals(
                lockResolution.GetContainedPhysicalPath(),
                context.Lease.PhysicalPath,
                PathComparison()))
        {
            cause = "The workspace lock path changed before directory creation.";
            return false;
        }

        if (!PhysicalContainment.Contains(
                workspace.LexicalRoot,
                context.Creation.LogicalPath)
            || !PhysicalContainment.Contains(
                workspace.PhysicalRoot,
                context.IntendedPhysicalPath))
        {
            cause = "The planned directory target is outside the selected workspace.";
            return false;
        }

        return true;
    }

    private static bool IsSameMatchedMissingCheck(
        ApplicationContext context,
        FileExpectationValidationResult check)
        => check.State == FileExpectationValidationState.Matched
            && check.Expectation == context.Creation.Expectation
            && check.Actual?.Expectation == context.Creation.Expectation
            && check.Actual.Kind == FileExpectationKind.Missing
            && string.Equals(
                check.PhysicalPath,
                context.IntendedPhysicalPath,
                PathComparison());

    private static bool IsOrdinaryDirectory(string physicalPath)
    {
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            return (attributes & FileAttributes.Directory) != 0
                && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return false;
        }
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static bool IsFilesystemException(Exception exception)
        => exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException
            or ArgumentException
            or PathTooLongException;

    private static FilesystemFailureKind FailureKind(Exception exception)
        => exception switch
        {
            UnauthorizedAccessException => FilesystemFailureKind.AccessDenied,
            NotSupportedException or PlatformNotSupportedException =>
                FilesystemFailureKind.Unsupported,
            ArgumentException or PathTooLongException => FilesystemFailureKind.InvalidPath,
            IOException => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(
                nameof(exception),
                exception.GetType(),
                "The filesystem exception is not defined."),
        };
}
