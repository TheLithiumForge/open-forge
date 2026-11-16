using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;

internal static class ToolWritePermissions
{
    internal static IReadOnlyList<ToolWriteFinding> Findings(string toolId, WorkspacePermissionEvaluation permissions)
    {
        if (!permissions.Missing.IsDefaultOrEmpty)
            return [.. permissions.Missing.Select(path => new ToolWriteFinding(
                ToolWriteFindingCode.PermissionRequired, CliSemanticStatus.Blocked, toolId, Path: path))];
        if (permissions.Decision is WorkspacePermissionDecision.Required or WorkspacePermissionDecision.Declined or WorkspacePermissionDecision.NotEvaluated)
            return [new(ToolWriteFindingCode.PermissionRequired, CliSemanticStatus.Blocked, toolId)];
        return [];
    }
}
