using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class FileChangeApplier
{
    private FileStateSnapshot ValidateMatchedCheck(
        PlannedFileChange change,
        FileExpectationValidationResult check)
    {
        if (check.State != FileExpectationValidationState.Matched
            || check.Expectation != change.Expectation
            || check.Actual is null
            || check.Actual.Expectation != change.Expectation
            || check.PhysicalPath is null)
        {
            throw new ArgumentException(
                "File application requires one matched check for the planned change.",
                nameof(check));
        }

        if (change.Kind == PlannedFileChangeKind.Delete
            && check.Actual.Kind != FileExpectationKind.File)
        {
            throw new ArgumentException(
                "File application deletion requires an ordinary-file matched check.",
                nameof(check));
        }

        return check.Actual;
    }

    private bool TryValidateLeaseAndTarget(
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

        if (context.Check.PhysicalPath is not { } checkPhysicalPath
            || !PhysicalContainment.Contains(workspace.LexicalRoot, context.Change.LogicalPath)
            || !PhysicalContainment.Contains(workspace.PhysicalRoot, checkPhysicalPath))
        {
            cause = "The planned file target is outside the selected workspace.";
            return false;
        }

        if (context.Change.Expectation.PhysicalPath is { } expectedPhysicalPath
            && !string.Equals(
                expectedPhysicalPath,
                context.Check.PhysicalPath,
                PathComparison()))
        {
            cause = "The matched file check does not retain the planned physical identity.";
            return false;
        }

        return true;
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

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
            NotSupportedException or PlatformNotSupportedException => FilesystemFailureKind.Unsupported,
            ArgumentException or PathTooLongException => FilesystemFailureKind.InvalidPath,
            IOException => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(
                nameof(exception),
                exception.GetType(),
                "The filesystem exception is not defined."),
        };
}
