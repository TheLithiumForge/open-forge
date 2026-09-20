using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal sealed class PhysicalPathResolver
{
    internal PhysicalPathResolution ResolveRoot(string workspaceRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        try
        {
            var normalized = Path.GetFullPath(workspaceRoot);
            return new PathComponentWalker(new PhysicalIdentityTracker())
                .ResolveUnboundedAbsolute(normalized, normalized);
        }
        catch (PlatformNotSupportedException exception)
        {
            return Unsupported(workspaceRoot, exception);
        }
        catch (NotSupportedException exception)
        {
            return Unsupported(workspaceRoot, exception);
        }
        catch (Exception exception) when (IsInvalidPathException(exception))
        {
            return Invalid(workspaceRoot, exception);
        }
    }

    internal PhysicalPathResolution ResolveCandidate(
        string lexicalWorkspaceRoot,
        string physicalWorkspaceRoot,
        string candidatePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lexicalWorkspaceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalWorkspaceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);
        try
        {
            var lexicalRoot = Path.GetFullPath(lexicalWorkspaceRoot);
            var physicalRoot = Path.GetFullPath(physicalWorkspaceRoot);
            var candidate = Path.GetFullPath(candidatePath);
            if (!PhysicalContainment.Contains(lexicalRoot, candidate))
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.External, candidate, candidate);
            }

            var relative = Path.GetRelativePath(lexicalRoot, candidate);
            return new PathComponentWalker(new PhysicalIdentityTracker())
                .ResolveContainedRelative(candidate, physicalRoot, relative);
        }
        catch (PlatformNotSupportedException exception)
        {
            return Unsupported(candidatePath, exception);
        }
        catch (NotSupportedException exception)
        {
            return Unsupported(candidatePath, exception);
        }
        catch (Exception exception) when (IsInvalidPathException(exception))
        {
            return Invalid(candidatePath, exception);
        }
    }

    private static bool IsInvalidPathException(Exception exception)
    {
        return exception is ArgumentException or PathTooLongException;
    }

    private static PhysicalPathResolution Invalid(string logicalPath, Exception exception)
    {
        return PhysicalPathResolution.Failed(
            PhysicalPathState.Invalid,
            logicalPath,
            FilesystemFailure.FromException(FilesystemFailureKind.InvalidPath, exception));
    }

    private static PhysicalPathResolution Unsupported(string logicalPath, Exception exception)
    {
        return PhysicalPathResolution.Failed(
            PhysicalPathState.Unsupported,
            logicalPath,
            FilesystemFailure.FromException(FilesystemFailureKind.Unsupported, exception));
    }
}
