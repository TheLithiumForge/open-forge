using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Shared.Layout;

internal static class WorkspaceLayoutPathDisplay
{
    internal static (string OldPath, string NewPath) Rename(WorkspaceLayout layout, string oldPath, string newPath)
    {
        if (layout.TryGetSourceRelativePath(oldPath, out var oldRelative)
            && layout.TryGetSourceRelativePath(newPath, out var newRelative))
        {
            var oldCategory = oldRelative.Split('/')[0];
            var newCategory = newRelative.Split('/')[0];
            if (oldCategory == newCategory && oldRelative.Contains('/') && newRelative.Contains('/'))
            {
                var prefix = layout.CategoryDirectory(oldCategory).Length + 1;
                return (oldPath[prefix..], newPath[prefix..]);
            }
        }
        return (oldPath, newPath);
    }
}
