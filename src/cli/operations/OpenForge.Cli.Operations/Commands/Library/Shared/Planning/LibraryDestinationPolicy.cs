using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning;

internal static class LibraryDestinationPolicy
{
    private const string WorkspaceLockPath = ".agents/open-forge.lock";

    internal static string? FindConflict(
        CliWorkspace workspace,
        LibraryRegistration selected,
        LibraryRegistrationSet? registered,
        IEnumerable<string> generatedTargets)
        => FindConflict(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), selected, registered, generatedTargets);

    internal static string? FindConflict(
        WorkspaceLayout layout,
        LibraryRegistration selected,
        LibraryRegistrationSet? registered,
        IEnumerable<string> generatedTargets)
    {
        var workspace = layout.Workspace;
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
            if (!IsAllowed(layout, path) || IsRuntimeDestination(workspace, path) || owned.Contains(key)
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
        foreach (var path in generatedTargets.Append(WorkspaceOwnershipDefinitions.RelativePath).Append(WorkspaceSettingsDefinitions.RelativePath))
        {
            var key = PortableWorkspacePath.CreatePortableKey(path);
            if (IsRuntimeDestination(workspace, path) || sources.Any(root => Within(key, root)))
            {
                return path;
            }
        }
        return null;
    }

    private static bool IsRuntimeDestination(CliWorkspace workspace, string path)
    {
        if (WorkspaceRuntimePaths.Contains(workspace, Path.GetFullPath(path, workspace.PhysicalRoot)))
        {
            return true;
        }
        var resolver = new PhysicalPathResolver();
        var current = Path.GetFullPath(path, workspace.LexicalRoot);
        while (current is not null)
        {
            var resolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, current);
            if (resolution.ResolvedPhysicalPath is { } physical && WorkspaceRuntimePaths.Contains(workspace, physical))
            {
                return true;
            }
            if (resolution.State != PhysicalPathState.Missing)
            {
                break;
            }
            current = Path.GetDirectoryName(current);
        }
        return false;
    }

    private static bool IsAllowed(WorkspaceLayout layout, string path)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        if (key == WorkspaceSettingsDefinitions.DirectoryName
            || layout.SourceRoots.Any(root => key == PortableWorkspacePath.CreatePortableKey(root))
            || key.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.Ordinal)
            || Within(key, WorkspaceSettingsDefinitions.RelativePath)
            || Within(key, WorkspaceOwnershipDefinitions.RelativePath)
            || Within(key, WorkspaceLockPath)
            || Within(key, PortableWorkspacePath.CreatePortableKey(layout.LoaderPath)))
        {
            return false;
        }
        if (!layout.IsContentPath(path))
        {
            return true;
        }
        if (key.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal)
            && !path.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal))
        {
            return false;
        }
        if (!layout.TryGetSourceRelativePath(path, out var relative)) return false;
        var segments = layout.SourcePath(PortableWorkspacePath.CreatePortableKey(relative)).Split('/');
        for (var length = 2; length <= segments.Length; length++)
        {
            var prefix = string.Join('/', segments.AsSpan(0, length).ToArray());
            if (SourceFormClassifier.TryClassify(layout, prefix, out var form)
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
