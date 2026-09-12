using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal static class ExtensionDestinationInspector
{
    internal static bool HasOrdinaryAncestors(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var root = Path.TrimEndingDirectorySeparator(workspace.LexicalRoot);
        var logical = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var parent = Path.GetDirectoryName(logical);
        while (parent is not null && !PhysicalIdentityTracker.PathComparer.Equals(parent, root))
        {
            var observation = NoFollowLeafObserver.Observe(resolver, workspace, parent, cancellationToken);
            if (observation.State is not (NoFollowLeafState.Directory or NoFollowLeafState.Missing))
            {
                return false;
            }
            parent = Path.GetDirectoryName(parent);
        }
        return parent is not null;
    }
}
