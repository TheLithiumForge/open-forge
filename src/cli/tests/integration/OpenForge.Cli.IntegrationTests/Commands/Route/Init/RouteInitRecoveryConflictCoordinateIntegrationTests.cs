using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitRecoveryConflictCoordinateIntegrationTests
{
    private const string RouteTarget = "memory/project-alpha";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Generic Route Init keeps an observed recovery candidate coordinate while unrelated work proceeds"), Trait("Feature", "route-init"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("valid-final", false)]
    [InlineData("draft", false)]
    [InlineData("malformed-final", false)]
    [InlineData("malformed-final", true)]
    public async Task GenericPlanningReportsObservedPathAndPreservesRequestedRoute(
        string candidateKind,
        bool dryRun)
    {
        var mode = dryRun ? RouteInitMode.DryRun : RouteInitMode.Apply;
        using var workspace = GenericRouteInitIntegrationWorkspace.Create(
            $"route-init-recovery-coordinate-{candidateKind}-{mode}");
        SeedGenericNavigation(workspace);
        var initialPlan = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .PlanBuilder.BuildAsync(
                workspace.Request(RouteTarget, mode),
                TestContext.Current.CancellationToken);
        Assert.NotNull(initialPlan.Plan);
        Assert.NotEmpty(initialPlan.Plan.RecoveryTargets);

        var candidate = await SeedCandidateAsync(workspace.Workspace, candidateKind);
        var before = workspace.SnapshotHashes();
        try
        {
            var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(RouteTarget, mode),
                    TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(0, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
            Assert.DoesNotContain(result.Findings, item => item.Code == RouteInitFindingCode.RecoveryConflict);
            var report = RouteInitReportSelector.Select(result, new CliSelection(CliDetail.Standard, null));
            if (candidateKind == "valid-final")
            {
                Assert.Empty(result.RecoveryHistory.Paths);
                Assert.DoesNotContain(report.Findings, item => item.Subject.Path == candidate.Path);
            }
            else
            {
                Assert.Contains(candidate.Path, result.RecoveryHistory.Paths);
                Assert.Contains(report.Findings, item => item.Code == "route-init.recovery-history"
                    && item.Subject.Path == candidate.Path && item.Severity == CliSeverity.Warning);
            }
            Assert.NotEqual(candidate.Path, result.Recovery.ResidualPath);
            Assert.NotEmpty(result.Effects);
            if (dryRun) Assert.Equal(before, workspace.SnapshotHashes());
            else Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
            Assert.Equal(
                candidate.Bytes,
                await File.ReadAllBytesAsync(candidate.Path, TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(candidate.Path);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework Route Init dry-run preserves the requested route and does not report verified recovery history"), Trait("Feature", "route-init-framework"), Trait("Evidence", "IntegrationSafety")]
    public async Task FrameworkDryRunReportsObservedPathWithoutEffects()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-recovery-coordinate",
            TestContext.Current.CancellationToken);
        const string target = "memory/release-notes/working";
        var initialPlan = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .PlanBuilder.BuildAsync(
                workspace.Request(target, RouteInitMode.DryRun),
                TestContext.Current.CancellationToken);
        Assert.NotNull(initialPlan.Plan);
        Assert.NotEmpty(initialPlan.Plan.RecoveryTargets);

        var candidate = await SeedCandidateAsync(workspace.Workspace, "valid-final");
        var before = workspace.SnapshotHashes();
        try
        {
            var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(target, RouteInitMode.DryRun),
                    TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(0, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
            Assert.DoesNotContain(result.Findings, item => item.Code == RouteInitFindingCode.RecoveryConflict);
            Assert.Empty(result.RecoveryHistory.Paths);
            var report = RouteInitReportSelector.Select(result, new CliSelection(CliDetail.Standard, null));
            Assert.DoesNotContain(report.Findings, item => item.Subject.Path == candidate.Path);
            Assert.NotEqual(candidate.Path, result.Recovery.ResidualPath);
            Assert.NotEmpty(result.Effects);
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Generic Route Init prepares its own bundle while preserving history added after planning"), Trait("Feature", "route-init"), Trait("Evidence", "IntegrationSafety")]
    public async Task ApplicationRecoveryPreparationPreservesLaterHistory()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create(
            "route-init-recovery-coordinate-application");
        SeedGenericNavigation(workspace);
        var build = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .PlanBuilder.BuildAsync(
                workspace.Request(RouteTarget),
                TestContext.Current.CancellationToken);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Init.Models.Planning.RouteInitPlan>(
            build.Plan);
        Assert.NotEmpty(plan.RecoveryTargets);

        var candidate = await SeedCandidateAsync(workspace.Workspace, "valid-final");
        var before = workspace.SnapshotHashes();
        try
        {
            var preparation = await RouteInitRecoveryLifecycle.PrepareAsync(
                plan,
                Guid.NewGuid(),
                TestContext.Current.CancellationToken);

            Assert.Equal(RouteInitRecoveryPreparationState.Prepared, preparation.State);
            Assert.NotNull(preparation.Preparation);
            Assert.NotEqual(candidate.Path, preparation.Preparation.BundlePath);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.Preparation.BundlePath);
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
    [Theory(DisplayName = "Route Init maps stored blocked preparation only when a residual coordinate exists"), Trait("Feature", "route-init"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoredBlockedPreparationUsesOnlyItsObservedResidualPath(bool hasResidualPath)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create(
            "route-init-stored-blocked-coordinate");
        SeedGenericNavigation(workspace);
        var build = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .PlanBuilder.BuildAsync(
                workspace.Request(RouteTarget),
                TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
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
                "init",
                "observed.bundle");
        }

        var stored = RecoveryBundlePreparationResult.Blocked(cause, candidatePath, failure);
        var mapped = RouteInitRecoveryLifecycle.FromPreparation(stored);

        Assert.Same(failure, stored.Failure);
        Assert.Equal(cause, stored.Cause);
        Assert.Equal(cause, mapped.Cause);
        Assert.Null(mapped.Preparation);
        Assert.Equal(candidatePath, mapped.Recovery.ResidualPath);

        var expectedState = RouteInitRecoveryPreparationState.Incomplete;
        var expectedCode = RouteInitFindingCode.RecoveryUnavailable;
        var expectedStatus = CliSemanticStatus.Incomplete;
        if (hasResidualPath)
        {
            expectedState = RouteInitRecoveryPreparationState.Blocked;
            expectedCode = RouteInitFindingCode.RecoveryConflict;
            expectedStatus = CliSemanticStatus.Blocked;
        }

        Assert.Equal(expectedState, mapped.State);
        var actualCode = RouteInitApplicationOperation.ReadPreparationFinding(mapped.State);
        Assert.Equal(expectedCode, actualCode);
        var actualCause = mapped.Cause
            ?? throw new InvalidOperationException("Route Init preparation omitted its cause.");
        var output = RouteInitApplicationOperation.Finish(
            plan,
            [],
            [],
            mapped.Recovery,
            RouteInitVerificationState.NotRequested,
            actualCode,
            actualCause).Formation;

        var finding = Assert.Single(output.Findings, item => item.Code == expectedCode);
        Assert.Equal(expectedCode, finding.Code);
        Assert.Equal(expectedStatus, finding.Status);
        Assert.Equal(actualCause, finding.Cause);
        Assert.DoesNotContain(output.Findings, item => item.Code == RouteInitFindingCode.OperationFailed);
        var expectedTarget = plan.Preview.Target.Id ?? plan.Request.RouteTarget;
        if (candidatePath is not null)
        {
            expectedTarget = candidatePath;
        }

        Assert.Equal(expectedTarget, finding.Target);
        Assert.All(
            output.Effects,
            effect => Assert.Equal(RouteInitEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void SeedGenericNavigation(GenericRouteInitIntegrationWorkspace workspace)
    {
        workspace.WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build("- [Memory](memory/_memory.md) - #Memory"));
        workspace.WriteText(
            ".agents/memory/_memory.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Memory",
                tags: ["Memory"],
                body: $"\n{OpenForgeDocumentSeed.GeneratedEntries("- none - No entries - #Empty")}"));
    }

    private static async ValueTask<(string Path, byte[] Bytes)> SeedCandidateAsync(
        CliWorkspace workspace,
        string candidateKind)
    {
        if (candidateKind == "valid-final")
        {
            var payloadPath = Path.Combine(workspace.PhysicalRoot, ".agents", "loader.md");
            var payloadBytes = await File.ReadAllBytesAsync(
                payloadPath,
                TestContext.Current.CancellationToken);
            var snapshot = FileStateSnapshot.File(payloadPath, payloadPath, payloadBytes);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    workspace,
                    RouteInitDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Init,
                        workspace),
                    Guid.NewGuid(),
                    [RecoveryBundleTarget.Create(
                        PlannedFileChange.Delete(snapshot.Expectation),
                        snapshot)]),
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            return (
                preparation.BundlePath,
                await File.ReadAllBytesAsync(
                    preparation.BundlePath,
                    TestContext.Current.CancellationToken));
        }

        var storeRoot = TestDataHome.LocalRecoveryStoreRoot(workspace.PhysicalRoot);
        var operationId = Guid.NewGuid();
        string path;
        if (candidateKind == "draft")
        {
            path = RecoveryBundlePathIdentity.DraftPath(storeRoot, workspace.PhysicalRoot, operationId);
        }
        else if (candidateKind == "malformed-final")
        {
            path = RecoveryBundlePathIdentity.FinalPath(storeRoot, workspace.PhysicalRoot, operationId);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(candidateKind), candidateKind, null);
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        byte[] bytes;
        if (candidateKind == "draft")
        {
            bytes = "incomplete recovery draft"u8.ToArray();
        }
        else if (candidateKind == "malformed-final")
        {
            bytes = "not a recovery ZIP"u8.ToArray();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(candidateKind), candidateKind, null);
        }
        await File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
        return (path, bytes);
    }
}
