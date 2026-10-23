using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

[Collection("Recovery store environment")]
[Trait("Feature", "recovery-store-coverage"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RecoveryStoreCoverageIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Unsafe or changed legacy items do not veto the frozen local Cleanup plan")]
    [InlineData(false), InlineData(true)]
    public async Task StoreSafetyAndRevalidationAreIndependent(bool unsafeBeforePlanning)
    {
        using var files = TemporaryWorkspace.Create("cleanup-store-revalidation");
        using var home = TestDataHome.Create("cleanup-store-revalidation");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid()), Token);
            var local = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
            Directory.CreateDirectory(legacy.WorkspaceDirectory);
            var legacyPath = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(local.BundlePath));
            File.Copy(local.BundlePath, legacyPath);
            if (unsafeBeforePlanning)
            {
                File.Delete(legacyPath);
                Directory.CreateDirectory(legacyPath);
            }

            var request = new CleanupRequest(workspace, CleanupMode.Apply);
            var frozen = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, Token);
            var stores = frozen.Select(observation =>
            {
                var plan = CleanupPlanner.Create(request, observation.Catalogue);
                return new CleanupStoreApplication(observation, new CleanupResultBuilder(plan)
                {
                    Findings = CleanupPlanner.ReadFindings(plan, observation.Catalogue.Cause),
                    Effects = [.. plan.Entries.Where(entry => entry.Action == CleanupPlanAction.Delete)
                        .Select(entry => CleanupEffect.Create(entry, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained))],
                });
            }).ToArray();
            if (!unsafeBeforePlanning) File.WriteAllText(legacyPath, "changed after planning");

            await new CleanupApplicationOperation(WorkspaceLockManager.CreateForCurrentUser()).ApplyAsync(stores, Token);
            var result = CleanupResultBuilder.BuildStores(request, stores);

            Assert.False(File.Exists(local.BundlePath));
            Assert.Equal(1, result.Effects.Count(effect => effect.Outcome == CleanupEffectOutcome.Verified));
            Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
            if (unsafeBeforePlanning)
            {
                Assert.True(Directory.Exists(legacyPath));
                Assert.Equal(CleanupPlanSafety.Blocked, result.Stores[1].Facts.Plan.Safety);
            }
            else
            {
                Assert.Equal("changed after planning", File.ReadAllText(legacyPath));
                Assert.Equal(CleanupCatalogueComparisonState.Changed, result.Stores[1].Facts.Revalidation.State);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Theory(DisplayName = "Cleanup validates each recorded store and preserves legacy items without matching authority")]
    [InlineData(false), InlineData(true)]
    public async Task BothStoresRequireTheirOwnAuthority(bool mismatchedAuthority)
    {
        using var files = TemporaryWorkspace.Create("cleanup-store-authority");
        using var home = TestDataHome.Create("cleanup-store-authority");
        using var other = TestDataHome.Create("cleanup-other-authority");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var input = RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid());
            var prepared = await RecoveryBundleStore.PrepareAsync(input, Token);
            var local = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
            Directory.CreateDirectory(legacy.WorkspaceDirectory);
            var legacyPath = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(local.BundlePath));
            File.Copy(local.BundlePath, legacyPath);
            var bytes = File.ReadAllBytes(legacyPath);
            var operation = mismatchedAuthority
                ? new CleanupOperation(WorkspaceLockStoreRoot.FromAbsolutePath(other.Path))
                : new CleanupOperation();

            var result = await operation.ExecuteAsync(new(workspace, CleanupMode.Apply), Token);

            Assert.Equal(["workspace", "legacy"], result.Stores.Select(store => store.Kind));
            Assert.False(File.Exists(local.BundlePath));
            Assert.Equal(mismatchedAuthority, File.Exists(legacyPath));
            Assert.Equal(mismatchedAuthority ? 1 : 2,
                result.Effects.Count(effect => effect.Outcome == CleanupEffectOutcome.Verified));
            if (mismatchedAuthority)
            {
                Assert.Equal(bytes, File.ReadAllBytes(legacyPath));
                Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
                Assert.Equal(CleanupCatalogueComparisonState.Blocked, result.Stores[1].Facts.Revalidation.State);
            }
            else
            {
                Assert.Equal(CliSemanticStatus.Complete, result.Status);
                Assert.All(result.Stores, store => Assert.Equal(CleanupCatalogueComparisonState.Matched, store.Facts.Revalidation.State));
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Theory(DisplayName = "Unavailable legacy coverage leaves local Cleanup eligible and aggregate totals unknown")]
    [InlineData(false), InlineData(true)]
    public async Task UnavailableLegacyDoesNotDisableLocalCleanup(bool unresolvedHome)
    {
        using var files = TemporaryWorkspace.Create("cleanup-unavailable-legacy");
        using var home = TestDataHome.Create("cleanup-unavailable-legacy");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, unresolvedHome ? "relative-home" : home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid()), Token);
            var local = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            if (!unresolvedHome)
            {
                var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
                Directory.CreateDirectory(legacy.RootPath);
                File.WriteAllText(legacy.WorkspaceDirectory, "preserve this non-directory");
            }

            var view = await RecoveryResidualOperationalContributor.ReadStatusAsync(workspace, Token);
            var status = StatusRecoveryAggregator.Build(view);
            Assert.Null(status.VerifiedFinals.Value);
            Assert.Single(status.Candidates);
            Assert.Equal(2, status.Stores.Count);
            var result = await new CleanupOperation().ExecuteAsync(new(workspace, CleanupMode.Apply), Token);

            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(CleanupCatalogueComparisonState.Incomplete, result.Revalidation.State);
            Assert.False(File.Exists(local.BundlePath));
            Assert.Equal(CleanupEffectOutcome.Verified, Assert.Single(result.Effects).Outcome);
            Assert.Equal(CleanupCatalogueCoverage.Incomplete, result.Stores[1].Facts.Catalogue.Coverage);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Fact(DisplayName = "Observers and empty Cleanup create neither store and observers tolerate a held workspace lease")]
    public async Task ObserversAndNoOpCreateNothing()
    {
        using var files = TemporaryWorkspace.Create("observer-store-coverage");
        using var home = TestDataHome.Create("observer-store-coverage");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var before = files.SnapshotHashes(includeRuntime: true);
            var resolver = new PhysicalPathResolver();
            var contributor = new RecoveryResidualOperationalContributor(new RecoveryBundleTargetStateReader(resolver), resolver);
            var status = StatusRecoveryAggregator.Build(await RecoveryResidualOperationalContributor.ReadStatusAsync(workspace, Token));
            var doctor = RecoveryResidualDoctorInspector.Inspect(await contributor.ReadDoctorAsync(workspace, Token));
            Assert.Equal(["workspace", "legacy"], status.Stores.Select(store => store.Kind));
            Assert.Equal(["workspace", "legacy"], doctor.RecoveryStores.Select(store => store.Kind));
            foreach (var mode in new[] { CleanupMode.DryRun, CleanupMode.Apply })
            {
                var cleanup = await new CleanupOperation().ExecuteAsync(new CleanupRequest(workspace, mode), Token);
                Assert.Equal(CliSemanticStatus.Complete, cleanup.Status);
                Assert.Equal(CleanupLeaseState.NotRequested, cleanup.Lease.State);
            }

            Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
            Assert.False(Directory.Exists(home.Path));
            var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new(workspace, "test", Guid.NewGuid()), Token);
            await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
            status = StatusRecoveryAggregator.Build(await RecoveryResidualOperationalContributor.ReadStatusAsync(workspace, Token));
            doctor = RecoveryResidualDoctorInspector.Inspect(await contributor.ReadDoctorAsync(workspace, Token));
            Assert.All(status.Stores, store => Assert.Empty(store.Candidates));
            Assert.All(doctor.RecoveryStores, store => Assert.Empty(store.Candidates));
            Assert.False(Directory.Exists(RecoveryStoreLocator.Local(workspace).RootPath));
            Assert.False(Directory.Exists(home.LegacyRecoveryStoreRoot));
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }
}
