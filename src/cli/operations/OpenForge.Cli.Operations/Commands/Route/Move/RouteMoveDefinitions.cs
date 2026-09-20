using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Move;

internal static class RouteMoveDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route move";

    internal const string RouteMoveHelpCommand = "open-forge route move --help";
    internal const string VerboseRouteMoveCommand = "open-forge route move --detail debug";

    internal static readonly CliSyntaxDefinition MoveCommand = new(
        "move",
        "Move one routed source or category while preserving its route meaning.");

    internal static readonly CliSyntaxDefinition SourceReference = new(
        "source-reference",
        "Select one routed source or category by ID or exact path.");

    internal static readonly CliSyntaxDefinition DestinationTarget = new(
        "destination-target",
        "Name the intended destination by exact path.");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete move plan without writing.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<RouteMoveFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<RouteMoveFindingCode>());

    internal static string ReadMachineName(RouteMoveMode mode)
        => mode switch
        {
            RouteMoveMode.Apply => "apply",
            RouteMoveMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RouteMoveSourceSelection selection)
        => selection switch
        {
            RouteMoveSourceSelection.SourceId => "source-id",
            RouteMoveSourceSelection.BasePath => "base-path",
            RouteMoveSourceSelection.OverwritePath => "overwrite-path",
            _ => Undefined(nameof(selection), selection),
        };

    internal static string ReadMachineName(RouteMoveSourceForm form)
        => form switch
        {
            RouteMoveSourceForm.OrdinaryMarkdown => "ordinary-markdown",
            RouteMoveSourceForm.CanonicalEntrypoint => "canonical-entrypoint",
            RouteMoveSourceForm.CompatibilityEntrypoint => "compatibility-entrypoint",
            _ => Undefined(nameof(form), form),
        };

    internal static string ReadMachineName(RouteMoveSubjectKind kind)
        => kind switch
        {
            RouteMoveSubjectKind.Leaf => "leaf",
            RouteMoveSubjectKind.Category => "category",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteMoveLayerKind layer)
        => layer switch
        {
            RouteMoveLayerKind.Base => "base",
            RouteMoveLayerKind.Overwrite => "overwrite",
            _ => Undefined(nameof(layer), layer),
        };

    internal static string ReadMachineName(RouteMoveItemKind kind)
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

    internal static string ReadMachineName(RouteMoveOwnershipState state)
        => state switch
        {
            RouteMoveOwnershipState.NotEstablished => "not-established",
            RouteMoveOwnershipState.Unmanaged => "unmanaged",
            RouteMoveOwnershipState.Claimed => "claimed",
            RouteMoveOwnershipState.Blocked => "blocked",
            RouteMoveOwnershipState.Interrupted => "interrupted",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteMoveOwnershipTrust trust)
        => trust switch
        {
            RouteMoveOwnershipTrust.NotEstablished => "not-established",
            RouteMoveOwnershipTrust.Trusted => "trusted",
            RouteMoveOwnershipTrust.Blocked => "blocked",
            RouteMoveOwnershipTrust.Interrupted => "interrupted",
            _ => Undefined(nameof(trust), trust),
        };

    internal static string ReadMachineName(RouteMoveOwnershipManager manager)
        => manager switch
        {
            RouteMoveOwnershipManager.Framework => "framework",
            RouteMoveOwnershipManager.Extension => "extension",
            _ => Undefined(nameof(manager), manager),
        };

    internal static string ReadMachineName(RouteMoveCoverage coverage)
        => coverage switch
        {
            RouteMoveCoverage.NotEstablished => "not-established",
            RouteMoveCoverage.Incomplete => "incomplete",
            RouteMoveCoverage.Complete => "complete",
            RouteMoveCoverage.Blocked => "blocked",
            RouteMoveCoverage.Interrupted => "interrupted",
            _ => Undefined(nameof(coverage), coverage),
        };

    internal static string ReadMachineName(RouteMoveGeneratedReason reason)
        => reason switch
        {
            RouteMoveGeneratedReason.OldParent => "old-parent",
            RouteMoveGeneratedReason.NewParent => "new-parent",
            RouteMoveGeneratedReason.Loader => "loader",
            RouteMoveGeneratedReason.MovedEntrypoint => "moved-entrypoint",
            _ => Undefined(nameof(reason), reason),
        };

    internal static string ReadMachineName(RouteMoveGeneratedState state)
        => state switch
        {
            RouteMoveGeneratedState.Changed => "changed",
            RouteMoveGeneratedState.Unchanged => "unchanged",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteMovePlanCompleteness completeness)
        => completeness switch
        {
            RouteMovePlanCompleteness.NotEstablished => "not-established",
            RouteMovePlanCompleteness.Incomplete => "incomplete",
            RouteMovePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string ReadMachineName(RouteMovePlanSafety safety)
        => safety switch
        {
            RouteMovePlanSafety.NotEstablished => "not-established",
            RouteMovePlanSafety.Safe => "safe",
            RouteMovePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string ReadMachineName(RouteMoveEffectKind kind)
        => kind switch
        {
            RouteMoveEffectKind.Directory => "directory",
            RouteMoveEffectKind.MovedFile => "moved-file",
            RouteMoveEffectKind.ReferenceSource => "reference-source",
            RouteMoveEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteMoveEffectAction action)
        => action switch
        {
            RouteMoveEffectAction.Create => "create",
            RouteMoveEffectAction.Replace => "replace",
            RouteMoveEffectAction.Delete => "delete",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(RouteMovePathStateKind kind)
        => kind switch
        {
            RouteMovePathStateKind.Missing => "missing",
            RouteMovePathStateKind.File => "file",
            RouteMovePathStateKind.Directory => "directory",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteMoveEffectOutcome outcome)
        => outcome switch
        {
            RouteMoveEffectOutcome.Planned => "planned",
            RouteMoveEffectOutcome.NotStarted => "not-started",
            RouteMoveEffectOutcome.Verified => "verified",
            RouteMoveEffectOutcome.VerificationFailed => "verification-failed",
            RouteMoveEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(RouteMoveEffectResidual residual)
        => residual switch
        {
            RouteMoveEffectResidual.None => "none",
            RouteMoveEffectResidual.Retained => "retained",
            RouteMoveEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(RouteMoveRecoveryState state)
        => state switch
        {
            RouteMoveRecoveryState.NotRequired => "not-required",
            RouteMoveRecoveryState.NotCreated => "not-created",
            RouteMoveRecoveryState.Removed => "removed",
            RouteMoveRecoveryState.Retained => "retained",
            RouteMoveRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteMoveVerificationState state)
        => state switch
        {
            RouteMoveVerificationState.NotRequested => "not-requested",
            RouteMoveVerificationState.Verified => "verified",
            RouteMoveVerificationState.Failed => "failed",
            RouteMoveVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteMoveFindingCode code)
        => code switch
        {
            RouteMoveFindingCode.InvalidInput => RouteMoveFindingCodes.InvalidInput,
            RouteMoveFindingCode.InvalidSource => RouteMoveFindingCodes.InvalidSource,
            RouteMoveFindingCode.SourceNotFound => RouteMoveFindingCodes.SourceNotFound,
            RouteMoveFindingCode.InvalidSubject => RouteMoveFindingCodes.InvalidSubject,
            RouteMoveFindingCode.InvalidDestination => RouteMoveFindingCodes.InvalidDestination,
            RouteMoveFindingCode.WorkspaceUnsafe => RouteMoveFindingCodes.WorkspaceUnsafe,
            RouteMoveFindingCode.SourceUnsafe => RouteMoveFindingCodes.SourceUnsafe,
            RouteMoveFindingCode.RouteAmbiguous => RouteMoveFindingCodes.RouteAmbiguous,
            RouteMoveFindingCode.IdentityCollision => RouteMoveFindingCodes.IdentityCollision,
            RouteMoveFindingCode.OverwriteAmbiguous => RouteMoveFindingCodes.OverwriteAmbiguous,
            RouteMoveFindingCode.CategoryUnsafe => RouteMoveFindingCodes.CategoryUnsafe,
            RouteMoveFindingCode.OwnershipUnavailable => RouteMoveFindingCodes.OwnershipUnavailable,
            RouteMoveFindingCode.OwnershipClaimed => RouteMoveFindingCodes.OwnershipClaimed,
            RouteMoveFindingCode.DestinationUnsafe => RouteMoveFindingCodes.DestinationUnsafe,
            RouteMoveFindingCode.DestinationParentMissing => RouteMoveFindingCodes.DestinationParentMissing,
            RouteMoveFindingCode.DestinationOccupied => RouteMoveFindingCodes.DestinationOccupied,
            RouteMoveFindingCode.SelfMove => RouteMoveFindingCodes.SelfMove,
            RouteMoveFindingCode.DestinationInsideSource => RouteMoveFindingCodes.DestinationInsideSource,
            RouteMoveFindingCode.ReferenceUnsafe => RouteMoveFindingCodes.ReferenceUnsafe,
            RouteMoveFindingCode.GeneratedRegionUnsafe => RouteMoveFindingCodes.GeneratedRegionUnsafe,
            RouteMoveFindingCode.WorkspaceLockUnavailable => RouteMoveFindingCodes.WorkspaceLockUnavailable,
            RouteMoveFindingCode.TargetChanged => RouteMoveFindingCodes.TargetChanged,
            RouteMoveFindingCode.RecoveryConflict => RouteMoveFindingCodes.RecoveryConflict,
            RouteMoveFindingCode.WorkspaceUnavailable => RouteMoveFindingCodes.WorkspaceUnavailable,
            RouteMoveFindingCode.CategoryInventoryIncomplete => RouteMoveFindingCodes.CategoryInventoryIncomplete,
            RouteMoveFindingCode.ReferenceCoverageIncomplete => RouteMoveFindingCodes.ReferenceCoverageIncomplete,
            RouteMoveFindingCode.ProjectionIncomplete => RouteMoveFindingCodes.ProjectionIncomplete,
            RouteMoveFindingCode.RecoveryUnavailable => RouteMoveFindingCodes.RecoveryUnavailable,
            RouteMoveFindingCode.InspectionIncomplete => RouteMoveFindingCodes.InspectionIncomplete,
            RouteMoveFindingCode.RecoveryArtifactRetained => RouteMoveFindingCodes.RecoveryArtifactRetained,
            RouteMoveFindingCode.TargetChangedDuringApply => RouteMoveFindingCodes.TargetChangedDuringApply,
            RouteMoveFindingCode.WriteFailed => RouteMoveFindingCodes.WriteFailed,
            RouteMoveFindingCode.VerificationFailed => RouteMoveFindingCodes.VerificationFailed,
            RouteMoveFindingCode.RecoveryFailed => RouteMoveFindingCodes.RecoveryFailed,
            RouteMoveFindingCode.OperationFailed => RouteMoveFindingCodes.OperationFailed,
            RouteMoveFindingCode.Interrupted => RouteMoveFindingCodes.Interrupted,
            _ => Undefined(nameof(code), code),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Move {name} value is not defined.");
}

internal static class RouteMoveFindingCodes
{
    internal const string InvalidInput = "route-move.invalid-input";
    internal const string InvalidSource = "route-move.invalid-source";
    internal const string SourceNotFound = "route-move.source-not-found";
    internal const string InvalidSubject = "route-move.invalid-subject";
    internal const string InvalidDestination = "route-move.invalid-destination";
    internal const string WorkspaceUnsafe = "route-move.workspace-unsafe";
    internal const string SourceUnsafe = "route-move.source-unsafe";
    internal const string RouteAmbiguous = "route-move.route-ambiguous";
    internal const string IdentityCollision = "route-move.identity-collision";
    internal const string OverwriteAmbiguous = "route-move.overwrite-ambiguous";
    internal const string CategoryUnsafe = "route-move.category-unsafe";
    internal const string OwnershipUnavailable = "route-move.ownership-unavailable";
    internal const string OwnershipClaimed = "route-move.ownership-claimed";
    internal const string DestinationUnsafe = "route-move.destination-unsafe";
    internal const string DestinationParentMissing = "route-move.destination-parent-missing";
    internal const string DestinationOccupied = "route-move.destination-occupied";
    internal const string SelfMove = "route-move.self-move";
    internal const string DestinationInsideSource = "route-move.destination-inside-source";
    internal const string ReferenceUnsafe = "route-move.reference-unsafe";
    internal const string GeneratedRegionUnsafe = "route-move.generated-region-unsafe";
    internal const string WorkspaceLockUnavailable = "route-move.workspace-lock-unavailable";
    internal const string TargetChanged = "route-move.target-changed";
    internal const string RecoveryConflict = "route-move.recovery-conflict";
    internal const string WorkspaceUnavailable = "route-move.workspace-unavailable";
    internal const string CategoryInventoryIncomplete = "route-move.category-inventory-incomplete";
    internal const string ReferenceCoverageIncomplete = "route-move.reference-coverage-incomplete";
    internal const string ProjectionIncomplete = "route-move.projection-incomplete";
    internal const string RecoveryUnavailable = "route-move.recovery-unavailable";
    internal const string InspectionIncomplete = "route-move.inspection-incomplete";
    internal const string RecoveryArtifactRetained = "route-move.recovery-artifact-retained";
    internal const string TargetChangedDuringApply = "route-move.target-changed-during-apply";
    internal const string WriteFailed = "route-move.write-failed";
    internal const string VerificationFailed = "route-move.verification-failed";
    internal const string RecoveryFailed = "route-move.recovery-failed";
    internal const string OperationFailed = "route-move.operation-failed";
    internal const string Interrupted = "route-move.interrupted";
}

internal static class RouteMoveNextActions
{
    internal static CliNextAction RetainedRecovery { get; } = new(
        CommandLines.Cleanup,
        "Review and remove the reported recovery artifact after confirming the verified Route Move result.");

    internal static CliNextAction MissingDestinationParent { get; } = new(
        CommandLines.RouteInit,
        "Initialize the exact missing destination parent route, then rerun Route Move.");

    internal static CliNextAction InvalidRequest { get; } = new(
        RouteMoveDefinitions.RouteMoveHelpCommand,
        "Correct the named Route Move input, then rerun the request.");

    internal static CliNextAction RetryableBlock { get; } = new(
        CommandLines.RouteMove,
        "Wait for the blocking condition or inspect the changed target, then rerun Route Move from a fresh plan.");

    internal static CliNextAction Blocked { get; } = new(
        CommandLines.Doctor,
        "Inspect the blocked workspace, route, ownership, reference, generated-region, destination, or recovery boundary before rerunning Route Move.");

    internal static CliNextAction Incomplete { get; } = new(
        CommandLines.Doctor,
        "Inspect the unavailable inventory, reference, projection, or recovery facts before relying on this Route Move result.");

    internal static CliNextAction Failed { get; } = new(
        RouteMoveDefinitions.VerboseRouteMoveCommand,
        "Report the failure and retry the same Route Move request with bounded diagnostics.");

    internal static CliNextAction Interrupted { get; } = new(
        CommandLines.RouteMove,
        "Rerun the same Route Move request.");
}
