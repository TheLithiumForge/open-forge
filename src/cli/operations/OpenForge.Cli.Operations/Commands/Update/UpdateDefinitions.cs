using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Update;

internal static class UpdateDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "update";
    internal const string UpdateCommandName = "update";
    internal const string HelpCommand = "open-forge update --help";

    internal static readonly CliSyntaxDefinition UpdateCommand = new(
        UpdateCommandName,
        "Update managed Framework files.");

    internal static readonly CliOptionDefinition<bool> Force = new(
        "--force",
        "Replace changed or restore missing managed content.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Prune = new(
        "--prune",
        "Delete eligible retired managed content.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        "--automatic",
        "Use safe defaults without prompting; does not imply --force or --prune.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete update without writing.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<UpdateFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<UpdateFindingCode>());

    internal static string ReadMachineName(UpdateMode value)
        => value switch
        {
            UpdateMode.Apply => "apply",
            UpdateMode.DryRun => "dry-run",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateComparisonTargetKind value)
        => value switch
        {
            UpdateComparisonTargetKind.File => "file",
            UpdateComparisonTargetKind.ManagedRegion => "managed-region",
            UpdateComparisonTargetKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateComparisonFingerprintKind value)
        => value switch
        {
            UpdateComparisonFingerprintKind.OpenForgeMarkdownV1 => "open-forge-markdown-v1",
            UpdateComparisonFingerprintKind.ExactBytes => "exact-bytes",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateComparisonCurrentState value)
        => value switch
        {
            UpdateComparisonCurrentState.Missing => "missing",
            UpdateComparisonCurrentState.Same => "same",
            UpdateComparisonCurrentState.FormatOnly => "format-only",
            UpdateComparisonCurrentState.Changed => "changed",
            UpdateComparisonCurrentState.Unavailable => "unavailable",
            UpdateComparisonCurrentState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateComparisonIntendedState value)
        => value switch
        {
            UpdateComparisonIntendedState.Same => "same",
            UpdateComparisonIntendedState.Changed => "changed",
            UpdateComparisonIntendedState.New => "new",
            UpdateComparisonIntendedState.Retired => "retired",
            UpdateComparisonIntendedState.Unavailable => "unavailable",
            UpdateComparisonIntendedState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateRetirementEligibility value)
        => value switch
        {
            UpdateRetirementEligibility.NotApplicable => "not-applicable",
            UpdateRetirementEligibility.Eligible => "eligible",
            UpdateRetirementEligibility.Ineligible => "ineligible",
            UpdateRetirementEligibility.Unavailable => "unavailable",
            UpdateRetirementEligibility.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdatePlanningDisposition value)
        => value switch
        {
            UpdatePlanningDisposition.NoOp => "no-op",
            UpdatePlanningDisposition.Create => "create",
            UpdatePlanningDisposition.Replace => "replace",
            UpdatePlanningDisposition.Restore => "restore",
            UpdatePlanningDisposition.Delete => "delete",
            UpdatePlanningDisposition.Preserve => "preserve",
            UpdatePlanningDisposition.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateLogicalChangeAction value)
        => value switch
        {
            UpdateLogicalChangeAction.Create => "create",
            UpdateLogicalChangeAction.Replace => "replace",
            UpdateLogicalChangeAction.Restore => "restore",
            UpdateLogicalChangeAction.Delete => "delete",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdatePhysicalEffectAction value)
        => value switch
        {
            UpdatePhysicalEffectAction.Create => "create",
            UpdatePhysicalEffectAction.Replace => "replace",
            UpdatePhysicalEffectAction.Delete => "delete",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdatePhysicalEffectOutcome value)
        => value switch
        {
            UpdatePhysicalEffectOutcome.Planned => "planned",
            UpdatePhysicalEffectOutcome.NotStarted => "not-started",
            UpdatePhysicalEffectOutcome.Verified => "verified",
            UpdatePhysicalEffectOutcome.VerificationFailed => "verification-failed",
            UpdatePhysicalEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdatePhysicalEffectResidual value)
        => value switch
        {
            UpdatePhysicalEffectResidual.None => "none",
            UpdatePhysicalEffectResidual.Retained => "retained",
            UpdatePhysicalEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateLifecycleTrust value)
        => value switch
        {
            UpdateLifecycleTrust.NotRequested => "not-requested",
            UpdateLifecycleTrust.Trusted => "trusted",
            UpdateLifecycleTrust.Unavailable => "unavailable",
            UpdateLifecycleTrust.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateLifecycleCoverage value)
        => value switch
        {
            UpdateLifecycleCoverage.NotRequested => "not-requested",
            UpdateLifecycleCoverage.Complete => "complete",
            UpdateLifecycleCoverage.Incomplete => "incomplete",
            UpdateLifecycleCoverage.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateLifecycleAction value)
        => value switch
        {
            UpdateLifecycleAction.None => "none",
            UpdateLifecycleAction.Preserve => "preserve",
            UpdateLifecycleAction.Publish => "publish",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateLifecycleOutcome value)
        => value switch
        {
            UpdateLifecycleOutcome.NotRequested => "not-requested",
            UpdateLifecycleOutcome.Planned => "planned",
            UpdateLifecycleOutcome.AlreadyCurrent => "already-current",
            UpdateLifecycleOutcome.NotStarted => "not-started",
            UpdateLifecycleOutcome.Verified => "verified",
            UpdateLifecycleOutcome.VerificationFailed => "verification-failed",
            UpdateLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateGeneratedNavigationCoverage value)
        => value switch
        {
            UpdateGeneratedNavigationCoverage.Complete => "complete",
            UpdateGeneratedNavigationCoverage.Incomplete => "incomplete",
            UpdateGeneratedNavigationCoverage.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateGeneratedNavigationRegionState value)
        => value switch
        {
            UpdateGeneratedNavigationRegionState.Unchanged => "unchanged",
            UpdateGeneratedNavigationRegionState.Changed => "changed",
            UpdateGeneratedNavigationRegionState.New => "new",
            UpdateGeneratedNavigationRegionState.Retired => "retired",
            UpdateGeneratedNavigationRegionState.Unavailable => "unavailable",
            UpdateGeneratedNavigationRegionState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateRecoveryState value)
        => value switch
        {
            UpdateRecoveryState.NotRequired => "not-required",
            UpdateRecoveryState.NotCreated => "not-created",
            UpdateRecoveryState.Removed => "removed",
            UpdateRecoveryState.Retained => "retained",
            UpdateRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(UpdateVerificationState value)
        => value switch
        {
            UpdateVerificationState.NotRequested => "not-requested",
            UpdateVerificationState.Verified => "verified",
            UpdateVerificationState.Failed => "failed",
            UpdateVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static UpdateFindingDefinition Read(UpdateFindingCode code)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The Update finding code is not defined.");
        }

        return code switch
        {
            UpdateFindingCode.InvalidInput => Definition(code, "update.invalid-input", CliSemanticStatus.Invalid),
            UpdateFindingCode.ConfirmationRequired => Definition(code, "update.confirmation-required", CliSemanticStatus.Invalid),
            UpdateFindingCode.WorkspaceUnavailable => Definition(code, "update.workspace-unavailable", CliSemanticStatus.Blocked),
            UpdateFindingCode.WorkspaceUnsafe => Definition(code, "update.workspace-unsafe", CliSemanticStatus.Blocked),
            UpdateFindingCode.PayloadUnavailable => Definition(code, "update.payload-unavailable", CliSemanticStatus.Incomplete),
            UpdateFindingCode.PayloadInvalid => Definition(code, "update.payload-invalid", CliSemanticStatus.Blocked),
            UpdateFindingCode.LifecycleMissing => Definition(code, "update.lifecycle-missing", CliSemanticStatus.Incomplete),
            UpdateFindingCode.LifecycleUnavailable => Definition(code, "update.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            UpdateFindingCode.LifecycleBlocked => Definition(code, "update.lifecycle-blocked", CliSemanticStatus.Blocked),
            UpdateFindingCode.OwnershipObservation => Definition(code, "update.ownership-observation", CliSemanticStatus.Complete),
            UpdateFindingCode.OwnershipConflict => Definition(code, "update.ownership-conflict", CliSemanticStatus.Blocked),
            UpdateFindingCode.TargetUnavailable => Definition(code, "update.target-unavailable", CliSemanticStatus.Incomplete),
            UpdateFindingCode.TargetUnsafe => Definition(code, "update.target-unsafe", CliSemanticStatus.Blocked),
            UpdateFindingCode.SourceProvenanceInvalid => Definition(code, "update.source-provenance-invalid", CliSemanticStatus.Blocked),
            UpdateFindingCode.FingerprintUnsupported => Definition(code, "update.fingerprint-unsupported", CliSemanticStatus.Blocked),
            UpdateFindingCode.RetiredContentPreserved => Definition(code, "update.retired-content-preserved", CliSemanticStatus.Attention),
            UpdateFindingCode.RetirementIneligible => Definition(code, "update.retirement-ineligible", CliSemanticStatus.Blocked),
            UpdateFindingCode.ProjectionUnavailable => Definition(code, "update.projection-unavailable", CliSemanticStatus.Incomplete),
            UpdateFindingCode.GeneratedRegionUnsafe => Definition(code, "update.generated-region-unsafe", CliSemanticStatus.Blocked),
            UpdateFindingCode.PlanBlocked => Definition(code, "update.plan-blocked", CliSemanticStatus.Blocked),
            UpdateFindingCode.RecoveryConflict => Definition(code, "update.recovery-conflict", CliSemanticStatus.Blocked),
            UpdateFindingCode.RecoveryUnavailable => Definition(code, "update.recovery-unavailable", CliSemanticStatus.Incomplete),
            UpdateFindingCode.RecoveryArtifactRetained => Definition(code, "update.recovery-artifact-retained", CliSemanticStatus.Attention),
            UpdateFindingCode.WriteFailed => Definition(code, "update.write-failed", CliSemanticStatus.Failed),
            UpdateFindingCode.VerificationFailed => Definition(code, "update.verification-failed", CliSemanticStatus.Failed),
            UpdateFindingCode.LifecyclePublicationFailed => Definition(code, "update.lifecycle-publication-failed", CliSemanticStatus.Failed),
            UpdateFindingCode.RecoveryFailed => Definition(code, "update.recovery-failed", CliSemanticStatus.Failed),
            UpdateFindingCode.OperationFailed => Definition(code, "update.operation-failed", CliSemanticStatus.Failed),
            UpdateFindingCode.Interrupted => Definition(code, "update.interrupted", CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Update finding code is not defined."),
        };
    }

    internal static string ReadMachineName(UpdateFindingCode code) => Read(code).MachineName;

    internal static CliSemanticStatus ReadStatus(UpdateFindingCode code) => Read(code).Status;

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<UpdateFinding> findings,
        UpdateResultFormation formation)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return status switch
        {
            CliSemanticStatus.Complete when findings.Any(finding =>
                finding.Code == UpdateFindingCode.OwnershipObservation) => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the ownership record before relying on Update."),
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid when findings.Any(finding =>
                finding.Code == UpdateFindingCode.ConfirmationRequired) => new CliNextAction(
                BuildCommand(formation.Force, formation.Prune, automatic: true, UpdateMode.Apply),
                "Rerun the same Update request with explicit automatic mode."),
            CliSemanticStatus.Invalid => new CliNextAction(
                UpdateDefinitions.HelpCommand,
                "Correct the named Update input, then rerun the request."),
            CliSemanticStatus.Blocked => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the blocked workspace, lifecycle, ownership, projection, or safety boundary before rerunning Update."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the unavailable workspace, source, lifecycle, projection, or recovery facts before relying on Update."),
            CliSemanticStatus.Attention when findings.Any(finding =>
                finding.Code == UpdateFindingCode.RecoveryArtifactRetained) => new CliNextAction(
                CommandLines.Cleanup,
                "Review and remove the reported recovery artifact after confirming the verified Update result."),
            CliSemanticStatus.Attention when findings.Any(finding =>
                finding.Code == UpdateFindingCode.RetiredContentPreserved) => new CliNextAction(
                BuildCommand(force: false, prune: true, automatic: false, mode: UpdateMode.DryRun),
                "Preview deleting the retained files before applying the prune."),
            CliSemanticStatus.Attention => new CliNextAction(
                BuildCommand(
                    formation.Force,
                    formation.Prune,
                    formation.Automatic,
                    formation.Mode),
                "Review the preserved Update divergence, then rerun with the named explicit authority."),
            CliSemanticStatus.Failed => new CliNextAction(
                BuildCommand(formation.Force, formation.Prune, formation.Automatic, formation.Mode, verbose: true),
                "Report the failure and retry the same Update request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                BuildCommand(formation.Force, formation.Prune, formation.Automatic, formation.Mode),
                "Rerun the same Update request."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Update status is not defined."),
        };
    }

    private static string BuildCommand(
        bool force,
        bool prune,
        bool automatic,
        UpdateMode mode,
        bool verbose = false)
    {
        var flags = new List<string>();
        if (force)
        {
            flags.Add(UpdateDefinitions.Force.Name);
        }
        if (prune)
        {
            flags.Add(UpdateDefinitions.Prune.Name);
        }
        if (automatic)
        {
            flags.Add(UpdateDefinitions.Automatic.Name);
        }
        if (mode == UpdateMode.DryRun)
        {
            flags.Add(UpdateDefinitions.DryRun.Name);
        }
        if (verbose)
        {
            flags.Add("--detail debug");
        }

        if (flags.Count == 0)
        {
            return $"open-forge {UpdateDefinitions.CommandIdentity}";
        }

        return $"open-forge {UpdateDefinitions.CommandIdentity} {string.Join(' ', flags)}";
    }

    private static UpdateFindingDefinition Definition(
        UpdateFindingCode code,
        string machineName,
        CliSemanticStatus status)
        => new(code, machineName, status);

    private static string Undefined(string name, object value)
        => throw new ArgumentOutOfRangeException(name, value, "The Update value is not defined.");
}

internal enum UpdateFindingCode
{
    InvalidInput,
    ConfirmationRequired,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    PayloadUnavailable,
    PayloadInvalid,
    LifecycleMissing,
    LifecycleUnavailable,
    LifecycleBlocked,
    OwnershipConflict,
    OwnershipObservation,
    TargetUnavailable,
    TargetUnsafe,
    SourceProvenanceInvalid,
    FingerprintUnsupported,
    RetiredContentPreserved,
    RetirementIneligible,
    ProjectionUnavailable,
    GeneratedRegionUnsafe,
    PlanBlocked,
    RecoveryConflict,
    RecoveryUnavailable,
    RecoveryArtifactRetained,
    WriteFailed,
    VerificationFailed,
    LifecyclePublicationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}

internal sealed record UpdateFindingDefinition(
    UpdateFindingCode Code,
    string MachineName,
    CliSemanticStatus Status);
