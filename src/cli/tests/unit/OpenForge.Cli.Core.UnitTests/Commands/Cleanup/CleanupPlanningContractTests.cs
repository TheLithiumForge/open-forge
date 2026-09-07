using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupPlanningContractTests
{
    [Fact(DisplayName = "Cleanup catalogue retains materialized candidates in deterministic ordinal path order"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void CatalogueRetainsUniqueOrdinalCandidates()
    {
        var workspace = CleanupTestData.Workspace("catalogue-order");
        var first = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Incomplete,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.draft"));
        var second = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Malformed,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"));

        var catalogue = CleanupTestData.Catalogue(
            CleanupCatalogueCoverage.Complete,
            first,
            second);

        Assert.Equal(CleanupCatalogueCoverage.Complete, catalogue.Coverage);
        Assert.Equal([first, second], catalogue.Candidates);
        Assert.Equal([first.Path, second.Path], catalogue.Candidates.Select(candidate => candidate.Path));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Catalogue(
            CleanupCatalogueCoverage.Complete,
            second,
            first));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Catalogue(
            CleanupCatalogueCoverage.Complete,
            first,
            first));
        Assert.Throws<ArgumentException>(() => CleanupCatalogue.Create(
            CleanupCatalogueCoverage.Complete,
            default));
    }

    [Fact(DisplayName = "Cleanup candidate facts distinguish verified finals, ordinary drafts, and preserved unsafe items"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void CandidateFactsMapRecognitionAndEligibility()
    {
        var workspace = CleanupTestData.Workspace("candidate-facts");
        var verified = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Verified,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.zip"));
        var draft = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Incomplete,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.draft"));
        var malformed = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Malformed,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-c.zip"));
        var unsafeDraft = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Incomplete,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-d.draft"),
            fileKind: CleanupArtifactFileKind.NonOrdinary);

        Assert.Equal(RecoveryBundleCandidateKind.Final, verified.Kind);
        Assert.Equal(RecoveryBundleIntegrity.Verified, verified.Integrity);
        Assert.Equal(CleanupArtifactFileKind.Ordinary, verified.FileKind);
        Assert.Equal(CleanupWorkspaceAssociationState.CurrentWorkspace, verified.WorkspaceAssociation.State);
        Assert.Equal(CleanupLeaseBoundaryState.Required, verified.LeaseBoundary.State);
        Assert.Equal(CleanupVerificationConditionState.SemanticFinal, verified.Verification.State);
        Assert.Equal(CleanupCandidateEligibility.Eligible, verified.Eligibility);
        Assert.Equal(CleanupPlanAction.Delete, verified.Action);
        Assert.NotNull(verified.Provenance);
        Assert.Equal(verified.Provenance?.WorkspaceKey, verified.WorkspaceAssociation.SelectedWorkspaceKey);

        Assert.Equal(RecoveryBundleCandidateKind.Draft, draft.Kind);
        Assert.Equal(RecoveryBundleIntegrity.Incomplete, draft.Integrity);
        Assert.Null(draft.Provenance);
        Assert.Equal(CleanupVerificationConditionState.ExactPathAndKind, draft.Verification.State);
        Assert.Equal(CleanupCandidateEligibility.Eligible, draft.Eligibility);
        Assert.Equal(CleanupPlanAction.Delete, draft.Action);

        Assert.Equal(CleanupCandidateEligibility.Blocked, malformed.Eligibility);
        Assert.Equal(CleanupPlanAction.Preserve, malformed.Action);
        Assert.Equal(RecoveryBundleIntegrity.Malformed, malformed.Verification.ExpectedIntegrity);
        Assert.Equal(CleanupCandidateEligibility.Blocked, unsafeDraft.Eligibility);
        Assert.Equal(CleanupPlanAction.Preserve, unsafeDraft.Action);
        Assert.Equal(CleanupArtifactFileKind.NonOrdinary, unsafeDraft.FileKind);
    }

    [Fact(DisplayName = "Cleanup candidate construction rejects impossible kind and integrity pairs"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void CandidateRejectsImpossibleKindIntegrityPairs()
    {
        Assert.ThrowsAny<ArgumentException>(() => CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Incomplete));
        Assert.ThrowsAny<ArgumentException>(() => CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Verified));
        Assert.ThrowsAny<ArgumentException>(() => CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Malformed));
    }

    [Fact(DisplayName = "Cleanup candidate validation rejects unsafe eligibility, lease, and provenance combinations"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void CandidateRejectsUnsafeCombinations()
    {
        var workspace = CleanupTestData.Workspace("candidate-validation");

        Assert.Throws<ArgumentException>(() => CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            fileKind: CleanupArtifactFileKind.NonOrdinary,
            eligibility: CleanupCandidateEligibility.Eligible,
            action: CleanupPlanAction.Delete));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            eligibility: CleanupCandidateEligibility.Blocked,
            action: CleanupPlanAction.Preserve,
            fileKind: CleanupArtifactFileKind.Ordinary));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            leaseState: CleanupLeaseBoundaryState.Held,
            eligibility: CleanupCandidateEligibility.Eligible,
            action: CleanupPlanAction.Delete));

        var candidateWorkspace = CleanupTestData.Workspace("candidate-provenance");
        var verified = CleanupTestData.VerifiedRead(
            candidateWorkspace,
            Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-provenance.zip"));
        var selectedPath = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot);
        var selectedKey = WorkspaceIdentity.Key(selectedPath);
        var candidatePath = WorkspaceIdentity.NormalizePhysicalPath(candidateWorkspace.PhysicalRoot);
        var candidateKey = WorkspaceIdentity.Key(candidatePath);
        var association = CleanupWorkspaceAssociation.Create(
            CleanupWorkspaceAssociationState.Mismatched,
            selectedPath,
            candidatePath,
            selectedKey,
            candidateKey);
        var lease = CleanupLeaseBoundary.Create(
            CleanupLeaseBoundaryState.Required,
            selectedKey,
            CleanupDefinitions.CommandIdentity,
            verified.OperationId);
        var verification = CleanupVerificationCondition.Create(
            CleanupVerificationConditionState.SemanticFinal,
            verified.BundlePath,
            CleanupArtifactFileKind.Ordinary,
            RecoveryBundleIntegrity.Verified);
        var mismatchedProvenance = CleanupTestData.Provenance(verified) with
        {
            Command = "route move",
        };

        Assert.Throws<ArgumentException>(() => CleanupCandidate.Create(
            RecoveryBundleCandidateSnapshot.VerifiedFinal(verified),
            CleanupArtifactFileKind.Ordinary,
            association,
            lease,
            mismatchedProvenance,
            verification,
            CleanupCandidateEligibility.Blocked,
            CleanupPlanAction.Preserve));
        Assert.Throws<ArgumentException>(() => CleanupWorkspaceAssociation.Create(
            CleanupWorkspaceAssociationState.CurrentWorkspace,
            selectedPath,
            selectedPath,
            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            selectedKey));
    }

    [Fact(DisplayName = "Cleanup plan selects every eligible candidate in exact catalogue order"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void SafePlanSelectsEveryEligibleCandidateInOrder()
    {
        var workspace = CleanupTestData.Workspace("safe-plan");
        var first = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Incomplete,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.draft"));
        var second = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Verified,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"),
            operationId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        var catalogue = CleanupTestData.Catalogue(
            CleanupCatalogueCoverage.Complete,
            first,
            second);
        var plan = CleanupTestData.Plan(CleanupTestData.Request(workspace), catalogue);

        Assert.Equal(CleanupPlanSafety.Safe, plan.Safety);
        Assert.Equal([0, 1], plan.Entries.Select(entry => entry.Ordinal));
        Assert.Equal(
            [first.Path, second.Path],
            plan.Entries.Select(entry => entry.Path));
        Assert.Equal(plan.Entries, plan.DeletionEntries);
        Assert.All(
            plan.Entries,
            entry =>
            {
                Assert.Equal(CleanupCandidateEligibility.Eligible, entry.Eligibility);
                Assert.Equal(CleanupPlanAction.Delete, entry.Action);
                Assert.Equal(CleanupEffectOutcome.Planned, entry.ResultEffect.Outcome);
                Assert.Equal(CleanupEffectResidual.None, entry.ResultEffect.Residual);
            });
    }

    [Fact(DisplayName = "Cleanup blocked candidate preserves every entry and removes all deletion authority"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void BlockedCandidatePreventsEveryDeletion()
    {
        var workspace = CleanupTestData.Workspace("blocked-plan");
        var eligible = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Incomplete,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.draft"));
        var blocked = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Final,
            RecoveryBundleIntegrity.Unsupported,
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"));
        var catalogue = CleanupTestData.Catalogue(
            CleanupCatalogueCoverage.Complete,
            eligible,
            blocked);
        var plan = CleanupTestData.Plan(CleanupTestData.Request(workspace), catalogue);

        Assert.Equal(CleanupPlanSafety.Blocked, plan.Safety);
        Assert.Equal(catalogue.Candidates, plan.Entries.Select(entry => entry.Candidate));
        Assert.Empty(plan.DeletionEntries);
        Assert.Equal(
            [CleanupEffectOutcome.Planned, CleanupEffectOutcome.NotStarted],
            plan.Entries.Select(entry => entry.ResultEffect.Outcome));
        Assert.Equal(
            [CleanupEffectResidual.None, CleanupEffectResidual.Retained],
            plan.Entries.Select(entry => entry.ResultEffect.Residual));
    }

    [Theory(DisplayName = "Cleanup incomplete or interrupted coverage has no deletion entries"),
     InlineData((int)CleanupCatalogueCoverage.Incomplete),
     InlineData((int)CleanupCatalogueCoverage.Interrupted),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void IncompleteCoveragePreventsDeletion(int coverageValue)
    {
        if (!Enum.IsDefined(typeof(CleanupCatalogueCoverage), coverageValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coverageValue),
                coverageValue,
                "The Cleanup catalogue coverage is not defined.");
        }

        var coverage = (CleanupCatalogueCoverage)coverageValue;
        var workspace = CleanupTestData.Workspace($"coverage-{coverage}");
        var plan = CleanupTestData.Plan(
            CleanupTestData.Request(workspace),
            CleanupTestData.Catalogue(coverage));

        Assert.Equal(CleanupPlanSafety.Blocked, plan.Safety);
        Assert.Empty(plan.Entries);
        Assert.Empty(plan.DeletionEntries);
    }

    [Fact(DisplayName = "Cleanup complete empty catalogue forms a safe verified no-op plan"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void EmptyCompleteCatalogueIsSafeNoOp()
    {
        var plan = CleanupTestData.Plan(
            CleanupTestData.Request(CleanupTestData.Workspace("empty-plan")),
            CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete));

        Assert.Equal(CleanupPlanSafety.Safe, plan.Safety);
        Assert.Empty(plan.Entries);
        Assert.Empty(plan.DeletionEntries);
    }

    [Fact(DisplayName = "Cleanup plan requires contiguous catalogue entry identity and exact deletion references"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void PlanRequiresContiguousEntryIdentity()
    {
        var workspace = CleanupTestData.Workspace("plan-identity");
        var candidate = CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-a.zip"));
        var catalogue = CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete, candidate);
        var request = CleanupTestData.Request(workspace);
        var effectCondition = CleanupEffectCondition.Create(
            CleanupEffectOutcome.Planned,
            CleanupEffectResidual.None);
        var wrongOrdinal = CleanupPlanEntry.Create(1, candidate, effectCondition);

        Assert.Throws<ArgumentException>(() => CleanupPlan.Create(
            request,
            catalogue,
            [wrongOrdinal],
            [wrongOrdinal],
            CleanupPlanSafety.Safe));

        var validEntry = CleanupPlanEntry.Create(0, candidate, effectCondition);
        var foreignEntry = CleanupPlanEntry.Create(
            0,
            CleanupTestData.Candidate(
                selectedWorkspace: workspace,
                path: Path.Combine(Path.GetTempPath(), "cleanup-recovery", "operation-b.zip"),
                operationId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")),
            effectCondition);
        Assert.Throws<ArgumentException>(() => CleanupPlan.Create(
            request,
            catalogue,
            [validEntry],
            [foreignEntry],
            CleanupPlanSafety.Safe));
    }
}
