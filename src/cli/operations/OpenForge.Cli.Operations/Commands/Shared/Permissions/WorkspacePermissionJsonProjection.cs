using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Commands.Shared.Permissions.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

namespace OpenForge.Cli.Core.Commands.Shared.Permissions;

internal static class WorkspacePermissionJsonProjection
{
    internal static WorkspacePermissionJson Create(WorkspacePermissionResult result)
        => new()
        {
            Path = WorkspaceSettingsDefinitions.RelativePath,
            Required = [.. result.Required],
            Missing = [.. result.Missing],
            Decision = ReadName(result.Decision),
            Action = ReadName(result.Action),
            Outcome = ReadName(result.Outcome),
        };

    internal static string ReadName(WorkspacePermissionDecision value)
        => value switch
        {
            WorkspacePermissionDecision.NotEvaluated => "not-evaluated",
            WorkspacePermissionDecision.NotRequired => "not-required",
            WorkspacePermissionDecision.Granted => "granted",
            WorkspacePermissionDecision.Required => "required",
            WorkspacePermissionDecision.Approved => "approved",
            WorkspacePermissionDecision.Declined => "declined",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The permission decision is not defined."),
        };

    internal static string ReadName(WorkspacePermissionAction value)
        => value switch
        {
            WorkspacePermissionAction.None => "none",
            WorkspacePermissionAction.Create => "create",
            WorkspacePermissionAction.Replace => "replace",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The permission action is not defined."),
        };

    internal static string ReadName(WorkspacePermissionOutcome value)
        => value switch
        {
            WorkspacePermissionOutcome.NotRequested => "not-requested",
            WorkspacePermissionOutcome.Planned => "planned",
            WorkspacePermissionOutcome.NotStarted => "not-started",
            WorkspacePermissionOutcome.Verified => "verified",
            WorkspacePermissionOutcome.VerificationFailed => "verification-failed",
            WorkspacePermissionOutcome.CompletionUnknown => "completion-unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The permission outcome is not defined."),
        };
}
