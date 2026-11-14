using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.List.Models;

internal sealed record ToolListResult : ICliCommandResult
{
    public string Command => "tool list";
    public required CliSemanticStatus Status { get; init; }
    public CliWorkspace? Workspace { get; init; }
    public string? WorkspacePath => Workspace?.LexicalRoot;
    public bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public CliNextAction? Next => null;
    internal string? NextCommand => ToolReadNextAction.Read(Command, Status, Issues, WorkspacePath);
    public ToolReadIssue[] Issues { get; init; } = [];
    public ToolCoverage[] Coverage { get; init; } = [];
    public ToolAvailableRow[] Available { get; init; } = [];
    public ToolInstalledRow[] Installed { get; init; } = [];
    public ToolCatalogueFact Catalogue { get; init; } = new("embedded catalogue", "unavailable", "The request hasn't been read.");
    public string? RoleFilter { get; init; }
}
