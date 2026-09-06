using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveDefinitionsTests
{
    [Fact(DisplayName = "Extension Remove definitions expose the exact command grammar and defaults"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void DefinitionsExposeExactGrammar()
    {
        Assert.Equal("extension remove", ExtensionRemoveDefinitions.CommandIdentity);
        Assert.Equal(1, ExtensionRemoveDefinitions.SchemaVersion);
        Assert.Equal("remove", ExtensionRemoveDefinitions.RemoveCommand.Name);
        Assert.Contains("Release selected managed Extension ownership", ExtensionRemoveDefinitions.RemoveCommand.Description, StringComparison.Ordinal);
        Assert.Equal("stable-id", ExtensionRemoveDefinitions.StableId.Name);
        Assert.Equal("--prune", ExtensionRemoveDefinitions.Prune.Name);
        Assert.Equal("--automatic", ExtensionRemoveDefinitions.Automatic.Name);
        Assert.Equal("--dry-run", ExtensionRemoveDefinitions.DryRun.Name);
        Assert.Equal(CliOptionArity.None, ExtensionRemoveDefinitions.Prune.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionRemoveDefinitions.Automatic.Arity);
        Assert.Equal(CliOptionArity.None, ExtensionRemoveDefinitions.DryRun.Arity);
        Assert.False(ExtensionRemoveDefinitions.Prune.DefaultValue);
        Assert.False(ExtensionRemoveDefinitions.Automatic.DefaultValue);
        Assert.False(ExtensionRemoveDefinitions.DryRun.DefaultValue);
    }

    [Fact(DisplayName = "Extension Remove definitions expose every finding wire name and semantic status"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void FindingDefinitionsMapEveryCodeAndStatus()
    {
        Assert.Equal(Enum.GetValues<ExtensionRemoveFindingCode>(), ExtensionRemoveDefinitions.FindingCodes);
        Assert.Equal(
        [
            ("extension-remove.invalid-input", CliSemanticStatus.Invalid),
            ("extension-remove.selection-required", CliSemanticStatus.Invalid),
            ("extension-remove.interaction-ended", CliSemanticStatus.Invalid),
            ("extension-remove.framework-unavailable", CliSemanticStatus.Incomplete),
            ("extension-remove.framework-unsafe", CliSemanticStatus.Blocked),
            ("extension-remove.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            ("extension-remove.lifecycle-blocked", CliSemanticStatus.Blocked),
            ("extension-remove.dependency-blocked", CliSemanticStatus.Blocked),
            ("extension-remove.ownership-conflict", CliSemanticStatus.Blocked),
            ("extension-remove.managed-divergence", CliSemanticStatus.Attention),
            ("extension-remove.target-outside-agents", CliSemanticStatus.Blocked),
            ("extension-remove.target-unsafe", CliSemanticStatus.Blocked),
            ("extension-remove.projection-unavailable", CliSemanticStatus.Incomplete),
            ("extension-remove.generated-region-unsafe", CliSemanticStatus.Blocked),
            ("extension-remove.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            ("extension-remove.target-changed", CliSemanticStatus.Blocked),
            ("extension-remove.recovery-conflict", CliSemanticStatus.Blocked),
            ("extension-remove.recovery-unavailable", CliSemanticStatus.Incomplete),
            ("extension-remove.lifecycle-observation", CliSemanticStatus.Attention),
            ("extension-remove.recovery-artifact-retained", CliSemanticStatus.Attention),
            ("extension-remove.write-failed", CliSemanticStatus.Failed),
            ("extension-remove.topology-verification-failed", CliSemanticStatus.Failed),
            ("extension-remove.lifecycle-publication-failed", CliSemanticStatus.Failed),
            ("extension-remove.verification-failed", CliSemanticStatus.Failed),
            ("extension-remove.recovery-failed", CliSemanticStatus.Failed),
            ("extension-remove.operation-failed", CliSemanticStatus.Failed),
            ("extension-remove.interrupted", CliSemanticStatus.Interrupted),
        ],
            ExtensionRemoveDefinitions.FindingCodes.Select(code =>
                (ExtensionRemoveDefinitions.ReadMachineName(code), ExtensionRemoveDefinitions.ReadStatus(code))));
        Assert.Equal(27, ExtensionRemoveDefinitions.FindingCodes.Count);
        Assert.Equal(
            ExtensionRemoveDefinitions.FindingCodes.Count,
            ExtensionRemoveDefinitions.FindingCodes.Distinct().Count());
    }

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
            (ExtensionRemovePathClassification.UnchangedFinalOwner, "unchanged-final-owner"),
            (ExtensionRemovePathClassification.ChangedFinalOwner, "changed-final-owner"),
            (ExtensionRemovePathClassification.Missing, "missing"),
        ],
            Enum.GetValues<ExtensionRemovePathClassification>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemovePathAction.RetainShared, "retain-shared"),
            (ExtensionRemovePathAction.Delete, "delete"),
            (ExtensionRemovePathAction.KeepAsUnmanaged, "keep-as-unmanaged"),
            (ExtensionRemovePathAction.ReleaseOwnership, "release-ownership"),
        ],
            Enum.GetValues<ExtensionRemovePathAction>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectKind.PackageFile, "package-file"),
            (ExtensionRemoveEffectKind.GeneratedRegion, "generated-region"),
            (ExtensionRemoveEffectKind.Lifecycle, "lifecycle"),
        ],
            Enum.GetValues<ExtensionRemoveEffectKind>().Select(value =>
                (value, ExtensionRemoveDefinitions.ReadMachineName(value))));
        Assert.Equal(
        [
            (ExtensionRemoveEffectAction.ReleaseOwnership, "release-ownership"),
            (ExtensionRemoveEffectAction.Delete, "delete"),
            (ExtensionRemoveEffectAction.Retain, "retain"),
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
            (ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged, "keep-as-unmanaged"),
            (ExtensionRemoveChangedContentPolicy.Delete, "delete"),
        ],
            Enum.GetValues<ExtensionRemoveChangedContentPolicy>().Select(value =>
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

    [Fact(DisplayName = "Extension Remove next actions preserve status and recovery guidance"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void NextActionsAreBounded()
    {
        var retainedRecovery = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.RecoveryArtifactRetained,
            "The recovery bundle remains.");
        var attention = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.ManagedDivergence,
            "Changed content is retained.");

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
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Blocked, []));
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Failed, []));
        Assert.Null(ExtensionRemoveDefinitions.ReadNextAction(CliSemanticStatus.Interrupted, []));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtensionRemoveDefinitions.ReadNextAction((CliSemanticStatus)int.MaxValue, []));
    }

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
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveChangedContentPolicy)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveGeneratedRegionState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleTrust)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleCoverage)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveLifecycleOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionRemoveDefinitions.ReadMachineName((ExtensionRemoveVerificationState)int.MaxValue));
    }
}
