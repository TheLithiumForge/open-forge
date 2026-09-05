using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Lifecycle;

internal sealed class LifecycleManagedTargetReader(
    PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<LifecycleManagedTargetReadResult> ReadAsync(
        CliWorkspace workspace,
        string canonicalPath,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return LifecycleManagedTargetReadResult.Cancelled();
        }

        var lexicalPath = Path.Combine(
            workspace.LexicalRoot,
            canonicalPath.Replace('/', Path.DirectorySeparatorChar));
        var resolution = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        var boundary = ReadBoundary(resolution);
        if (boundary is not null)
        {
            return boundary;
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(
                    resolution.GetContainedPhysicalPath(),
                    cancellationToken)
                .ConfigureAwait(false);
            return LifecycleManagedTargetReadResult.Available(bytes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LifecycleManagedTargetReadResult.Cancelled();
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException)
        {
            return LifecycleManagedTargetReadResult.Missing();
        }
        catch (UnauthorizedAccessException exception)
        {
            return LifecycleManagedTargetReadResult.Unavailable(
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (IOException exception)
        {
            return LifecycleManagedTargetReadResult.Unavailable(
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }
    }

    private static LifecycleManagedTargetReadResult? ReadBoundary(
        PhysicalPathResolution resolution)
        => resolution.State switch
        {
            PhysicalPathState.Contained => null,
            PhysicalPathState.Missing => LifecycleManagedTargetReadResult.Missing(),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
                => LifecycleManagedTargetReadResult.Unavailable(
                    resolution.Failure
                        ?? throw new InvalidOperationException(
                            "An unavailable lifecycle target requires a filesystem failure.")),
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                => LifecycleManagedTargetReadResult.Blocked(resolution.Failure),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The lifecycle target physical path state is not defined."),
        };
}
