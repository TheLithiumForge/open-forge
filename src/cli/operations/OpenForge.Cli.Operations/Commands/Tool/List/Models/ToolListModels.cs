using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.List.Models;

internal sealed record ToolListRequest(CliWorkspace Workspace);
internal sealed record ToolListResult : ICliCommandResult
{
    public string Command => ToolListDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required ToolReadPayload Data { get; init; }
    public IReadOnlyList<ToolReadFinding> Findings { get; init; } = [];
    public CliNextAction? Next { get; init; }
    internal string? WorkspacePath => Workspace?.LexicalRoot;
    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
}
