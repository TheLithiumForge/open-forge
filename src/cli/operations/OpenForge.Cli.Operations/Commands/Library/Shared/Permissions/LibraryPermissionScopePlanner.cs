using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal static class LibraryPermissionScopePlanner
{
    internal static LibraryPermissionApproval Plan(
        LibraryPermissionRequest request,
        ImmutableArray<string> allowInstallPaths = default)
    {
        var targets = request.Targets.Where(IsExternal).ToArray();
        var leaves = WorkspaceAllowList.Evaluate(allowInstallPaths, [.. targets.Select(target => target.DestinationPath)]);
        var missing = leaves.Missing.Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
        var proposed = targets.Where(target => missing.Contains(PortableWorkspacePath.CreatePortableKey(target.DestinationPath)))
            .Select(target => CreateScope(target))
            .DistinctBy(scope => (scope.Kind, PortableWorkspacePath.CreatePortableKey(scope.Path)))
            .ToArray();
        var directories = proposed.Where(scope => scope.Kind == LibraryPermissionScopeKind.Directory)
            .Select(scope => PortableWorkspacePath.CreatePortableKey(scope.Path)).ToHashSet(StringComparer.Ordinal);
        return new()
        {
            Leaves = leaves,
            ProposedScopes = [.. proposed.Where(scope => !HasGrantedParent(scope.Path, directories)).OrderBy(scope => scope.Path, StringComparer.Ordinal)],
            ApprovedScopes = [],
        };
    }

    private static bool IsExternal(LibraryPermissionTarget target)
    {
        _ = target.Use switch
        {
            LibraryPermissionTargetUse.Live => true,
            LibraryPermissionTargetUse.Retired => false,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target.Use, "The Library permission target use is not defined."),
        };
        return !target.DestinationPath.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal);
    }

    private static LibraryPermissionScope CreateScope(LibraryPermissionTarget target)
    {
        var separator = target.DestinationPath.LastIndexOf('/');
        if (target.Use == LibraryPermissionTargetUse.Live && separator > 0)
        {
            return new(LibraryPermissionScopeKind.Directory, target.DestinationPath[..separator]);
        }
        return new(LibraryPermissionScopeKind.File, target.DestinationPath);
    }

    private static bool HasGrantedParent(string path, HashSet<string> directories)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        for (var separator = key.LastIndexOf('/'); separator > 0; separator = key.LastIndexOf('/', separator - 1))
        {
            if (directories.Contains(key[..separator]))
            {
                return true;
            }
        }
        return false;
    }
}
