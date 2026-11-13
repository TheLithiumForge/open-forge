using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect.Models;

internal sealed record ToolInspectResult : ICliCommandResult
{
    public string Command => "tool inspect";
    public required CliSemanticStatus Status { get; init; }
    public CliWorkspace? Workspace { get; init; }
    public string? WorkspacePath => Workspace?.LexicalRoot;
    public bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public CliNextAction? Next => null;
    public ToolReadIssue[] Issues { get; init; } = [];
    public ToolCoverage[] Coverage { get; init; } = [];
    public string? Id { get; init; }
    public string? Version { get; init; }
    public JsonElement? Definition { get; init; }
    public string Target { get; init; } = "unknown";
    public ToolInstalledFact? Installed { get; init; }
    public string[] Requirements { get; init; } = [];
    public string[] Limitations { get; init; } = [];
    public ToolInstruction Instructions { get; init; } = new(null, null, null, null, "The selected definition is unavailable.");
}
