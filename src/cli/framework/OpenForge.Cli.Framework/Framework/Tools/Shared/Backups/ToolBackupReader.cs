using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal static class ToolBackupReader
{
    internal static ToolBackupCandidate[] Read(CliWorkspace workspace, IEnumerable<string> targets, CancellationToken token)
    {
        var result = new List<ToolBackupCandidate>();
        foreach (var target in targets.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            var logicalPath = Path.GetFullPath(target, workspace.LexicalRoot);
            var backup = ToolBackupPaths.Beside(logicalPath);
            var leaf = ToolFileReader.Observe(workspace, backup, token);
            if (leaf == NoFollowLeafState.Missing) continue;
            var targetLeaf = ToolFileReader.Observe(workspace, logicalPath, token);
            var safe = leaf == NoFollowLeafState.OrdinaryFile
                && targetLeaf is NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Missing;
            result.Add(new(logicalPath, backup, safe, safe ? null
                : "Backup restoration requires ordinary files and directory ancestors, or a missing target."));
        }
        return [.. result];
    }
}
