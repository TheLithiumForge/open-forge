using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;

namespace OpenForge.Cli.Core.Framework.Permissions.Models.Result;

internal enum WorkspacePermissionDecision
{
    NotEvaluated,
    NotRequired,
    Granted,
    Required,
    Approved,
    Declined,
}

internal enum WorkspacePermissionAction
{
    None,
    Create,
    Replace,
}

internal enum WorkspacePermissionOutcome
{
    NotRequested,
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal sealed record WorkspacePermissionResult(
    ImmutableArray<WorkspacePermissionRequirement> Required,
    ImmutableArray<WorkspacePermissionRequirement> Missing,
    WorkspacePermissionDecision Decision,
    WorkspacePermissionAction Action,
    WorkspacePermissionOutcome Outcome)
{
    internal static readonly WorkspacePermissionResult NotEvaluated = new(
        Required: [],
        Missing: [],
        Decision: WorkspacePermissionDecision.NotEvaluated,
        Action: WorkspacePermissionAction.None,
        Outcome: WorkspacePermissionOutcome.NotRequested);
}
