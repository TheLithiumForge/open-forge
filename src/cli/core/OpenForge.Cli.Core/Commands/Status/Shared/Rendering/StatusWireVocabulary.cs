using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusWireVocabulary
{
    internal static string WorkspaceSelection(CliWorkspaceSelectionMethod value)
        => value switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => Undefined(nameof(value), value),
        };

    internal static string ValueState(OperationalValueState value)
        => value switch
        {
            OperationalValueState.Available => "available",
            OperationalValueState.Unavailable => "unavailable",
            OperationalValueState.NotApplicable => "not-applicable",
            _ => Undefined(nameof(value), value),
        };

    internal static string InstallationState(OperationalInstallationState value)
        => value switch
        {
            OperationalInstallationState.Installed => "installed",
            OperationalInstallationState.Uninstalled => "uninstalled",
            OperationalInstallationState.Incomplete => "incomplete",
            OperationalInstallationState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string LifecycleState(OperationalLifecycleState value)
        => value switch
        {
            OperationalLifecycleState.Absent => "absent",
            OperationalLifecycleState.Trusted => "trusted",
            OperationalLifecycleState.Untrusted => "untrusted",
            OperationalLifecycleState.Incomplete => "incomplete",
            OperationalLifecycleState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string SourceAvailability(OperationalSourceAvailability value)
        => value switch
        {
            OperationalSourceAvailability.Available => "available",
            OperationalSourceAvailability.Unavailable => "unavailable",
            OperationalSourceAvailability.NotApplicable => "not-applicable",
            _ => Undefined(nameof(value), value),
        };

    internal static string FrameworkTargetKind(FrameworkManagedTargetKind value)
        => value switch
        {
            FrameworkManagedTargetKind.File => "file",
            FrameworkManagedTargetKind.ManagedRegion => "managed-region",
            FrameworkManagedTargetKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(value), value),
        };

    internal static string TargetState(OperationalTargetState value)
        => value switch
        {
            OperationalTargetState.Current => "current",
            OperationalTargetState.Changed => "changed",
            OperationalTargetState.Missing => "missing",
            OperationalTargetState.Unavailable => "unavailable",
            OperationalTargetState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string GeneratedNavigationState(OperationalGeneratedNavigationState value)
        => value switch
        {
            OperationalGeneratedNavigationState.Current => "current",
            OperationalGeneratedNavigationState.Changed => "changed",
            OperationalGeneratedNavigationState.Missing => "missing",
            OperationalGeneratedNavigationState.Unavailable => "unavailable",
            OperationalGeneratedNavigationState.Blocked => "blocked",
            OperationalGeneratedNavigationState.NotApplicable => "not-applicable",
            _ => Undefined(nameof(value), value),
        };

    internal static string RecoveryKind(RecoveryBundleCandidateKind value)
        => value switch
        {
            RecoveryBundleCandidateKind.Final => "final",
            RecoveryBundleCandidateKind.Draft => "draft",
            _ => Undefined(nameof(value), value),
        };

    internal static string RecoveryIntegrity(RecoveryBundleIntegrity value)
        => value switch
        {
            RecoveryBundleIntegrity.Verified => "verified",
            RecoveryBundleIntegrity.Malformed => "malformed",
            RecoveryBundleIntegrity.Unsupported => "unsupported",
            RecoveryBundleIntegrity.Unavailable => "unavailable",
            RecoveryBundleIntegrity.Incomplete => "incomplete",
            _ => Undefined(nameof(value), value),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(name, value, "The Status wire value is not defined.");
}
