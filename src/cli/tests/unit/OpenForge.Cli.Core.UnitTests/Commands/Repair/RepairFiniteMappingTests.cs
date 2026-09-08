using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairFiniteMappingTests
{
    [Fact(DisplayName = "Repair finite values map to the exact machine vocabulary and preserve enum coverage"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void FiniteValuesMapExactly()
    {
        Assert.Equal(
            ["apply", "dry-run"],
            Enum.GetValues<RepairMode>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            [
                "interactive-wizard",
                "automatic",
                "explicit-relinks",
                "automatic-and-explicit",
                "non-interactive-blocked",
            ],
            Enum.GetValues<RepairSelectionMode>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            [
                "same-target-path",
                "same-target-case",
                "same-target-encoding",
                "unique-canonical-fragment",
                "missing-target-relink",
            ],
            Enum.GetValues<RepairCatalogueMember>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["automatic", "wizard", "explicit-relink"],
            Enum.GetValues<RepairSelectionOrigin>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["filename", "title", "literal-content", "route-neighborhood"],
            Enum.GetValues<RepairCandidateEvidenceKind>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["none", "one", "several"],
            Enum.GetValues<RepairCandidateCardinality>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["workspace-containment", "route-and-heading", "local-reference", "library-record", "library-residual"],
            Enum.GetValues<RepairDependencyDomain>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["destination-literal", "same-target-identity", "resulting-bytes", "no-follow-identity", "prior-state"],
            Enum.GetValues<RepairVerificationKind>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["required", "not-required"],
            Enum.GetValues<RepairRecoveryRequirementKind>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["planned", "no-op", "applied", "verified", "blocked", "failed", "interrupted"],
            Enum.GetValues<RepairStepOutcome>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            [
                "overlapping-changes",
                "expected-state-mismatch",
                "target-identity-mismatch",
                "unsafe-boundary",
                "missing-authority",
                "incomplete-facts",
            ],
            Enum.GetValues<RepairConflictKind>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "complete", "incomplete", "blocked"],
            Enum.GetValues<RepairCoverageState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "ready", "incomplete", "blocked"],
            Enum.GetValues<RepairPreflightState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "confirmed", "not-started", "applied", "failed", "interrupted", "unknown"],
            Enum.GetValues<RepairApplicationState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "planned", "verified", "failed", "unknown"],
            Enum.GetValues<RepairVerificationState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            [
                "not-required",
                "not-created",
                "prepared",
                "removed",
                "retained",
                "incomplete",
                "blocked",
                "unknown",
            ],
            Enum.GetValues<RepairRecoveryState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["none", "retained", "unknown"],
            Enum.GetValues<RepairResidualState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "complete", "incomplete", "blocked", "failed"],
            Enum.GetValues<RepairPostDiagnosisState>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["missing", "file", "directory"],
            Enum.GetValues<FileExpectationKind>().Select(RepairDefinitions.ReadMachineName));
        Assert.Equal(
            ["create", "replace", "delete", "replace-generated-region"],
            Enum.GetValues<PlannedFileChangeKind>().Select(RepairDefinitions.ReadMachineName));
    }

    [Fact(DisplayName = "Repair finding definitions map every accepted code to its exact status and machine name"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void FindingDefinitionsAreComplete()
    {
        var expected = new (RepairFindingCode Code, string Name, CliSemanticStatus Status)[]
        {
            (RepairFindingCode.InvalidInput, "repair.invalid-input", CliSemanticStatus.Invalid),
            (RepairFindingCode.RelinkInvalid, "repair.relink-invalid", CliSemanticStatus.Invalid),
            (RepairFindingCode.ContradictoryRelink, "repair.contradictory-relink", CliSemanticStatus.Invalid),
            (RepairFindingCode.SelectionRequired, "repair.selection-required", CliSemanticStatus.Blocked),
            (RepairFindingCode.DiagnosisIncomplete, "repair.diagnosis-incomplete", CliSemanticStatus.Incomplete),
            (RepairFindingCode.DiagnosisBlocked, "repair.diagnosis-blocked", CliSemanticStatus.Blocked),
            (RepairFindingCode.ProposalUnavailable, "repair.proposal-unavailable", CliSemanticStatus.Incomplete),
            (RepairFindingCode.ProposalUnsupported, "repair.proposal-unsupported", CliSemanticStatus.Blocked),
            (RepairFindingCode.MissingAuthority, "repair.missing-authority", CliSemanticStatus.Blocked),
            (RepairFindingCode.TargetChanged, "repair.target-changed", CliSemanticStatus.Blocked),
            (RepairFindingCode.TargetUnsafe, "repair.target-unsafe", CliSemanticStatus.Blocked),
            (RepairFindingCode.PlanConflict, "repair.plan-conflict", CliSemanticStatus.Blocked),
            (RepairFindingCode.WorkspaceLockUnavailable, "repair.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            (RepairFindingCode.RecoveryUnavailable, "repair.recovery-unavailable", CliSemanticStatus.Incomplete),
            (RepairFindingCode.RecoveryConflict, "repair.recovery-conflict", CliSemanticStatus.Blocked),
            (RepairFindingCode.GuidedFindingRemaining, "repair.guided-finding-remaining", CliSemanticStatus.Attention),
            (RepairFindingCode.ManualFindingRemaining, "repair.manual-finding-remaining", CliSemanticStatus.Attention),
            (RepairFindingCode.RecoveryArtifactRetained, "repair.recovery-artifact-retained", CliSemanticStatus.Attention),
            (RepairFindingCode.WriteFailed, "repair.write-failed", CliSemanticStatus.Failed),
            (RepairFindingCode.VerificationFailed, "repair.verification-failed", CliSemanticStatus.Failed),
            (RepairFindingCode.RecoveryFailed, "repair.recovery-failed", CliSemanticStatus.Failed),
            (RepairFindingCode.OperationFailed, "repair.operation-failed", CliSemanticStatus.Failed),
            (RepairFindingCode.Interrupted, "repair.interrupted", CliSemanticStatus.Interrupted),
        };

        Assert.Equal(Enum.GetValues<RepairFindingCode>(), expected.Select(value => value.Code));
        foreach (var value in expected)
        {
            var definition = RepairDefinitions.Read(value.Code);
            Assert.Equal(value.Name, definition.MachineName);
            Assert.Equal(value.Status, definition.Status);
            Assert.Equal(value.Name, RepairDefinitions.ReadMachineName(value.Code));
        }
    }

    [Fact(DisplayName = "Repair finite mappings reject unnamed runtime enum values"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void UndefinedFiniteValuesFailClosed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairSelectionMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairCatalogueMember)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairSelectionOrigin)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairCandidateEvidenceKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairCandidateCardinality)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairDependencyDomain)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairVerificationKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairRecoveryRequirementKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairStepOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairConflictKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairCoverageState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairPreflightState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairApplicationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairVerificationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairResidualState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairPostDiagnosisState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((FileExpectationKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((PlannedFileChangeKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RepairDefinitions.ReadMachineName((RepairFindingCode)int.MaxValue));
    }
}
