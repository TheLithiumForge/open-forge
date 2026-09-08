using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupWireVocabulary
{
    internal static string Status(CliSemanticStatus value)
        => CliStatusDefinitions.Read(value).MachineName;

    internal static string WorkspaceSelection(CliWorkspaceSelectionMethod value)
        => value switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => Undefined(nameof(value), value),
        };

    internal static string Mode(CleanupMode value)
        => value switch
        {
            CleanupMode.NotEstablished => "not-established",
            CleanupMode.Apply => "apply",
            CleanupMode.DryRun => "dry-run",
            _ => Undefined(nameof(value), value),
        };

    internal static string Coverage(CleanupCatalogueCoverage value)
        => value switch
        {
            CleanupCatalogueCoverage.NotEstablished => "not-established",
            CleanupCatalogueCoverage.Complete => "complete",
            CleanupCatalogueCoverage.Incomplete => "incomplete",
            CleanupCatalogueCoverage.Interrupted => "interrupted",
            _ => Undefined(nameof(value), value),
        };

    internal static string Eligibility(CleanupCandidateEligibility value)
        => value switch
        {
            CleanupCandidateEligibility.NotEstablished => "not-established",
            CleanupCandidateEligibility.Eligible => "eligible",
            CleanupCandidateEligibility.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string Action(CleanupPlanAction value)
        => value switch
        {
            CleanupPlanAction.NotEstablished => "not-established",
            CleanupPlanAction.Delete => "delete",
            CleanupPlanAction.Preserve => "preserve",
            _ => Undefined(nameof(value), value),
        };

    internal static string Safety(CleanupPlanSafety value)
        => value switch
        {
            CleanupPlanSafety.NotEstablished => "not-established",
            CleanupPlanSafety.Safe => "safe",
            CleanupPlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string CandidateKind(RecoveryBundleCandidateKind value)
        => value switch
        {
            RecoveryBundleCandidateKind.Final => "final",
            RecoveryBundleCandidateKind.Draft => "draft",
            _ => Undefined(nameof(value), value),
        };

    internal static string Integrity(RecoveryBundleIntegrity value)
        => value switch
        {
            RecoveryBundleIntegrity.Verified => "verified",
            RecoveryBundleIntegrity.Malformed => "malformed",
            RecoveryBundleIntegrity.Unsupported => "unsupported",
            RecoveryBundleIntegrity.Unavailable => "unavailable",
            RecoveryBundleIntegrity.Incomplete => "incomplete",
            _ => Undefined(nameof(value), value),
        };

    internal static string FileKind(CleanupArtifactFileKind value)
        => value switch
        {
            CleanupArtifactFileKind.NotEstablished => "not-established",
            CleanupArtifactFileKind.Ordinary => "ordinary",
            CleanupArtifactFileKind.NonOrdinary => "non-ordinary",
            _ => Undefined(nameof(value), value),
        };

    internal static string WorkspaceAssociation(CleanupWorkspaceAssociationState value)
        => value switch
        {
            CleanupWorkspaceAssociationState.NotEstablished => "not-established",
            CleanupWorkspaceAssociationState.CurrentWorkspace => "current-workspace",
            CleanupWorkspaceAssociationState.Mismatched => "mismatched",
            CleanupWorkspaceAssociationState.Unavailable => "unavailable",
            _ => Undefined(nameof(value), value),
        };

    internal static string LeaseBoundary(CleanupLeaseBoundaryState value)
        => value switch
        {
            CleanupLeaseBoundaryState.NotEstablished => "not-established",
            CleanupLeaseBoundaryState.NotRequested => "not-requested",
            CleanupLeaseBoundaryState.Required => "required",
            CleanupLeaseBoundaryState.Held => "held",
            CleanupLeaseBoundaryState.Mismatched => "mismatched",
            _ => Undefined(nameof(value), value),
        };

    internal static string VerificationCondition(CleanupVerificationConditionState value)
        => value switch
        {
            CleanupVerificationConditionState.NotEstablished => "not-established",
            CleanupVerificationConditionState.NotRequested => "not-requested",
            CleanupVerificationConditionState.ExactPathAndKind => "exact-path-and-kind",
            CleanupVerificationConditionState.SemanticFinal => "semantic-final",
            CleanupVerificationConditionState.Absence => "absence",
            _ => Undefined(nameof(value), value),
        };

    internal static string Preflight(CleanupPreflightState value)
        => value switch
        {
            CleanupPreflightState.NotRequested => "not-requested",
            CleanupPreflightState.Complete => "complete",
            CleanupPreflightState.Incomplete => "incomplete",
            CleanupPreflightState.Blocked => "blocked",
            CleanupPreflightState.Failed => "failed",
            CleanupPreflightState.Interrupted => "interrupted",
            _ => Undefined(nameof(value), value),
        };

    internal static string Lease(CleanupLeaseState value)
        => value switch
        {
            CleanupLeaseState.NotRequested => "not-requested",
            CleanupLeaseState.Acquired => "acquired",
            CleanupLeaseState.Failed => "failed",
            CleanupLeaseState.Cancelled => "cancelled",
            _ => Undefined(nameof(value), value),
        };

    internal static string Comparison(CleanupCatalogueComparisonState value)
        => value switch
        {
            CleanupCatalogueComparisonState.NotRequested => "not-requested",
            CleanupCatalogueComparisonState.Matched => "matched",
            CleanupCatalogueComparisonState.Changed => "changed",
            CleanupCatalogueComparisonState.Incomplete => "incomplete",
            CleanupCatalogueComparisonState.Blocked => "blocked",
            CleanupCatalogueComparisonState.Cancelled => "cancelled",
            _ => Undefined(nameof(value), value),
        };

    internal static string EffectOutcome(CleanupEffectOutcome value)
        => value switch
        {
            CleanupEffectOutcome.Planned => "planned",
            CleanupEffectOutcome.NotStarted => "not-started",
            CleanupEffectOutcome.Verified => "verified",
            CleanupEffectOutcome.VerificationFailed => "verification-failed",
            CleanupEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string EffectResidual(CleanupEffectResidual value)
        => value switch
        {
            CleanupEffectResidual.None => "none",
            CleanupEffectResidual.Retained => "retained",
            CleanupEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string Verification(CleanupVerificationState value)
        => value switch
        {
            CleanupVerificationState.NotRequested => "not-requested",
            CleanupVerificationState.Verified => "verified",
            CleanupVerificationState.Failed => "failed",
            CleanupVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string FindingCode(CleanupFindingCode value)
        => value switch
        {
            CleanupFindingCode.InvalidInput => "cleanup.invalid-input",
            CleanupFindingCode.WorkspaceUnavailable => "cleanup.workspace-unavailable",
            CleanupFindingCode.WorkspaceNotDirectory => "cleanup.workspace-not-directory",
            CleanupFindingCode.WorkspaceUnsafe => "cleanup.workspace-unsafe",
            CleanupFindingCode.CatalogueIncomplete => "cleanup.catalogue-incomplete",
            CleanupFindingCode.RecoveryFinalMalformed => "cleanup.recovery-final-malformed",
            CleanupFindingCode.RecoveryFinalUnsupported => "cleanup.recovery-final-unsupported",
            CleanupFindingCode.RecoveryFinalUnavailable => "cleanup.recovery-final-unavailable",
            CleanupFindingCode.RecoveryDraftUnsafe => "cleanup.recovery-draft-unsafe",
            CleanupFindingCode.WorkspaceLockUnavailable => "cleanup.workspace-lock-unavailable",
            CleanupFindingCode.CatalogueChangedDuringApply => "cleanup.catalogue-changed-during-apply",
            CleanupFindingCode.CandidateChangedDuringApply => "cleanup.candidate-changed-during-apply",
            CleanupFindingCode.DeletionFailed => "cleanup.deletion-failed",
            CleanupFindingCode.VerificationFailed => "cleanup.verification-failed",
            CleanupFindingCode.OperationFailed => "cleanup.operation-failed",
            CleanupFindingCode.Interrupted => "cleanup.interrupted",
            _ => Undefined(nameof(value), value),
        };

    internal static string RecoveryProducer(RecoveryBundleProducer value)
        => value switch
        {
            RecoveryBundleProducer.Framework => "framework",
            RecoveryBundleProducer.Extension => "extension",
            RecoveryBundleProducer.Index => "index",
            RecoveryBundleProducer.Route => "route",
            RecoveryBundleProducer.Repair => "repair",
            RecoveryBundleProducer.Library => "library",
            _ => Undefined(nameof(value), value),
        };

    internal static string RecoveryOperation(RecoveryBundleOperation value)
        => value switch
        {
            RecoveryBundleOperation.Install => "install",
            RecoveryBundleOperation.Index => "index",
            RecoveryBundleOperation.Create => "create",
            RecoveryBundleOperation.Init => "init",
            RecoveryBundleOperation.Move => "move",
            RecoveryBundleOperation.Update => "update",
            RecoveryBundleOperation.Remove => "remove",
            RecoveryBundleOperation.Repair => "repair",
            RecoveryBundleOperation.Attach => "attach",
            RecoveryBundleOperation.Sync => "sync",
            RecoveryBundleOperation.Detach => "detach",
            _ => Undefined(nameof(value), value),
        };

    internal static string RecoverySubjectKind(RecoveryBundleSubjectKind value)
        => value switch
        {
            RecoveryBundleSubjectKind.Workspace => "workspace",
            _ => Undefined(nameof(value), value),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Cleanup {name} value is not defined.");
}
