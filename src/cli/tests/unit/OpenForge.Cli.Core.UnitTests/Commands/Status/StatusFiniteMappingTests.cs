using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusFiniteMappingTests
{
    [Fact(DisplayName = "Status maps every named operational value and rejects undefined runtime values"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void NamedOperationalValuesMapToExactMachineNamesAndUndefinedValuesFailClosed()
    {
        AssertMappings(
        [
            (CliWorkspaceSelectionMethod.CurrentDirectory, "current-directory"),
            (CliWorkspaceSelectionMethod.ExplicitWorkspace, "explicit-workspace"),
        ], StatusWireVocabulary.WorkspaceSelection);
        AssertMappings(
        [
            (OperationalValueState.Available, "available"),
            (OperationalValueState.Unavailable, "unavailable"),
            (OperationalValueState.NotApplicable, "not-applicable"),
        ], StatusWireVocabulary.ValueState);
        AssertMappings(
        [
            (OperationalInstallationState.Installed, "installed"),
            (OperationalInstallationState.Uninstalled, "uninstalled"),
            (OperationalInstallationState.Incomplete, "incomplete"),
            (OperationalInstallationState.Blocked, "blocked"),
        ], StatusWireVocabulary.InstallationState);
        AssertMappings(
        [
            (OperationalLifecycleState.Absent, "absent"),
            (OperationalLifecycleState.Trusted, "trusted"),
            (OperationalLifecycleState.Untrusted, "untrusted"),
            (OperationalLifecycleState.Incomplete, "incomplete"),
            (OperationalLifecycleState.Blocked, "blocked"),
        ], StatusWireVocabulary.LifecycleState);
        AssertMappings(
        [
            (OperationalSourceAvailability.Available, "available"),
            (OperationalSourceAvailability.Unavailable, "unavailable"),
            (OperationalSourceAvailability.NotApplicable, "not-applicable"),
        ], StatusWireVocabulary.SourceAvailability);
        AssertMappings(
        [
            (FrameworkManagedTargetKind.File, "file"),
            (FrameworkManagedTargetKind.ManagedRegion, "managed-region"),
            (FrameworkManagedTargetKind.GeneratedRegion, "generated-region"),
        ], StatusWireVocabulary.FrameworkTargetKind);
        AssertMappings(
        [
            (OperationalTargetState.Current, "current"),
            (OperationalTargetState.Changed, "changed"),
            (OperationalTargetState.Missing, "missing"),
            (OperationalTargetState.Unavailable, "unavailable"),
            (OperationalTargetState.Blocked, "blocked"),
        ], StatusWireVocabulary.TargetState);
        AssertMappings(
        [
            (OperationalGeneratedNavigationState.Current, "current"),
            (OperationalGeneratedNavigationState.Changed, "changed"),
            (OperationalGeneratedNavigationState.Missing, "missing"),
            (OperationalGeneratedNavigationState.Unavailable, "unavailable"),
            (OperationalGeneratedNavigationState.Blocked, "blocked"),
            (OperationalGeneratedNavigationState.NotApplicable, "not-applicable"),
        ], StatusWireVocabulary.GeneratedNavigationState);
        AssertMappings(
        [
            (RecoveryBundleCandidateKind.Final, "final"),
            (RecoveryBundleCandidateKind.Draft, "draft"),
        ], StatusWireVocabulary.RecoveryKind);
        AssertMappings(
        [
            (RecoveryBundleIntegrity.Verified, "verified"),
            (RecoveryBundleIntegrity.Malformed, "malformed"),
            (RecoveryBundleIntegrity.Unsupported, "unsupported"),
            (RecoveryBundleIntegrity.Unavailable, "unavailable"),
            (RecoveryBundleIntegrity.Incomplete, "incomplete"),
        ], StatusWireVocabulary.RecoveryIntegrity);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.WorkspaceSelection((CliWorkspaceSelectionMethod)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.ValueState((OperationalValueState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.InstallationState((OperationalInstallationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.LifecycleState((OperationalLifecycleState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.SourceAvailability((OperationalSourceAvailability)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.FrameworkTargetKind((FrameworkManagedTargetKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.TargetState((OperationalTargetState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.GeneratedNavigationState((OperationalGeneratedNavigationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.RecoveryKind((RecoveryBundleCandidateKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatusWireVocabulary.RecoveryIntegrity((RecoveryBundleIntegrity)int.MaxValue));
    }

    [Fact(DisplayName = "Status finding codes preserve every exact machine name and semantic status"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void FindingCodesMapToExactMachineNamesAndStatusesAndUndefinedValueFailsClosed()
    {
        var expected = new (StatusFindingCode Code, string MachineName, CliSemanticStatus Status)[]
        {
            (StatusFindingCode.InvalidInput, "invalid-input", CliSemanticStatus.Invalid),
            (StatusFindingCode.WorkspaceUnavailable, "workspace-unavailable", CliSemanticStatus.Blocked),
            (StatusFindingCode.WorkspaceNotDirectory, "workspace-not-directory", CliSemanticStatus.Blocked),
            (StatusFindingCode.WorkspaceUnsafe, "workspace-unsafe", CliSemanticStatus.Blocked),
            (StatusFindingCode.EntryUnavailable, "entry-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.EmbeddedFrameworkUnavailable, "embedded-framework-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ContextInventoryIncomplete, "context-inventory-incomplete", CliSemanticStatus.Incomplete),
            (StatusFindingCode.StartupContextUnavailable, "startup-context-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ContinuityContextUnavailable, "continuity-context-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.RootCategoriesUnavailable, "root-categories-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.GeneratedNavigationChanged, "generated-navigation-changed", CliSemanticStatus.Attention),
            (StatusFindingCode.GeneratedNavigationMissing, "generated-navigation-missing", CliSemanticStatus.Attention),
            (StatusFindingCode.GeneratedNavigationUnavailable, "generated-navigation-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.GeneratedNavigationBlocked, "generated-navigation-blocked", CliSemanticStatus.Blocked),
            (StatusFindingCode.FrameworkLifecycleUntrusted, "framework-lifecycle-untrusted", CliSemanticStatus.Incomplete),
            (StatusFindingCode.FrameworkLifecycleIncomplete, "framework-lifecycle-incomplete", CliSemanticStatus.Incomplete),
            (StatusFindingCode.FrameworkLifecycleBlocked, "framework-lifecycle-blocked", CliSemanticStatus.Blocked),
            (StatusFindingCode.FrameworkTargetChanged, "framework-target-changed", CliSemanticStatus.Attention),
            (StatusFindingCode.FrameworkTargetMissing, "framework-target-missing", CliSemanticStatus.Attention),
            (StatusFindingCode.FrameworkTargetUnavailable, "framework-target-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.FrameworkTargetBlocked, "framework-target-blocked", CliSemanticStatus.Blocked),
            (StatusFindingCode.ExtensionLifecycleUntrusted, "extension-lifecycle-untrusted", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ExtensionLifecycleIncomplete, "extension-lifecycle-incomplete", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ExtensionLifecycleBlocked, "extension-lifecycle-blocked", CliSemanticStatus.Blocked),
            (StatusFindingCode.ExtensionSourceUnavailable, "extension-source-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ExtensionTargetChanged, "extension-target-changed", CliSemanticStatus.Attention),
            (StatusFindingCode.ExtensionTargetMissing, "extension-target-missing", CliSemanticStatus.Attention),
            (StatusFindingCode.ExtensionTargetUnavailable, "extension-target-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.ExtensionTargetBlocked, "extension-target-blocked", CliSemanticStatus.Blocked),
            (StatusFindingCode.RecoveryCandidateVerified, "recovery-candidate-verified", CliSemanticStatus.Attention),
            (StatusFindingCode.RecoveryDraftIncomplete, "recovery-draft-incomplete", CliSemanticStatus.Incomplete),
            (StatusFindingCode.RecoveryFinalMalformed, "recovery-final-malformed", CliSemanticStatus.Incomplete),
            (StatusFindingCode.RecoveryFinalUnsupported, "recovery-final-unsupported", CliSemanticStatus.Incomplete),
            (StatusFindingCode.RecoveryFinalUnavailable, "recovery-final-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.RecoveryCatalogueUnavailable, "recovery-catalogue-unavailable", CliSemanticStatus.Incomplete),
            (StatusFindingCode.OperationFailed, "operation-failed", CliSemanticStatus.Failed),
            (StatusFindingCode.Interrupted, "interrupted", CliSemanticStatus.Interrupted),
        };

        Assert.Equal(Enum.GetValues<StatusFindingCode>(), expected.Select(item => item.Code));
        foreach (var (code, machineName, status) in expected)
        {
            Assert.Equal(machineName, StatusDefinitions.ReadFindingCode(code));
            Assert.Equal(status, StatusDefinitions.ReadFindingStatus(code));
        }

        var undefined = (StatusFindingCode)int.MaxValue;
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusDefinitions.ReadFindingCode(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusDefinitions.ReadFindingStatus(undefined));
    }

    private static void AssertMappings<T>(
        IReadOnlyList<(T Value, string MachineName)> expected,
        Func<T, string> readMachineName)
        where T : struct, Enum
    {
        Assert.Equal(Enum.GetValues<T>(), expected.Select(item => item.Value));
        foreach (var (value, machineName) in expected)
        {
            Assert.Equal(machineName, readMachineName(value));
        }
    }
}
