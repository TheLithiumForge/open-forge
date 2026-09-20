using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallRecoveryMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install cleanup maps only typed deletion disposition and residual-path facts"), Trait("Feature", "install-result"), Trait("Evidence", "Unit")]
    public void CleanupMappingPreservesExactDispositionCertainty()
    {
        var residualPath = Path.GetFullPath(Path.Combine("recovery", "candidate.zip"));
        var cases = new[]
        {
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.Deleted(),
                FindingCode = null,
                RecoveryState = InstallRecoveryState.Removed,
                ResidualPath = null,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.FailedRetained(
                    residualPath,
                    "The candidate remains."),
                FindingCode = InstallFindingCode.RecoveryArtifactRetained,
                RecoveryState = InstallRecoveryState.Retained,
                ResidualPath = residualPath,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.FailedUnknown(
                    residualPath,
                    "The candidate disposition is unknown."),
                FindingCode = InstallFindingCode.RecoveryFailed,
                RecoveryState = InstallRecoveryState.Unknown,
                ResidualPath = residualPath,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.BlockedUnknown(
                    "The candidate could not be classified."),
                FindingCode = InstallFindingCode.RecoveryFailed,
                RecoveryState = InstallRecoveryState.Unknown,
                ResidualPath = null,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.BlockedRetained(
                    residualPath,
                    "The retained candidate is unsafe to delete."),
                FindingCode = InstallFindingCode.RecoveryFailed,
                RecoveryState = InstallRecoveryState.Retained,
                ResidualPath = residualPath,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.CancelledUnknown(),
                FindingCode = InstallFindingCode.Interrupted,
                RecoveryState = InstallRecoveryState.Unknown,
                ResidualPath = null,
            },
            new RecoveryDeletionExpectation
            {
                Result = RecoveryBundleDeletionResult.CancelledRetained(residualPath),
                FindingCode = InstallFindingCode.Interrupted,
                RecoveryState = InstallRecoveryState.Retained,
                ResidualPath = residualPath,
            },
        };

        foreach (var expectation in cases)
        {
            var mapped = InstallRecoveryOperation.ReadDeletion(expectation.Result);

            Assert.Equal(expectation.FindingCode, mapped.Finding?.Code);
            Assert.Equal(expectation.RecoveryState, mapped.Recovery.State);
            Assert.Equal(expectation.ResidualPath, mapped.Recovery.ResidualPath);
        }
    }
}

internal sealed record RecoveryDeletionExpectation
{
    public required RecoveryBundleDeletionResult Result { get; init; }

    public required InstallFindingCode? FindingCode { get; init; }

    public required InstallRecoveryState RecoveryState { get; init; }

    public required string? ResidualPath { get; init; }
}
