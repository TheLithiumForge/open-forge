using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;

internal sealed record CleanupCatalogue
{
    private CleanupCatalogue(
        CleanupCatalogueCoverage coverage,
        ImmutableArray<CleanupCandidate> candidates)
    {
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coverage),
                coverage,
                "The Cleanup catalogue coverage is not defined.");
        }

        if (candidates.IsDefault)
        {
            throw new ArgumentException(
                "The Cleanup catalogue candidates must be materialized.",
                nameof(candidates));
        }

        string? previousPath = null;
        var paths = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < candidates.Length; index++)
        {
            var candidate = candidates[index];
            ArgumentNullException.ThrowIfNull(candidate);

            if (!paths.Add(candidate.Path))
            {
                throw new ArgumentException(
                    "The Cleanup catalogue cannot contain duplicate candidate paths.",
                    nameof(candidates));
            }

            if (previousPath is not null
                && string.CompareOrdinal(previousPath, candidate.Path) > 0)
            {
                throw new ArgumentException(
                    "The Cleanup catalogue candidates must be supplied in ordinal path order.",
                    nameof(candidates));
            }

            previousPath = candidate.Path;
        }

        if (coverage == CleanupCatalogueCoverage.NotEstablished && candidates.Length != 0)
        {
            throw new ArgumentException(
                "A not-established Cleanup catalogue cannot contain candidates.",
                nameof(candidates));
        }

        Coverage = coverage;
        Candidates = candidates;
    }

    internal static CleanupCatalogue Create(
        CleanupCatalogueCoverage coverage,
        ImmutableArray<CleanupCandidate> candidates)
        => new(coverage, candidates);

    public CleanupCatalogueCoverage Coverage { get; }

    public ImmutableArray<CleanupCandidate> Candidates { get; }
}

internal sealed record CleanupCandidate
{
    private CleanupCandidate(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupArtifactFileKind fileKind,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary,
        CleanupRecoveryProvenance? provenance,
        CleanupVerificationCondition verification,
        CleanupCandidateEligibility eligibility,
        CleanupPlanAction action,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(workspaceAssociation);
        ArgumentNullException.ThrowIfNull(leaseBoundary);
        ArgumentNullException.ThrowIfNull(verification);

        if (string.IsNullOrWhiteSpace(snapshot.Path))
        {
            throw new ArgumentException(
                "The Cleanup candidate snapshot path is required.",
                nameof(snapshot));
        }

        ValidateEnum(snapshot.Kind, nameof(snapshot.Kind), "candidate kind");
        ValidateEnum(snapshot.Integrity, nameof(snapshot.Integrity), "candidate integrity");
        ValidateEnum(fileKind, nameof(fileKind), "candidate file kind");
        ValidateEnum(
            workspaceAssociation.State,
            nameof(workspaceAssociation.State),
            "workspace association state");
        ValidateEnum(leaseBoundary.State, nameof(leaseBoundary.State), "lease boundary state");
        ValidateEnum(verification.State, nameof(verification.State), "verification state");
        ValidateEnum(
            verification.ExpectedFileKind,
            nameof(verification.ExpectedFileKind),
            "expected file kind");
        if (verification.ExpectedIntegrity is RecoveryBundleIntegrity expectedIntegrity)
        {
            ValidateEnum(expectedIntegrity, nameof(verification.ExpectedIntegrity), "expected integrity");
        }

        ValidateEligibilityAction(eligibility, action);
        if (eligibility == CleanupCandidateEligibility.NotEstablished)
        {
            ValidateNotEstablishedFacts(
                fileKind,
                workspaceAssociation,
                leaseBoundary,
                provenance,
                verification);
        }
        else
        {
            ValidateEstablishedFacts(
                snapshot,
                fileKind,
                workspaceAssociation,
                leaseBoundary,
                provenance,
                verification);
            ValidateEligibilityFacts(
                snapshot,
                fileKind,
                workspaceAssociation,
                leaseBoundary,
                provenance,
                verification,
                eligibility,
                action);
        }

        if (cause is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }

        Snapshot = snapshot;
        FileKind = fileKind;
        WorkspaceAssociation = workspaceAssociation;
        LeaseBoundary = leaseBoundary;
        Provenance = provenance;
        Verification = verification;
        Eligibility = eligibility;
        Action = action;
        Cause = cause;
    }

    internal static CleanupCandidate Create(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupArtifactFileKind fileKind,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary,
        CleanupRecoveryProvenance? provenance,
        CleanupVerificationCondition verification,
        CleanupCandidateEligibility eligibility,
        CleanupPlanAction action,
        string? cause = null)
        => new(
            snapshot,
            fileKind,
            workspaceAssociation,
            leaseBoundary,
            provenance,
            verification,
            eligibility,
            action,
            cause);

    public string Path => Snapshot.Path;

    public RecoveryBundleCandidateKind Kind => Snapshot.Kind;

    public RecoveryBundleIntegrity Integrity => Snapshot.Integrity;

    public CleanupArtifactFileKind FileKind { get; }

    public CleanupWorkspaceAssociation WorkspaceAssociation { get; }

    public CleanupLeaseBoundary LeaseBoundary { get; }

    public CleanupRecoveryProvenance? Provenance { get; }

    public CleanupVerificationCondition Verification { get; }

    public CleanupCandidateEligibility Eligibility { get; }

    public CleanupPlanAction Action { get; }

    public RecoveryBundleCandidateSnapshot Snapshot { get; }

    public string? Cause { get; }

    private static void ValidateNotEstablishedFacts(
        CleanupArtifactFileKind fileKind,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary,
        CleanupRecoveryProvenance? provenance,
        CleanupVerificationCondition verification)
    {
        if (fileKind != CleanupArtifactFileKind.NotEstablished
            || workspaceAssociation.State != CleanupWorkspaceAssociationState.NotEstablished
            || leaseBoundary.State != CleanupLeaseBoundaryState.NotEstablished
            || verification.State != CleanupVerificationConditionState.NotEstablished
            || verification.ExpectedPath is not null
            || verification.ExpectedFileKind != CleanupArtifactFileKind.NotEstablished
            || verification.ExpectedIntegrity is not null
            || provenance is not null)
        {
            throw new ArgumentException(
                "A not-established Cleanup candidate must contain no observed facts.",
                nameof(fileKind));
        }
    }

    private static void ValidateEstablishedFacts(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupArtifactFileKind fileKind,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary,
        CleanupRecoveryProvenance? provenance,
        CleanupVerificationCondition verification)
    {
        if (fileKind == CleanupArtifactFileKind.NotEstablished
            || workspaceAssociation.State == CleanupWorkspaceAssociationState.NotEstablished
            || leaseBoundary.State == CleanupLeaseBoundaryState.NotEstablished
            || verification.State is CleanupVerificationConditionState.NotEstablished
                or CleanupVerificationConditionState.NotRequested)
        {
            throw new ArgumentException(
                "An established Cleanup candidate requires observed file, association, lease, and verification facts.",
                nameof(fileKind));
        }

        var expectedVerificationState = snapshot.Kind switch
        {
            RecoveryBundleCandidateKind.Final => CleanupVerificationConditionState.SemanticFinal,
            RecoveryBundleCandidateKind.Draft => CleanupVerificationConditionState.ExactPathAndKind,
            _ => throw new ArgumentOutOfRangeException(
                nameof(snapshot),
                snapshot.Kind,
                "The candidate kind is not defined."),
        };

        if (((snapshot.Kind, snapshot.Integrity) is
                (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Incomplete)
                or (RecoveryBundleCandidateKind.Draft, not (RecoveryBundleIntegrity.Incomplete or RecoveryBundleIntegrity.Unavailable)))
            || (snapshot.Kind == RecoveryBundleCandidateKind.Draft
                && snapshot.Integrity == RecoveryBundleIntegrity.Unavailable
                && fileKind != CleanupArtifactFileKind.NonOrdinary)
            || verification.State != expectedVerificationState
            || !Same(snapshot.Path, verification.ExpectedPath)
            || verification.ExpectedFileKind != fileKind
            || verification.ExpectedIntegrity != snapshot.Integrity)
        {
            throw new ArgumentException(
                "The Cleanup candidate verification condition must match its snapshot.",
                nameof(verification));
        }

        ValidateProvenanceFacts(snapshot, workspaceAssociation, provenance);
    }

    private static void ValidateEligibilityFacts(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupArtifactFileKind fileKind,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary,
        CleanupRecoveryProvenance? provenance,
        CleanupVerificationCondition verification,
        CleanupCandidateEligibility eligibility,
        CleanupPlanAction action)
    {
        var fullyEligible = snapshot.Kind switch
        {
            RecoveryBundleCandidateKind.Final => snapshot.Integrity == RecoveryBundleIntegrity.Verified
                && fileKind == CleanupArtifactFileKind.Ordinary
                && verification.State == CleanupVerificationConditionState.SemanticFinal
                && MatchesCurrentLease(workspaceAssociation, leaseBoundary)
                && MatchesCurrentWorkspace(workspaceAssociation, provenance),
            RecoveryBundleCandidateKind.Draft => snapshot.Integrity == RecoveryBundleIntegrity.Incomplete
                && fileKind == CleanupArtifactFileKind.Ordinary
                && verification.State == CleanupVerificationConditionState.ExactPathAndKind
                && MatchesCurrentLease(workspaceAssociation, leaseBoundary)
                && provenance is null,
            _ => false,
        };

        if (eligibility == CleanupCandidateEligibility.Eligible && !fullyEligible)
        {
            throw new ArgumentException(
                "Only fully validated Cleanup candidates may be eligible for deletion.",
                nameof(eligibility));
        }

        if (eligibility == CleanupCandidateEligibility.Blocked
            && fullyEligible)
        {
            throw new ArgumentException(
                "A fully eligible Cleanup candidate cannot be blocked.",
                nameof(eligibility));
        }

        if (eligibility == CleanupCandidateEligibility.Eligible
            && action != CleanupPlanAction.Delete)
        {
            throw new ArgumentException(
                "An eligible Cleanup candidate must use the delete action.",
                nameof(action));
        }
    }

    private static void ValidateProvenanceFacts(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupRecoveryProvenance? provenance)
    {
        if (snapshot.Kind != RecoveryBundleCandidateKind.Final
            || snapshot.Integrity != RecoveryBundleIntegrity.Verified)
        {
            if (provenance is not null)
            {
                throw new ArgumentException(
                    "Only a verified final Cleanup candidate may carry recovery provenance.",
                    nameof(provenance));
            }

            return;
        }

        var verifiedProvenance = provenance
            ?? throw new ArgumentNullException(nameof(provenance));
        var verifiedSnapshot = snapshot.Verified
            ?? throw new ArgumentException(
                "A verified Cleanup candidate requires verified snapshot facts.",
                nameof(snapshot));
        ValidateProvenance(verifiedProvenance);
        ValidateProvenanceMatchesSnapshot(verifiedProvenance, verifiedSnapshot);
        if (workspaceAssociation.CandidatePhysicalPath is not null
            && !SamePhysicalPath(
                verifiedProvenance.WorkspacePhysicalPath,
                workspaceAssociation.CandidatePhysicalPath))
        {
            throw new ArgumentException(
                "Cleanup provenance must match the candidate workspace path.",
                nameof(provenance));
        }

        if (workspaceAssociation.CandidateWorkspaceKey is not null
            && !Same(verifiedProvenance.WorkspaceKey, workspaceAssociation.CandidateWorkspaceKey))
        {
            throw new ArgumentException(
                "Cleanup provenance must match the candidate workspace key.",
                nameof(provenance));
        }
    }

    private static void ValidateProvenanceMatchesSnapshot(
        CleanupRecoveryProvenance provenance,
        RecoveryBundleVerifiedRead verified)
    {
        ArgumentNullException.ThrowIfNull(verified.Attribution);
        ArgumentNullException.ThrowIfNull(verified.Attribution.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(verified.Attribution.Subject.Identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(verified.Command);
        ArgumentException.ThrowIfNullOrWhiteSpace(verified.WorkspacePhysicalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(verified.WorkspaceKey);
        if (verified.OperationId == Guid.Empty)
        {
            throw new ArgumentException(
                "A verified Cleanup snapshot requires a non-empty operation ID.",
                nameof(verified));
        }

        if (!Same(provenance.Command, verified.Command)
            || !SamePhysicalPath(provenance.WorkspacePhysicalPath, verified.WorkspacePhysicalPath)
            || !Same(provenance.WorkspaceKey, verified.WorkspaceKey)
            || provenance.OperationId != verified.OperationId
            || provenance.Attribution.Producer != verified.Attribution.Producer
            || provenance.Attribution.Operation != verified.Attribution.Operation
            || provenance.Attribution.Subject.Kind != verified.Attribution.Subject.Kind
            || !Same(
                provenance.Attribution.Subject.Identity,
                verified.Attribution.Subject.Identity))
        {
            throw new ArgumentException(
                "Cleanup provenance must match the verified snapshot identity.",
                nameof(provenance));
        }
    }

    private static bool MatchesCurrentWorkspace(
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupRecoveryProvenance? provenance)
        => workspaceAssociation.State == CleanupWorkspaceAssociationState.CurrentWorkspace
            && provenance is not null
            && SamePhysicalPath(provenance.WorkspacePhysicalPath, workspaceAssociation.SelectedPhysicalPath)
            && SamePhysicalPath(provenance.WorkspacePhysicalPath, workspaceAssociation.CandidatePhysicalPath)
            && Same(provenance.WorkspaceKey, workspaceAssociation.SelectedWorkspaceKey)
            && Same(provenance.WorkspaceKey, workspaceAssociation.CandidateWorkspaceKey)
            && provenance.Attribution.Subject.Kind == RecoveryBundleSubjectKind.Workspace
            && Same(provenance.Attribution.Subject.Identity, workspaceAssociation.SelectedWorkspaceKey);

    private static bool MatchesCurrentLease(
        CleanupWorkspaceAssociation workspaceAssociation,
        CleanupLeaseBoundary leaseBoundary)
        => workspaceAssociation.State == CleanupWorkspaceAssociationState.CurrentWorkspace
            && leaseBoundary.State == CleanupLeaseBoundaryState.Required
            && Same(leaseBoundary.WorkspaceKey, workspaceAssociation.SelectedWorkspaceKey)
            && Same(leaseBoundary.Command, CleanupDefinitions.CommandIdentity)
            && leaseBoundary.OperationId is Guid operationId
            && operationId != Guid.Empty;

    private static void ValidateProvenance(CleanupRecoveryProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(provenance.Attribution);
        ArgumentNullException.ThrowIfNull(provenance.Attribution.Subject);
        ValidateEnum(provenance.Attribution.Producer, nameof(provenance.Attribution.Producer), "provenance producer");
        ValidateEnum(
            provenance.Attribution.Operation,
            nameof(provenance.Attribution.Operation),
            "provenance operation");
        ValidateEnum(
            provenance.Attribution.Subject.Kind,
            nameof(provenance.Attribution.Subject.Kind),
            "provenance subject kind");
        if (provenance.Attribution.Subject.Kind != RecoveryBundleSubjectKind.Workspace)
        {
            throw new ArgumentException(
                "Cleanup provenance requires a workspace subject.",
                nameof(provenance));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(provenance.Attribution.Subject.Identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenance.Command);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenance.WorkspacePhysicalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenance.WorkspaceKey);
        if (provenance.OperationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Cleanup provenance requires a non-empty operation ID.",
                nameof(provenance));
        }

        if (!Same(provenance.Attribution.Subject.Identity, provenance.WorkspaceKey))
        {
            throw new ArgumentException(
                "Cleanup provenance subject identity must match its workspace key.",
                nameof(provenance));
        }
    }

    private static bool Same(string? left, string? right)
        => string.Equals(left, right, StringComparison.Ordinal);

    private static bool SamePhysicalPath(string? left, string? right)
        => PhysicalIdentityTracker.PathComparer.Equals(left, right);

    private static void ValidateEligibilityAction(
        CleanupCandidateEligibility eligibility,
        CleanupPlanAction action)
    {
        ValidateEnum(eligibility, nameof(eligibility), "candidate eligibility");
        ValidateEnum(action, nameof(action), "plan action");

        if ((eligibility, action) is not
            ((CleanupCandidateEligibility.Eligible, CleanupPlanAction.Delete)
                or (CleanupCandidateEligibility.Blocked, CleanupPlanAction.Preserve)
                or (CleanupCandidateEligibility.NotEstablished, CleanupPlanAction.NotEstablished)))
        {
            throw new ArgumentException(
                "The Cleanup candidate eligibility and action are incoherent.",
                nameof(action));
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
