using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveFiniteContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove finite machine names retain the accepted wire vocabulary"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void FiniteMachineNamesRetainAcceptedVocabulary()
    {
        Assert.Equal(
            ["apply", "dry-run"],
            Enum.GetValues<RouteRemoveMode>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["source-id", "base-path", "overwrite-path"],
            Enum.GetValues<RouteRemoveSourceSelection>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["ordinary-markdown", "canonical-entrypoint", "compatibility-entrypoint"],
            Enum.GetValues<RouteRemoveSourceForm>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["leaf", "category"],
            Enum.GetValues<RouteRemoveSubjectKind>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["base", "overwrite"],
            Enum.GetValues<RouteRemoveLayerKind>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["directory", "entrypoint", "routed-markdown", "unrouted-markdown", "native-source", "resource"],
            Enum.GetValues<RouteRemoveItemKind>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "unmanaged", "claimed", "blocked", "interrupted"],
            Enum.GetValues<RouteRemoveOwnershipState>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "trusted", "blocked", "interrupted"],
            Enum.GetValues<RouteRemoveOwnershipTrust>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["framework", "extension"],
            Enum.GetValues<RouteRemoveOwnershipManager>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "incomplete", "complete", "blocked", "interrupted"],
            Enum.GetValues<RouteRemoveCoverage>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["old-parent", "loader"],
            Enum.GetValues<RouteRemoveGeneratedReason>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["changed", "unchanged"],
            Enum.GetValues<RouteRemoveGeneratedState>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "incomplete", "complete"],
            Enum.GetValues<RouteRemovePlanCompleteness>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-established", "safe", "blocked"],
            Enum.GetValues<RouteRemovePlanSafety>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["directory", "removed-file", "reference-source", "generated-region"],
            Enum.GetValues<RouteRemoveEffectKind>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["replace", "delete"],
            Enum.GetValues<RouteRemoveEffectAction>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["missing", "file", "directory"],
            Enum.GetValues<RouteRemovePathStateKind>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["planned", "not-started", "verified", "verification-failed", "completion-unknown"],
            Enum.GetValues<RouteRemoveEffectOutcome>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["none", "retained", "unknown"],
            Enum.GetValues<RouteRemoveEffectResidual>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-required", "not-created", "removed", "retained", "unknown"],
            Enum.GetValues<RouteRemoveRecoveryState>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "verified", "failed", "unknown"],
            Enum.GetValues<RouteRemoveVerificationState>().Select(RouteRemoveDefinitions.ReadMachineName));
        Assert.Equal(
            [
                "route-remove.invalid-input",
                "route-remove.confirmation-required",
                "route-remove.invalid-source",
                "route-remove.source-not-found",
                "route-remove.invalid-subject",
                "route-remove.workspace-unsafe",
                "route-remove.source-unsafe",
                "route-remove.route-ambiguous",
                "route-remove.identity-collision",
                "route-remove.overwrite-ambiguous",
                "route-remove.category-unsafe",
                "route-remove.ownership-unavailable",
                "route-remove.ownership-claimed",
                "route-remove.reference-unsafe",
                "route-remove.generated-region-unsafe",
                "route-remove.workspace-lock-unavailable",
                "route-remove.target-changed",
                "route-remove.recovery-conflict",
                "route-remove.workspace-unavailable",
                "route-remove.category-inventory-incomplete",
                "route-remove.reference-coverage-incomplete",
                "route-remove.projection-incomplete",
                "route-remove.recovery-unavailable",
                "route-remove.inspection-incomplete",
                "route-remove.recovery-artifact-retained",
                "route-remove.target-changed-during-apply",
                "route-remove.write-failed",
                "route-remove.verification-failed",
                "route-remove.recovery-failed",
                "route-remove.operation-failed",
                "route-remove.interrupted",
            ],
            Enum.GetValues<RouteRemoveFindingCode>().Select(RouteRemoveDefinitions.ReadMachineName));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove rejects undefined finite values instead of inventing wire names"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void UndefinedFiniteValuesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveSourceSelection)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveSourceForm)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveSubjectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveLayerKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveItemKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveOwnershipState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveOwnershipTrust)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveOwnershipManager)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveCoverage)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveGeneratedReason)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveGeneratedState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemovePlanCompleteness)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemovePlanSafety)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemovePathStateKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveVerificationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RouteRemoveDefinitions.ReadMachineName((RouteRemoveFindingCode)int.MaxValue));
    }
}
