using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

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
    ImmutableArray<string> Required,
    ImmutableArray<string> Missing,
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
