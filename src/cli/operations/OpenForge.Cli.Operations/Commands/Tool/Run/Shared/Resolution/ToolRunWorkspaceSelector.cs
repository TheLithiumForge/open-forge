using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Shared.Resolution;

internal static class ToolRunWorkspaceSelector
{
    internal static CliWorkspace? Select(ToolRunRequest request, CancellationToken token)
    {
        var selector = new CliWorkspaceSelector(new PhysicalPathResolver());
        var supplied = request.WorkspacePath ?? request.HostProjectDirectory;
        if (supplied is not null)
        {
            if (string.IsNullOrWhiteSpace(supplied)) return null;
            var selected = selector.Select(new(supplied, request.CurrentDirectory));
            return selected.Workspace is { } workspace && HasMarker(workspace, token)
                ? new(workspace.LexicalRoot, workspace.PhysicalRoot, request.WorkspacePath is not null ? CliWorkspaceSelectionMethod.ExplicitWorkspace : CliWorkspaceSelectionMethod.CurrentDirectory)
                : null;
        }
        var directory = Path.GetFullPath(request.CurrentDirectory);
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var selected = selector.Select(new(directory, request.CurrentDirectory));
            if (selected.Workspace is not { } workspace) return null;
            if (HasMarker(workspace, token)) return new(workspace.LexicalRoot, workspace.PhysicalRoot, CliWorkspaceSelectionMethod.CurrentDirectory);
            var parent = Path.GetDirectoryName(directory);
            if (parent is null) return null;
            directory = parent;
        }
    }

    private static bool HasMarker(CliWorkspace workspace, CancellationToken token)
        => ToolFileReader.Observe(workspace, WorkspaceSettingsDefinitions.RelativePath, token) != NoFollowLeafState.Missing
            || ToolFileReader.Observe(workspace, Path.Combine(WorkspaceSettingsDefinitions.DirectoryName, WorkspaceLayoutNames.LoaderFileName), token)
                != NoFollowLeafState.Missing;
}
