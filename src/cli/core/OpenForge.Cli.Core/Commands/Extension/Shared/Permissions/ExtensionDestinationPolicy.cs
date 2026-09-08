using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal static class ExtensionDestinationPolicy
{
    private const string GitDirectoryName = ".git";
    private const string WorkspaceLockPath = ".agents/open-forge.lock";

    internal static bool IsImplicit(string path)
        => path.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.Ordinal);

    internal static bool IsAllowed(CliWorkspace workspace, string path)
    {
        if (!IsAllowed(path))
        {
            return false;
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
        if (key == WorkspacePermissionDefinitions.ImplicitDirectoryPath
            || (key.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.Ordinal) && !IsImplicit(path))
            || key.Split('/').Contains(GitDirectoryName, StringComparer.Ordinal)
            || IsReservedFileOrDescendant(key, LifecycleSchema.RelativePath)
            || IsReservedFileOrDescendant(key, LibraryPathIdentity.RecordRelativePath)
            || IsReservedFileOrDescendant(key, WorkspacePermissionDefinitions.RelativePath)
            || IsReservedFileOrDescendant(key, WorkspaceLockPath))
        {
            return false;
        }
        return !SourceFormClassifier.HasOverwriteSuffix(key);
    }
}
