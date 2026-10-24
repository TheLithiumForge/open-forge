using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal static class ExtensionDestinationPolicy
{
    private const string GitDirectoryName = ".git";
    private const string WorkspaceLockPath = ".agents/open-forge.lock";

    internal static bool IsImplicit(string path)
        => path.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal);

    internal static bool IsAllowed(CliWorkspace workspace, string path)
    {
        if (!IsAllowed(path))
        {
            return false;
        }
        if (WorkspaceRuntimePaths.Contains(workspace, Path.GetFullPath(path, workspace.PhysicalRoot)))
        {
            return false;
        }
        var resolver = new PhysicalPathResolver();
        var current = Path.GetFullPath(path, workspace.LexicalRoot);
        while (current is not null)
        {
            var resolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, current);
            if (resolution.ResolvedPhysicalPath is { } physical && WorkspaceRuntimePaths.Contains(workspace, physical))
            {
                return false;
            }
            if (resolution.State != PhysicalPathState.Missing)
            {
                break;
            }
            current = Path.GetDirectoryName(current);
        }
        var store = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        if (store is null)
        {
            return true;
        }
        var relativeStore = Path.GetRelativePath(workspace.PhysicalRoot, store).Replace(Path.DirectorySeparatorChar, '/');
        var storeKey = PortableWorkspacePath.CreatePortableKey(relativeStore);
        var targetKey = PortableWorkspacePath.CreatePortableKey(path);
        return storeKey != "." && targetKey != storeKey && !targetKey.StartsWith($"{storeKey}/", StringComparison.Ordinal);
    }

    private static bool IsReservedFileOrDescendant(string key, string reserved)
        => key == reserved || key.StartsWith($"{reserved}/", StringComparison.Ordinal);

    internal static bool IsAllowed(string path)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized) || normalized != path)
        {
            return false;
        }
        var key = PortableWorkspacePath.CreatePortableKey(path);
        if (key == WorkspaceSettingsDefinitions.DirectoryName
            || (key.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal) && !IsImplicit(path))
            || key.Split('/').Contains(GitDirectoryName, StringComparer.Ordinal)
            || IsReservedFileOrDescendant(key, WorkspaceSettingsDefinitions.RelativePath)
            || IsReservedFileOrDescendant(key, WorkspaceOwnershipDefinitions.RelativePath)
            || IsReservedFileOrDescendant(key, WorkspaceLockPath))
        {
            return false;
        }
        return !SourceOverwritePath.HasSuffix(key);
    }
}
