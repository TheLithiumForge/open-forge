using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairDefinitions
{
    internal const int SchemaVersion = 1;

    internal const string CommandIdentity = "repair";

    internal static readonly CliSyntaxDefinition RepairCommand = new(
        CommandIdentity,
        "Repair local references and Library state.");

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        "--automatic",
        "Select all currently verified safe repairs without prompting.",
        CliOptionArity.None,
        false);

    internal static readonly RepairRelinkDefinition Relink = new(
        "--relink",
        "Select one exact source occurrence and contained target.",
        "source expected target");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete repair without writing files.",
        CliOptionArity.None,
        false);

    internal static string ReadMachineName(RepairMode mode)
        => mode switch
        {
            RepairMode.Apply => "apply",
            RepairMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RepairSelectionMode mode)
        => mode switch
        {
            RepairSelectionMode.InteractiveWizard => "interactive-wizard",
            RepairSelectionMode.Automatic => "automatic",
            RepairSelectionMode.ExplicitRelinks => "explicit-relinks",
            RepairSelectionMode.AutomaticAndExplicit => "automatic-and-explicit",
            RepairSelectionMode.NonInteractiveBlocked => "non-interactive-blocked",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RepairCatalogueMember member)
        => member switch
        {
            RepairCatalogueMember.SameTargetPath => "same-target-path",
            RepairCatalogueMember.SameTargetCase => "same-target-case",
            RepairCatalogueMember.SameTargetEncoding => "same-target-encoding",
            RepairCatalogueMember.UniqueCanonicalFragment => "unique-canonical-fragment",
            RepairCatalogueMember.MissingTargetRelink => "missing-target-relink",
            _ => Undefined(nameof(member), member),
        };

    internal static string ReadMachineName(RepairSelectionOrigin origin)
        => origin switch
        {
            RepairSelectionOrigin.Automatic => "automatic",
            RepairSelectionOrigin.Wizard => "wizard",
            RepairSelectionOrigin.ExplicitRelink => "explicit-relink",
            _ => Undefined(nameof(origin), origin),
        };

    internal static string ReadMachineName(RepairCandidateEvidenceKind kind)
        => kind switch
        {
            RepairCandidateEvidenceKind.Filename => "filename",
            RepairCandidateEvidenceKind.Title => "title",
            RepairCandidateEvidenceKind.LiteralContent => "literal-content",
            RepairCandidateEvidenceKind.RouteNeighborhood => "route-neighborhood",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RepairCandidateCardinality cardinality)
        => cardinality switch
        {
            RepairCandidateCardinality.None => "none",
            RepairCandidateCardinality.One => "one",
            RepairCandidateCardinality.Several => "several",
            _ => Undefined(nameof(cardinality), cardinality),
        };

    internal static string ReadMachineName(RepairDependencyDomain domain)
        => domain switch
        {
            RepairDependencyDomain.WorkspaceContainment => "workspace-containment",
            RepairDependencyDomain.RouteAndHeading => "route-and-heading",
            RepairDependencyDomain.LocalReference => "local-reference",
            RepairDependencyDomain.LibraryRecord => "library-record",
            RepairDependencyDomain.LibraryResidual => "library-residual",
            _ => Undefined(nameof(domain), domain),
        };

    internal static string ReadMachineName(RepairVerificationKind kind)
        => kind switch
        {
            RepairVerificationKind.DestinationLiteral => "destination-literal",
            RepairVerificationKind.SameTargetIdentity => "same-target-identity",
            RepairVerificationKind.ResultingBytes => "resulting-bytes",
            RepairVerificationKind.NoFollowIdentity => "no-follow-identity",
            RepairVerificationKind.PriorState => "prior-state",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RepairRecoveryRequirementKind kind)
        => kind switch
        {
            RepairRecoveryRequirementKind.Required => "required",
            RepairRecoveryRequirementKind.NotRequired => "not-required",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RepairStepOutcome outcome)
        => outcome switch
        {
            RepairStepOutcome.Planned => "planned",
            RepairStepOutcome.NoOp => "no-op",
            RepairStepOutcome.Applied => "applied",
            RepairStepOutcome.Verified => "verified",
            RepairStepOutcome.Blocked => "blocked",
            RepairStepOutcome.Failed => "failed",
            RepairStepOutcome.Interrupted => "interrupted",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(RepairConflictKind kind)
        => kind switch
        {
            RepairConflictKind.OverlappingChanges => "overlapping-changes",
            RepairConflictKind.ExpectedStateMismatch => "expected-state-mismatch",
            RepairConflictKind.TargetIdentityMismatch => "target-identity-mismatch",
            RepairConflictKind.UnsafeBoundary => "unsafe-boundary",
            RepairConflictKind.MissingAuthority => "missing-authority",
            RepairConflictKind.IncompleteFacts => "incomplete-facts",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RepairCoverageState state)
        => state switch
        {
            RepairCoverageState.NotRequested => "not-requested",
            RepairCoverageState.Complete => "complete",
            RepairCoverageState.Incomplete => "incomplete",
            RepairCoverageState.Blocked => "blocked",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairPreflightState state)
        => state switch
        {
            RepairPreflightState.NotRequested => "not-requested",
            RepairPreflightState.Ready => "ready",
            RepairPreflightState.Incomplete => "incomplete",
            RepairPreflightState.Blocked => "blocked",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairApplicationState state)
        => state switch
        {
            RepairApplicationState.NotRequested => "not-requested",
            RepairApplicationState.Confirmed => "confirmed",
            RepairApplicationState.NotStarted => "not-started",
            RepairApplicationState.Applied => "applied",
            RepairApplicationState.Failed => "failed",
            RepairApplicationState.Interrupted => "interrupted",
            RepairApplicationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairVerificationState state)
        => state switch
        {
            RepairVerificationState.NotRequested => "not-requested",
            RepairVerificationState.Planned => "planned",
            RepairVerificationState.Verified => "verified",
            RepairVerificationState.Failed => "failed",
            RepairVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairRecoveryState state)
        => state switch
        {
            RepairRecoveryState.NotRequired => "not-required",
            RepairRecoveryState.NotCreated => "not-created",
            RepairRecoveryState.Prepared => "prepared",
            RepairRecoveryState.Removed => "removed",
            RepairRecoveryState.Retained => "retained",
            RepairRecoveryState.Incomplete => "incomplete",
            RepairRecoveryState.Blocked => "blocked",
            RepairRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairResidualState state)
        => state switch
        {
            RepairResidualState.None => "none",
            RepairResidualState.Retained => "retained",
            RepairResidualState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RepairPostDiagnosisState state)
        => state switch
        {
            RepairPostDiagnosisState.NotRequested => "not-requested",
            RepairPostDiagnosisState.Complete => "complete",
            RepairPostDiagnosisState.Incomplete => "incomplete",
            RepairPostDiagnosisState.Blocked => "blocked",
            RepairPostDiagnosisState.Failed => "failed",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(FileExpectationKind kind)
        => kind switch
        {
            FileExpectationKind.Missing => "missing",
            FileExpectationKind.File => "file",
            FileExpectationKind.Directory => "directory",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(PlannedFileChangeKind kind)
        => kind switch
        {
            PlannedFileChangeKind.Create => "create",
            PlannedFileChangeKind.Replace => "replace",
            PlannedFileChangeKind.Delete => "delete",
            PlannedFileChangeKind.ReplaceGeneratedRegion => "replace-generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RepairFindingCode code)
        => Read(code).MachineName;

    internal static RepairFindingDefinition Read(RepairFindingCode code)
        => code switch
        {
            RepairFindingCode.InvalidInput => Definition(
                code,
                "repair.invalid-input",
                CliSemanticStatus.Invalid),
            RepairFindingCode.RelinkInvalid => Definition(
                code,
                "repair.relink-invalid",
                CliSemanticStatus.Invalid),
            RepairFindingCode.ContradictoryRelink => Definition(
                code,
                "repair.contradictory-relink",
                CliSemanticStatus.Invalid),
            RepairFindingCode.SelectionRequired => Definition(
                code,
                "repair.selection-required",
                CliSemanticStatus.Blocked),
            RepairFindingCode.DiagnosisIncomplete => Definition(
                code,
                "repair.diagnosis-incomplete",
                CliSemanticStatus.Incomplete),
            RepairFindingCode.DiagnosisBlocked => Definition(
                code,
                "repair.diagnosis-blocked",
                CliSemanticStatus.Blocked),
            RepairFindingCode.ProposalUnavailable => Definition(
                code,
                "repair.proposal-unavailable",
                CliSemanticStatus.Incomplete),
            RepairFindingCode.ProposalUnsupported => Definition(
                code,
                "repair.proposal-unsupported",
                CliSemanticStatus.Blocked),
            RepairFindingCode.MissingAuthority => Definition(
                code,
                "repair.missing-authority",
                CliSemanticStatus.Blocked),
            RepairFindingCode.TargetChanged => Definition(
                code,
                "repair.target-changed",
                CliSemanticStatus.Blocked),
            RepairFindingCode.TargetUnsafe => Definition(
                code,
                "repair.target-unsafe",
                CliSemanticStatus.Blocked),
            RepairFindingCode.PlanConflict => Definition(
                code,
                "repair.plan-conflict",
                CliSemanticStatus.Blocked),
            RepairFindingCode.WorkspaceLockUnavailable => Definition(
                code,
                "repair.workspace-lock-unavailable",
                CliSemanticStatus.Blocked),
            RepairFindingCode.RecoveryUnavailable => Definition(
                code,
                "repair.recovery-unavailable",
                CliSemanticStatus.Incomplete),
            RepairFindingCode.RecoveryConflict => Definition(
                code,
                "repair.recovery-conflict",
                CliSemanticStatus.Blocked),
            RepairFindingCode.GuidedFindingRemaining => Definition(
                code,
                "repair.guided-finding-remaining",
                CliSemanticStatus.Attention),
            RepairFindingCode.ManualFindingRemaining => Definition(
                code,
                "repair.manual-finding-remaining",
                CliSemanticStatus.Attention),
            RepairFindingCode.RecoveryArtifactRetained => Definition(
                code,
                "repair.recovery-artifact-retained",
                CliSemanticStatus.Attention),
            RepairFindingCode.WriteFailed => Definition(
                code,
                "repair.write-failed",
                CliSemanticStatus.Failed),
            RepairFindingCode.VerificationFailed => Definition(
                code,
                "repair.verification-failed",
                CliSemanticStatus.Failed),
            RepairFindingCode.RecoveryFailed => Definition(
                code,
                "repair.recovery-failed",
                CliSemanticStatus.Failed),
            RepairFindingCode.OperationFailed => Definition(
                code,
                "repair.operation-failed",
                CliSemanticStatus.Failed),
            RepairFindingCode.Interrupted => Definition(
                code,
                "repair.interrupted",
                CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Repair finding code is not defined."),
        };

    internal static CliSemanticStatus ReadStatus(RepairFindingCode code)
        => Read(code).Status;

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<RepairFinding> findings,
        bool recoveryArtifactRetained = false)
    {
        ArgumentNullException.ThrowIfNull(findings);
        if (status == CliSemanticStatus.Complete)
        {
            return null;
        }

        if (status == CliSemanticStatus.Blocked
            && findings.Any(finding => finding.Code == RepairFindingCode.SelectionRequired))
        {
            return new CliNextAction(
                "open-forge repair --automatic",
                "Select safe-exact repairs with --automatic, provide an explicit --relink target, or use the interactive Repair wizard.");
        }

        if (status == CliSemanticStatus.Attention
            && (recoveryArtifactRetained
                || findings.Any(finding => finding.Code == RepairFindingCode.RecoveryArtifactRetained)))
        {
            return new CliNextAction(
                "open-forge cleanup",
                "Remove the retained Repair recovery artifact with the separate cleanup operation after reviewing its exact path.");
        }

        return status switch
        {
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge repair --help",
                "Correct the named Repair input, then rerun the request."),
            CliSemanticStatus.Blocked => new CliNextAction(
                "open-forge doctor",
                "Inspect the blocked diagnosis, selection, target, plan, or recovery boundary before retrying Repair."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                "open-forge doctor",
                "Inspect unavailable Repair diagnosis or recovery facts before relying on this result."),
            CliSemanticStatus.Attention => new CliNextAction(
                "open-forge repair",
                "Review remaining guided or manual findings in Repair, then select a candidate in the interactive wizard or provide an explicit --relink target."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge repair --verbose",
                "Inspect the bounded failure details and rerun Repair from a fresh plan."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge repair",
                "Rerun the same Repair request from a fresh diagnosis."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Repair status is not defined."),
        };
    }

    internal static void ValidateRepairRecoveryAttribution(
        RecoveryBundleAttribution attribution,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(attribution, parameterName);
        if (attribution.Producer != RecoveryBundleProducer.Repair
            || attribution.Operation != RecoveryBundleOperation.Repair
            || attribution.Subject.Kind != RecoveryBundleSubjectKind.Workspace)
        {
            throw new ArgumentException(
                "Repair recovery attribution must be the schema-v1 Repair/Repair workspace tuple.",
                parameterName);
        }
    }

    private static RepairFindingDefinition Definition(
        RepairFindingCode code,
        string machineName,
        CliSemanticStatus status)
        => new(code, machineName, status);

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(name, value, $"The Repair {name} value is not defined.");
}

internal enum RepairFindingCode
{
    InvalidInput,
    RelinkInvalid,
    ContradictoryRelink,
    SelectionRequired,
    DiagnosisIncomplete,
    DiagnosisBlocked,
    ProposalUnavailable,
    ProposalUnsupported,
    MissingAuthority,
    TargetChanged,
    TargetUnsafe,
    PlanConflict,
    WorkspaceLockUnavailable,
    RecoveryUnavailable,
    RecoveryConflict,
    GuidedFindingRemaining,
    ManualFindingRemaining,
    RecoveryArtifactRetained,
    WriteFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}

internal sealed record RepairFindingDefinition(
    RepairFindingCode Code,
    string MachineName,
    CliSemanticStatus Status);

internal sealed record RepairRelinkDefinition
{
    internal RepairRelinkDefinition(
        string name,
        string description,
        string valueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(valueName);
        Name = name;
        Description = description;
        ValueName = valueName;
    }

    internal string Name { get; }

    internal string Description { get; }

    internal string ValueName { get; }

    internal int MinimumArguments { get; } = 3;

    internal int MaximumArguments { get; } = 3;

    internal bool AllowMultipleArgumentsPerToken { get; } = true;
}
