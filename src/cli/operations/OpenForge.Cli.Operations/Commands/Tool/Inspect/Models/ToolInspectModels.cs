using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect.Models;

internal sealed record ToolInspectRequest(CliWorkspace Workspace, string? Id);
internal sealed record ToolInspectResult : ICliCommandResult
{
    public string Command => ToolInspectDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    public required ToolReadPayload Data { get; init; }
    public IReadOnlyList<ToolReadFinding> Findings { get; init; } = [];
    public CliNextAction? Next { get; init; }
    internal string? WorkspacePath => Workspace?.LexicalRoot;
    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
}

internal sealed record ToolInspectSymbols(Command Command, Argument<string?> Id);
