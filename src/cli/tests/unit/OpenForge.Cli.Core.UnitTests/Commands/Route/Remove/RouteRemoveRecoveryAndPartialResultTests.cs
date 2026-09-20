using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveRecoveryAndPartialResultTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove plan revalidation requires a cause for every non-exact state"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void PlanRevalidationRequiresCauseForNonExactStates()
    {
        var exact = new RouteRemovePlanRevalidation(
            RouteRemovePlanRevalidationState.Exact,
            null);
        var changed = new RouteRemovePlanRevalidation(
            RouteRemovePlanRevalidationState.Changed,
            "The planned target changed.");

        Assert.Equal(RouteRemovePlanRevalidationState.Exact, exact.State);
        Assert.Null(exact.Cause);
        Assert.Equal(RouteRemovePlanRevalidationState.Changed, changed.State);
        Assert.Equal("The planned target changed.", changed.Cause);
        Assert.Throws<ArgumentException>(
            () => new RouteRemovePlanRevalidation(
                RouteRemovePlanRevalidationState.Exact,
                "unexpected cause"));
        Assert.Throws<ArgumentException>(
            () => new RouteRemovePlanRevalidation(
                RouteRemovePlanRevalidationState.Changed,
                " "));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RouteRemovePlanRevalidation(
                (RouteRemovePlanRevalidationState)int.MaxValue,
                "invalid state"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove applied verification preserves verified and interrupted boundaries"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void AppliedVerificationPreservesBoundaries()
    {
        var verified = new RouteRemoveAppliedVerification(
            RouteRemoveAppliedVerificationState.Verified,
            null);
        var failed = new RouteRemoveAppliedVerification(
            RouteRemoveAppliedVerificationState.Failed,
            "The removed target could not be verified.");
        var interrupted = new RouteRemoveAppliedVerification(
            RouteRemoveAppliedVerificationState.Interrupted,
            "The operation was cancelled.");

        Assert.Equal(RouteRemoveAppliedVerificationState.Verified, verified.State);
        Assert.Null(verified.Cause);
        Assert.Equal(RouteRemoveAppliedVerificationState.Failed, failed.State);
        Assert.Equal(RouteRemoveAppliedVerificationState.Interrupted, interrupted.State);
        Assert.Throws<ArgumentException>(
            () => new RouteRemoveAppliedVerification(
                RouteRemoveAppliedVerificationState.Verified,
                "unexpected cause"));
        Assert.Throws<ArgumentException>(
            () => new RouteRemoveAppliedVerification(
                RouteRemoveAppliedVerificationState.Failed,
                null));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove partial progress retains receipts recovery verification and findings together"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void PartialProgressRetainsAllOutcomeCoordinates()
    {
        var finding = RouteRemoveTestData.Finding(
            RouteRemoveFindingCode.RecoveryArtifactRetained,
            CliSemanticStatus.Attention,
            cause: "The recovery artifact remains for cleanup.");
        var progress = new RouteRemoveApplicationProgress
        {
            Receipts = [],
            Recovery = RouteRemoveTestData.Recovery(
                RouteRemoveRecoveryState.Retained,
                "recovery.zip"),
            Verification = RouteRemoveVerificationState.Verified,
            Findings = [finding],
        };

        Assert.Empty(progress.Receipts);
        Assert.Equal(RouteRemoveRecoveryState.Retained, progress.Recovery.State);
        Assert.Equal(RouteRemoveVerificationState.Verified, progress.Verification);
        Assert.Same(finding, Assert.Single(progress.Findings));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove recovery preparation records pre-effect state without claiming completion"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void RecoveryPreparationRecordsPreEffectState()
    {
        var preparation = new RouteRemoveRecoveryPreparationResult
        {
            State = RouteRemoveRecoveryPreparationState.Incomplete,
            Preparation = null,
            Recovery = RouteRemoveTestData.Recovery(RouteRemoveRecoveryState.NotCreated),
            Finding = RouteRemoveTestData.Finding(
                RouteRemoveFindingCode.RecoveryUnavailable,
                CliSemanticStatus.Incomplete),
        };

        Assert.Equal(RouteRemoveRecoveryPreparationState.Incomplete, preparation.State);
        Assert.Null(preparation.Preparation);
        Assert.Equal(RouteRemoveRecoveryState.NotCreated, preparation.Recovery.State);
        Assert.Equal(RouteRemoveFindingCode.RecoveryUnavailable, preparation.Finding?.Code);
    }
}
