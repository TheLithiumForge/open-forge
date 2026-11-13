using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

internal static class ToolFileReader
{
    internal static NoFollowLeafState Observe(CliWorkspace workspace, string path, CancellationToken token)
    {
        var absolute = Path.GetFullPath(path, workspace.LexicalRoot);
        var relative = Path.GetRelativePath(workspace.LexicalRoot, absolute).Replace(Path.DirectorySeparatorChar, '/');
        if (!ToolIdentity.IsRelativePath(relative)) return NoFollowLeafState.Unknown;
        var paths = new PhysicalPathResolver();
        var logical = workspace.LexicalRoot;
        var segments = relative.Split('/');
        for (var index = 0; index < segments.Length; index++)
        {
            logical = Path.Combine(logical, segments[index]);
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, logical, token);
            if (index != segments.Length - 1 && leaf.State == NoFollowLeafState.OrdinaryFile) return NoFollowLeafState.Unknown;
            if (index == segments.Length - 1 || leaf.State != NoFollowLeafState.Directory) return leaf.State;
        }
        return NoFollowLeafState.Unknown;
    }

    internal static async ValueTask<FileStateSnapshot> ReadAsync(CliWorkspace workspace, string logicalPath, CancellationToken token)
    {
        logicalPath = Path.GetFullPath(logicalPath, workspace.LexicalRoot);
        var paths = new PhysicalPathResolver();
        var leaf = Observe(workspace, logicalPath, token);
        if (leaf == NoFollowLeafState.Missing) return FileStateSnapshot.Missing(logicalPath);
        if (leaf != NoFollowLeafState.OrdinaryFile)
            throw new IOException($"An ordinary file and directory ancestors are required at {logicalPath}.");
        var resolved = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logicalPath);
        if (resolved.State != PhysicalPathState.Contained)
            throw new IOException(resolved.Failure?.DirectCause ?? $"The file is outside the physical workspace: {logicalPath}.");
        var physicalPath = resolved.GetContainedPhysicalPath();
        var bytes = await File.ReadAllBytesAsync(physicalPath, token).ConfigureAwait(false);
        if (Observe(workspace, logicalPath, token) != NoFollowLeafState.OrdinaryFile)
            throw new IOException($"The path changed during the read: {logicalPath}.");
        return FileStateSnapshot.File(logicalPath, physicalPath, bytes);
    }
}
