using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Storage;

[Trait("Feature", "tool-store"), Trait("Evidence", "Integration")]
public sealed class ToolStorePublisherTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Publication verifies members, keeps both workspaces and releases bytes only after the last reference")]
    public async Task PublishesAndReleasesReferences()
    {
        using var fixture = new ToolStorePublicationFixture();
        var first = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var published = await fixture.Publisher.PublishAsync(lease, first, ToolStorePublicationFixture.Registration(fixture.First), Token);
        Assert.True(published.Unit.Verified);
        Assert.Null(published.Unit.StagingPath);
        Assert.Null(published.StagingCleanupCause);
        Assert.False(Directory.Exists(fixture.Store.Installs.At(first.StagingPath)));
        Assert.True((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, published.Unit, Token)).Verified);
        var second = await fixture.PrepareAsync(Token);
        await fixture.Publisher.PublishAsync(lease, second, ToolStorePublicationFixture.Registration(fixture.Second), Token);
        Assert.False(await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(first), fixture.First, Token));
        var shared = await fixture.Store.ReadAsync(Token);
        Assert.Single(shared.Document.Units[ToolStorePublicationFixture.Key(first)].Workspaces);
        Assert.True(shared.Document.Units[ToolStorePublicationFixture.Key(first)].Workspaces.ContainsKey(WorkspaceIdentity.Key(fixture.Second)));
        Assert.True(File.Exists(fixture.Store.Installs.At(first.Path + "/" + ToolStorePublicationFixture.Entry)));
        Assert.True(await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(first), fixture.Second, Token));
        Assert.Empty((await fixture.Store.ReadAsync(Token)).Document.Units);
        Assert.False(Directory.Exists(fixture.Store.Installs.At(first.Path)));
    }

    [Theory(DisplayName = "Same-recipe publication repairs missing or changed immutable members")]
    [InlineData(false), InlineData(true)]
    public async Task RepairsKnownMembers(bool missing)
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var first = await fixture.Publisher.PublishAsync(lease, prepared, ToolStorePublicationFixture.Registration(fixture.First), Token);
        var entry = fixture.Store.Installs.At(first.Unit.Path + "/" + ToolStorePublicationFixture.Entry);
        if (missing) File.Delete(entry);
        else await File.WriteAllBytesAsync(entry, [1, 2, 3], Token);
        Assert.False((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, first.Unit, Token)).Verified);
        var replacement = await fixture.PrepareAsync(Token);
        var repaired = await fixture.Publisher.PublishAsync(lease, replacement, ToolStorePublicationFixture.Registration(fixture.First), Token);
        Assert.True((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, repaired.Unit, Token)).Verified);
        Assert.Equal(ToolStorePublicationFixture.Bytes, await File.ReadAllBytesAsync(entry, Token));
    }

    [Fact(DisplayName = "A second workspace can reuse a verified unit without staging or replacing its files")]
    public async Task AttachesVerifiedUnit()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, prepared, ToolStorePublicationFixture.Registration(fixture.First), Token);
        var attached = await fixture.Publisher.AttachAsync(lease, ToolStorePublicationFixture.Key(prepared), ToolStorePublicationFixture.Registration(fixture.Second), Token);
        Assert.Equal(2, attached.Workspaces.Count);
        Assert.True((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, attached, Token)).Verified);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Store.Installs.At(ToolStoreRegistryDefinitions.StagingDirectory)));
    }

    [Theory(DisplayName = "Reusing a unit rechecks both its own files and its complete dependency closure")]
    [InlineData(false), InlineData(true)]
    public async Task AttachmentRechecksClosure(bool dependencyChanged)
    {
        using var fixture = new ToolStorePublicationFixture();
        var dependency = await fixture.PrepareAsync(Token, ToolStoreRegistryDefinitions.Runtime, "dependency/1.0.0/test-target/pin");
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, dependency, null, Token);
        var parent = await fixture.PrepareAsync(Token, dependencies: [ToolStorePublicationFixture.Key(dependency)]);
        await fixture.Publisher.PublishAsync(lease, parent, ToolStorePublicationFixture.Registration(fixture.First), Token);
        var changed = dependencyChanged ? dependency : parent;
        File.Delete(fixture.Store.Installs.At(changed.Path + "/" + ToolStorePublicationFixture.Entry));
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.AttachAsync(lease, ToolStorePublicationFixture.Key(parent),
            ToolStorePublicationFixture.Registration(fixture.Second), Token));
        Assert.Equal(before, fixture.SnapshotHashes());
    }

    [Fact(DisplayName = "Interrupted publication keeps pending coverage and can be completed from fresh staged bytes")]
    public async Task RepeatsInterruptedPublication()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var publisher = new ToolStorePublisher(fixture.Store, progress =>
        {
            if (progress.Kind == ToolStorePublicationEventKind.FileMoved) cancellation.Cancel();
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await publisher.PublishAsync(lease, prepared,
            ToolStorePublicationFixture.Registration(fixture.First), cancellation.Token));
        var pending = (await fixture.Store.ReadAsync(Token)).Document.Units[ToolStorePublicationFixture.Key(prepared)];
        Assert.False(pending.Verified);
        Assert.Equal(prepared.StagingPath, pending.StagingPath);
        Assert.False((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, pending, Token)).Verified);
        Assert.True(File.Exists(fixture.Store.Installs.At(prepared.Path + "/" + ToolStorePublicationFixture.Entry)));
        Assert.True(File.Exists(fixture.Store.Installs.At(prepared.StagingPath + "/" + ToolStorePublicationFixture.Other)));
        var replacement = await fixture.PrepareAsync(Token);
        var completed = await fixture.Publisher.PublishAsync(lease, replacement, ToolStorePublicationFixture.Registration(fixture.First), Token);
        Assert.True((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, completed.Unit, Token)).Verified);
        Assert.False(Directory.Exists(fixture.Store.Installs.At(prepared.StagingPath)));
    }

    [Fact(DisplayName = "Publication keeps an unexpected file that appears after the first move")]
    public async Task RejectsChangedDestinationDuringPublication()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var foreign = fixture.Store.Installs.At(prepared.Path + "/" + ToolStorePublicationFixture.Other);
        var publisher = new ToolStorePublisher(fixture.Store, progress =>
        {
            if (progress.Kind == ToolStorePublicationEventKind.FileMoved) File.WriteAllBytes(foreign, [1, 2, 3]);
        });
        await Assert.ThrowsAsync<IOException>(async () => await publisher.PublishAsync(lease, prepared, null, Token));
        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(foreign, Token));
        Assert.False((await fixture.Store.ReadAsync(Token)).Document.Units[ToolStorePublicationFixture.Key(prepared)].Verified);
        var retry = await fixture.PrepareAsync(Token);
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, retry, null, Token));
        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(foreign, Token));
    }

    [Fact(DisplayName = "An interrupted repair keeps prior ownership of changed immutable files")]
    public async Task RepeatsInterruptedRepair()
    {
        using var fixture = new ToolStorePublicationFixture();
        var initial = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, initial, ToolStorePublicationFixture.Registration(fixture.First), Token);
        foreach (var file in initial.Files) await File.WriteAllBytesAsync(fixture.Store.Installs.At(initial.Path + "/" + file.Path), [1, 2, 3], Token);
        var repair = await fixture.PrepareAsync(Token);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var publisher = new ToolStorePublisher(fixture.Store, progress =>
        {
            if (progress.Kind == ToolStorePublicationEventKind.FileMoved) cancellation.Cancel();
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await publisher.PublishAsync(lease, repair, null, cancellation.Token));
        var retry = await fixture.PrepareAsync(Token);
        var completed = await fixture.Publisher.PublishAsync(lease, retry, null, Token);
        Assert.True((await ToolStoreUnitReader.ReadAsync(fixture.Store.Installs, completed.Unit, Token)).Verified);
    }

    [Fact(DisplayName = "An unregistered destination is never adopted or overwritten")]
    public async Task RejectsUnregisteredDestination()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        fixture.Store.Installs.EnsureDirectory(prepared.Path, Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, prepared, null, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
        Assert.Empty((await fixture.Store.ReadAsync(Token)).Document.Units);
    }

    [Fact(DisplayName = "Changed staged bytes block publication without a registration or file effect")]
    public async Task RejectsChangedStaging()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        await File.WriteAllBytesAsync(fixture.Store.Installs.At(prepared.StagingPath + "/" + ToolStorePublicationFixture.Entry), [1, 2, 3], Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, prepared, null, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
    }

    [Fact(DisplayName = "Foreign unit members block repair and final deletion without losing a workspace reference")]
    public async Task KeepsForeignMembers()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, prepared, ToolStorePublicationFixture.Registration(fixture.First), Token);
        await File.WriteAllBytesAsync(fixture.Store.Installs.At(prepared.Path + "/foreign.txt"), [1, 2, 3], Token);
        var next = await fixture.PrepareAsync(Token);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, next, null, Token));
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(prepared), fixture.First, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
        Assert.Single((await fixture.Store.ReadAsync(Token)).Document.Units[ToolStorePublicationFixture.Key(prepared)].Workspaces);
    }

    [Theory(DisplayName = "A shared runtime or archive stays while an install depends on it")]
    [InlineData(ToolStoreRegistryDefinitions.Runtime), InlineData(ToolStoreRegistryDefinitions.Archive)]
    public async Task KeepsDependenciesUntilParentRelease(string kind)
    {
        using var fixture = new ToolStorePublicationFixture();
        var dependency = await fixture.PrepareAsync(Token, kind, "dependency/1.0.0/test-target/pin");
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, dependency, null, Token);
        var parent = await fixture.PrepareAsync(Token, dependencies: [ToolStorePublicationFixture.Key(dependency)]);
        await fixture.Publisher.PublishAsync(lease, parent, ToolStorePublicationFixture.Registration(fixture.First), Token);
        Assert.False(await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(dependency), null, Token));
        Assert.True(await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(parent), fixture.First, Token));
        Assert.True(await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(dependency), null, Token));
        Assert.Empty((await fixture.Store.ReadAsync(Token)).Document.Units);
    }

    [Fact(DisplayName = "A stale verified marker can't admit a missing dependency file")]
    public async Task RechecksDependencyBytes()
    {
        using var fixture = new ToolStorePublicationFixture();
        var dependency = await fixture.PrepareAsync(Token, ToolStoreRegistryDefinitions.Runtime, "dependency/1.0.0/test-target/pin");
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, dependency, null, Token);
        File.Delete(fixture.Store.Installs.At(dependency.Path + "/" + ToolStorePublicationFixture.Entry));
        var parent = await fixture.PrepareAsync(Token, dependencies: [ToolStorePublicationFixture.Key(dependency)]);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, parent, null, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
    }

    [Fact(DisplayName = "Missing unit or workspace reference coverage never permits deletion")]
    public async Task RejectsMissingReferences()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        var key = ToolStorePublicationFixture.Key(prepared);
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.ReleaseAsync(lease, key, fixture.First, Token));
        await fixture.Publisher.PublishAsync(lease, prepared, ToolStorePublicationFixture.Registration(fixture.First), Token);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.ReleaseAsync(lease, key, fixture.Second, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
    }

    [Fact(DisplayName = "Publication and release reject a disposed writing lease")]
    public async Task RequiresLiveLease()
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        lease.Dispose();
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fixture.Publisher.PublishAsync(lease, prepared, null, Token));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(prepared), null, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
    }

    [Theory(DisplayName = "Changed files or unsafe member types block final deletion without losing references")]
    [InlineData(false), InlineData(true)]
    public async Task RejectsChangedFinalMembers(bool directory)
    {
        using var fixture = new ToolStorePublicationFixture();
        var prepared = await fixture.PrepareAsync(Token);
        using var lease = ToolStoreLease.Acquire(fixture.Store.State, Token);
        await fixture.Publisher.PublishAsync(lease, prepared, ToolStorePublicationFixture.Registration(fixture.First), Token);
        var target = fixture.Store.Installs.At(prepared.Path + "/" + ToolStorePublicationFixture.Entry);
        if (directory) { File.Delete(target); Directory.CreateDirectory(target); }
        else await File.WriteAllBytesAsync(target, [1, 2, 3], Token);
        var before = fixture.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.ReleaseAsync(lease, ToolStorePublicationFixture.Key(prepared), fixture.First, Token));
        Assert.Equal(before, fixture.SnapshotHashes());
        if (directory)
        {
            var next = await fixture.PrepareAsync(Token);
            var beforeRepair = fixture.SnapshotHashes();
            await Assert.ThrowsAsync<IOException>(async () => await fixture.Publisher.PublishAsync(lease, next, null, Token));
            Assert.Equal(beforeRepair, fixture.SnapshotHashes());
        }
    }
}
