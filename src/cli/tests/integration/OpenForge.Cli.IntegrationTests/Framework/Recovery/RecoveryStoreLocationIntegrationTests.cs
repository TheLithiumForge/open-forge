using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Install.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

[CollectionDefinition("Recovery store environment", DisableParallelization = true)]
public sealed class RecoveryStoreEnvironmentCollection;

[Collection("Recovery store environment")]
[Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RecoveryStoreLocationIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Absent local and legacy recovery observations create neither store")]
    public async Task AbsentStoresAreReadOnly()
    {
        using var files = TemporaryWorkspace.Create("recovery-absent-stores");
        using var home = TestDataHome.Create("recovery-absent-stores");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var before = files.SnapshotHashes(includeRuntime: true);
            var stores = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, Token);
            Assert.Equal([RecoveryStoreKind.Workspace, RecoveryStoreKind.Legacy], stores.Select(store => store.Kind));
            Assert.All(stores, store =>
            {
                Assert.NotNull(store.Location);
                Assert.Equal(RecoveryBundleCatalogueState.Available, store.Catalogue.State);
                Assert.Empty(store.Catalogue.Candidates);
            });
            Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
            Assert.False(Directory.Exists(home.Path));
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Fact(DisplayName = "Identical recovery operation IDs retain distinct local and legacy identity for finals and drafts")]
    public async Task BothStoresValidateTheirRecordedIdentity()
    {
        using var files = TemporaryWorkspace.Create("recovery-both-stores");
        using var home = TestDataHome.Create("recovery-both-stores");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var input = RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid());
            var prepared = await RecoveryBundleStore.PrepareAsync(input, Token);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            Assert.Equal(RecoveryStoreKind.Workspace, preparation.Location.Kind);
            Assert.Equal(TestDataHome.LocalRecoveryWorkspaceDirectory(workspace.PhysicalRoot), Path.GetDirectoryName(preparation.BundlePath));
            Assert.False(Directory.Exists(home.Path));
            var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
            Assert.Equal(home.LegacyRecoveryWorkspaceDirectory(workspace.PhysicalRoot), legacy.WorkspaceDirectory);
            Directory.CreateDirectory(legacy.WorkspaceDirectory);
            var legacyFinal = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
            File.Copy(preparation.BundlePath, legacyFinal);
            var stores = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, Token);
            var candidates = stores.Select(store => Assert.Single(store.Catalogue.Candidates)).ToArray();
            Assert.NotEqual(candidates[0].Path, candidates[1].Path);
            Assert.All(candidates, candidate => Assert.Equal(input.OperationId, candidate.Verified?.OperationId));
            foreach (var candidate in candidates)
            {
                var selected = await RecoveryBundleReader.ReadSelectedFinalAsync(workspace, candidate, Token);
                Assert.Equal(candidate.Location, selected.Preparation?.Location);
                var draft = Path.Combine(candidate.Location.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(input.OperationId));
                File.Move(candidate.Path, draft);
                var read = await RecoveryBundleReader.VerifyExpectedAsync(input, candidate.Location, draft, RecoveryBundleCandidateKind.Draft, Token);
                Assert.Equal(RecoveryBundleReadState.Valid, read.State);
                Assert.Equal(candidate.Location, read.Verified?.Location);
                var catalogue = await RecoveryBundleCatalogue.ReadAsync(candidate.Location, Token);
                Assert.Equal(candidate.Location, Assert.Single(catalogue.Candidates).Location);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Theory(DisplayName = "Recovery deletion uses its observed location after the data home changes")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task EnvironmentChangesCannotRedirectDeletion(bool legacy, bool session)
    {
        using var files = TemporaryWorkspace.Create("recovery-recorded-deletion");
        using var home = TestDataHome.Create("recovery-recorded-deletion");
        using var otherHome = TestDataHome.Create("recovery-other-deletion");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var input = RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid());
            var prepared = await RecoveryBundleStore.PrepareAsync(input, Token);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var location = legacy ? Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace)) : preparation.Location;
            Directory.CreateDirectory(location.WorkspaceDirectory);
            var path = Path.Combine(location.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
            if (legacy) File.Copy(preparation.BundlePath, path);
            var frozen = await RecoveryBundleCatalogue.ReadAsync(location, Token);
            var candidate = Assert.Single(frozen.Candidates);
            var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new WorkspaceLockRequest(workspace, CleanupDefinitions.CommandIdentity, Guid.NewGuid()), Token);
            await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);

            Environment.SetEnvironmentVariable(TestDataHome.Variable, otherHome.Path);
            var redirected = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
            Directory.CreateDirectory(redirected.WorkspaceDirectory);
            var untouched = Path.Combine(redirected.WorkspaceDirectory, Path.GetFileName(path));
            File.Copy(path, untouched);
            var bytes = File.ReadAllBytes(untouched);
            var reread = await RecoveryBundleReader.ReadSelectedFinalAsync(workspace, candidate, Token);
            Assert.Equal(RecoveryBundleReadState.Valid, reread.Read.State);
            RecoveryBundleDeletionResult deleted;
            if (session)
            {
                var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, new(location.Kind, location, frozen), Token);
                Assert.NotNull(opened.Session);
                deleted = await opened.Session.DeleteAsync(candidate, Token);
            }
            else
            {
                deleted = await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidate, Token);
            }

            Assert.Equal(RecoveryBundleDeletionState.Deleted, deleted.State);
            Assert.False(File.Exists(path));
            Assert.Equal(bytes, File.ReadAllBytes(untouched));
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Fact(DisplayName = "A mismatched legacy lease refuses that deletion while allowing local recovery deletion")]
    public async Task LegacyDeletionRequiresMatchingAuthority()
    {
        using var files = TemporaryWorkspace.Create("recovery-legacy-authority");
        using var home = TestDataHome.Create("recovery-legacy-authority");
        using var otherHome = TestDataHome.Create("recovery-other-authority");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var input = RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid());
            var prepared = await RecoveryBundleStore.PrepareAsync(input, Token);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            var legacy = Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace));
            Directory.CreateDirectory(legacy.WorkspaceDirectory);
            var path = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
            File.Copy(preparation.BundlePath, path);
            var catalogue = await RecoveryBundleCatalogue.ReadAsync(legacy, Token);
            var candidate = Assert.Single(catalogue.Candidates);
            var manager = new WorkspaceLockManager(WorkspaceLockStoreRoot.FromAbsolutePath(otherHome.Path));
            var acquired = await manager.AcquireAsync(new WorkspaceLockRequest(workspace, CleanupDefinitions.CommandIdentity, Guid.NewGuid()), Token);
            await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
            Assert.Equal(RecoveryBundleDeletionState.Blocked, (await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidate, Token)).State);
            var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, new(legacy.Kind, legacy, catalogue), Token);
            Assert.Null(opened.Session);
            Assert.True(File.Exists(path));
            Assert.Equal(RecoveryBundleDeletionState.Deleted,
                (await RecoveryBundleDeletionGuard.DeleteAsync(lease, RecoveryBundleCandidateSnapshot.VerifiedFinal(preparation.Verified), Token)).State);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Fact(DisplayName = "Unavailable legacy coverage preserves local candidates and does not veto Install")]
    public async Task UnavailableLegacyDoesNotVetoLocalWork()
    {
        using var files = TemporaryWorkspace.Create("recovery-unavailable-legacy");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, "relative-data-home");
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var input = RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid());
            var prepared = await RecoveryBundleStore.PrepareAsync(input, Token);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            var stores = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, Token);
            Assert.Single(stores[0].Catalogue.Candidates);
            Assert.Null(stores[1].Location);
            Assert.Equal(RecoveryBundleCatalogueState.Unavailable, stores[1].Catalogue.State);
            var status = await RecoveryResidualOperationalContributor.ReadStatusAsync(workspace, Token);
            Assert.Equal(OperationalViewState.Incomplete, status.State);
            Assert.Single(status.Candidates);
            Assert.Equal(2, status.Stores.Count);
            var resolver = new PhysicalPathResolver();
            var contributor = new RecoveryResidualOperationalContributor(new RecoveryBundleTargetStateReader(resolver), resolver);
            var doctor = await contributor.ReadDoctorAsync(workspace, Token);
            Assert.Equal(OperationalViewState.Incomplete, doctor.State);
            Assert.Single(doctor.Candidates);
            Assert.Equal(2, doctor.Stores.Count);
            Assert.Equal(OperationalViewState.Complete, doctor.Stores[0].State);
            Assert.Equal(OperationalViewState.Incomplete, doctor.Stores[1].State);
            var blocking = await InstallRecoveryCatalogueReader.ReadBlockingAsync(workspace, Token);
            Assert.Equal(RecoveryBundleCatalogueState.Available, blocking.State);
            Assert.Empty(blocking.Candidates);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }

    [Theory(DisplayName = "Install applies its recovery state filter to both available stores")]
    [InlineData(false), InlineData(true)]
    public async Task InstallReadsBothAvailableStores(bool legacy)
    {
        using var files = TemporaryWorkspace.Create("install-two-stores");
        using var home = TestDataHome.Create("install-two-stores");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
            var location = legacy ? Assert.IsType<RecoveryStoreLocation>(RecoveryStoreLocator.Legacy(workspace)) : RecoveryStoreLocator.Local(workspace);
            Directory.CreateDirectory(location.WorkspaceDirectory);
            var path = Path.Combine(location.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()));
            File.WriteAllText(path, "unverified draft");

            var blocking = await InstallRecoveryCatalogueReader.ReadBlockingAsync(workspace, Token);

            Assert.Equal(RecoveryBundleCatalogueState.Available, blocking.State);
            var candidate = Assert.Single(blocking.Candidates);
            Assert.Equal(location, candidate.Location);
            Assert.Equal(path, candidate.Path);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }
}
