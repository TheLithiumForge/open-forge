using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorFindingWireVocabulary
{
    internal static string Severity(DoctorFindingSeverity value)
        => value switch
        {
            DoctorFindingSeverity.Information => "information",
            DoctorFindingSeverity.Warning => "warning",
            DoctorFindingSeverity.Error => "error",
            _ => Undefined(value),
        };

    internal static string Resolution(DoctorResolutionLane value)
        => value switch
        {
            DoctorResolutionLane.SafeExact => "safe-exact",
            DoctorResolutionLane.GuidedChoice => "guided-choice",
            DoctorResolutionLane.TargetedOperation => "targeted-operation",
            DoctorResolutionLane.ManualDecision => "manual-decision",
            DoctorResolutionLane.BlockedRepair => "blocked-repair",
            DoctorResolutionLane.Informational => "informational",
            _ => Undefined(value),
        };

    internal static string Subject(DoctorSubjectKind value)
        => value switch
        {
            DoctorSubjectKind.Workspace => "workspace",
            DoctorSubjectKind.Path => "path",
            DoctorSubjectKind.Route => "route",
            DoctorSubjectKind.GeneratedRegion => "generated-region",
            DoctorSubjectKind.SourceOccurrence => "source-occurrence",
            DoctorSubjectKind.Target => "target",
            DoctorSubjectKind.RecoveryItem => "recovery-item",
            DoctorSubjectKind.ManagedFile => "managed-file",
            DoctorSubjectKind.Extension => "extension",
            DoctorSubjectKind.Dependency => "dependency",
            _ => Undefined(value),
        };

    internal static string Evidence(DoctorEvidenceKind value)
        => value switch
        {
            DoctorEvidenceKind.Availability => "availability",
            DoctorEvidenceKind.State => "state",
            DoctorEvidenceKind.Comparison => "comparison",
            DoctorEvidenceKind.Integrity => "integrity",
            DoctorEvidenceKind.AuthoredValue => "authored-value",
            DoctorEvidenceKind.CandidateBasis => "candidate-basis",
            _ => Undefined(value),
        };

    internal static string ObservedState(DoctorObservedState value)
        => value switch
        {
            DoctorObservedState.Present => "present",
            DoctorObservedState.Absent => "absent",
            DoctorObservedState.Current => "current",
            DoctorObservedState.Changed => "changed",
            DoctorObservedState.Missing => "missing",
            DoctorObservedState.Unavailable => "unavailable",
            DoctorObservedState.Blocked => "blocked",
            DoctorObservedState.Incomplete => "incomplete",
            DoctorObservedState.Valid => "valid",
            DoctorObservedState.Invalid => "invalid",
            DoctorObservedState.Unsupported => "unsupported",
            DoctorObservedState.Malformed => "malformed",
            DoctorObservedState.Untrusted => "untrusted",
            DoctorObservedState.Verified => "verified",
            _ => Undefined(value),
        };

    internal static string Integrity(DoctorIntegrityState value)
        => value switch
        {
            DoctorIntegrityState.Verified => "verified",
            DoctorIntegrityState.Malformed => "malformed",
            DoctorIntegrityState.Unsupported => "unsupported",
            DoctorIntegrityState.Unavailable => "unavailable",
            DoctorIntegrityState.Incomplete => "incomplete",
            _ => Undefined(value),
        };

    internal static string CandidateBasis(DoctorCandidateBasisKind value)
        => value switch
        {
            DoctorCandidateBasisKind.Filename => "filename",
            DoctorCandidateBasisKind.Title => "title",
            DoctorCandidateBasisKind.LiteralContent => "literal-content",
            DoctorCandidateBasisKind.RouteNeighborhood => "route-neighborhood",
            _ => Undefined(value),
        };

    internal static string Provenance(DoctorProvenanceSource value)
        => value switch
        {
            DoctorProvenanceSource.WorkspaceEntry => "workspace-entry",
            DoctorProvenanceSource.RecoveryResiduals => "recovery-residuals",
            DoctorProvenanceSource.RouteInventory => "route-inventory",
            DoctorProvenanceSource.RouteMetadata => "route-metadata",
            DoctorProvenanceSource.GeneratedNavigation => "generated-navigation",
            DoctorProvenanceSource.LocalReferences => "local-references",
            DoctorProvenanceSource.FrameworkLifecycle => "framework-lifecycle",
            DoctorProvenanceSource.FrameworkPayload => "framework-payload",
            DoctorProvenanceSource.ExtensionLifecycle => "extension-lifecycle",
            DoctorProvenanceSource.ExtensionSource => "extension-source",
            DoctorProvenanceSource.LifecycleOwnership => "lifecycle-ownership",
            _ => Undefined(value),
        };

    internal static string Cardinality(DoctorCandidateCardinality value)
        => value switch
        {
            DoctorCandidateCardinality.None => "none",
            DoctorCandidateCardinality.One => "one",
            DoctorCandidateCardinality.Several => "several",
            _ => Undefined(value),
        };

    internal static string Proposal(DoctorProposalKind value)
        => value switch
        {
            DoctorProposalKind.ReferenceCanonicalization => "reference-canonicalization",
            _ => Undefined(value),
        };

    internal static string Verification(DoctorProposalVerificationKind value)
        => value switch
        {
            DoctorProposalVerificationKind.SameTargetIdentity => "same-target-identity",
            DoctorProposalVerificationKind.ResultingBytes => "resulting-bytes",
            _ => Undefined(value),
        };

    internal static string Recovery(DoctorProposalRecoveryKind value)
        => value switch
        {
            DoctorProposalRecoveryKind.NoPersistentState => "no-persistent-state",
            DoctorProposalRecoveryKind.RepairReceiptRequired => "repair-receipt-required",
            _ => Undefined(value),
        };

    internal static string Action(DoctorNextActionKind value)
        => value switch
        {
            DoctorNextActionKind.RepairPreview => "repair-preview",
            DoctorNextActionKind.AcceptedOperation => "accepted-operation",
            DoctorNextActionKind.FutureOperation => "future-operation",
            DoctorNextActionKind.ReviewCandidates => "review-candidates",
            DoctorNextActionKind.ManualDecision => "manual-decision",
            _ => Undefined(value),
        };

    internal static string Operation(DoctorNextOperation value)
        => value switch
        {
            DoctorNextOperation.Repair => "repair",
            DoctorNextOperation.Index => "index",
            DoctorNextOperation.Cleanup => "cleanup",
            DoctorNextOperation.Install => "install",
            DoctorNextOperation.Update => "update",
            DoctorNextOperation.ExtensionCreate => "extension-create",
            DoctorNextOperation.ExtensionInstall => "extension-install",
            DoctorNextOperation.ExtensionUpdate => "extension-update",
            DoctorNextOperation.ExtensionRemove => "extension-remove",
            _ => Undefined(value),
        };

    private static string Undefined<T>(T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(nameof(value), value, $"The {typeof(T).Name} value is not defined.");
}
