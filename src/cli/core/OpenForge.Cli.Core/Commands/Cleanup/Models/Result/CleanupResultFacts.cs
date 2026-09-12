using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Result;

internal sealed record CleanupWorkspaceAssociation
{
    private CleanupWorkspaceAssociation(
        CleanupWorkspaceAssociationState state,
        string? selectedPhysicalPath,
        string? candidatePhysicalPath,
        string? selectedWorkspaceKey,
        string? candidateWorkspaceKey,
        string? cause)
    {
        ValidateEnum(state, nameof(state), "workspace association state");
        ValidatePair(selectedPhysicalPath, selectedWorkspaceKey, "selected workspace");
        ValidatePair(candidatePhysicalPath, candidateWorkspaceKey, "candidate workspace");
        ValidateWorkspaceIdentity(selectedPhysicalPath, selectedWorkspaceKey, "selected workspace");
        ValidateWorkspaceIdentity(candidatePhysicalPath, candidateWorkspaceKey, "candidate workspace");
        ValidateCause(cause);

        switch (state)
        {
            case CleanupWorkspaceAssociationState.NotEstablished:
                RequireEmpty(
                    selectedPhysicalPath,
                    candidatePhysicalPath,
                    selectedWorkspaceKey,
                    candidateWorkspaceKey,
                    cause,
                    "A not-established workspace association cannot contain facts.");
                break;
            case CleanupWorkspaceAssociationState.CurrentWorkspace:
                RequirePresent(
                    selectedPhysicalPath,
                    candidatePhysicalPath,
                    selectedWorkspaceKey,
                    candidateWorkspaceKey,
                    "A current workspace association requires both workspace identities.");
                if (!SamePhysicalPath(selectedPhysicalPath, candidatePhysicalPath)
                    || !Same(selectedWorkspaceKey, candidateWorkspaceKey)
                    || cause is not null)
                {
                    throw new ArgumentException(
                        "A current workspace association must agree on path and key without a cause.",
                        nameof(candidateWorkspaceKey));
                }

                break;
            case CleanupWorkspaceAssociationState.Mismatched:
                RequirePresent(
                    selectedPhysicalPath,
                    candidatePhysicalPath,
                    selectedWorkspaceKey,
                    candidateWorkspaceKey,
                    "A mismatched workspace association requires both workspace identities.");
                if (SamePhysicalPath(selectedPhysicalPath, candidatePhysicalPath)
                    && Same(selectedWorkspaceKey, candidateWorkspaceKey))
                {
                    throw new ArgumentException(
                        "A mismatched workspace association must differ by path or key.",
                        nameof(candidateWorkspaceKey));
                }

                break;
            case CleanupWorkspaceAssociationState.Unavailable:
                if (selectedPhysicalPath is null
                    || candidatePhysicalPath is not null
                    || candidateWorkspaceKey is not null)
                {
                    throw new ArgumentException(
                        "An unavailable workspace association may retain only "
                        + "the supplied selected workspace identity.",
                        nameof(candidateWorkspaceKey));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The workspace association state is not defined.");
        }

        State = state;
        SelectedPhysicalPath = selectedPhysicalPath;
        CandidatePhysicalPath = candidatePhysicalPath;
        SelectedWorkspaceKey = selectedWorkspaceKey;
        CandidateWorkspaceKey = candidateWorkspaceKey;
        Cause = cause;
    }

    internal static CleanupWorkspaceAssociation Create(
        CleanupWorkspaceAssociationState state,
        string? selectedPhysicalPath,
        string? candidatePhysicalPath,
        string? selectedWorkspaceKey,
        string? candidateWorkspaceKey,
        string? cause = null)
        => new(
            state,
            selectedPhysicalPath,
            candidatePhysicalPath,
            selectedWorkspaceKey,
            candidateWorkspaceKey,
            cause);

    public CleanupWorkspaceAssociationState State { get; }

    public string? SelectedPhysicalPath { get; }

    public string? CandidatePhysicalPath { get; }

    public string? SelectedWorkspaceKey { get; }

    public string? CandidateWorkspaceKey { get; }

    public string? Cause { get; }

    private static void RequirePresent(
        string? selectedPhysicalPath,
        string? candidatePhysicalPath,
        string? selectedWorkspaceKey,
        string? candidateWorkspaceKey,
        string message)
    {
        if (selectedPhysicalPath is null
            || candidatePhysicalPath is null
            || selectedWorkspaceKey is null
            || candidateWorkspaceKey is null)
        {
            throw new ArgumentException(message);
        }
    }

    private static void RequireEmpty(
        string? selectedPhysicalPath,
        string? candidatePhysicalPath,
        string? selectedWorkspaceKey,
        string? candidateWorkspaceKey,
        string? cause,
        string message)
    {
        if (selectedPhysicalPath is not null
            || candidatePhysicalPath is not null
            || selectedWorkspaceKey is not null
            || candidateWorkspaceKey is not null
            || cause is not null)
        {
            throw new ArgumentException(message);
        }
    }

    private static void ValidatePair(string? path, string? key, string description)
    {
        if ((path is null) != (key is null))
        {
            throw new ArgumentException(
                $"The {description} path and key must both be present or absent.");
        }

        if (path is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
        }
    }

    private static void ValidateCause(string? cause)
    {
        if (cause is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }
    }

    private static bool Same(string? left, string? right)
        => string.Equals(left, right, StringComparison.Ordinal);

    private static bool SamePhysicalPath(string? left, string? right)
        => PhysicalIdentityTracker.PathComparer.Equals(left, right);

    private static void ValidateWorkspaceIdentity(
        string? physicalPath,
        string? workspaceKey,
        string description)
    {
        if (physicalPath is null)
        {
            return;
        }

        var expectedKey = WorkspaceIdentity.Key(physicalPath);
        if (!Same(expectedKey, workspaceKey))
        {
            throw new ArgumentException(
                $"The {description} key must match its physical path identity.",
                nameof(workspaceKey));
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName, string description)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The {description} is not defined.");
        }
    }
}

internal sealed record CleanupLeaseBoundary
{
    private CleanupLeaseBoundary(
        CleanupLeaseBoundaryState state,
        string? workspaceKey,
        string? command,
        Guid? operationId,
        string? cause)
    {
        ValidateEnum(state, nameof(state), "lease boundary state");
        ValidateOptional(workspaceKey, nameof(workspaceKey));
        ValidateOptional(command, nameof(command));
        ValidateOptional(cause, nameof(cause));
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException(
                "A Cleanup lease boundary operation ID cannot be empty.",
                nameof(operationId));
        }

        var absent = workspaceKey is null && command is null && operationId is null;
        switch (state)
        {
            case CleanupLeaseBoundaryState.NotEstablished:
            case CleanupLeaseBoundaryState.NotRequested:
                if (!absent || cause is not null)
                {
                    throw new ArgumentException(
                        "A lease boundary without an established request cannot contain lease facts.",
                        nameof(state));
                }

                break;
            case CleanupLeaseBoundaryState.Required:
            case CleanupLeaseBoundaryState.Held:
                RequireLeaseFacts(workspaceKey, command, operationId, state);
                if (cause is not null)
                {
                    throw new ArgumentException(
                        "A required or held lease boundary cannot carry a cause.",
                        nameof(cause));
                }

                break;
            case CleanupLeaseBoundaryState.Mismatched:
                RequireLeaseFacts(workspaceKey, command, operationId, state);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The lease boundary state is not defined.");
        }

        State = state;
        WorkspaceKey = workspaceKey;
        Command = command;
        OperationId = operationId;
        Cause = cause;
    }

    internal static CleanupLeaseBoundary Create(
        CleanupLeaseBoundaryState state,
        string? workspaceKey,
        string? command,
        Guid? operationId,
        string? cause = null)
        => new(state, workspaceKey, command, operationId, cause);

    public CleanupLeaseBoundaryState State { get; }

    public string? WorkspaceKey { get; }

    public string? Command { get; }

    public Guid? OperationId { get; }

    public string? Cause { get; }

    private static void RequireLeaseFacts(
        string? workspaceKey,
        string? command,
        Guid? operationId,
        CleanupLeaseBoundaryState state)
    {
        if (workspaceKey is null || command is null || operationId is null)
        {
            throw new ArgumentException(
                $"A {state} lease boundary requires a workspace key, command, and operation ID.",
                nameof(state));
        }
    }

    private static void ValidateOptional(string? value, string parameterName)
    {
        if (value is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName, string description)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The {description} is not defined.");
        }
    }
}

internal sealed record CleanupRecoveryProvenance
{
    public required RecoveryBundleAttribution Attribution { get; init; }

    public required string Command { get; init; }

    public required string WorkspacePhysicalPath { get; init; }

    public required string WorkspaceKey { get; init; }

    public required Guid OperationId { get; init; }
}

internal sealed record CleanupVerificationCondition
{
    private CleanupVerificationCondition(
        CleanupVerificationConditionState state,
        string? expectedPath,
        CleanupArtifactFileKind expectedFileKind,
        RecoveryBundleIntegrity? expectedIntegrity,
        string? cause)
    {
        ValidateEnum(state, nameof(state), "verification condition state");
        ValidateEnum(expectedFileKind, nameof(expectedFileKind), "expected file kind");
        if (expectedIntegrity is RecoveryBundleIntegrity integrity)
        {
            ValidateEnum(integrity, nameof(expectedIntegrity), "expected integrity");
        }

        if (cause is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }

        switch (state)
        {
            case CleanupVerificationConditionState.NotEstablished:
            case CleanupVerificationConditionState.NotRequested:
                if (expectedPath is not null
                    || expectedFileKind != CleanupArtifactFileKind.NotEstablished
                    || expectedIntegrity is not null
                    || cause is not null)
                {
                    throw new ArgumentException(
                        "An unestablished Cleanup verification condition cannot contain expectations.",
                        nameof(state));
                }

                break;
            case CleanupVerificationConditionState.ExactPathAndKind:
                RequireExpectedPathAndKind(expectedPath, expectedFileKind, expectedIntegrity);
                if (expectedIntegrity != RecoveryBundleIntegrity.Incomplete
                    && (expectedIntegrity != RecoveryBundleIntegrity.Unavailable
                        || expectedFileKind != CleanupArtifactFileKind.NonOrdinary))
                {
                    throw new ArgumentException(
                        "An exact path-and-kind condition requires an incomplete draft or an unavailable non-ordinary draft.",
                        nameof(expectedIntegrity));
                }

                break;
            case CleanupVerificationConditionState.SemanticFinal:
                RequireExpectedPathAndKind(expectedPath, expectedFileKind, expectedIntegrity);
                if (expectedIntegrity == RecoveryBundleIntegrity.Incomplete)
                {
                    throw new ArgumentException(
                        "A semantic-final condition cannot expect draft integrity.",
                        nameof(expectedIntegrity));
                }

                break;
            case CleanupVerificationConditionState.Absence:
                if (string.IsNullOrWhiteSpace(expectedPath)
                    || expectedFileKind == CleanupArtifactFileKind.NotEstablished
                    || expectedIntegrity is not null)
                {
                    throw new ArgumentException(
                        "An absence condition requires a path and file kind without integrity.",
                        nameof(expectedPath));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The verification condition state is not defined.");
        }

        State = state;
        ExpectedPath = expectedPath;
        ExpectedFileKind = expectedFileKind;
        ExpectedIntegrity = expectedIntegrity;
        Cause = cause;
    }

    internal static CleanupVerificationCondition Create(
        CleanupVerificationConditionState state,
        string? expectedPath,
        CleanupArtifactFileKind expectedFileKind,
        RecoveryBundleIntegrity? expectedIntegrity,
        string? cause = null)
        => new(state, expectedPath, expectedFileKind, expectedIntegrity, cause);

    public CleanupVerificationConditionState State { get; }

    public string? ExpectedPath { get; }

    public CleanupArtifactFileKind ExpectedFileKind { get; }

    public RecoveryBundleIntegrity? ExpectedIntegrity { get; }

    public string? Cause { get; }

    private static void RequireExpectedPathAndKind(
        string? expectedPath,
        CleanupArtifactFileKind expectedFileKind,
        RecoveryBundleIntegrity? expectedIntegrity)
    {
        if (string.IsNullOrWhiteSpace(expectedPath)
            || expectedFileKind == CleanupArtifactFileKind.NotEstablished
            || expectedIntegrity is null)
        {
            throw new ArgumentException(
                "A Cleanup verification condition requires a path, file kind, and integrity.",
                nameof(expectedPath));
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName, string description)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The {description} is not defined.");
        }
    }
}

internal sealed record CleanupEffectCondition
{
    private CleanupEffectCondition(
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual)
    {
        ValidatePlanPair(outcome, residual);

        Outcome = outcome;
        Residual = residual;
    }

    internal static CleanupEffectCondition Create(
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual)
        => new(outcome, residual);

    internal void ValidateFor(CleanupPlanAction action)
    {
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Cleanup plan action is not defined.");
        }

        if ((action, Outcome, Residual) is not
            ((CleanupPlanAction.Delete, CleanupEffectOutcome.Planned, CleanupEffectResidual.None)
                or (CleanupPlanAction.Preserve, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained)))
        {
            throw new ArgumentException(
                "The Cleanup plan action and anticipated effect condition are incoherent.",
                nameof(action));
        }
    }

    public CleanupEffectOutcome Outcome { get; }

    public CleanupEffectResidual Residual { get; }

    private static void ValidatePlanPair(
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Cleanup effect outcome is not defined.");
        }

        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(
                nameof(residual),
                residual,
                "The Cleanup effect residual is not defined.");
        }

        if ((outcome, residual) is not
            ((CleanupEffectOutcome.Planned, CleanupEffectResidual.None)
                or (CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained)))
        {
            throw new ArgumentException(
                "A Cleanup plan effect condition must be planned or preserved without an effect.",
                nameof(residual));
        }
    }
}

internal static class CleanupEffectFactsValidation
{
    internal static void ValidateActual(
        CleanupPlanAction action,
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual)
    {
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Cleanup plan action is not defined.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Cleanup effect outcome is not defined.");
        }

        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(
                nameof(residual),
                residual,
                "The Cleanup effect residual is not defined.");
        }

        var admitted = action switch
        {
            CleanupPlanAction.Delete => (outcome, residual) is
                (CleanupEffectOutcome.Planned, CleanupEffectResidual.None)
                or (CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained)
                or (CleanupEffectOutcome.Verified, CleanupEffectResidual.None)
                or (CleanupEffectOutcome.VerificationFailed,
                    CleanupEffectResidual.Retained or CleanupEffectResidual.Unknown)
                or (CleanupEffectOutcome.CompletionUnknown, CleanupEffectResidual.Unknown),
            CleanupPlanAction.Preserve => (outcome, residual)
                is (CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained),
            _ => false,
        };

        if (!admitted)
        {
            throw new ArgumentException(
                "The Cleanup effect outcome and residual are incoherent.",
                nameof(residual));
        }
    }

    internal static void ValidateCause(string? cause)
    {
        if (cause is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause, nameof(cause));
        }
    }
}

internal sealed record CleanupPreflight
{
    public required CleanupPreflightState State { get; init; }

    public string? Cause { get; init; }
}

internal sealed record CleanupLease
{
    public required CleanupLeaseState State { get; init; }

    public string? Cause { get; init; }
}

internal sealed record CleanupCatalogueComparison
{
    public required CleanupCatalogueComparisonState State { get; init; }

    public CleanupCatalogue? Planned { get; init; }

    public CleanupCatalogue? Observed { get; init; }

    public string? Cause { get; init; }
}

internal sealed record CleanupEffect
{
    private CleanupEffect(
        CleanupPlanEntry planEntry,
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(planEntry);
        CleanupEffectFactsValidation.ValidateActual(planEntry.Action, outcome, residual);
        CleanupEffectFactsValidation.ValidateCause(cause);

        PlanEntry = planEntry;
        Outcome = outcome;
        Residual = residual;
        Cause = cause;
    }

    internal static CleanupEffect Create(
        CleanupPlanEntry planEntry,
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual,
        string? cause = null)
        => new(planEntry, outcome, residual, cause);

    public CleanupPlanEntry PlanEntry { get; }

    public string Path => PlanEntry.Path;

    public RecoveryBundleCandidateKind Kind => PlanEntry.Kind;

    public RecoveryBundleIntegrity Integrity => PlanEntry.Integrity;

    public CleanupArtifactFileKind FileKind => PlanEntry.FileKind;

    public CleanupPlanAction Action => PlanEntry.Action;

    public CleanupEffectOutcome Outcome { get; }

    public CleanupEffectResidual Residual { get; }

    public CleanupWorkspaceAssociation WorkspaceAssociation => PlanEntry.WorkspaceAssociation;

    public CleanupLeaseBoundary LeaseBoundary => PlanEntry.LeaseBoundary;

    public CleanupRecoveryProvenance? Provenance => PlanEntry.Provenance;

    public CleanupVerificationCondition Verification => PlanEntry.Verification;

    public string? Cause { get; }
}

internal sealed record CleanupResidual
{
    private CleanupResidual(CleanupEffect effect, string? cause)
    {
        ArgumentNullException.ThrowIfNull(effect);
        if (effect.Residual == CleanupEffectResidual.None)
        {
            throw new ArgumentException(
                "A Cleanup residual must retain or have an unknown residual state.",
                nameof(effect));
        }

        CleanupEffectFactsValidation.ValidateCause(cause);

        Effect = effect;
        Cause = cause;
    }

    internal static CleanupResidual Create(CleanupEffect effect, string? cause = null)
        => new(effect, cause);

    public CleanupEffect Effect { get; }

    public CleanupPlanEntry PlanEntry => Effect.PlanEntry;

    public string Path => Effect.Path;

    public RecoveryBundleCandidateKind Kind => Effect.Kind;

    public RecoveryBundleIntegrity Integrity => Effect.Integrity;

    public CleanupArtifactFileKind FileKind => Effect.FileKind;

    public CleanupPlanAction Action => Effect.Action;

    public CleanupEffectOutcome Outcome => Effect.Outcome;

    public CleanupEffectResidual Residual => Effect.Residual;

    public CleanupWorkspaceAssociation WorkspaceAssociation => Effect.WorkspaceAssociation;

    public CleanupLeaseBoundary LeaseBoundary => Effect.LeaseBoundary;

    public CleanupRecoveryProvenance? Provenance => Effect.Provenance;

    public CleanupVerificationCondition Verification => Effect.Verification;

    public string? Cause { get; }
}

internal sealed record CleanupVerification
{
    public required CleanupVerificationState State { get; init; }

    public string? Cause { get; init; }
}

internal sealed record CleanupFinding
{
    private CleanupFinding(
        CleanupFindingCode code,
        string cause,
        string? subject = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Cleanup finding code is not defined.");
        }

        var status = CleanupDefinitions.ReadStatus(code);
        if (status == CliSemanticStatus.Complete)
        {
            throw new ArgumentException(
                "A Cleanup finding cannot carry complete status.",
                nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (subject is not null && subject.Length == 0)
        {
            throw new ArgumentException(
                "A Cleanup finding subject cannot be empty.",
                nameof(subject));
        }

        Code = code;
        Status = status;
        Subject = subject;
        Cause = cause;
    }

    internal static CleanupFinding Create(
        CleanupFindingCode code,
        string cause,
        string? subject = null)
        => new(code, cause, subject);

    internal CleanupFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Subject { get; }

    internal string Cause { get; }
}

internal sealed record CleanupResultFacts
{
    private CleanupResultFacts(
        CleanupPlan plan,
        CleanupPreflight preflight,
        CleanupLease lease,
        CleanupCatalogueComparison revalidation,
        ImmutableArray<CleanupEffect> effects,
        ImmutableArray<CleanupResidual> residuals,
        CleanupVerification verification,
        ImmutableArray<CleanupFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(preflight);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(revalidation);
        ArgumentNullException.ThrowIfNull(verification);
        ValidateEffects(plan, effects);
        ValidateResiduals(plan, effects, residuals);
        ValidateFindings(findings);

        Plan = plan;
        Preflight = preflight;
        Lease = lease;
        Revalidation = revalidation;
        Effects = effects;
        Residuals = residuals;
        Verification = verification;
        Findings = findings;
    }

    internal static CleanupResultFacts Create(
        CleanupPlan plan,
        CleanupPreflight preflight,
        CleanupLease lease,
        CleanupCatalogueComparison revalidation,
        ImmutableArray<CleanupEffect> effects,
        ImmutableArray<CleanupResidual> residuals,
        CleanupVerification verification,
        ImmutableArray<CleanupFinding> findings)
        => new(
            plan,
            preflight,
            lease,
            revalidation,
            effects,
            residuals,
            verification,
            findings);

    public CleanupMode Mode => Plan.Request?.Mode ?? CleanupMode.NotEstablished;

    public CleanupCatalogue Catalogue => Plan.Catalogue;

    public CleanupPlan Plan { get; }

    public CleanupPreflight Preflight { get; }

    public CleanupLease Lease { get; }

    public CleanupCatalogueComparison Revalidation { get; }

    public ImmutableArray<CleanupEffect> Effects { get; }

    public ImmutableArray<CleanupResidual> Residuals { get; }

    public CleanupVerification Verification { get; }

    public ImmutableArray<CleanupFinding> Findings { get; }

    private static void ValidateEffects(
        CleanupPlan plan,
        ImmutableArray<CleanupEffect> effects)
    {
        ValidateMaterialized(effects, nameof(effects));
        var previousOrdinal = -1;
        for (var index = 0; index < effects.Length; index++)
        {
            var effect = effects[index];
            ArgumentNullException.ThrowIfNull(effect);
            if (!ContainsEntry(plan, effect.PlanEntry))
            {
                throw new ArgumentException(
                    "Every Cleanup effect must reference an entry in its plan.",
                    nameof(effects));
            }

            if (ContainsEntry(effects, index, effect.PlanEntry))
            {
                throw new ArgumentException(
                    "Cleanup effects must contain at most one effect for each plan entry.",
                    nameof(effects));
            }

            if (effect.PlanEntry.Ordinal <= previousOrdinal)
            {
                throw new ArgumentException(
                    "Cleanup effects must be supplied in increasing plan-ordinal order.",
                    nameof(effects));
            }

            previousOrdinal = effect.PlanEntry.Ordinal;
        }
    }

    private static void ValidateResiduals(
        CleanupPlan plan,
        ImmutableArray<CleanupEffect> effects,
        ImmutableArray<CleanupResidual> residuals)
    {
        ValidateMaterialized(residuals, nameof(residuals));
        var previousOrdinal = -1;
        for (var index = 0; index < residuals.Length; index++)
        {
            var residual = residuals[index];
            ArgumentNullException.ThrowIfNull(residual);
            ArgumentNullException.ThrowIfNull(residual.Effect);
            if (!ContainsEntry(plan, residual.PlanEntry)
                || !ContainsEffect(effects, residual.Effect))
            {
                throw new ArgumentException(
                    "Every Cleanup residual must reference an effect in its plan result.",
                    nameof(residuals));
            }

            if (residual.Residual == CleanupEffectResidual.None)
            {
                throw new ArgumentException(
                    "A Cleanup residual must retain or have an unknown residual state.",
                    nameof(residuals));
            }

            if (ContainsResidualEffect(residuals, index, residual.Effect))
            {
                throw new ArgumentException(
                    "Cleanup residuals must contain at most one residual for each effect.",
                    nameof(residuals));
            }

            if (residual.PlanEntry.Ordinal <= previousOrdinal)
            {
                throw new ArgumentException(
                    "Cleanup residuals must be supplied in increasing plan-ordinal order.",
                    nameof(residuals));
            }

            previousOrdinal = residual.PlanEntry.Ordinal;
        }
    }

    private static void ValidateFindings(ImmutableArray<CleanupFinding> findings)
    {
        ValidateMaterialized(findings, nameof(findings));
        for (var index = 0; index < findings.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(findings[index]);
        }
    }

    private static bool ContainsEntry(CleanupPlan plan, CleanupPlanEntry candidate)
    {
        for (var index = 0; index < plan.Entries.Length; index++)
        {
            if (ReferenceEquals(plan.Entries[index], candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsEntry(
        ImmutableArray<CleanupEffect> effects,
        int count,
        CleanupPlanEntry candidate)
    {
        for (var index = 0; index < count; index++)
        {
            if (ReferenceEquals(effects[index].PlanEntry, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsEffect(
        ImmutableArray<CleanupEffect> effects,
        CleanupEffect candidate)
    {
        for (var index = 0; index < effects.Length; index++)
        {
            if (ReferenceEquals(effects[index], candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsResidualEffect(
        ImmutableArray<CleanupResidual> residuals,
        int count,
        CleanupEffect candidate)
    {
        for (var index = 0; index < count; index++)
        {
            if (ReferenceEquals(residuals[index].Effect, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateMaterialized<T>(
        ImmutableArray<T> values,
        string parameterName)
    {
        if (values.IsDefault)
        {
            throw new ArgumentException(
                "The Cleanup result collection must be materialized.",
                parameterName);
        }
    }
}
