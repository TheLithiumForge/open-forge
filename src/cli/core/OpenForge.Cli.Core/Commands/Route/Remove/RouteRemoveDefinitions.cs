using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove;

internal static class RouteRemoveDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route remove";

    internal const string RouteRemoveHelpCommand = "open-forge route remove --help";
    internal const string RouteRemoveCommand = "open-forge route remove";
    internal const string CleanupCommand = "open-forge cleanup";
    internal const string DoctorCommand = "open-forge doctor";
    internal const string VerboseRouteRemoveCommand = "open-forge route remove --verbose";

    internal static readonly CliSyntaxDefinition RemoveCommand = new(
        "remove",
        "Remove one eligible routed source or complete category without releasing managed ownership.");

    internal static readonly CliSyntaxDefinition SourceReference = new(
        "source-reference",
        "Select one routed source or category by ID or exact path.");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete removal plan without writing.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<RouteRemoveFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<RouteRemoveFindingCode>());

    internal static string ReadMachineName(RouteRemoveMode mode)
        => mode switch
        {
            RouteRemoveMode.Apply => "apply",
            RouteRemoveMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RouteRemoveSourceSelection selection)
        => selection switch
        {
            RouteRemoveSourceSelection.SourceId => "source-id",
            RouteRemoveSourceSelection.BasePath => "base-path",
            RouteRemoveSourceSelection.OverwritePath => "overwrite-path",
            _ => Undefined(nameof(selection), selection),
        };

    internal static string ReadMachineName(RouteRemoveSourceForm form)
        => form switch
        {
            RouteRemoveSourceForm.OrdinaryMarkdown => "ordinary-markdown",
            RouteRemoveSourceForm.CanonicalEntrypoint => "canonical-entrypoint",
            RouteRemoveSourceForm.CompatibilityEntrypoint => "compatibility-entrypoint",
            _ => Undefined(nameof(form), form),
        };

    internal static string ReadMachineName(RouteRemoveSubjectKind kind)
        => kind switch
        {
            RouteRemoveSubjectKind.Leaf => "leaf",
            RouteRemoveSubjectKind.Category => "category",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteRemoveLayerKind layer)
        => layer switch
        {
            RouteRemoveLayerKind.Base => "base",
            RouteRemoveLayerKind.Overwrite => "overwrite",
            _ => Undefined(nameof(layer), layer),
        };

    internal static string ReadMachineName(RouteRemoveItemKind kind)
        => kind switch
        {
            RouteRemoveItemKind.Directory => "directory",
            RouteRemoveItemKind.Entrypoint => "entrypoint",
            RouteRemoveItemKind.RoutedMarkdown => "routed-markdown",
            RouteRemoveItemKind.UnroutedMarkdown => "unrouted-markdown",
            RouteRemoveItemKind.NativeSource => "native-source",
            RouteRemoveItemKind.Resource => "resource",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteRemoveOwnershipState state)
        => state switch
        {
            RouteRemoveOwnershipState.NotEstablished => "not-established",
            RouteRemoveOwnershipState.Unmanaged => "unmanaged",
            RouteRemoveOwnershipState.Claimed => "claimed",
            RouteRemoveOwnershipState.Blocked => "blocked",
            RouteRemoveOwnershipState.Interrupted => "interrupted",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteRemoveOwnershipTrust trust)
        => trust switch
        {
            RouteRemoveOwnershipTrust.NotEstablished => "not-established",
            RouteRemoveOwnershipTrust.Trusted => "trusted",
            RouteRemoveOwnershipTrust.Blocked => "blocked",
            RouteRemoveOwnershipTrust.Interrupted => "interrupted",
            _ => Undefined(nameof(trust), trust),
        };

    internal static string ReadMachineName(RouteRemoveOwnershipManager manager)
        => manager switch
        {
            RouteRemoveOwnershipManager.Framework => "framework",
            RouteRemoveOwnershipManager.Extension => "extension",
            _ => Undefined(nameof(manager), manager),
        };

    internal static string ReadMachineName(RouteRemoveCoverage coverage)
        => coverage switch
        {
            RouteRemoveCoverage.NotEstablished => "not-established",
            RouteRemoveCoverage.Incomplete => "incomplete",
            RouteRemoveCoverage.Complete => "complete",
            RouteRemoveCoverage.Blocked => "blocked",
            RouteRemoveCoverage.Interrupted => "interrupted",
            _ => Undefined(nameof(coverage), coverage),
        };

    internal static string ReadMachineName(RouteRemoveGeneratedReason reason)
        => reason switch
        {
            RouteRemoveGeneratedReason.OldParent => "old-parent",
            RouteRemoveGeneratedReason.Loader => "loader",
            _ => Undefined(nameof(reason), reason),
        };

    internal static string ReadMachineName(RouteRemoveGeneratedState state)
        => state switch
        {
            RouteRemoveGeneratedState.Changed => "changed",
            RouteRemoveGeneratedState.Unchanged => "unchanged",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteRemovePlanCompleteness completeness)
        => completeness switch
        {
            RouteRemovePlanCompleteness.NotEstablished => "not-established",
            RouteRemovePlanCompleteness.Incomplete => "incomplete",
            RouteRemovePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string ReadMachineName(RouteRemovePlanSafety safety)
        => safety switch
        {
            RouteRemovePlanSafety.NotEstablished => "not-established",
            RouteRemovePlanSafety.Safe => "safe",
            RouteRemovePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string ReadMachineName(RouteRemoveEffectKind kind)
        => kind switch
        {
            RouteRemoveEffectKind.Directory => "directory",
            RouteRemoveEffectKind.RemovedFile => "removed-file",
            RouteRemoveEffectKind.ReferenceSource => "reference-source",
            RouteRemoveEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteRemoveEffectAction action)
        => action switch
        {
            RouteRemoveEffectAction.Replace => "replace",
            RouteRemoveEffectAction.Delete => "delete",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(RouteRemovePathStateKind kind)
        => kind switch
        {
            RouteRemovePathStateKind.Missing => "missing",
            RouteRemovePathStateKind.File => "file",
            RouteRemovePathStateKind.Directory => "directory",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteRemoveEffectOutcome outcome)
        => outcome switch
        {
            RouteRemoveEffectOutcome.Planned => "planned",
            RouteRemoveEffectOutcome.NotStarted => "not-started",
            RouteRemoveEffectOutcome.Verified => "verified",
            RouteRemoveEffectOutcome.VerificationFailed => "verification-failed",
            RouteRemoveEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(RouteRemoveEffectResidual residual)
        => residual switch
        {
            RouteRemoveEffectResidual.None => "none",
            RouteRemoveEffectResidual.Retained => "retained",
            RouteRemoveEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(RouteRemoveRecoveryState state)
        => state switch
        {
            RouteRemoveRecoveryState.NotRequired => "not-required",
            RouteRemoveRecoveryState.NotCreated => "not-created",
            RouteRemoveRecoveryState.Removed => "removed",
            RouteRemoveRecoveryState.Retained => "retained",
            RouteRemoveRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteRemoveVerificationState state)
        => state switch
        {
            RouteRemoveVerificationState.NotRequested => "not-requested",
            RouteRemoveVerificationState.Verified => "verified",
            RouteRemoveVerificationState.Failed => "failed",
            RouteRemoveVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteRemoveFindingCode code)
        => code switch
        {
            RouteRemoveFindingCode.InvalidInput => RouteRemoveFindingCodes.InvalidInput,
            RouteRemoveFindingCode.InvalidSource => RouteRemoveFindingCodes.InvalidSource,
            RouteRemoveFindingCode.SourceNotFound => RouteRemoveFindingCodes.SourceNotFound,
            RouteRemoveFindingCode.InvalidSubject => RouteRemoveFindingCodes.InvalidSubject,
            RouteRemoveFindingCode.WorkspaceUnsafe => RouteRemoveFindingCodes.WorkspaceUnsafe,
            RouteRemoveFindingCode.SourceUnsafe => RouteRemoveFindingCodes.SourceUnsafe,
            RouteRemoveFindingCode.RouteAmbiguous => RouteRemoveFindingCodes.RouteAmbiguous,
            RouteRemoveFindingCode.IdentityCollision => RouteRemoveFindingCodes.IdentityCollision,
            RouteRemoveFindingCode.OverwriteAmbiguous => RouteRemoveFindingCodes.OverwriteAmbiguous,
            RouteRemoveFindingCode.CategoryUnsafe => RouteRemoveFindingCodes.CategoryUnsafe,
            RouteRemoveFindingCode.OwnershipUnavailable => RouteRemoveFindingCodes.OwnershipUnavailable,
            RouteRemoveFindingCode.OwnershipClaimed => RouteRemoveFindingCodes.OwnershipClaimed,
            RouteRemoveFindingCode.ReferenceUnsafe => RouteRemoveFindingCodes.ReferenceUnsafe,
            RouteRemoveFindingCode.GeneratedRegionUnsafe => RouteRemoveFindingCodes.GeneratedRegionUnsafe,
            RouteRemoveFindingCode.WorkspaceLockUnavailable => RouteRemoveFindingCodes.WorkspaceLockUnavailable,
            RouteRemoveFindingCode.TargetChanged => RouteRemoveFindingCodes.TargetChanged,
            RouteRemoveFindingCode.RecoveryConflict => RouteRemoveFindingCodes.RecoveryConflict,
            RouteRemoveFindingCode.WorkspaceUnavailable => RouteRemoveFindingCodes.WorkspaceUnavailable,
            RouteRemoveFindingCode.CategoryInventoryIncomplete => RouteRemoveFindingCodes.CategoryInventoryIncomplete,
            RouteRemoveFindingCode.ReferenceCoverageIncomplete => RouteRemoveFindingCodes.ReferenceCoverageIncomplete,
            RouteRemoveFindingCode.ProjectionIncomplete => RouteRemoveFindingCodes.ProjectionIncomplete,
            RouteRemoveFindingCode.RecoveryUnavailable => RouteRemoveFindingCodes.RecoveryUnavailable,
            RouteRemoveFindingCode.InspectionIncomplete => RouteRemoveFindingCodes.InspectionIncomplete,
            RouteRemoveFindingCode.RecoveryArtifactRetained => RouteRemoveFindingCodes.RecoveryArtifactRetained,
            RouteRemoveFindingCode.TargetChangedDuringApply => RouteRemoveFindingCodes.TargetChangedDuringApply,
            RouteRemoveFindingCode.WriteFailed => RouteRemoveFindingCodes.WriteFailed,
            RouteRemoveFindingCode.VerificationFailed => RouteRemoveFindingCodes.VerificationFailed,
            RouteRemoveFindingCode.RecoveryFailed => RouteRemoveFindingCodes.RecoveryFailed,
            RouteRemoveFindingCode.OperationFailed => RouteRemoveFindingCodes.OperationFailed,
            RouteRemoveFindingCode.Interrupted => RouteRemoveFindingCodes.Interrupted,
            _ => Undefined(nameof(code), code),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Remove {name} value is not defined.");
}

internal static class RouteRemoveFindingCodes
{
    internal const string InvalidInput = "route-remove.invalid-input";
    internal const string InvalidSource = "route-remove.invalid-source";
    internal const string SourceNotFound = "route-remove.source-not-found";
    internal const string InvalidSubject = "route-remove.invalid-subject";
    internal const string WorkspaceUnsafe = "route-remove.workspace-unsafe";
    internal const string SourceUnsafe = "route-remove.source-unsafe";
    internal const string RouteAmbiguous = "route-remove.route-ambiguous";
    internal const string IdentityCollision = "route-remove.identity-collision";
    internal const string OverwriteAmbiguous = "route-remove.overwrite-ambiguous";
    internal const string CategoryUnsafe = "route-remove.category-unsafe";
    internal const string OwnershipUnavailable = "route-remove.ownership-unavailable";
    internal const string OwnershipClaimed = "route-remove.ownership-claimed";
    internal const string ReferenceUnsafe = "route-remove.reference-unsafe";
    internal const string GeneratedRegionUnsafe = "route-remove.generated-region-unsafe";
    internal const string WorkspaceLockUnavailable = "route-remove.workspace-lock-unavailable";
    internal const string TargetChanged = "route-remove.target-changed";
    internal const string RecoveryConflict = "route-remove.recovery-conflict";
    internal const string WorkspaceUnavailable = "route-remove.workspace-unavailable";
    internal const string CategoryInventoryIncomplete = "route-remove.category-inventory-incomplete";
    internal const string ReferenceCoverageIncomplete = "route-remove.reference-coverage-incomplete";
    internal const string ProjectionIncomplete = "route-remove.projection-incomplete";
    internal const string RecoveryUnavailable = "route-remove.recovery-unavailable";
    internal const string InspectionIncomplete = "route-remove.inspection-incomplete";
    internal const string RecoveryArtifactRetained = "route-remove.recovery-artifact-retained";
    internal const string TargetChangedDuringApply = "route-remove.target-changed-during-apply";
    internal const string WriteFailed = "route-remove.write-failed";
    internal const string VerificationFailed = "route-remove.verification-failed";
    internal const string RecoveryFailed = "route-remove.recovery-failed";
    internal const string OperationFailed = "route-remove.operation-failed";
    internal const string Interrupted = "route-remove.interrupted";
}

internal static class RouteRemoveNextActions
{
    internal static CliNextAction RetainedRecovery { get; } = new(
        RouteRemoveDefinitions.CleanupCommand,
        "Review and remove the reported recovery artifact after confirming the verified Route Remove result.");

    internal static CliNextAction InvalidRequest { get; } = new(
        RouteRemoveDefinitions.RouteRemoveHelpCommand,
        "Correct the named Route Remove input, then rerun the request.");

    internal static CliNextAction RetryableBlock { get; } = new(
        RouteRemoveDefinitions.RouteRemoveCommand,
        "Wait for the blocking condition or inspect the changed target, then rerun Route Remove from a fresh plan.");

    internal static CliNextAction Blocked { get; } = new(
        RouteRemoveDefinitions.DoctorCommand,
        "Inspect the blocked workspace, route, ownership, reference, generated-region, or recovery boundary before rerunning Route Remove.");

    internal static CliNextAction Incomplete { get; } = new(
        RouteRemoveDefinitions.DoctorCommand,
        "Inspect the unavailable inventory, reference, projection, or recovery facts before relying on this Route Remove result.");

    internal static CliNextAction Failed { get; } = new(
        RouteRemoveDefinitions.VerboseRouteRemoveCommand,
        "Report the failure and retry the same Route Remove request with bounded diagnostics.");

    internal static CliNextAction Interrupted { get; } = new(
        RouteRemoveDefinitions.RouteRemoveCommand,
        "Rerun the same Route Remove request.");
}
