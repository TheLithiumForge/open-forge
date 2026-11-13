using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal static class ToolGitRecoveryReader
{
    internal const string Head = "HEAD";
    internal const string GitDirectoryPrefix = "gitdir: ";

    // Preview must not launch Git. Ordinary worktree metadata selects the backup policy.
    internal static ToolBackupMode Read(CliWorkspace workspace, CancellationToken token)
    {
        var paths = new PhysicalPathResolver();
        Span<byte> prefix = stackalloc byte[GitDirectoryPrefix.Length];
        for (var directory = new DirectoryInfo(workspace.PhysicalRoot); directory is not null; directory = directory.Parent)
        {
            token.ThrowIfCancellationRequested();
            var scope = new CliWorkspace(directory.FullName, directory.FullName, CliWorkspaceSelectionMethod.CurrentDirectory);
            var marker = Path.Combine(directory.FullName, WorkspaceLayoutNames.GitMetadataDirectoryName);
            var leaf = NoFollowLeafObserver.Observe(paths, scope, marker, token);
            if (leaf.State == NoFollowLeafState.Missing) continue;
            if (leaf.State == NoFollowLeafState.Directory)
            {
                var head = NoFollowLeafObserver.Observe(paths, scope, Path.Combine(marker, Head), token);
                if (head.State == NoFollowLeafState.OrdinaryFile) return ToolBackupMode.Git;
            }
            else if (leaf.State == NoFollowLeafState.OrdinaryFile)
            {
                using var stream = new FileStream(marker, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.ReadAtLeast(prefix, prefix.Length, throwOnEndOfStream: false) == prefix.Length
                    && prefix.SequenceEqual(Encoding.ASCII.GetBytes(GitDirectoryPrefix)) && stream.Length > prefix.Length)
                    return ToolBackupMode.Git;
            }
            throw new IOException($"Git backup policy cannot be determined from the metadata at {marker}.");
        }
        return ToolBackupMode.Sibling;
    }
}
