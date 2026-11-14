using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Configure.Models;

internal sealed record ToolConfigureResult : ICliCommandResult
{
    public string Command => "tool configure";
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public string? WorkspacePath => Workspace?.LexicalRoot;
    public bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public CliNextAction? Next { get; init; }
    public required ToolWriteData Data { get; init; }
    public ToolWriteIssue[] Issues { get; init; } = [];

}
