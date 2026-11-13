using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Status.Models;

internal sealed record ToolStatusResult : ICliCommandResult
{
    public string Command => "tool status";
    public required CliSemanticStatus Status { get; init; }
    public CliWorkspace? Workspace { get; init; }
    public string? WorkspacePath => Workspace?.LexicalRoot;
    public bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public CliNextAction? Next => null;
    public ToolReadIssue[] Issues { get; init; } = [];
    public ToolCoverage[] Coverage { get; init; } = [];
    public ToolStatusRow[] Tools { get; init; } = [];
}
