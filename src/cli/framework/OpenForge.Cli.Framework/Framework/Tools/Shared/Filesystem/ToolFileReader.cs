using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

internal static class ToolFileReader
{
    internal static CliWorkspace Scope(string root)
    {
        root = Path.GetFullPath(root);
        var resolved = new PhysicalPathResolver().ResolveRoot(root);
        if (resolved.State != PhysicalPathState.Contained) throw new IOException("The Tool file scope is unavailable.");
        return new(root, resolved.GetContainedPhysicalPath(), CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    internal static string Target(CliWorkspace scope, string relative)
    {
        if (!PortableWorkspacePath.TryNormalize(relative, out var normalized)) throw new InvalidDataException("The Tool target must be a portable workspace-relative file path.");
        return Path.GetFullPath(normalized, scope.LexicalRoot);
    }

    internal static NoFollowLeafState Observe(CliWorkspace scope, string path, CancellationToken token)
    {
        var absolute = Path.GetFullPath(path, scope.LexicalRoot);
        var relative = Path.GetRelativePath(scope.LexicalRoot, absolute).Replace(Path.DirectorySeparatorChar, '/');
        _ = Target(scope, relative);
        var paths = new PhysicalPathResolver();
        var logical = scope.LexicalRoot;
        var segments = relative.Split('/');
        for (var index = 0; index < segments.Length; index++)
        {
            logical = Path.Combine(logical, segments[index]);
            var leaf = NoFollowLeafObserver.Observe(paths, scope, logical, token);
            if (index != segments.Length - 1 && leaf.State == NoFollowLeafState.OrdinaryFile) return NoFollowLeafState.Unknown;
            if (index == segments.Length - 1 || leaf.State != NoFollowLeafState.Directory) return leaf.State;
        }
        return NoFollowLeafState.Unknown;
    }

    internal static async ValueTask<FileStateSnapshot> ReadAsync(CliWorkspace scope, string path, CancellationToken token)
    {
        path = Path.GetFullPath(path, scope.LexicalRoot);
        var paths = new PhysicalPathResolver();
        var leaf = Observe(scope, path, token);
        if (leaf == NoFollowLeafState.Missing) return FileStateSnapshot.Missing(path);
        if (leaf != NoFollowLeafState.OrdinaryFile) throw new IOException($"An ordinary file and directory ancestors are required at {path}.");
        var resolved = paths.ResolveCandidate(scope.LexicalRoot, scope.PhysicalRoot, path);
        if (resolved.State != PhysicalPathState.Contained) throw new IOException($"The file is outside the physical workspace: {path}.");
        var physical = resolved.GetContainedPhysicalPath();
        var bytes = await File.ReadAllBytesAsync(physical, token).ConfigureAwait(false);
        var confirmed = paths.ResolveCandidate(scope.LexicalRoot, scope.PhysicalRoot, path);
        if (Observe(scope, path, token) != NoFollowLeafState.OrdinaryFile || confirmed.State != PhysicalPathState.Contained
            || !string.Equals(physical, confirmed.ResolvedPhysicalPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new IOException($"The path changed during the read: {path}.");
        return FileStateSnapshot.File(path, physical, bytes);
    }
}
