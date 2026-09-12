using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Rendering;

internal static class WorkspaceSelectionWireVocabulary
{
    internal static string Read(CliWorkspaceSelectionMethod workspace)
        => workspace switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace, "The workspace selection method is not defined."),
        };
}
