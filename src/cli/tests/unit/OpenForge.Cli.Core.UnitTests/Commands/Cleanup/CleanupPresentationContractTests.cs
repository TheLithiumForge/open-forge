using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupPresentationContractTests
{
    [Fact(DisplayName = "Cleanup wire vocabulary exposes every accepted finite machine spelling"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void WireVocabularyMapsEveryFiniteValue()
    {
        Assert.Equal(
            ["complete", "failed", "attention", "incomplete", "invalid", "blocked", "interrupted"],
            Enum.GetValues<CliSemanticStatus>().Select(CleanupWireVocabulary.Status));
        Assert.Equal(
            ["current-directory", "explicit-workspace"],
            Enum.GetValues<CliWorkspaceSelectionMethod>().Select(CleanupWireVocabulary.WorkspaceSelection));
        Assert.Equal(
            ["not-established", "apply", "dry-run"],
            Enum.GetValues<CleanupMode>().Select(CleanupWireVocabulary.Mode));
        Assert.Equal(
            ["not-established", "complete", "incomplete", "interrupted"],
            Enum.GetValues<CleanupCatalogueCoverage>().Select(CleanupWireVocabulary.Coverage));
        Assert.Equal(
            ["not-established", "eligible", "blocked"],
            Enum.GetValues<CleanupCandidateEligibility>().Select(CleanupWireVocabulary.Eligibility));
        Assert.Equal(
            ["not-established", "delete", "preserve"],
            Enum.GetValues<CleanupPlanAction>().Select(CleanupWireVocabulary.Action));
        Assert.Equal(
            ["not-established", "safe", "blocked"],
            Enum.GetValues<CleanupPlanSafety>().Select(CleanupWireVocabulary.Safety));
        Assert.Equal(
            ["final", "draft"],
            Enum.GetValues<RecoveryBundleCandidateKind>().Select(CleanupWireVocabulary.CandidateKind));
        Assert.Equal(
            ["verified", "malformed", "unsupported", "unavailable", "incomplete"],
            Enum.GetValues<RecoveryBundleIntegrity>().Select(CleanupWireVocabulary.Integrity));
        Assert.Equal(
            ["not-established", "ordinary", "non-ordinary"],
            Enum.GetValues<CleanupArtifactFileKind>().Select(CleanupWireVocabulary.FileKind));
        Assert.Equal(
            ["not-established", "current-workspace", "mismatched", "unavailable"],
            Enum.GetValues<CleanupWorkspaceAssociationState>().Select(CleanupWireVocabulary.WorkspaceAssociation));
        Assert.Equal(
            ["not-established", "not-requested", "required", "held", "mismatched"],
            Enum.GetValues<CleanupLeaseBoundaryState>().Select(CleanupWireVocabulary.LeaseBoundary));
        Assert.Equal(
            ["not-established", "not-requested", "exact-path-and-kind", "semantic-final", "absence"],
            Enum.GetValues<CleanupVerificationConditionState>().Select(CleanupWireVocabulary.VerificationCondition));
        Assert.Equal(
            ["not-requested", "complete", "incomplete", "blocked", "failed", "interrupted"],
            Enum.GetValues<CleanupPreflightState>().Select(CleanupWireVocabulary.Preflight));
        Assert.Equal(
            ["not-requested", "acquired", "failed", "cancelled"],
            Enum.GetValues<CleanupLeaseState>().Select(CleanupWireVocabulary.Lease));
        Assert.Equal(
            ["not-requested", "matched", "changed", "incomplete", "blocked", "cancelled"],
            Enum.GetValues<CleanupCatalogueComparisonState>().Select(CleanupWireVocabulary.Comparison));
        Assert.Equal(
            ["planned", "not-started", "verified", "verification-failed", "completion-unknown"],
            Enum.GetValues<CleanupEffectOutcome>().Select(CleanupWireVocabulary.EffectOutcome));
        Assert.Equal(
            ["none", "retained", "unknown"],
            Enum.GetValues<CleanupEffectResidual>().Select(CleanupWireVocabulary.EffectResidual));
        Assert.Equal(
            ["not-requested", "verified", "failed", "unknown"],
            Enum.GetValues<CleanupVerificationState>().Select(CleanupWireVocabulary.Verification));
        Assert.Equal(
            ["framework", "extension", "index", "route", "repair"],
            Enum.GetValues<RecoveryBundleProducer>().Select(CleanupWireVocabulary.RecoveryProducer));
        Assert.Equal(
            ["install", "index", "create", "init", "move", "update", "remove", "repair"],
            Enum.GetValues<RecoveryBundleOperation>().Select(CleanupWireVocabulary.RecoveryOperation));
        Assert.Equal(
            ["workspace"],
            Enum.GetValues<RecoveryBundleSubjectKind>().Select(CleanupWireVocabulary.RecoverySubjectKind));
        Assert.Equal(
            [
                "cleanup.invalid-input",
                "cleanup.workspace-unavailable",
                "cleanup.workspace-not-directory",
                "cleanup.workspace-unsafe",
                "cleanup.catalogue-incomplete",
                "cleanup.recovery-final-malformed",
                "cleanup.recovery-final-unsupported",
                "cleanup.recovery-final-unavailable",
                "cleanup.recovery-draft-unsafe",
                "cleanup.workspace-lock-unavailable",
                "cleanup.catalogue-changed-during-apply",
                "cleanup.candidate-changed-during-apply",
                "cleanup.deletion-failed",
                "cleanup.verification-failed",
                "cleanup.operation-failed",
                "cleanup.interrupted",
            ],
            Enum.GetValues<CleanupFindingCode>().Select(CleanupWireVocabulary.FindingCode));
    }

    [Fact(DisplayName = "Cleanup wire vocabulary rejects every undefined finite value"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void UndefinedWireValuesThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Status((CliSemanticStatus)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.WorkspaceSelection((CliWorkspaceSelectionMethod)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Mode((CleanupMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Coverage((CleanupCatalogueCoverage)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Eligibility((CleanupCandidateEligibility)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Action((CleanupPlanAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Safety((CleanupPlanSafety)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.CandidateKind((RecoveryBundleCandidateKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Integrity((RecoveryBundleIntegrity)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.FileKind((CleanupArtifactFileKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.WorkspaceAssociation((CleanupWorkspaceAssociationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.LeaseBoundary((CleanupLeaseBoundaryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.VerificationCondition((CleanupVerificationConditionState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Preflight((CleanupPreflightState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Lease((CleanupLeaseState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Comparison((CleanupCatalogueComparisonState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.EffectOutcome((CleanupEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.EffectResidual((CleanupEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.Verification((CleanupVerificationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.FindingCode((CleanupFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.RecoveryProducer((RecoveryBundleProducer)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.RecoveryOperation((RecoveryBundleOperation)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CleanupWireVocabulary.RecoverySubjectKind((RecoveryBundleSubjectKind)int.MaxValue));
    }

    [Fact(DisplayName = "Cleanup help retains stable sections and names the non-recursive command boundary"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void HelpRetainsStableSectionsAndBoundaries()
    {
        var help = CleanupHelpSections.Create();
        var headings = help.Sections.Select(section => section.Heading);
        var text = string.Join(
            Environment.NewLine,
            help.Sections.Select(section => $"{section.Heading}\n{section.Body}"));

        Assert.Equal(
            ["Syntax", "Catalogue", "Write policy", "Global options", "Notes"],
            headings);
        Assert.Contains("open-forge cleanup [--dry-run] [global flags]", text, StringComparison.Ordinal);
        Assert.Contains("every positively recognized recovery final and ordinary exact-name draft", text, StringComparison.Ordinal);
        Assert.Contains("without acquiring a lease or writing files", text, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version", text, StringComparison.Ordinal);
        Assert.Contains("accepts no operands, selectors, prompts, confirmations", text, StringComparison.Ordinal);
        Assert.Contains("force mode, age filters, glob filters,", text, StringComparison.Ordinal);
        Assert.Contains("or recursive arbitrary deletion.", text, StringComparison.Ordinal);
        Assert.Contains("revalidates the exact catalogue before effects", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", text, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", text, StringComparison.Ordinal);
    }
}
