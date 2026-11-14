using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Install.Models;

internal sealed record ToolInstallResult : ICliCommandResult
{
    public string Command => "tool install";
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public string? WorkspacePath => Workspace?.LexicalRoot;
    public bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public CliNextAction? Next { get; init; }
    public required ToolWriteData Data { get; init; }
    public ToolWriteIssue[] Issues { get; init; } = [];
    public ToolInstallItemResult[] Tools { get; init; } = [];
}

internal sealed record ToolInstallItemResult(string Id, CliSemanticStatus Status, ToolWriteData Data, ToolWriteIssue[] Issues);
