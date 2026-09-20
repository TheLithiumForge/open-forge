using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Selection;

internal static class RouteMoveWireVocabulary
{
    internal static string Name(RouteMoveMode mode)
        => mode switch
        {
            RouteMoveMode.Apply => "apply",
            RouteMoveMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string Name(RouteMoveSourceSelection selection)
        => selection switch
        {
            RouteMoveSourceSelection.SourceId => "source-id",
            RouteMoveSourceSelection.BasePath => "base-path",
            RouteMoveSourceSelection.OverwritePath => "overwrite-path",
            _ => Undefined(nameof(selection), selection),
        };

    internal static string Name(RouteMoveSourceForm form)
        => form switch
        {
            RouteMoveSourceForm.OrdinaryMarkdown => "ordinary-markdown",
            RouteMoveSourceForm.CanonicalEntrypoint => "canonical-entrypoint",
            RouteMoveSourceForm.CompatibilityEntrypoint => "compatibility-entrypoint",
            _ => Undefined(nameof(form), form),
        };

    internal static string Name(RouteMoveSubjectKind kind)
        => kind switch
        {
            RouteMoveSubjectKind.Leaf => "leaf",
            RouteMoveSubjectKind.Category => "category",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteMoveLayerKind layer)
        => layer switch
        {
            RouteMoveLayerKind.Base => "base",
            RouteMoveLayerKind.Overwrite => "overwrite",
            _ => Undefined(nameof(layer), layer),
        };

    internal static string Name(RouteMoveItemKind kind)
        => kind switch
        {
            RouteMoveItemKind.Directory => "directory",
            RouteMoveItemKind.Entrypoint => "entrypoint",
            RouteMoveItemKind.RoutedMarkdown => "routed-markdown",
            RouteMoveItemKind.UnroutedMarkdown => "unrouted-markdown",
            RouteMoveItemKind.NativeSource => "native-source",
            RouteMoveItemKind.Resource => "resource",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteMoveOwnershipState state)
        => state switch
        {
            RouteMoveOwnershipState.NotEstablished => "not-established",
            RouteMoveOwnershipState.Unmanaged => "unmanaged",
            RouteMoveOwnershipState.Claimed => "claimed",
            RouteMoveOwnershipState.Blocked => "blocked",
            RouteMoveOwnershipState.Interrupted => "interrupted",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteMoveOwnershipTrust trust)
        => trust switch
        {
            RouteMoveOwnershipTrust.NotEstablished => "not-established",
            RouteMoveOwnershipTrust.Trusted => "trusted",
            RouteMoveOwnershipTrust.Blocked => "blocked",
            RouteMoveOwnershipTrust.Interrupted => "interrupted",
            _ => Undefined(nameof(trust), trust),
        };

    internal static string Name(RouteMoveOwnershipManager manager)
        => manager switch
        {
            RouteMoveOwnershipManager.Framework => "framework",
            RouteMoveOwnershipManager.Extension => "extension",
            _ => Undefined(nameof(manager), manager),
        };

    internal static string Name(RouteMoveCoverage coverage)
        => coverage switch
        {
            RouteMoveCoverage.NotEstablished => "not-established",
            RouteMoveCoverage.Incomplete => "incomplete",
            RouteMoveCoverage.Complete => "complete",
            RouteMoveCoverage.Blocked => "blocked",
            RouteMoveCoverage.Interrupted => "interrupted",
            _ => Undefined(nameof(coverage), coverage),
        };

    internal static string Name(RouteMoveGeneratedReason reason)
        => reason switch
        {
            RouteMoveGeneratedReason.OldParent => "old-parent",
            RouteMoveGeneratedReason.NewParent => "new-parent",
            RouteMoveGeneratedReason.Loader => "loader",
            RouteMoveGeneratedReason.MovedEntrypoint => "moved-entrypoint",
            _ => Undefined(nameof(reason), reason),
        };

    internal static string Name(RouteMoveGeneratedState state)
        => state switch
        {
            RouteMoveGeneratedState.Changed => "changed",
            RouteMoveGeneratedState.Unchanged => "unchanged",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteMovePlanCompleteness completeness)
        => completeness switch
        {
            RouteMovePlanCompleteness.NotEstablished => "not-established",
            RouteMovePlanCompleteness.Incomplete => "incomplete",
            RouteMovePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string Name(RouteMovePlanSafety safety)
        => safety switch
        {
            RouteMovePlanSafety.NotEstablished => "not-established",
            RouteMovePlanSafety.Safe => "safe",
            RouteMovePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string Name(RouteMoveEffectKind kind)
        => kind switch
        {
            RouteMoveEffectKind.Directory => "directory",
            RouteMoveEffectKind.MovedFile => "moved-file",
            RouteMoveEffectKind.ReferenceSource => "reference-source",
            RouteMoveEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteMoveEffectAction action)
        => action switch
        {
            RouteMoveEffectAction.Create => "create",
            RouteMoveEffectAction.Replace => "replace",
            RouteMoveEffectAction.Delete => "delete",
            _ => Undefined(nameof(action), action),
        };

    internal static string Name(RouteMovePathStateKind kind)
        => kind switch
        {
            RouteMovePathStateKind.Missing => "missing",
            RouteMovePathStateKind.File => "file",
            RouteMovePathStateKind.Directory => "directory",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteMoveEffectOutcome outcome)
        => outcome switch
        {
            RouteMoveEffectOutcome.Planned => "planned",
            RouteMoveEffectOutcome.NotStarted => "not-started",
            RouteMoveEffectOutcome.Verified => "verified",
            RouteMoveEffectOutcome.VerificationFailed => "verification-failed",
            RouteMoveEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string Name(RouteMoveEffectResidual residual)
        => residual switch
        {
            RouteMoveEffectResidual.None => "none",
            RouteMoveEffectResidual.Retained => "retained",
            RouteMoveEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string Name(RouteMoveRecoveryState state)
        => state switch
        {
            RouteMoveRecoveryState.NotRequired => "not-required",
            RouteMoveRecoveryState.NotCreated => "not-created",
            RouteMoveRecoveryState.Removed => "removed",
            RouteMoveRecoveryState.Retained => "retained",
            RouteMoveRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteMoveVerificationState state)
        => state switch
        {
            RouteMoveVerificationState.NotRequested => "not-requested",
            RouteMoveVerificationState.Verified => "verified",
            RouteMoveVerificationState.Failed => "failed",
            RouteMoveVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteMoveFindingCode code)
        => code switch
        {
            RouteMoveFindingCode.InvalidInput => "route-move.invalid-input",
            RouteMoveFindingCode.InvalidSource => "route-move.invalid-source",
            RouteMoveFindingCode.SourceNotFound => "route-move.source-not-found",
            RouteMoveFindingCode.InvalidSubject => "route-move.invalid-subject",
            RouteMoveFindingCode.InvalidDestination => "route-move.invalid-destination",
            RouteMoveFindingCode.WorkspaceUnsafe => "route-move.workspace-unsafe",
            RouteMoveFindingCode.SourceUnsafe => "route-move.source-unsafe",
            RouteMoveFindingCode.RouteAmbiguous => "route-move.route-ambiguous",
            RouteMoveFindingCode.IdentityCollision => "route-move.identity-collision",
            RouteMoveFindingCode.OverwriteAmbiguous => "route-move.overwrite-ambiguous",
            RouteMoveFindingCode.CategoryUnsafe => "route-move.category-unsafe",
            RouteMoveFindingCode.OwnershipUnavailable => "route-move.ownership-unavailable",
            RouteMoveFindingCode.OwnershipClaimed => "route-move.ownership-claimed",
            RouteMoveFindingCode.DestinationUnsafe => "route-move.destination-unsafe",
            RouteMoveFindingCode.DestinationParentMissing => "route-move.destination-parent-missing",
            RouteMoveFindingCode.DestinationOccupied => "route-move.destination-occupied",
            RouteMoveFindingCode.SelfMove => "route-move.self-move",
            RouteMoveFindingCode.DestinationInsideSource => "route-move.destination-inside-source",
            RouteMoveFindingCode.ReferenceUnsafe => "route-move.reference-unsafe",
            RouteMoveFindingCode.GeneratedRegionUnsafe => "route-move.generated-region-unsafe",
            RouteMoveFindingCode.WorkspaceLockUnavailable => "route-move.workspace-lock-unavailable",
            RouteMoveFindingCode.TargetChanged => "route-move.target-changed",
            RouteMoveFindingCode.RecoveryConflict => "route-move.recovery-conflict",
            RouteMoveFindingCode.WorkspaceUnavailable => "route-move.workspace-unavailable",
            RouteMoveFindingCode.CategoryInventoryIncomplete => "route-move.category-inventory-incomplete",
            RouteMoveFindingCode.ReferenceCoverageIncomplete => "route-move.reference-coverage-incomplete",
            RouteMoveFindingCode.ProjectionIncomplete => "route-move.projection-incomplete",
            RouteMoveFindingCode.RecoveryUnavailable => "route-move.recovery-unavailable",
            RouteMoveFindingCode.InspectionIncomplete => "route-move.inspection-incomplete",
            RouteMoveFindingCode.RecoveryArtifactRetained => "route-move.recovery-artifact-retained",
            RouteMoveFindingCode.TargetChangedDuringApply => "route-move.target-changed-during-apply",
            RouteMoveFindingCode.WriteFailed => "route-move.write-failed",
            RouteMoveFindingCode.VerificationFailed => "route-move.verification-failed",
            RouteMoveFindingCode.RecoveryFailed => "route-move.recovery-failed",
            RouteMoveFindingCode.OperationFailed => "route-move.operation-failed",
            RouteMoveFindingCode.Interrupted => "route-move.interrupted",
            _ => Undefined(nameof(code), code),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Move {name} value is not defined.");
}
