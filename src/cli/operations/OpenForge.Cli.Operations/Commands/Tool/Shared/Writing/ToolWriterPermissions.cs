using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal static class ToolWriterPermissions
{
    internal static ToolWriterPermissionCheck Check(ToolWriterState state, IEnumerable<string> outputs, IEnumerable<string> grants, CancellationToken token)
    {
        var workspace = state.Layout.Workspace;
        var supplied = grants.Select(path => ToolWriterPaths.Relative(workspace, path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        ImmutableArray<string> allowed = [.. state.Settings.Document.AllowInstallPaths.Concat(supplied)];
        var decisions = new List<ToolDestinationDecision>();
        var issues = new List<ToolWriteIssue>();
        foreach (var output in outputs.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            var path = ToolWriterPaths.Relative(workspace, output);
            var leaf = ToolFileReader.Observe(workspace, path, token);
            var ordinary = leaf is NoFollowLeafState.Missing or NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory;
            var excluded = WorkspaceRemovals.IsPathRemoved(state.Layout, path, state.Settings.Document)
                || OperatingSystem.IsWindows() && state.Settings.Document.RemovedFiles.Any(removed => string.Equals(removed, path, StringComparison.OrdinalIgnoreCase))
                || state.Settings.Document.RemovedDirectories.Concat(state.Settings.Document.RemovedCategories.Select(state.Layout.CategoryDirectory))
                    .Any(removed => ToolWriterPaths.Contains(removed, path))
                || state.Settings.Document.RemovedFiles.Concat(state.Settings.Document.RemovedDirectories)
                    .Concat(state.Settings.Document.RemovedCategories.Select(state.Layout.CategoryDirectory)).Any(removed => ToolWriterPaths.Contains(path, removed));
            var owned = Protected(path) || Owned(state, path);
            var primary = state.Layout.TryGetMaintenanceBoundary(path, out var boundary)
                && boundary.Kind is WorkspaceMaintenanceBoundaryKind.Primary or WorkspaceMaintenanceBoundaryKind.NativeSkills;
            var permitted = primary || WorkspaceAllowList.Admits(allowed, path);
            var save = !primary && WorkspaceAllowList.Admits([.. supplied], path) && !WorkspaceAllowList.Admits(state.Settings.Document.AllowInstallPaths, path);
            string? cause = null;
            if (!ordinary) cause = "The output has a link, alias or unavailable parent.";
            else if (excluded) cause = "A persistent removal exclusion covers the declared output.";
            else if (owned) cause = "The declared output intersects another managed file or Library scope.";
            else if (!permitted) cause = "The output requires a shared destination grant through --allow-path.";
            decisions.Add(new(path, permitted && ordinary && !excluded && !owned, save, cause));
            if (cause is null) continue;
            var code = ToolWriteIssueCode.VerificationFailed;
            if (excluded) code = ToolWriteIssueCode.PathExcluded;
            else if (ordinary && !owned) code = ToolWriteIssueCode.PermissionMissing;
            issues.Add(new(code, CliSemanticStatus.Blocked, path, cause));
        }
        var saved = supplied.Where(grant => decisions.Any(decision => decision.SaveGrant && WorkspaceAllowList.Admits([grant], decision.Path))).ToArray();
        return new([.. decisions], saved, [.. issues]);
    }

    private static bool Protected(string path)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        if (key.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.Ordinal)) return true;
        foreach (var control in new[] { WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath, WorkspaceRuntimePaths.DirectoryName })
            if (ToolWriterPaths.Contains(key, control) || ToolWriterPaths.Contains(control, key)) return true;
        return false;
    }

    private static bool Owned(ToolWriterState state, string path)
    {
        if (state.Lock.Document.ManagedPaths().Any(claim => ToolWriterPaths.Contains(path, claim.Path) || ToolWriterPaths.Contains(claim.Path, path))) return true;
        foreach (var library in state.Lock.Document.Libraries)
            if (ToolWriterPaths.Contains(library.DestinationRoot, path) || ToolWriterPaths.Contains(path, library.DestinationRoot)
                || ToolWriterPaths.Contains(library.SourceRoot, path)) return true;
        return false;
    }
}
