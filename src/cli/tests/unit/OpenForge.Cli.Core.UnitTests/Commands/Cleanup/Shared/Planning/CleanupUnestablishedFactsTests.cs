using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup.Shared.Planning;

public sealed class CleanupUnestablishedFactsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Invalid Cleanup ingress can retain empty unestablished facts without inventing a request or workspace"),
     Trait("Feature", "cleanup-gray-corrections"), Trait("Evidence", "UnitContract")]
    public void UnestablishedPlanHasNoRequestOrDeletionAuthority()
    {
        var plan = CleanupPlan.NotEstablished();
        var facts = CleanupResultFacts.Create(
            plan,
            new CleanupPreflight { State = CleanupPreflightState.NotRequested },
            new CleanupLease { State = CleanupLeaseState.NotRequested },
            new CleanupCatalogueComparison { State = CleanupCatalogueComparisonState.NotRequested },
            [],
            [],
            new CleanupVerification { State = CleanupVerificationState.NotRequested },
            [CleanupFinding.Create(CleanupFindingCode.InvalidInput, "The operand is invalid.")]);

        Assert.Null(plan.Request);
        Assert.Equal(CleanupPlanSafety.NotEstablished, plan.Safety);
        Assert.Equal(CleanupCatalogueCoverage.NotEstablished, plan.Catalogue.Coverage);
        Assert.Empty(plan.Catalogue.Candidates);
        Assert.Empty(plan.Entries);
        Assert.Empty(plan.DeletionEntries);
        Assert.Equal(CleanupMode.NotEstablished, facts.Mode);
        Assert.Empty(facts.Effects);
        Assert.Empty(facts.Residuals);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Unavailable non-ordinary drafts are represented only as blocked preservation"), Trait("Feature", "cleanup-gray-corrections"), Trait("Evidence", "UnitContract")]
    public void UnavailableDraftCannotBecomeDeletionEligible()
    {
        var candidate = CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Unavailable,
            fileKind: CleanupArtifactFileKind.NonOrdinary);

        Assert.Equal(CleanupCandidateEligibility.Blocked, candidate.Eligibility);
        Assert.Equal(CleanupPlanAction.Preserve, candidate.Action);
        Assert.Null(candidate.Provenance);
        Assert.Equal(candidate.Path, candidate.Verification.ExpectedPath);
        Assert.Equal(RecoveryBundleIntegrity.Unavailable, candidate.Verification.ExpectedIntegrity);
        Assert.Throws<ArgumentException>(() => CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Unavailable,
            fileKind: CleanupArtifactFileKind.Ordinary));
        Assert.Throws<ArgumentException>(() => CleanupTestData.Candidate(
            RecoveryBundleCandidateKind.Draft,
            RecoveryBundleIntegrity.Unavailable,
            fileKind: CleanupArtifactFileKind.NonOrdinary,
            eligibility: CleanupCandidateEligibility.Eligible,
            action: CleanupPlanAction.Delete));
    }
}
