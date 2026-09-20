using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveFiniteContractTests
{
    public static TheoryData<int, string> FindingNames => new()
    {
        { (int)RouteMoveFindingCode.InvalidInput, "route-move.invalid-input" },
        { (int)RouteMoveFindingCode.InvalidSource, "route-move.invalid-source" },
        { (int)RouteMoveFindingCode.SourceNotFound, "route-move.source-not-found" },
        { (int)RouteMoveFindingCode.InvalidSubject, "route-move.invalid-subject" },
        { (int)RouteMoveFindingCode.InvalidDestination, "route-move.invalid-destination" },
        { (int)RouteMoveFindingCode.WorkspaceUnsafe, "route-move.workspace-unsafe" },
        { (int)RouteMoveFindingCode.SourceUnsafe, "route-move.source-unsafe" },
        { (int)RouteMoveFindingCode.RouteAmbiguous, "route-move.route-ambiguous" },
        { (int)RouteMoveFindingCode.IdentityCollision, "route-move.identity-collision" },
        { (int)RouteMoveFindingCode.OverwriteAmbiguous, "route-move.overwrite-ambiguous" },
        { (int)RouteMoveFindingCode.CategoryUnsafe, "route-move.category-unsafe" },
        { (int)RouteMoveFindingCode.OwnershipUnavailable, "route-move.ownership-unavailable" },
        { (int)RouteMoveFindingCode.OwnershipClaimed, "route-move.ownership-claimed" },
        { (int)RouteMoveFindingCode.DestinationUnsafe, "route-move.destination-unsafe" },
        { (int)RouteMoveFindingCode.DestinationParentMissing, "route-move.destination-parent-missing" },
        { (int)RouteMoveFindingCode.DestinationOccupied, "route-move.destination-occupied" },
        { (int)RouteMoveFindingCode.SelfMove, "route-move.self-move" },
        { (int)RouteMoveFindingCode.DestinationInsideSource, "route-move.destination-inside-source" },
        { (int)RouteMoveFindingCode.ReferenceUnsafe, "route-move.reference-unsafe" },
        { (int)RouteMoveFindingCode.GeneratedRegionUnsafe, "route-move.generated-region-unsafe" },
        { (int)RouteMoveFindingCode.WorkspaceLockUnavailable, "route-move.workspace-lock-unavailable" },
        { (int)RouteMoveFindingCode.TargetChanged, "route-move.target-changed" },
        { (int)RouteMoveFindingCode.RecoveryConflict, "route-move.recovery-conflict" },
        { (int)RouteMoveFindingCode.WorkspaceUnavailable, "route-move.workspace-unavailable" },
        { (int)RouteMoveFindingCode.CategoryInventoryIncomplete, "route-move.category-inventory-incomplete" },
        { (int)RouteMoveFindingCode.ReferenceCoverageIncomplete, "route-move.reference-coverage-incomplete" },
        { (int)RouteMoveFindingCode.ProjectionIncomplete, "route-move.projection-incomplete" },
        { (int)RouteMoveFindingCode.RecoveryUnavailable, "route-move.recovery-unavailable" },
        { (int)RouteMoveFindingCode.InspectionIncomplete, "route-move.inspection-incomplete" },
        { (int)RouteMoveFindingCode.RecoveryArtifactRetained, "route-move.recovery-artifact-retained" },
        { (int)RouteMoveFindingCode.TargetChangedDuringApply, "route-move.target-changed-during-apply" },
        { (int)RouteMoveFindingCode.WriteFailed, "route-move.write-failed" },
        { (int)RouteMoveFindingCode.VerificationFailed, "route-move.verification-failed" },
        { (int)RouteMoveFindingCode.RecoveryFailed, "route-move.recovery-failed" },
        { (int)RouteMoveFindingCode.OperationFailed, "route-move.operation-failed" },
        { (int)RouteMoveFindingCode.Interrupted, "route-move.interrupted" },
    };

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move result and operation finite graphs are closed and exact")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void FiniteGraphsContainOnlyAcceptedValues()
    {
        AssertNames<RouteMoveMode>("Apply", "DryRun");
        AssertNames<RouteMoveSourceSelection>("SourceId", "BasePath", "OverwritePath");
        AssertNames<RouteMoveSourceForm>("OrdinaryMarkdown", "CanonicalEntrypoint", "CompatibilityEntrypoint");
        AssertNames<RouteMoveSubjectKind>("Leaf", "Category");
        AssertNames<RouteMoveLayerKind>("Base", "Overwrite");
        AssertNames<RouteMoveItemKind>(
            "Directory", "Entrypoint", "RoutedMarkdown", "UnroutedMarkdown", "NativeSource", "Resource");
        AssertNames<RouteMoveOwnershipState>("NotEstablished", "Unmanaged", "Claimed", "Blocked", "Interrupted");
        AssertNames<RouteMoveOwnershipTrust>("NotEstablished", "Trusted", "Blocked", "Interrupted");
        AssertNames<RouteMoveOwnershipManager>("Framework", "Extension");
        AssertNames<RouteMoveCoverage>("NotEstablished", "Incomplete", "Complete", "Blocked", "Interrupted");
        AssertNames<RouteMoveGeneratedReason>("OldParent", "NewParent", "Loader", "MovedEntrypoint");
        AssertNames<RouteMoveGeneratedState>("Changed", "Unchanged");
        AssertNames<RouteMovePlanCompleteness>("NotEstablished", "Incomplete", "Complete");
        AssertNames<RouteMovePlanSafety>("NotEstablished", "Safe", "Blocked");
        AssertNames<RouteMoveEffectKind>("Directory", "MovedFile", "ReferenceSource", "GeneratedRegion");
        AssertNames<RouteMoveEffectAction>("Create", "Replace", "Delete");
        AssertNames<RouteMovePathStateKind>("Missing", "File", "Directory");
        AssertNames<RouteMoveEffectOutcome>(
            "Planned", "NotStarted", "Verified", "VerificationFailed", "CompletionUnknown");
        AssertNames<RouteMoveEffectResidual>("None", "Retained", "Unknown");
        AssertNames<RouteMoveRecoveryState>("NotRequired", "NotCreated", "Removed", "Retained", "Unknown");
        AssertNames<RouteMoveVerificationState>("NotRequested", "Verified", "Failed", "Unknown");
        AssertNames<RouteMovePlanRevalidationState>("Exact", "Changed", "Failed", "Interrupted");
        AssertNames<RouteMoveAppliedVerificationState>("Verified", "Failed", "Interrupted");
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Move finding machine names match the complete independent oracle"),
        MemberData(nameof(FindingNames))]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void FindingMachineNamesAreComplete(
        int codeValue,
        string expected)
    {
        var code = (RouteMoveFindingCode)codeValue;
        var actual = RouteMoveDefinitions.ReadMachineName(code);

        Assert.Equal(expected, actual);
        Assert.Equal(Enum.GetValues<RouteMoveFindingCode>().Length, FindingNames.Count);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move request and subject machine-name mappings are exact")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void RequestAndSubjectMachineNamesAreExact()
    {
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveMode.Apply, "apply"),
            (RouteMoveMode.DryRun, "dry-run"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveSourceSelection.SourceId, "source-id"),
            (RouteMoveSourceSelection.BasePath, "base-path"),
            (RouteMoveSourceSelection.OverwritePath, "overwrite-path"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveSourceForm.OrdinaryMarkdown, "ordinary-markdown"),
            (RouteMoveSourceForm.CanonicalEntrypoint, "canonical-entrypoint"),
            (RouteMoveSourceForm.CompatibilityEntrypoint, "compatibility-entrypoint"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveSubjectKind.Leaf, "leaf"),
            (RouteMoveSubjectKind.Category, "category"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveLayerKind.Base, "base"),
            (RouteMoveLayerKind.Overwrite, "overwrite"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveItemKind.Directory, "directory"),
            (RouteMoveItemKind.Entrypoint, "entrypoint"),
            (RouteMoveItemKind.RoutedMarkdown, "routed-markdown"),
            (RouteMoveItemKind.UnroutedMarkdown, "unrouted-markdown"),
            (RouteMoveItemKind.NativeSource, "native-source"),
            (RouteMoveItemKind.Resource, "resource"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move ownership and coverage machine-name mappings are exact")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void OwnershipAndCoverageMachineNamesAreExact()
    {
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveOwnershipState.NotEstablished, "not-established"),
            (RouteMoveOwnershipState.Unmanaged, "unmanaged"),
            (RouteMoveOwnershipState.Claimed, "claimed"),
            (RouteMoveOwnershipState.Blocked, "blocked"),
            (RouteMoveOwnershipState.Interrupted, "interrupted"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveOwnershipTrust.NotEstablished, "not-established"),
            (RouteMoveOwnershipTrust.Trusted, "trusted"),
            (RouteMoveOwnershipTrust.Blocked, "blocked"),
            (RouteMoveOwnershipTrust.Interrupted, "interrupted"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveOwnershipManager.Framework, "framework"),
            (RouteMoveOwnershipManager.Extension, "extension"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveCoverage.NotEstablished, "not-established"),
            (RouteMoveCoverage.Incomplete, "incomplete"),
            (RouteMoveCoverage.Complete, "complete"),
            (RouteMoveCoverage.Blocked, "blocked"),
            (RouteMoveCoverage.Interrupted, "interrupted"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move generated and plan machine-name mappings are exact")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void GeneratedAndPlanMachineNamesAreExact()
    {
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveGeneratedReason.OldParent, "old-parent"),
            (RouteMoveGeneratedReason.NewParent, "new-parent"),
            (RouteMoveGeneratedReason.Loader, "loader"),
            (RouteMoveGeneratedReason.MovedEntrypoint, "moved-entrypoint"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveGeneratedState.Changed, "changed"),
            (RouteMoveGeneratedState.Unchanged, "unchanged"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMovePlanCompleteness.NotEstablished, "not-established"),
            (RouteMovePlanCompleteness.Incomplete, "incomplete"),
            (RouteMovePlanCompleteness.Complete, "complete"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMovePlanSafety.NotEstablished, "not-established"),
            (RouteMovePlanSafety.Safe, "safe"),
            (RouteMovePlanSafety.Blocked, "blocked"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move effect and terminal machine-name mappings are exact")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void EffectAndTerminalMachineNamesAreExact()
    {
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveEffectKind.Directory, "directory"),
            (RouteMoveEffectKind.MovedFile, "moved-file"),
            (RouteMoveEffectKind.ReferenceSource, "reference-source"),
            (RouteMoveEffectKind.GeneratedRegion, "generated-region"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveEffectAction.Create, "create"),
            (RouteMoveEffectAction.Replace, "replace"),
            (RouteMoveEffectAction.Delete, "delete"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMovePathStateKind.Missing, "missing"),
            (RouteMovePathStateKind.File, "file"),
            (RouteMovePathStateKind.Directory, "directory"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveEffectOutcome.Planned, "planned"),
            (RouteMoveEffectOutcome.NotStarted, "not-started"),
            (RouteMoveEffectOutcome.Verified, "verified"),
            (RouteMoveEffectOutcome.VerificationFailed, "verification-failed"),
            (RouteMoveEffectOutcome.CompletionUnknown, "completion-unknown"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveEffectResidual.None, "none"),
            (RouteMoveEffectResidual.Retained, "retained"),
            (RouteMoveEffectResidual.Unknown, "unknown"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveRecoveryState.NotRequired, "not-required"),
            (RouteMoveRecoveryState.NotCreated, "not-created"),
            (RouteMoveRecoveryState.Removed, "removed"),
            (RouteMoveRecoveryState.Retained, "retained"),
            (RouteMoveRecoveryState.Unknown, "unknown"));
        AssertMappings(
            RouteMoveDefinitions.ReadMachineName,
            (RouteMoveVerificationState.NotRequested, "not-requested"),
            (RouteMoveVerificationState.Verified, "verified"),
            (RouteMoveVerificationState.Failed, "failed"),
            (RouteMoveVerificationState.Unknown, "unknown"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Every Route Move machine-name finite rejects one undefined value")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void MachineNameFinitesRejectUndefinedValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveMode)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveSourceSelection)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveSourceForm)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveSubjectKind)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveLayerKind)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveItemKind)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveOwnershipState)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveOwnershipTrust)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveOwnershipManager)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveCoverage)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveGeneratedReason)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveGeneratedState)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMovePlanCompleteness)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMovePlanSafety)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveEffectKind)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveEffectAction)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMovePathStateKind)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveEffectOutcome)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveEffectResidual)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveRecoveryState)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveVerificationState)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteMoveDefinitions.ReadMachineName((RouteMoveFindingCode)99));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Move result builder maps every semantic status"),
        InlineData((int)CliSemanticStatus.Complete, (int)RouteMoveFindingCode.InvalidInput, false),
        InlineData((int)CliSemanticStatus.Attention, (int)RouteMoveFindingCode.RecoveryArtifactRetained, true),
        InlineData((int)CliSemanticStatus.Incomplete, (int)RouteMoveFindingCode.ReferenceCoverageIncomplete, true),
        InlineData((int)CliSemanticStatus.Invalid, (int)RouteMoveFindingCode.InvalidInput, true),
        InlineData((int)CliSemanticStatus.Blocked, (int)RouteMoveFindingCode.DestinationOccupied, true),
        InlineData((int)CliSemanticStatus.Failed, (int)RouteMoveFindingCode.WriteFailed, true),
        InlineData((int)CliSemanticStatus.Interrupted, (int)RouteMoveFindingCode.Interrupted, true)]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void ResultBuilderMapsEverySemanticStatus(
        int statusValue,
        int findingCodeValue,
        bool includeFinding)
    {
        var expectedStatus = (CliSemanticStatus)statusValue;
        var findings = includeFinding
            ? new[] { RouteMoveTestData.Finding((RouteMoveFindingCode)findingCodeValue, expectedStatus) }
            : [];

        var result = new RouteMoveResultBuilder().Build(
            RouteMoveTestData.Formation(RouteMoveMode.Apply, findings));

        Assert.Equal(expectedStatus, result.Status);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Move result builder applies exact next-action precedence"),
        InlineData((int)RouteMoveFindingCode.RecoveryArtifactRetained, (int)CliSemanticStatus.Attention, "open-forge cleanup", "Review and remove the reported recovery artifact after confirming the verified Route Move result."),
        InlineData((int)RouteMoveFindingCode.DestinationParentMissing, (int)CliSemanticStatus.Blocked, "open-forge route init", "Initialize the exact missing destination parent route, then rerun Route Move."),
        InlineData((int)RouteMoveFindingCode.InvalidDestination, (int)CliSemanticStatus.Invalid, "open-forge route move --help", "Correct the named Route Move input, then rerun the request."),
        InlineData((int)RouteMoveFindingCode.WorkspaceLockUnavailable, (int)CliSemanticStatus.Blocked, "open-forge route move", "Wait for the blocking condition or inspect the changed target, then rerun Route Move from a fresh plan."),
        InlineData((int)RouteMoveFindingCode.OwnershipClaimed, (int)CliSemanticStatus.Blocked, "open-forge doctor", "Inspect the blocked workspace, route, ownership, reference, generated-region, destination, or recovery boundary before rerunning Route Move."),
        InlineData((int)RouteMoveFindingCode.ReferenceCoverageIncomplete, (int)CliSemanticStatus.Incomplete, "open-forge doctor", "Inspect the unavailable inventory, reference, projection, or recovery facts before relying on this Route Move result."),
        InlineData((int)RouteMoveFindingCode.WriteFailed, (int)CliSemanticStatus.Failed, "open-forge route move --detail debug", "Report the failure and retry the same Route Move request with bounded diagnostics."),
        InlineData((int)RouteMoveFindingCode.Interrupted, (int)CliSemanticStatus.Interrupted, "open-forge route move", "Rerun the same Route Move request.")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void ResultBuilderAppliesExactNextAction(
        int codeValue,
        int statusValue,
        string command,
        string reason)
    {
        var finding = RouteMoveTestData.Finding(
            (RouteMoveFindingCode)codeValue,
            (CliSemanticStatus)statusValue);

        var result = new RouteMoveResultBuilder().Build(
            RouteMoveTestData.Formation(RouteMoveMode.Apply, finding));

        Assert.Equal(command, result.Next?.Command);
        Assert.Equal(reason, result.Next?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Move complete result has no next action")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void CompleteResultHasNoNextAction()
    {
        var result = new RouteMoveResultBuilder().Build(RouteMoveTestData.Formation());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Next);
    }

    private static void AssertNames<T>(params string[] expected)
        where T : struct, Enum
        => Assert.Equal(expected, Enum.GetNames<T>());

    private static void AssertMappings<T>(
        Func<T, string> read,
        params (T Value, string Expected)[] mappings)
        where T : struct, Enum
    {
        Assert.Equal(Enum.GetValues<T>().Length, mappings.Length);
        foreach (var mapping in mappings)
        {
            Assert.Equal(mapping.Expected, read(mapping.Value));
        }
    }
}
