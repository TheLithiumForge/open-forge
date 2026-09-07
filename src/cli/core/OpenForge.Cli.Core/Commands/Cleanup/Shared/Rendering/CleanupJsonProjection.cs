using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupJsonProjection
{
    internal static CleanupJsonDocument Create(CleanupResult result)
        => new()
        {
            SchemaVersion = CleanupDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CleanupWireVocabulary.Status(result.Status),
            Workspace = Workspace(result.Workspace),
            Result = new CleanupJsonResult
            {
                Mode = CleanupWireVocabulary.Mode(result.Mode),
                Catalogue = Catalogue(result.Catalogue),
                Plan = new CleanupJsonPlan
                {
                    Safety = CleanupWireVocabulary.Safety(result.Plan.Safety),
                    Entries = [.. result.Plan.Entries.Where(entry => entry.Action == CleanupPlanAction.Delete).Select(PlanEntry)],
                },
                Preflight = new CleanupJsonPreflight { State = CleanupWireVocabulary.Preflight(result.Preflight.State), Cause = result.Preflight.Cause },
                Lease = new CleanupJsonLease { State = CleanupWireVocabulary.Lease(result.Lease.State), Cause = result.Lease.Cause },
                Revalidation = new CleanupJsonCatalogueComparison
                {
                    State = CleanupWireVocabulary.Comparison(result.Revalidation.State),
                    Planned = result.Revalidation.Planned is { } planned ? Catalogue(planned) : null,
                    Observed = result.Revalidation.Observed is { } observed ? Catalogue(observed) : null,
                    Cause = result.Revalidation.Cause,
                },
                Effects = [.. result.Effects.Select(Effect)],
                Residuals = [.. result.Residuals.Select(Residual)],
                Verification = new CleanupJsonVerification { State = CleanupWireVocabulary.Verification(result.Verification.State), Cause = result.Verification.Cause },
                Findings = [.. result.Findings.Select(Finding)],
            },
            Next = result.Next is { } next ? new CleanupJsonNext { Command = next.Command, Reason = next.Reason } : null,
        };

    private static CleanupJsonWorkspace? Workspace(CliWorkspace? workspace)
        => workspace is null ? null : new CleanupJsonWorkspace
        {
            Path = workspace.LexicalRoot,
            SelectedBy = CleanupWireVocabulary.WorkspaceSelection(workspace.SelectedBy),
        };

    private static CleanupJsonCatalogue Catalogue(CleanupCatalogue catalogue)
        => new()
        {
            Coverage = CleanupWireVocabulary.Coverage(catalogue.Coverage),
            Candidates = [.. catalogue.Candidates.Select(Candidate)],
        };

    private static CleanupJsonCandidate Candidate(CleanupCandidate value)
        => new()
        {
            Path = value.Path,
            Kind = CleanupWireVocabulary.CandidateKind(value.Kind),
            Integrity = CleanupWireVocabulary.Integrity(value.Integrity),
            FileKind = CleanupWireVocabulary.FileKind(value.FileKind),
            WorkspaceAssociation = WorkspaceAssociation(value.WorkspaceAssociation),
            LeaseBoundary = LeaseBoundary(value.LeaseBoundary),
            Provenance = Provenance(value.Provenance),
            Verification = VerificationCondition(value.Verification),
            Eligibility = CleanupWireVocabulary.Eligibility(value.Eligibility),
            Action = CleanupWireVocabulary.Action(value.Action),
            Cause = value.Cause,
        };

    private static CleanupJsonPlanEntry PlanEntry(CleanupPlanEntry value)
        => new()
        {
            Ordinal = value.Ordinal,
            Path = value.Path,
            Kind = CleanupWireVocabulary.CandidateKind(value.Kind),
            Integrity = CleanupWireVocabulary.Integrity(value.Integrity),
            FileKind = CleanupWireVocabulary.FileKind(value.FileKind),
            WorkspaceAssociation = WorkspaceAssociation(value.WorkspaceAssociation),
            LeaseBoundary = LeaseBoundary(value.LeaseBoundary),
            Provenance = Provenance(value.Provenance),
            Verification = VerificationCondition(value.Verification),
            Eligibility = CleanupWireVocabulary.Eligibility(value.Eligibility),
            Action = CleanupWireVocabulary.Action(value.Action),
            ResultEffect = new CleanupJsonEffectCondition
            {
                Outcome = CleanupWireVocabulary.EffectOutcome(value.ResultEffect.Outcome),
                Residual = CleanupWireVocabulary.EffectResidual(value.ResultEffect.Residual),
            },
            Cause = value.Cause,
        };

    private static CleanupJsonEffect Effect(CleanupEffect value)
        => new()
        {
            Path = value.Path,
            Kind = CleanupWireVocabulary.CandidateKind(value.Kind),
            Integrity = CleanupWireVocabulary.Integrity(value.Integrity),
            FileKind = CleanupWireVocabulary.FileKind(value.FileKind),
            WorkspaceAssociation = WorkspaceAssociation(value.WorkspaceAssociation),
            LeaseBoundary = LeaseBoundary(value.LeaseBoundary),
            Provenance = Provenance(value.Provenance),
            Verification = VerificationCondition(value.Verification),
            Action = CleanupWireVocabulary.Action(value.Action),
            Outcome = CleanupWireVocabulary.EffectOutcome(value.Outcome),
            Residual = CleanupWireVocabulary.EffectResidual(value.Residual),
            Cause = value.Cause,
        };

    private static CleanupJsonResidual Residual(CleanupResidual value)
        => new()
        {
            Path = value.Path,
            Kind = CleanupWireVocabulary.CandidateKind(value.Kind),
            Integrity = CleanupWireVocabulary.Integrity(value.Integrity),
            FileKind = CleanupWireVocabulary.FileKind(value.FileKind),
            WorkspaceAssociation = WorkspaceAssociation(value.WorkspaceAssociation),
            LeaseBoundary = LeaseBoundary(value.LeaseBoundary),
            Provenance = Provenance(value.Provenance),
            Verification = VerificationCondition(value.Verification),
            Action = CleanupWireVocabulary.Action(value.Action),
            Outcome = CleanupWireVocabulary.EffectOutcome(value.Outcome),
            Residual = CleanupWireVocabulary.EffectResidual(value.Residual),
            Cause = value.Cause,
        };

    private static CleanupJsonWorkspaceAssociation WorkspaceAssociation(CleanupWorkspaceAssociation value)
        => new()
        {
            State = CleanupWireVocabulary.WorkspaceAssociation(value.State),
            SelectedPhysicalPath = value.SelectedPhysicalPath,
            CandidatePhysicalPath = value.CandidatePhysicalPath,
            SelectedWorkspaceKey = value.SelectedWorkspaceKey,
            CandidateWorkspaceKey = value.CandidateWorkspaceKey,
            Cause = value.Cause,
        };

    private static CleanupJsonLeaseBoundary LeaseBoundary(CleanupLeaseBoundary value)
        => new()
        {
            State = CleanupWireVocabulary.LeaseBoundary(value.State),
            WorkspaceKey = value.WorkspaceKey,
            Command = value.Command,
            OperationId = value.OperationId?.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            Cause = value.Cause,
        };

    private static CleanupJsonRecoveryProvenance? Provenance(CleanupRecoveryProvenance? value)
        => value is null ? null : new CleanupJsonRecoveryProvenance
        {
            Producer = CleanupWireVocabulary.RecoveryProducer(value.Attribution.Producer),
            Operation = CleanupWireVocabulary.RecoveryOperation(value.Attribution.Operation),
            Subject = new CleanupJsonRecoverySubject
            {
                Kind = CleanupWireVocabulary.RecoverySubjectKind(value.Attribution.Subject.Kind),
                Identity = value.Attribution.Subject.Identity,
            },
            Command = value.Command,
            WorkspacePhysicalPath = value.WorkspacePhysicalPath,
            WorkspaceKey = value.WorkspaceKey,
            OperationId = value.OperationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
        };

    private static CleanupJsonVerificationCondition VerificationCondition(CleanupVerificationCondition value)
        => new()
        {
            State = CleanupWireVocabulary.VerificationCondition(value.State),
            ExpectedPath = value.ExpectedPath,
            ExpectedFileKind = CleanupWireVocabulary.FileKind(value.ExpectedFileKind),
            ExpectedIntegrity = value.ExpectedIntegrity is { } integrity ? CleanupWireVocabulary.Integrity(integrity) : null,
            Cause = value.Cause,
        };

    private static CleanupJsonFinding Finding(CleanupFinding value)
        => new()
        {
            Code = CleanupWireVocabulary.FindingCode(value.Code),
            Status = CleanupWireVocabulary.Status(value.Status),
            Subject = value.Subject,
            Cause = value.Cause,
        };
}
