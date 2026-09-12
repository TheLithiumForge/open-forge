using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class LifecycleDocumentSnapshotReader(PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<LifecycleDocumentSnapshot> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return LifecycleDocumentSnapshot.Interrupted(workspace);
        }

        var logicalPath = Path.Combine(workspace.LexicalRoot, LifecycleSchema.RelativePath);
        var resolution = Resolve(workspace, logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return LifecycleDocumentSnapshot.Missing(workspace, FileStateSnapshot.Missing(logicalPath));
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(
                workspace,
                resolution,
                LifecycleDocumentFailureStage.PhysicalResolution);
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                return LifecycleDocumentSnapshot.Blocked(
                    workspace,
                    FileStateSnapshot.Directory(logicalPath, physicalPath),
                    "The lifecycle document path is a directory.");
            }

            if ((attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return LifecycleDocumentSnapshot.Blocked(
                    workspace,
                    file: null,
                    "The lifecycle document path is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return LifecycleDocumentSnapshot.Interrupted(workspace);
            }

            var confirmed = Resolve(workspace, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained)
            {
                return FromResolution(
                    workspace,
                    confirmed,
                    LifecycleDocumentFailureStage.PhysicalReconfirmation);
            }

            var confirmedPhysicalPath = confirmed.GetContainedPhysicalPath();
            if (!PhysicalIdentityTracker.PathComparer.Equals(physicalPath, confirmedPhysicalPath))
            {
                return LifecycleDocumentSnapshot.Blocked(
                    workspace,
                    file: null,
                    "The lifecycle document changed its resolved physical path during inspection.");
            }

            return LifecycleDocumentSnapshot.Available(
                workspace,
                FileStateSnapshot.File(logicalPath, confirmedPhysicalPath, bytes));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LifecycleDocumentSnapshot.Interrupted(workspace);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return LifecycleDocumentSnapshot.Missing(workspace, FileStateSnapshot.Missing(logicalPath));
        }
        catch (UnauthorizedAccessException exception)
        {
            return LifecycleDocumentSnapshot.Unavailable(
                workspace,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception),
                LifecycleDocumentFailureStage.ContainedFileAccess);
        }
        catch (IOException exception)
        {
            return LifecycleDocumentSnapshot.Unavailable(
                workspace,
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception),
                LifecycleDocumentFailureStage.ContainedFileAccess);
        }
    }

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string logicalPath)
        => physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);

    private static LifecycleDocumentSnapshot FromResolution(
        CliWorkspace workspace,
        PhysicalPathResolution resolution,
        LifecycleDocumentFailureStage failureStage)
    {
        if (resolution.Failure is { } failure)
        {
            return LifecycleDocumentSnapshot.Unavailable(workspace, failure, failureStage);
        }

        return LifecycleDocumentSnapshot.Blocked(
            workspace,
            file: null,
            "The lifecycle document physical boundary is unsafe or unavailable.");
    }
}
