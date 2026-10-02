using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreateRecoveryConflictCoordinateIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Create keeps the observed recovery candidate coordinate and blocks without effects"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("valid-final")]
    [InlineData("draft")]
    [InlineData("malformed-final")]
    public async Task ExistingRecoveryCandidateUsesObservedPathWithoutApplying(
        string candidateKind)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            $"route-create-recovery-coordinate-{candidateKind}");
        workspace.SeedBase();
        var candidate = await SeedCandidateAsync(workspace.Workspace, candidateKind);
        var before = workspace.SnapshotHashes();

        try
        {
            var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(),
                    TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(5, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
            Assert.Equal(RouteCreateIntegrationWorkspace.TargetId, result.Target.Requested);
            var finding = Assert.Single(
                result.Findings,
                item => item.Code == RouteCreateFindingCode.RecoveryConflict);
            Assert.Equal(candidate.Path, finding.Target);
            Assert.Equal(RouteCreateRecoveryState.Retained, result.Recovery.State);
            Assert.Equal(candidate.Path, result.Recovery.ResidualPath);
            Assert.All(
                result.Effects,
                effect => Assert.Equal(RouteCreateEffectOutcome.NotStarted, effect.Outcome));
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(
                candidate.Bytes,
                await File.ReadAllBytesAsync(candidate.Path, TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(candidate.Path);
        }
    }

    [Trait("Boundary", "Managed")]
    [Theory(DisplayName = "Route Create maps stored blocked preparation only when a residual coordinate exists"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoredBlockedPreparationUsesOnlyItsObservedResidualPath(bool hasResidualPath)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-stored-blocked-coordinate");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        Assert.NotEmpty(plan.RecoveryTargets);
        var before = workspace.SnapshotHashes();
        var cause = "The recovery archive publication failed after candidate inspection.";
        var failure = new FilesystemFailure(
            FilesystemFailureKind.InputOutput,
            "The recovery archive could not be published.");
        string? candidatePath = null;
        if (hasResidualPath)
        {
            candidatePath = Path.Combine(
                Path.GetTempPath(),
                "task64-observed-recovery-candidate",
                "create",
                "observed.bundle");
        }

        var stored = RecoveryBundlePreparationResult.Blocked(cause, candidatePath, failure);
        var mapped = RouteCreateRecoveryLifecycle.FromPreparation(stored);

        Assert.Same(failure, stored.Failure);
        Assert.Equal(cause, stored.Cause);
        Assert.Equal(cause, mapped.Cause);
        Assert.Null(mapped.Preparation);
        Assert.Equal(candidatePath, mapped.Recovery.ResidualPath);

        var expectedState = RouteCreateRecoveryPreparationState.Incomplete;
        var expectedCode = RouteCreateFindingCode.RecoveryUnavailable;
        var expectedStatus = CliSemanticStatus.Incomplete;
        if (hasResidualPath)
        {
            expectedState = RouteCreateRecoveryPreparationState.Blocked;
            expectedCode = RouteCreateFindingCode.RecoveryConflict;
            expectedStatus = CliSemanticStatus.Blocked;
        }

        Assert.Equal(expectedState, mapped.State);
        var actualCode = RouteCreateApplicationOperation.PreparationFinding(mapped.State);
        Assert.Equal(expectedCode, actualCode);
        var actualCause = mapped.Cause
            ?? throw new InvalidOperationException("Route Create preparation omitted its cause.");
        var output = RouteCreateApplicationOperation.Failure(
            plan,
            actualCode,
            actualCause,
            mapped.Recovery);

        var finding = Assert.Single(output.Findings, item => item.Code == expectedCode);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
        Assert.Equal(actualCause, finding.Cause);
        Assert.DoesNotContain(output.Findings, item => item.Code == RouteCreateFindingCode.OperationFailed);
        var expectedTarget = plan.Preview.Target.Path ?? plan.Preview.Target.Requested;
        if (candidatePath is not null)
        {
            expectedTarget = candidatePath;
        }

        Assert.Equal(expectedTarget, finding.Target);
        Assert.All(
            output.Effects,
            effect => Assert.Equal(RouteCreateEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Create reports the first catalogue candidate when several are retained"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationSafety")]
    public async Task MultipleCandidatesPreserveCatalogueOrdering()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-recovery-coordinate-multiple");
        workspace.SeedBase();
        var first = await SeedCandidateAsync(workspace.Workspace, "valid-final");
        var second = await SeedCandidateAsync(workspace.Workspace, "draft");
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(
            workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.NotEmpty(catalogue.Candidates);
        var expected = catalogue.Candidates[0].Path;
        var expectedBytes = await File.ReadAllBytesAsync(
            expected,
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        try
        {
            var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(),
                    TestContext.Current.CancellationToken);

            var finding = Assert.Single(
                result.Findings,
                item => item.Code == RouteCreateFindingCode.RecoveryConflict);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(expected, finding.Target);
            Assert.Equal(expected, result.Recovery.ResidualPath);
            Assert.All(
                result.Effects,
                effect => Assert.Equal(RouteCreateEffectOutcome.NotStarted, effect.Outcome));
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(
                expectedBytes,
                await File.ReadAllBytesAsync(expected, TestContext.Current.CancellationToken));
            Assert.Equal(
                first.Bytes,
                await File.ReadAllBytesAsync(first.Path, TestContext.Current.CancellationToken));
            Assert.Equal(
                second.Bytes,
                await File.ReadAllBytesAsync(second.Path, TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(first.Path);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(second.Path);
        }
    }

    private static async ValueTask<(string Path, byte[] Bytes)> SeedCandidateAsync(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace,
        string candidateKind)
    {
        if (candidateKind == "valid-final")
        {
            var payloadPath = Path.Combine(
                workspace.PhysicalRoot,
                ".agents",
                "memory",
                "project-alpha",
                "_project-alpha.md");
            var payloadBytes = await File.ReadAllBytesAsync(
                payloadPath,
                TestContext.Current.CancellationToken);
            var snapshot = FileStateSnapshot.File(payloadPath, payloadPath, payloadBytes);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    workspace,
                    RouteCreateDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Create,
                        workspace),
                    Guid.NewGuid(),
                    [RecoveryBundleTarget.Create(
                        PlannedFileChange.Delete(snapshot.Expectation),
                        snapshot)]),
                TestContext.Current.CancellationToken);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            return (
                preparation.BundlePath,
                await File.ReadAllBytesAsync(
                    preparation.BundlePath,
                    TestContext.Current.CancellationToken));
        }

        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace);
        Directory.CreateDirectory(directory);
        var operationId = Guid.NewGuid();
        var candidatePath = Path.Combine(
            directory,
            candidateKind == "draft"
                ? RecoveryBundleFormatV1.DraftFileName(operationId)
                : RecoveryBundleFormatV1.FinalFileName(operationId));
        var candidateBytes = candidateKind switch
        {
            "draft" => "incomplete recovery draft"u8.ToArray(),
            "malformed-final" => "not a recovery ZIP"u8.ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(candidateKind), candidateKind, null),
        };
        await File.WriteAllBytesAsync(
            candidatePath,
            candidateBytes,
            TestContext.Current.CancellationToken);
        return (candidatePath, candidateBytes);
    }
}
