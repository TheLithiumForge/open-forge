using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Storage;

[Trait("Feature", "tool-store"), Trait("Evidence", "Integration")]
public sealed class ToolStoreRegistryTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private const string UnitPath = "fixture/1.0.0/test-target/pin";

    [Fact(DisplayName = "Account references keep both physical workspaces and never alter their files")]
    public async Task PublishesWorkspaceReferences()
    {
        using var temporary = TemporaryWorkspace.Create("tool-store-refs");
        var store = Create(temporary, "store");
        var first = temporary.CreateDirectory("first");
        var second = temporary.CreateDirectory("second");
        try
        {
            var before = temporary.SnapshotHashes();
            var absent = await store.ReadAsync(Token);
            Assert.False(absent.Snapshot.Expectation.Exists);
            Assert.Equal(before, temporary.SnapshotHashes());
            await store.InitializeAsync(Token);
            using var lease = ToolStoreLease.Acquire(store.State, Token);
            var read = await store.ReadAsync(Token);
            var unit = Unit(store);
            unit.Workspaces.Add(WorkspaceIdentity.Key(first), new() { PhysicalWorkspace = first, CodeRoot = "." });
            unit.Workspaces.Add(WorkspaceIdentity.Key(second), new() { PhysicalWorkspace = second, CodeRoot = "." });
            var key = ToolStoreIdentity.UnitKey(unit.Kind, unit.Path);
            read.Document.Units.Add(key, unit);
            var written = await store.WriteAsync(lease, read, read.Document, Token);
            Assert.Equal(2, written.Document.Units[key].Workspaces.Count);
            Assert.Empty(Directory.EnumerateFileSystemEntries(first));
            Assert.Empty(Directory.EnumerateFileSystemEntries(second));
            written.Document.Units[key].Workspaces.Remove(WorkspaceIdentity.Key(first));
            var released = await store.WriteAsync(lease, written, written.Document, Token);
            Assert.Single(released.Document.Units[key].Workspaces);
            Assert.True(released.Document.Units[key].Workspaces.ContainsKey(WorkspaceIdentity.Key(second)));
        }
        finally { Cleanup(store); }
    }

    [Theory(DisplayName = "A missing registry beside existing stores never becomes an empty reference set")]
    [InlineData(ToolStoreRegistryDefinitions.Install)]
    [InlineData(ToolStoreRegistryDefinitions.Archive)]
    public async Task RefusesMissingReferenceCoverage(string kind)
    {
        using var temporary = TemporaryWorkspace.Create("tool-store-unknown");
        var store = Create(temporary, "store");
        temporary.CreateDirectory(Path.GetRelativePath(temporary.Path, kind == ToolStoreRegistryDefinitions.Install ? store.Installs.Root : store.Archives.Root));
        var before = temporary.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await store.ReadAsync(Token));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Store publication requires its own live lease and exact prior metadata bytes")]
    public async Task RejectsWrongLeaseAndChangedMetadata()
    {
        using var temporary = TemporaryWorkspace.Create("tool-store-guard");
        var first = Create(temporary, "first-store");
        var second = Create(temporary, "second-store");
        try
        {
            await first.InitializeAsync(Token);
            await second.InitializeAsync(Token);
            using var lease = ToolStoreLease.Acquire(first.State, Token);
            var other = await second.ReadAsync(Token);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await second.WriteAsync(lease, other, other.Document, Token));
            var initial = await first.ReadAsync(Token);
            var changed = initial.Document with { Units = new(initial.Document.Units) };
            var unit = Unit(first);
            changed.Units.Add(ToolStoreIdentity.UnitKey(unit.Kind, unit.Path), unit);
            await first.WriteAsync(lease, initial, changed, Token);
            await Assert.ThrowsAsync<IOException>(async () => await first.WriteAsync(lease, initial, initial.Document, Token));
        }
        finally { Cleanup(first); Cleanup(second); }
    }

    [Fact(DisplayName = "Incomplete dependency references block registry publication")]
    public async Task RejectsMissingDependency()
    {
        using var temporary = TemporaryWorkspace.Create("tool-store-dependency");
        var store = Create(temporary, "store");
        try
        {
            await store.InitializeAsync(Token);
            using var lease = ToolStoreLease.Acquire(store.State, Token);
            var prior = await store.ReadAsync(Token);
            var unit = Unit(store) with { Dependencies = [new string('0', 64)] };
            prior.Document.Units.Add(ToolStoreIdentity.UnitKey(unit.Kind, unit.Path), unit);
            var before = await ToolFileReader.ReadAsync(store.State.Scope, store.State.At(ToolStoreRegistryDefinitions.File), Token);
            await Assert.ThrowsAsync<JsonException>(async () => await store.WriteAsync(lease, prior, prior.Document, Token));
            var after = await ToolFileReader.ReadAsync(store.State.Scope, store.State.At(ToolStoreRegistryDefinitions.File), Token);
            Assert.Equal(before.ContentHash, after.ContentHash);
        }
        finally { Cleanup(store); }
    }

    private static ToolStoreRegistry Create(TemporaryWorkspace temporary, string root)
        => new(ToolStoreLocator.FromRoot(temporary.CreateDirectory(root)));

    private static ToolStoredUnit Unit(ToolStoreRegistry store)
    {
        var binding = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.Install, store.Installs.At(UnitPath));
        return new()
        {
            Kind = ToolStoreRegistryDefinitions.Install,
            Verified = false,
            Path = UnitPath,
            Binding = binding,
            RecipeSha256 = new string('0', 64),
            Files = [new ToolFileReceipt(binding.Id, "entry.bin", new string('0', 64), false)],
            Directories = [],
            Workspaces = new(StringComparer.Ordinal),
            Dependencies = [],
            Dependents = [],
        };
    }

    private static void Cleanup(ToolStoreRegistry store)
    {
        if (!Directory.Exists(store.State.Root)) return;
        File.Delete(store.State.At(ToolStoreRegistryDefinitions.File));
        File.Delete(store.State.At(ToolStoreRegistryDefinitions.Lock));
        Directory.Delete(store.State.Root, recursive: false);
    }
}
