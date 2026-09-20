using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.Shared.Reading;

internal sealed class ManagedTargetReader(
    PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<ManagedTargetReadResult> ReadAsync(
        CliWorkspace workspace,
        string canonicalPath,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ManagedTargetReadResult.Cancelled();
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
            return ManagedTargetReadResult.Available(bytes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ManagedTargetReadResult.Cancelled();
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException)
        {
            return ManagedTargetReadResult.Missing();
        }
        catch (UnauthorizedAccessException exception)
        {
            return ManagedTargetReadResult.Unavailable(
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (IOException exception)
        {
            return ManagedTargetReadResult.Unavailable(
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }
    }

    private static ManagedTargetReadResult? ReadBoundary(
        PhysicalPathResolution resolution)
        => resolution.State switch
        {
            PhysicalPathState.Contained => null,
            PhysicalPathState.Missing => ManagedTargetReadResult.Missing(),
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure
                => ManagedTargetReadResult.Unavailable(
                    resolution.Failure
                        ?? throw new InvalidOperationException(
                            "An unavailable lifecycle target requires a filesystem failure.")),
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported
                => ManagedTargetReadResult.Blocked(resolution.Failure),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The lifecycle target physical path state is not defined."),
        };
}
