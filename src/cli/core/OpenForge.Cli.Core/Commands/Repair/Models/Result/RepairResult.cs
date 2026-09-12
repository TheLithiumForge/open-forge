using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairResultFacts
{
    internal RepairLibraryExecution? LibraryExecution { get; init; }

    internal required RepairDiagnosisCoverage Diagnosis { get; init; }

    internal required RepairSelection? Selection { get; init; }

    internal required RepairPlan? Plan { get; init; }

    internal required IReadOnlyList<RepairFinding> Findings { get; init; }

    internal required IReadOnlyList<string> AffectedPaths { get; init; }

    internal required RepairCounts Counts { get; init; }

    internal required RepairPreflight Preflight { get; init; }

    internal required RepairApplication Application { get; init; }

    internal required RepairVerification Verification { get; init; }

    internal required RepairRecovery Recovery { get; init; }

    internal required RepairPostDiagnosis PostDiagnosis { get; init; }
}

internal sealed record RepairResultFormation
{
    internal required CliWorkspace? Workspace { get; init; }

    internal required RepairMode Mode { get; init; }

    internal required bool Automatic { get; init; }

    internal required IReadOnlyList<RepairRelinkRequest> Relinks { get; init; }

    internal required RepairSelectionMode SelectionMode { get; init; }

    internal required RepairResultFacts Facts { get; init; }
}

internal sealed record RepairResult : ICliCommandResult
{
    internal RepairResult(RepairResultFormation formation)
        : this(formation, formation?.Facts?.Findings ?? throw new ArgumentNullException(nameof(formation)))
    {
    }

    internal RepairResult(
        RepairResultFormation formation,
        IEnumerable<RepairFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(formation);
        ArgumentNullException.ThrowIfNull(formation.Facts);
        ArgumentNullException.ThrowIfNull(formation.Facts.Diagnosis);
        ArgumentNullException.ThrowIfNull(formation.Facts.Counts);
        ArgumentNullException.ThrowIfNull(formation.Facts.Preflight);
        ArgumentNullException.ThrowIfNull(formation.Facts.Application);
        ArgumentNullException.ThrowIfNull(formation.Facts.Verification);
        ArgumentNullException.ThrowIfNull(formation.Facts.Recovery);
        ArgumentNullException.ThrowIfNull(formation.Facts.PostDiagnosis);
        ArgumentNullException.ThrowIfNull(formation.Facts.Findings);
        ArgumentNullException.ThrowIfNull(formation.Facts.AffectedPaths);
        ArgumentNullException.ThrowIfNull(formation.Relinks);
        ArgumentNullException.ThrowIfNull(findings);
        if (!Enum.IsDefined(formation.Mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(formation),
                formation.Mode,
                "The Repair mode is not defined.");
        }

        if (!Enum.IsDefined(formation.SelectionMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(formation),
                formation.SelectionMode,
                "The Repair selection mode is not defined.");
        }

        Workspace = formation.Workspace;
        Mode = formation.Mode;
        Automatic = formation.Automatic;
        Relinks = new ReadOnlyCollection<RepairRelinkRequest>([.. formation.Relinks
            .Select(value => value ?? throw new ArgumentException(
                "Repair result relinks cannot contain null members.",
                nameof(formation)))]);
        SelectionMode = formation.SelectionMode;
        LibraryExecution = formation.Facts.LibraryExecution;
        Diagnosis = formation.Facts.Diagnosis;
        Selection = formation.Facts.Selection;
        Plan = formation.Facts.Plan;
        Findings = new ReadOnlyCollection<RepairFinding>([.. findings
            .Select(value => value ?? throw new ArgumentException(
                "Repair findings cannot contain null members.",
                nameof(findings)))
            .OrderBy(value => value.Code)
            .ThenBy(value => value.SourceCanonicalPath, StringComparer.Ordinal)
            .ThenBy(value => value.Occurrence?.Line)
            .ThenBy(value => value.Occurrence?.Column)
            .ThenBy(value => value.Cause, StringComparer.Ordinal)]);
        AffectedPaths = new ReadOnlyCollection<string>([.. formation.Facts.AffectedPaths
            .Select(value => value ?? throw new ArgumentException(
                "Repair affected paths cannot contain null members.",
                nameof(formation)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)]);
        Counts = formation.Facts.Counts;
        Preflight = formation.Facts.Preflight;
        Application = formation.Facts.Application;
        Verification = formation.Facts.Verification;
        Recovery = formation.Facts.Recovery;
        PostDiagnosis = formation.Facts.PostDiagnosis;
        Status = ReadStatus(
            Mode,
            Findings,
            PostDiagnosis.Findings,
            formation.Facts);
        Next = RepairDefinitions.ReadNextAction(
            Status,
            [.. Findings, .. PostDiagnosis.Findings],
            Recovery.Residual == RepairResidualState.Retained);
    }

    public string Command => RepairDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal RepairMode Mode { get; }

    internal bool Automatic { get; }

    internal IReadOnlyList<RepairRelinkRequest> Relinks { get; }

    internal RepairSelectionMode SelectionMode { get; }

    internal RepairLibraryExecution? LibraryExecution { get; }

    internal RepairDiagnosisCoverage Diagnosis { get; }

    internal RepairSelection? Selection { get; }

    internal RepairPlan? Plan { get; }

    internal IReadOnlyList<RepairFinding> Findings { get; }

    internal IReadOnlyList<string> AffectedPaths { get; }

    internal RepairCounts Counts { get; }

    internal RepairPreflight Preflight { get; }

    internal RepairApplication Application { get; }

    internal RepairVerification Verification { get; }

    internal RepairRecovery Recovery { get; }

    internal RepairPostDiagnosis PostDiagnosis { get; }

    internal static RepairResult Empty(
        CliWorkspace? workspace,
        RepairMode mode,
        bool automatic,
        RepairSelectionMode selectionMode,
        params RepairFinding[] findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        if (findings.Length == 0)
        {
            throw new ArgumentException(
                "An empty Repair result requires an explicit typed finding.",
                nameof(findings));
        }

        return new RepairResult(
            new RepairResultFormation
            {
                Workspace = workspace,
                Mode = mode,
                Automatic = automatic,
                Relinks = [],
                SelectionMode = selectionMode,
                Facts = new RepairResultFacts
                {
                    Diagnosis = new RepairDiagnosisCoverage(
                        RepairCoverageState.NotRequested,
                        RepairCoverageState.NotRequested,
                        RepairCoverageState.NotRequested,
                        RepairCoverageState.NotRequested),
                    Selection = null,
                    Plan = null,
                    Findings = findings,
                    AffectedPaths = [],
                    Counts = RepairCounts.Empty,
                    Preflight = RepairPreflight.NotRequested,
                    Application = RepairApplication.NotRequested,
                    Verification = RepairVerification.NotRequested,
                    Recovery = RepairRecovery.NotRequired,
                    PostDiagnosis = RepairPostDiagnosis.NotRequested,
                },
            });
    }

    private static CliSemanticStatus ReadStatus(
        RepairMode mode,
        IReadOnlyList<RepairFinding> findings,
        IReadOnlyList<RepairFinding> postDiagnosisFindings,
        RepairResultFacts facts)
    {
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(postDiagnosisFindings);
        ArgumentNullException.ThrowIfNull(facts);
        CliSemanticStatus[] precedence =
        [
            CliSemanticStatus.Failed,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Interrupted,
            CliSemanticStatus.Attention,
        ];
        return precedence.FirstOrDefault(
            status => findings.Any(finding => finding.Status == status)
                || postDiagnosisFindings.Any(finding => finding.Status == status)
                || HasTypedStatus(status, mode, facts),
            CliSemanticStatus.Complete);
    }

    private static bool HasTypedStatus(
        CliSemanticStatus status,
        RepairMode mode,
        RepairResultFacts facts)
    {
        var hasDiagnosisScope = facts.Selection is not null || facts.Plan is not null;
        var hasEffects = facts.Plan is { } plan
            && (plan.Effects.Count > 0 || plan.LibrarySteps.Any(step => step.Effect is not null));
        var libraryOnly = facts.Plan is { Effects.Count: 0, LibrarySteps.IsEmpty: false };
        var applyingSelectedEffects = mode == RepairMode.Apply && hasEffects;
        var applyingReferenceEffects = mode == RepairMode.Apply && facts.Plan?.Effects.Count > 0;
        var applyingSelectedScope = mode == RepairMode.Apply && hasDiagnosisScope;
        var interrupted = facts.Application.State == RepairApplicationState.Interrupted
            || HasStepOutcome(facts.Plan, RepairStepOutcome.Interrupted);

        return status switch
        {
            CliSemanticStatus.Failed => facts.Application.State is RepairApplicationState.Failed
                    or RepairApplicationState.Unknown
                || facts.LibraryExecution?.UnexpectedFailure is not null
                || HasVerificationFailure(facts.Verification)
                || facts.Recovery.State == RepairRecoveryState.Unknown
                || facts.PostDiagnosis.State == RepairPostDiagnosisState.Failed
                || HasStepOutcome(facts.Plan, RepairStepOutcome.Failed)
                || (facts.Application.State == RepairApplicationState.Interrupted
                    || HasStepOutcome(facts.Plan, RepairStepOutcome.Interrupted))
                    && facts.Recovery.State == RepairRecoveryState.Incomplete,
            CliSemanticStatus.Interrupted => facts.Application.State == RepairApplicationState.Interrupted
                || facts.LibraryExecution?.Cancellation is not null
                || HasStepOutcome(facts.Plan, RepairStepOutcome.Interrupted),
            CliSemanticStatus.Invalid => false,
            CliSemanticStatus.Blocked => facts.Plan?.IsBlocked == true
                || facts.Preflight.State == RepairPreflightState.Blocked
                || facts.Preflight.Recovery == RepairRecoveryState.Blocked
                || facts.Recovery.State == RepairRecoveryState.Blocked
                || facts.PostDiagnosis.State == RepairPostDiagnosisState.Blocked
                || HasRequiredCoverage(
                    facts.Diagnosis,
                    hasDiagnosisScope,
                    RepairCoverageState.Blocked,
                    includeReferenceDomains: !libraryOnly)
                || HasRequiredCoverage(
                    facts.PostDiagnosis.Coverage,
                    applyingSelectedScope,
                    RepairCoverageState.Blocked,
                    includeReferenceDomains: !libraryOnly)
                || applyingReferenceEffects && !interrupted
                    && facts.Preflight.Recovery == RepairRecoveryState.NotRequired
                || applyingReferenceEffects && !interrupted
                    && facts.Recovery.State == RepairRecoveryState.NotRequired,
            CliSemanticStatus.Incomplete => facts.Preflight.State == RepairPreflightState.Incomplete
                || facts.Preflight.Recovery is RepairRecoveryState.Incomplete
                    or RepairRecoveryState.Unknown
                || facts.Recovery.State == RepairRecoveryState.Incomplete
                || facts.PostDiagnosis.State == RepairPostDiagnosisState.Incomplete
                || HasIncompleteRequiredCoverage(facts.Diagnosis, hasDiagnosisScope, includeReferenceDomains: !libraryOnly)
                || HasIncompleteRequiredCoverage(
                    facts.PostDiagnosis.Coverage,
                    applyingSelectedScope && !interrupted,
                    includeReferenceDomains: !libraryOnly)
                || facts.Selection is not null && facts.Plan is null
                || HasStepPendingApplication(facts.Plan, applyingSelectedEffects && !interrupted, facts.Application)
                || HasPendingVerification(facts.Verification, applyingSelectedEffects && !interrupted)
                || hasEffects && !interrupted
                    && facts.Preflight.State == RepairPreflightState.NotRequested
                || applyingSelectedEffects && !interrupted
                    && facts.Preflight.Recovery == RepairRecoveryState.NotCreated
                || applyingSelectedEffects && !interrupted
                    && facts.Recovery.State == RepairRecoveryState.NotCreated
                || applyingSelectedScope && !interrupted
                    && facts.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested,
            CliSemanticStatus.Attention => facts.Recovery.Residual == RepairResidualState.Retained,
            CliSemanticStatus.Complete => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Repair status is not defined."),
        };
    }

    private static bool HasVerificationFailure(RepairVerification verification)
        => verification.Targets is RepairVerificationState.Failed or RepairVerificationState.Unknown
            || verification.ResultingBytes is RepairVerificationState.Failed or RepairVerificationState.Unknown
            || verification.PostConditions is RepairVerificationState.Failed or RepairVerificationState.Unknown;

    private static bool HasStepOutcome(RepairPlan? plan, RepairStepOutcome outcome)
        => plan?.Steps.Any(step => step.Outcome == outcome) == true
            || plan?.LibrarySteps.Any(step => step.Outcome == outcome) == true;

    private static bool HasStepPendingApplication(
        RepairPlan? plan,
        bool required,
        RepairApplication application)
        => required
            && (application.State is RepairApplicationState.NotRequested
                or RepairApplicationState.Confirmed
                or RepairApplicationState.NotStarted
                || HasStepOutcome(plan, RepairStepOutcome.Planned));

    private static bool HasPendingVerification(
        RepairVerification verification,
        bool required)
        => required
            && (verification.Targets is RepairVerificationState.NotRequested or RepairVerificationState.Planned
                || verification.ResultingBytes is
                    RepairVerificationState.NotRequested or RepairVerificationState.Planned
                || verification.PostConditions is
                    RepairVerificationState.NotRequested or RepairVerificationState.Planned);

    private static bool HasRequiredCoverage(
        RepairDiagnosisCoverage coverage,
        bool required,
        RepairCoverageState state,
        bool includeReferenceDomains)
        => required
            && (coverage.WorkspaceAndPath == state
                || includeReferenceDomains && coverage.RouteAndHeading == state
                || includeReferenceDomains && coverage.LocalReferences == state
                || coverage.SelectedScope == state);

    private static bool HasIncompleteRequiredCoverage(
        RepairDiagnosisCoverage coverage,
        bool required,
        bool includeReferenceDomains)
        => required
            && (IsIncomplete(coverage.WorkspaceAndPath)
                || includeReferenceDomains && IsIncomplete(coverage.RouteAndHeading)
                || includeReferenceDomains && IsIncomplete(coverage.LocalReferences)
                || IsIncomplete(coverage.SelectedScope));

    private static bool IsIncomplete(RepairCoverageState state)
        => state is RepairCoverageState.NotRequested or RepairCoverageState.Incomplete;
}
