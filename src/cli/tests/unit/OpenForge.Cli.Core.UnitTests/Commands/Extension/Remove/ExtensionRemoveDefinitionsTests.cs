using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveDefinitionsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove definitions expose the exact command grammar and defaults"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void DefinitionsExposeExactGrammar()
    {
        Assert.Equal("extension remove", ExtensionRemoveDefinitions.CommandIdentity);
        Assert.Equal(1, ExtensionRemoveDefinitions.SchemaVersion);
        Assert.Equal("remove", ExtensionRemoveDefinitions.RemoveCommand.Name);
        Assert.Contains("Remove selected Extension ownership", ExtensionRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Equal("stable-id", ExtensionRemoveDefinitions.StableId.Name);
        Assert.Equal("--automatic", ExtensionRemoveDefinitions.Automatic.Name);
        Assert.Equal("--dry-run", ExtensionRemoveDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.None, ExtensionRemoveDefinitions.Automatic.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionRemoveDefinitions.DryRun.Arity);
        Assert.False(ExtensionRemoveDefinitions.Automatic.DefaultValue);
        Assert.False(ExtensionRemoveDefinitions.DryRun.DefaultValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove definitions expose every finding wire name and semantic status"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void FindingDefinitionsMapEveryCodeAndStatus()
    {
        Assert.Equal(Enum.GetValues<ExtensionRemoveFindingCode>(), ExtensionRemoveDefinitions.FindingCodes);
        (ExtensionRemoveFindingCode Code, CliSemanticStatus Status)[] expected =
        [
            (ExtensionRemoveFindingCode.InvalidInput, CliSemanticStatus.Invalid),
            (ExtensionRemoveFindingCode.SelectionRequired, CliSemanticStatus.Invalid),
            (ExtensionRemoveFindingCode.InteractionEnded, CliSemanticStatus.Invalid),
            (ExtensionRemoveFindingCode.ConfirmationRequired, CliSemanticStatus.Invalid),
            (ExtensionRemoveFindingCode.FrameworkUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.FrameworkUnsafe, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.LifecycleUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.LifecycleBlocked, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.DependencyBlocked, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.SettingsInvalid, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.SettingsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.PathExcluded, CliSemanticStatus.Complete),
            (ExtensionRemoveFindingCode.OwnershipConflict, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.PermissionRequired, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.PermissionDeclined, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.PermissionsInvalid, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.PermissionsUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.PermissionsChanged, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.PermissionWriteFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.TargetUnsafe, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.ProjectionUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.GeneratedRegionUnsafe, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.WorkspaceLockUnavailable, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.TargetChanged, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.RecoveryConflict, CliSemanticStatus.Blocked),
            (ExtensionRemoveFindingCode.RecoveryUnavailable, CliSemanticStatus.Incomplete),
            (ExtensionRemoveFindingCode.LifecycleObservation, CliSemanticStatus.Attention),
            (ExtensionRemoveFindingCode.OwnershipObservation, CliSemanticStatus.Complete),
            (ExtensionRemoveFindingCode.RecoveryArtifactRetained, CliSemanticStatus.Attention),
            (ExtensionRemoveFindingCode.RecoveryHistory, CliSemanticStatus.Attention),
            (ExtensionRemoveFindingCode.RecoveryCoverageUnavailable, CliSemanticStatus.Attention),
            (ExtensionRemoveFindingCode.WriteFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.TopologyVerificationFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.LifecyclePublicationFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.VerificationFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.RecoveryFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.OperationFailed, CliSemanticStatus.Failed),
            (ExtensionRemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted),
        ];
        Assert.Equal(expected.Select(item => item.Code), Enum.GetValues<ExtensionRemoveFindingCode>());
        Assert.Equal(expected.Select(item => item.Code), ExtensionRemoveDefinitions.FindingCodes);
        Assert.All(expected, item => Assert.Equal(item.Status, ExtensionRemoveDefinitions.ReadStatus(item.Code)));
        CommandOutputSnapshot.MatchSnapshot(string.Join("\n", ExtensionRemoveDefinitions.FindingCodes.Select(code =>
            $"{code}: {ExtensionRemoveDefinitions.ReadMachineName(code)}")), "finding-codes");
        Assert.Equal(
            ExtensionRemoveDefinitions.FindingCodes.Count,
            ExtensionRemoveDefinitions.FindingCodes.Distinct().Count());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove definitions map every finite mode, selection, path, effect, and result value"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void FiniteValuesMapToStableWireNames()
    {
        Assert.Equal(
            [(ExtensionRemoveMode.Apply, "apply"), (ExtensionRemoveMode.DryRun, "dry-run")],
            Enum.GetValues<ExtensionRemoveMode>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
            [(ExtensionRemoveSelectionKind.ExplicitIds, "explicit-ids"), (ExtensionRemoveSelectionKind.InteractiveIds, "interactive-ids")],
            Enum.GetValues<ExtensionRemoveSelectionKind>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemovePathClassification.Shared, "shared"),
            (ExtensionRemovePathClassification.FinalOwner, "final-owner"),
            (ExtensionRemovePathClassification.Missing, "missing"),
        ],
            Enum.GetValues<ExtensionRemovePathClassification>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemovePathAction.RetainShared, "retain-shared"),
            (ExtensionRemovePathAction.Delete, "delete"),
            (ExtensionRemovePathAction.ReleaseOwnership, "release-ownership"),
        ],
            Enum.GetValues<ExtensionRemovePathAction>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectKind.PackageFile, "package-file"),
            (ExtensionRemoveEffectKind.GeneratedRegion, "generated-region"),
            (ExtensionRemoveEffectKind.Lifecycle, "lifecycle"),
            (ExtensionRemoveEffectKind.Settings, "settings"),
            (ExtensionRemoveEffectKind.Directory, "directory"),
        ],
            Enum.GetValues<ExtensionRemoveEffectKind>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectAction.ReleaseOwnership, "release-ownership"),
            (ExtensionRemoveEffectAction.Delete, "delete"),
            (ExtensionRemoveEffectAction.Retain, "retain"),
            (ExtensionRemoveEffectAction.RecordExclusion, "record-exclusion"),
            (ExtensionRemoveEffectAction.Create, "create"),
        ],
            Enum.GetValues<ExtensionRemoveEffectAction>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectOutcome.Planned, "planned"),
            (ExtensionRemoveEffectOutcome.NotStarted, "not-started"),
            (ExtensionRemoveEffectOutcome.Verified, "verified"),
            (ExtensionRemoveEffectOutcome.VerificationFailed, "verification-failed"),
            (ExtensionRemoveEffectOutcome.CompletionUnknown, "completion-unknown"),
        ],
            Enum.GetValues<ExtensionRemoveEffectOutcome>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectResidual.None, "none"),
            (ExtensionRemoveEffectResidual.Retained, "retained"),
            (ExtensionRemoveEffectResidual.Unknown, "unknown"),
        ],
            Enum.GetValues<ExtensionRemoveEffectResidual>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveGeneratedRegionState.Unchanged, "unchanged"),
            (ExtensionRemoveGeneratedRegionState.Changed, "changed"),
            (ExtensionRemoveGeneratedRegionState.Unavailable, "unavailable"),
            (ExtensionRemoveGeneratedRegionState.Blocked, "blocked"),
        ],
            Enum.GetValues<ExtensionRemoveGeneratedRegionState>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveLifecycleTrust.NotRequested, "not-requested"),
            (ExtensionRemoveLifecycleTrust.Trusted, "trusted"),
            (ExtensionRemoveLifecycleTrust.Unavailable, "unavailable"),
            (ExtensionRemoveLifecycleTrust.Blocked, "blocked"),
        ],
            Enum.GetValues<ExtensionRemoveLifecycleTrust>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveLifecycleCoverage.NotRequested, "not-requested"),
            (ExtensionRemoveLifecycleCoverage.Complete, "complete"),
            (ExtensionRemoveLifecycleCoverage.Incomplete, "incomplete"),
            (ExtensionRemoveLifecycleCoverage.Blocked, "blocked"),
        ],
            Enum.GetValues<ExtensionRemoveLifecycleCoverage>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveLifecycleAction.None, "none"),
            (ExtensionRemoveLifecycleAction.Preserve, "preserve"),
            (ExtensionRemoveLifecycleAction.Publish, "publish"),
        ],
            Enum.GetValues<ExtensionRemoveLifecycleAction>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveLifecycleOutcome.NotRequested, "not-requested"),
            (ExtensionRemoveLifecycleOutcome.Planned, "planned"),
            (ExtensionRemoveLifecycleOutcome.AlreadyCurrent, "already-current"),
            (ExtensionRemoveLifecycleOutcome.NotStarted, "not-started"),
            (ExtensionRemoveLifecycleOutcome.Verified, "verified"),
            (ExtensionRemoveLifecycleOutcome.VerificationFailed, "verification-failed"),
            (ExtensionRemoveLifecycleOutcome.CompletionUnknown, "completion-unknown"),
        ],
            Enum.GetValues<ExtensionRemoveLifecycleOutcome>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveRecoveryState.NotRequired, "not-required"),
            (ExtensionRemoveRecoveryState.NotCreated, "not-created"),
            (ExtensionRemoveRecoveryState.Removed, "removed"),
            (ExtensionRemoveRecoveryState.Retained, "retained"),
            (ExtensionRemoveRecoveryState.Unknown, "unknown"),
        ],
            Enum.GetValues<ExtensionRemoveRecoveryState>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveVerificationState.NotRequested, "not-requested"),
            (ExtensionRemoveVerificationState.Planned, "planned"),
            (ExtensionRemoveVerificationState.Verified, "verified"),
            (ExtensionRemoveVerificationState.Failed, "failed"),
            (ExtensionRemoveVerificationState.Unknown, "unknown"),
        ],
            Enum.GetValues<ExtensionRemoveVerificationState>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove next actions preserve status and recovery guidance"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void NextActionsAreBounded()
    {
        var retainedRecovery = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.RecoveryArtifactRetained,
            "The recovery bundle remains.");
        var attention = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.LifecycleObservation,
            "The package is not recorded as installed.");

        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Complete, []));
        Assert.Equal(
            "open-forge extension remove --help",
            Assert.IsType<CliNextAction>(ExtensionRemoveDefinitions.ReadNextAction(
                CliSemanticStatus.Invalid,
                [new ExtensionRemoveFinding(ExtensionRemoveFindingCode.InvalidInput, "Invalid input.")])).Command);
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Attention, [attention]));
        Assert.Equal(
            "open-forge cleanup",
            Assert.IsType<CliNextAction>(ExtensionRemoveDefinitions.ReadNextAction(
                CliSemanticStatus.Attention,
                [retainedRecovery])).Command);
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Incomplete, []));
        var invalidSettingsNext = Assert.IsType<CliNextAction>(ExtensionRemoveDefinitions.ReadNextAction(
            CliSemanticStatus.Blocked,
            [new ExtensionRemoveFinding(ExtensionRemoveFindingCode.SettingsInvalid, "Invalid settings.")]));
        Assert.Equal("Correct .agents/open-forge.json, then rerun the removal.", invalidSettingsNext.Command);
        Assert.Equal(CliNextActionKind.Sentence, invalidSettingsNext.Kind);
        var unavailableSettingsNext = Assert.IsType<CliNextAction>(ExtensionRemoveDefinitions.ReadNextAction(
            CliSemanticStatus.Incomplete,
            [new ExtensionRemoveFinding(ExtensionRemoveFindingCode.SettingsUnavailable, "Unavailable settings.")]));
        Assert.Equal("Restore access to .agents/open-forge.json, then rerun the removal.", unavailableSettingsNext.Command);
        Assert.Equal(CliNextActionKind.Sentence, unavailableSettingsNext.Kind);
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Blocked, []));
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Failed, []));
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Interrupted, []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionRemoveDefinitions.ReadNextAction((CliSemanticStatus)int.MaxValue, []));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove definitions reject undefined enum values"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void UndefinedValuesFailClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadStatus((ExtensionRemoveFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveSelectionKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemovePathClassification)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemovePathAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveGeneratedRegionState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleTrust)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleCoverage)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveVerificationState)int.MaxValue));
    }
}
