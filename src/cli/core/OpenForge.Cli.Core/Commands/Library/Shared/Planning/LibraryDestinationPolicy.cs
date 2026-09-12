using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning;

internal static class LibraryDestinationPolicy
{
    private const string GitDirectory = ".git";
    private const string WorkspaceLockPath = ".agents/open-forge.lock";

    internal static string? FindConflict(
        CliWorkspace workspace,
        LibraryRecord selected,
        LibrariesRecord? registered,
        IEnumerable<string> generatedTargets)
    {
        var others = registered?.Libraries.Where(library => library.Id != selected.Id).ToArray() ?? [];
        var owned = others.SelectMany(static library => LibraryPathIdentity.Mappings(library))
            .Select(mapping => PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value)).ToHashSet(StringComparer.Ordinal);
        var sources = others.Select(library => library.SourceRoot).Append(selected.SourceRoot)
            .Select(root => PortableWorkspacePath.CreatePortableKey(root.Value)).Distinct(StringComparer.Ordinal).ToArray();
        var recoveryRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        var recovery = recoveryRoot is null ? null : PortableWorkspacePath.CreatePortableKey(
            Path.GetRelativePath(workspace.PhysicalRoot, recoveryRoot).Replace(Path.DirectorySeparatorChar, '/'));
        var mappings = LibraryPathIdentity.Mappings(selected);
        var selectedKeys = mappings.Select(mapping => PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value)).ToHashSet(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            var path = mapping.DestinationPath.Value;
            var key = PortableWorkspacePath.CreatePortableKey(path);
            if (!IsAllowed(path) || owned.Contains(key)
                || Parents(key).Any(parent => owned.Contains(parent) || selectedKeys.Contains(parent))
                || sources.Any(root => Within(key, root))
                || (recovery is not null && (recovery == "." || Within(key, recovery))))
            {
                return path;
            }
        }
        foreach (var path in owned)
        {
            var conflict = Parents(path).FirstOrDefault(selectedKeys.Contains);
            if (conflict is not null)
            {
                return mappings.First(mapping => PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value) == conflict).DestinationPath.Value;
            }
        }
        foreach (var path in generatedTargets.Append(LibraryPathIdentity.RecordRelativePath).Append(WorkspacePermissionDefinitions.RelativePath))
        {
            var key = PortableWorkspacePath.CreatePortableKey(path);
            if (sources.Any(root => Within(key, root)))
            {
                return path;
            }
        }
        return null;
    }

    private static bool IsAllowed(string path)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        if (key == WorkspacePermissionDefinitions.ImplicitDirectoryPath
            || key.Split('/').Contains(GitDirectory, StringComparer.Ordinal)
            || Within(key, LifecycleSchema.RelativePath)
            || Within(key, LibraryPathIdentity.RecordRelativePath)
            || Within(key, WorkspacePermissionDefinitions.RelativePath)
            || Within(key, WorkspaceLockPath)
            || Within(key, SourceLogicalPath.LoaderPath))
        {
            return false;
        }
        if (!key.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.Ordinal))
        {
            return true;
        }
        if (!path.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.Ordinal))
        {
            return false;
        }
        var segments = key.Split('/');
        for (var length = 2; length <= segments.Length; length++)
        {
            var prefix = string.Join('/', segments.AsSpan(0, length).ToArray());
            if (SourceFormClassifier.TryClassify(prefix, out var form)
                && (form == SourceDocumentForm.OverwriteCompanion || SourceFormClassifier.IsEntrypoint(form)))
            {
                return false;
            }
        }
        return true;
    }

    private static bool Within(string path, string root) => path == root || path.StartsWith($"{root}/", StringComparison.Ordinal);

    private static IEnumerable<string> Parents(string path)
    {
        var end = path.LastIndexOf('/');
        while (end > 0)
        {
            yield return path[..end];
            end = path.LastIndexOf('/', end - 1);
        }
    }
}
