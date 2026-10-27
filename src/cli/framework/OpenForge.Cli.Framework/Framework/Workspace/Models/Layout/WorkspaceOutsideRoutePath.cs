using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

internal static class WorkspaceOutsideRoutePath
{
    internal static ImmutableArray<WorkspaceOutsideRoute> Validate(string mainRoot, IEnumerable<string> directories)
    {
        ArgumentNullException.ThrowIfNull(directories);
        if (!WorkspaceMainRootPath.IsCanonical(mainRoot))
            throw new ArgumentException("The main root must be a canonical workspace content directory.", nameof(mainRoot));

        var routes = ImmutableArray.CreateBuilder<WorkspaceOutsideRoute>();
        foreach (var directory in directories)
        {
            if (!PortableWorkspacePath.TryNormalize(directory, out var canonical)
                || canonical.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.OrdinalIgnoreCase)
                || Overlaps(canonical, mainRoot)
                || Overlaps(canonical, WorkspaceLayout.DefaultMainRoot)
                || Overlaps(canonical, WorkspaceRuntimePaths.DirectoryName)
                || routes.Any(route => Overlaps(canonical, route.DirectoryPath)))
            {
                throw new ArgumentException("Outside routes must be canonical workspace-relative directories without overlapping content, controls or other outside routes.", nameof(directories));
            }

            routes.Add(new(canonical, WorkspaceRoutePath.CanonicalEntrypointPath(canonical)));
        }

        return routes.ToImmutable();
    }

    private static bool Overlaps(string left, string right)
    {
        var leftKey = PortableWorkspacePath.CreatePortableKey(left);
        var rightKey = PortableWorkspacePath.CreatePortableKey(right);
        return leftKey == rightKey
            || leftKey.StartsWith(rightKey + "/", StringComparison.Ordinal)
            || rightKey.StartsWith(leftKey + "/", StringComparison.Ordinal);
    }
}
