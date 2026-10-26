using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

internal static class WorkspaceMainRootPath
{
    internal static bool IsCanonical(string? value)
    {
        if (!PortableWorkspacePath.TryNormalize(value, out var normalized)) return false;
        if (normalized == WorkspaceLayout.DefaultMainRoot) return true;

        var key = PortableWorkspacePath.CreatePortableKey(normalized);
        return key != WorkspaceLayout.DefaultMainRoot
            && !key.StartsWith(WorkspaceLayout.DefaultMainRoot + "/", StringComparison.Ordinal)
            && key != WorkspaceRuntimePaths.DirectoryName
            && !key.StartsWith(WorkspaceRuntimePaths.DirectoryName + "/", StringComparison.Ordinal)
            && !key.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.Ordinal)
            && !string.Equals(normalized, WorkspaceLayoutNames.AgentHostFileName, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalized, WorkspaceLayoutNames.ClaudeHostFileName, StringComparison.OrdinalIgnoreCase)
            && !normalized.StartsWith(WorkspaceLayoutNames.AgentHostFileName + "/", StringComparison.OrdinalIgnoreCase)
            && !normalized.StartsWith(WorkspaceLayoutNames.ClaudeHostFileName + "/", StringComparison.OrdinalIgnoreCase);
    }
}
