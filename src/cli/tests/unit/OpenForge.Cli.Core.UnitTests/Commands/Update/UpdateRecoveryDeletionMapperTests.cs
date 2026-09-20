using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateRecoveryDeletionMapperTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update maps every recovery deletion state and disposition without losing residual certainty"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void MapsEveryDeletionStateAndDispositionWithoutLosingResidualCertainty()
    {
        var residualPath = Path.GetFullPath("update-recovery.zip");
        const string failedCause = "The verified recovery bundle could not be deleted.";
        const string blockedCause = "Recovery deletion was blocked.";
        var cases = new[]
        {
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.Deleted(),
                UpdateRecoveryState.Removed,
                ExpectedResidualPath: null,
                ExpectedFindingCode: null,
                ExpectedFindingCause: null),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.FailedRetained(residualPath, failedCause),
                UpdateRecoveryState.Retained,
                residualPath,
                UpdateFindingCode.RecoveryArtifactRetained,
                failedCause),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.FailedUnknown(residualPath, failedCause),
                UpdateRecoveryState.Unknown,
                residualPath,
                UpdateFindingCode.RecoveryFailed,
                failedCause),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.BlockedRetained(residualPath, blockedCause),
                UpdateRecoveryState.Retained,
                residualPath,
                UpdateFindingCode.RecoveryFailed,
                blockedCause),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.BlockedUnknown(blockedCause),
                UpdateRecoveryState.Unknown,
                ExpectedResidualPath: null,
                ExpectedFindingCode: UpdateFindingCode.RecoveryFailed,
                ExpectedFindingCause: blockedCause),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.CancelledRetained(residualPath),
                UpdateRecoveryState.Retained,
                residualPath,
                UpdateFindingCode.Interrupted,
                ExpectedFindingCause: null),
            new DeletionMappingCase(
                RecoveryBundleDeletionResult.CancelledUnknown(),
                UpdateRecoveryState.Unknown,
                ExpectedResidualPath: null,
                ExpectedFindingCode: UpdateFindingCode.Interrupted,
                ExpectedFindingCause: null),
        };

        foreach (var testCase in cases)
        {
            var mapped = UpdateRecoveryDeletionMapper.Map(testCase.Deletion);

            Assert.Equal(testCase.ExpectedState, mapped.State);
            Assert.Equal(testCase.ExpectedResidualPath, mapped.ResidualPath);
            Assert.Equal(testCase.ExpectedFindingCode, mapped.Finding?.Code);
            Assert.Equal(testCase.ExpectedResidualPath, mapped.Finding?.Target);
            if (testCase.ExpectedFindingCause is not null)
            {
                Assert.Equal(testCase.ExpectedFindingCause, mapped.Finding?.Cause);
            }
            else if (mapped.Finding is not null)
            {
                Assert.False(string.IsNullOrWhiteSpace(mapped.Finding.Cause));
            }
        }
    }
}

internal sealed record DeletionMappingCase(
    RecoveryBundleDeletionResult Deletion,
    UpdateRecoveryState ExpectedState,
    string? ExpectedResidualPath,
    UpdateFindingCode? ExpectedFindingCode,
    string? ExpectedFindingCause);
