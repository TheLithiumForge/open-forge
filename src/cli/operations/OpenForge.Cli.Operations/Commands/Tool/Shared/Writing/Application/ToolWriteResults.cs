using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal static class ToolWriteResults
{
    internal static ToolWriteResult Invalid(ToolWriteKind kind, ToolWriteFinding finding, CliWorkspace? workspace = null, bool preview = false)
        => new()
        {
            Kind = kind,
            Status = finding.Status,
            Workspace = workspace,
            WorkspacePath = workspace?.LexicalRoot,
            WorkspaceExplicit = workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace,
            Findings = [finding],
            Data = new()
            {
                Tools = [],
                Preview = preview,
                Outcomes = [],
                Plan = new() { UsedNetwork = false, BinPath = string.Empty, PathReadiness = "unavailable", PathInstructions = [] },
            },
        };
}
