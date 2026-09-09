using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Source;

internal static class LibrarySourceRootReader
{
    internal static LibrarySourceRootObservation Read(PhysicalPathResolver resolver, LibrarySourceRootRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Workspace);
        ArgumentNullException.ThrowIfNull(request.SourceRoot);
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = request.Workspace;
        var lexical = Path.GetFullPath(Path.Combine(workspace.LexicalRoot, request.SourceRoot.Value.Replace('/', Path.DirectorySeparatorChar)));
        var observation = new LibrarySourceRootObservation
        {
            Request = request,
            State = LibrarySourceRootState.Unavailable,
            LexicalSourceRoot = lexical,
            PhysicalSourceRoot = null,
            LexicallyContained = PhysicalContainment.Contains(workspace.LexicalRoot, lexical),
            PhysicallyContained = null,
            Cause = null,
        };
        if (observation.LexicallyContained != true)
        {
            return observation with { State = LibrarySourceRootState.Invalid, Cause = "The Library source root is outside the selected workspace." };
        }

        var boundary = LibraryDirectoryBoundaryObserver.Observe(workspace, lexical, cancellationToken);
        if (boundary.State != NoFollowLeafState.Directory)
        {
            return observation with
            {
                State = ReadBoundaryState(boundary.State),
                Cause = boundary.Failure?.DirectCause ?? "The Library source root is missing, non-directory, or unsafe.",
            };
        }

        var resolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexical);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return observation with
            {
                State = resolution.State == PhysicalPathState.Missing ? LibrarySourceRootState.Missing : LibrarySourceRootState.Blocked,
                PhysicalSourceRoot = resolution.ResolvedPhysicalPath,
                PhysicallyContained = false,
                Cause = resolution.Failure?.DirectCause ?? "The Library source root is not physically contained.",
            };
        }
        var physical = resolution.GetContainedPhysicalPath();
        var contained = PhysicalContainment.Contains(workspace.PhysicalRoot, physical);
        if (contained)
        {
            try
            {
                using var entries = Directory.EnumerateFileSystemEntries(physical).GetEnumerator();
                _ = entries.MoveNext();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return observation with
                {
                    State = exception is UnauthorizedAccessException ? LibrarySourceRootState.Inaccessible : LibrarySourceRootState.Unavailable,
                    PhysicalSourceRoot = physical,
                    PhysicallyContained = true,
                    Cause = "The Library source root cannot be read.",
                };
            }
        }
        return observation with
        {
            State = contained ? LibrarySourceRootState.Available : LibrarySourceRootState.Blocked,
            PhysicalSourceRoot = physical,
            PhysicallyContained = contained,
            Cause = contained ? null : "The resolved Library source root escaped the selected workspace.",
        };
    }

    private static LibrarySourceRootState ReadBoundaryState(NoFollowLeafState state)
        => state switch
        {
            NoFollowLeafState.Missing => LibrarySourceRootState.Missing,
            NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory => LibrarySourceRootState.Invalid,
            NoFollowLeafState.Inaccessible => LibrarySourceRootState.Inaccessible,
            NoFollowLeafState.Unknown => LibrarySourceRootState.Unavailable,
            NoFollowLeafState.RelativeFileLink or NoFollowLeafState.Link or NoFollowLeafState.ReparsePoint or NoFollowLeafState.Special => LibrarySourceRootState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The no-follow leaf state is not defined."),
        };
}
