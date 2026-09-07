using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;

internal static class CleanupPlanner
{
    internal static CleanupPlan Create(CleanupRequest request, RecoveryBundleCatalogueResult source)
    {
        var catalogue = ReadCatalogue(request, source);
        var entries = catalogue.Candidates.Select((candidate, ordinal) => CleanupPlanEntry.Create(
            ordinal,
            candidate,
            ReadAnticipatedEffect(candidate.Action))).ToImmutableArray();
        var safe = catalogue.Coverage == CleanupCatalogueCoverage.Complete
            && entries.All(entry => entry.Eligibility == CleanupCandidateEligibility.Eligible);
        return CleanupPlan.Create(request, catalogue, entries, safe ? entries : [], safe ? CleanupPlanSafety.Safe : CleanupPlanSafety.Blocked);
    }

    internal static CleanupCatalogue ReadCatalogue(CleanupRequest request, RecoveryBundleCatalogueResult source)
    {
        var coverage = source.State switch
        {
            RecoveryBundleCatalogueState.Available => CleanupCatalogueCoverage.Complete,
            RecoveryBundleCatalogueState.Unavailable => CleanupCatalogueCoverage.Incomplete,
            RecoveryBundleCatalogueState.Cancelled => CleanupCatalogueCoverage.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The recovery catalogue state is not defined."),
        };
        var selectedPath = WorkspaceIdentity.NormalizePhysicalPath(request.Workspace.PhysicalRoot);
        var selectedKey = WorkspaceIdentity.Key(selectedPath);
        var association = CleanupWorkspaceAssociation.Create(CleanupWorkspaceAssociationState.CurrentWorkspace, selectedPath, selectedPath, selectedKey, selectedKey);
        var lease = CleanupLeaseBoundary.Create(CleanupLeaseBoundaryState.Required, selectedKey, CleanupDefinitions.CommandIdentity, LeaseOperationId(request));
        var candidates = source.Candidates.OrderBy(candidate => candidate.Path, StringComparer.Ordinal)
            .Select(candidate => ReadCandidate(candidate, association, lease)).ToImmutableArray();
        return CleanupCatalogue.Create(coverage, candidates);
    }

    internal static Guid LeaseOperationId(CleanupRequest request)
        => new(Convert.FromHexString(WorkspaceIdentity.Key(request.Workspace.PhysicalRoot)).AsSpan(0, 16));

    internal static ImmutableArray<CleanupFinding> ReadFindings(CleanupPlan plan, string? catalogueCause)
    {
        if (plan.Catalogue.Coverage == CleanupCatalogueCoverage.Incomplete)
        {
            return [CleanupFinding.Create(CleanupFindingCode.CatalogueIncomplete, catalogueCause ?? "The recovery catalogue is unavailable.")];
        }

        if (plan.Catalogue.Coverage == CleanupCatalogueCoverage.Interrupted)
        {
            return [CleanupFinding.Create(CleanupFindingCode.Interrupted, "Cleanup catalogue observation was interrupted.")];
        }

        return [.. plan.Catalogue.Candidates
            .Where(candidate => candidate.Eligibility == CleanupCandidateEligibility.Blocked)
            .Select(candidate => CleanupFinding.Create(
                ReadFinding(candidate),
                candidate.Cause ?? "The recovery candidate is unsafe for deletion.",
                candidate.Path))];
    }

    private static CleanupCandidate ReadCandidate(
        RecoveryBundleCandidateSnapshot snapshot,
        CleanupWorkspaceAssociation currentWorkspace,
        CleanupLeaseBoundary lease)
    {
        var observed = RecoveryBundleStorage.TryObservePath(snapshot.Path, out var attributes, out var failure);
        var ordinary = observed && attributes is not null
            && (attributes.Value & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
        var fileKind = ordinary ? CleanupArtifactFileKind.Ordinary : CleanupArtifactFileKind.NonOrdinary;
        var associated = snapshot.Kind == RecoveryBundleCandidateKind.Draft || snapshot.Verified is not null;
        var association = associated ? currentWorkspace : CleanupWorkspaceAssociation.Create(
            CleanupWorkspaceAssociationState.Unavailable,
            currentWorkspace.SelectedPhysicalPath,
            null,
            currentWorkspace.SelectedWorkspaceKey,
            null);
        CleanupRecoveryProvenance? provenance = null;
        if (snapshot.Verified is { } verified)
        {
            provenance = new CleanupRecoveryProvenance
            {
                Attribution = verified.Attribution,
                Command = verified.Command,
                WorkspacePhysicalPath = verified.WorkspacePhysicalPath,
                WorkspaceKey = verified.WorkspaceKey,
                OperationId = verified.OperationId,
            };
        }

        var condition = snapshot.Kind switch
        {
            RecoveryBundleCandidateKind.Final => CleanupVerificationConditionState.SemanticFinal,
            RecoveryBundleCandidateKind.Draft => CleanupVerificationConditionState.ExactPathAndKind,
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot), snapshot.Kind, "The recovery candidate kind is not defined."),
        };
        var eligible = ordinary && (snapshot.Kind, snapshot.Integrity) is
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Verified)
            or (RecoveryBundleCandidateKind.Draft, RecoveryBundleIntegrity.Incomplete);
        return CleanupCandidate.Create(
            snapshot: snapshot,
            fileKind: fileKind,
            workspaceAssociation: association,
            leaseBoundary: lease,
            provenance: provenance,
            verification: CleanupVerificationCondition.Create(condition, snapshot.Path, fileKind, snapshot.Integrity),
            eligibility: eligible ? CleanupCandidateEligibility.Eligible : CleanupCandidateEligibility.Blocked,
            action: eligible ? CleanupPlanAction.Delete : CleanupPlanAction.Preserve,
            cause: snapshot.Cause ?? failure?.DirectCause);
    }

    private static CleanupEffectCondition ReadAnticipatedEffect(CleanupPlanAction action)
        => action switch
        {
            CleanupPlanAction.Delete => CleanupEffectCondition.Create(CleanupEffectOutcome.Planned, CleanupEffectResidual.None),
            CleanupPlanAction.Preserve => CleanupEffectCondition.Create(CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained),
            CleanupPlanAction.NotEstablished => throw new InvalidOperationException("An observed candidate requires an established action."),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The Cleanup action is not defined."),
        };

    private static CleanupFindingCode ReadFinding(CleanupCandidate candidate)
    {
        if (candidate.Kind == RecoveryBundleCandidateKind.Draft)
        {
            return CleanupFindingCode.RecoveryDraftUnsafe;
        }

        return candidate.Integrity switch
        {
            RecoveryBundleIntegrity.Malformed => CleanupFindingCode.RecoveryFinalMalformed,
            RecoveryBundleIntegrity.Unsupported => CleanupFindingCode.RecoveryFinalUnsupported,
            RecoveryBundleIntegrity.Unavailable or RecoveryBundleIntegrity.Verified => CleanupFindingCode.RecoveryFinalUnavailable,
            RecoveryBundleIntegrity.Incomplete => throw new InvalidOperationException("A final cannot have draft integrity."),
            _ => throw new ArgumentOutOfRangeException(nameof(candidate), candidate.Integrity, "The recovery integrity is not defined."),
        };
    }
}
