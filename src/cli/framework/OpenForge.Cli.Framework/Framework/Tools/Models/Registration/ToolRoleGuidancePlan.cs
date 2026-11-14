using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Registration;

internal sealed record ToolRoleGuidanceSelection(string ToolId, ToolRoleGuidance Guidance, string SkillPath);

// Active is the complete desired role-line set. An empty set clears the conditional lines.
internal sealed record ToolRoleGuidanceRequest(string WorkspaceRoot, WorkspaceLayout Layout, ToolRoleGuidanceSelection[] Active);
internal enum ToolRoleLineState { Matching, Missing, Drifted, Unavailable }
internal sealed record ToolRoleLineCheck(string Role, string ToolId, string File, ToolRoleLineState State, string? Cause);
internal sealed record ToolRoleLineEdit(string File, byte[]? Before, byte[] After);
internal sealed record ToolRoleGuidancePlan(ToolRoleLineEdit[] Edits, ToolRoleLineCheck[] Checks);

internal delegate ValueTask<ToolRoleGuidancePlan> ToolPlanRoleGuidance(ToolRoleGuidanceRequest request, CancellationToken token);
internal delegate ValueTask<ToolRoleLineCheck[]> ToolObserveRoleGuidance(ToolRoleGuidanceRequest request, CancellationToken token);
internal sealed record ToolRoleGuidanceServices(ToolPlanRoleGuidance Plan, ToolObserveRoleGuidance Observe);
