using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.EndToEndTests.Shared.Route;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteMoveWorkspace : IDisposable
{
    internal const string DestinationPath = ".agents/archive/new guide.md";
    internal const string DestinationOverwritePath = ".agents/archive/new guide.overwrite.md";
    internal const string CategoryDestination = ".agents/archive/topics/_topics.md";

    private readonly TemporaryWorkspace temporary;
    private readonly PublishedWorkspaceLockStore lockStore;
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private bool disposed;

    private PublishedRouteMoveWorkspace(
        TemporaryWorkspace temporary,
        PublishedWorkspaceLockStore lockStore)
    {
        this.temporary = temporary;
        this.lockStore = lockStore;
        _ = lockStore.Track(temporary.Path);
    }

    internal string Path => temporary.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment => lockStore.EnvironmentVariables;

    internal static async Task<PublishedRouteMoveWorkspace> CreateAsync(
        PublishedExecutableTarget target)
    {
        var temporary = TemporaryWorkspace.Create("e2e-route-move");
        PublishedWorkspaceLockStore? lockStore = null;
        try
        {
            await PublishedRouteWorkspaceSeed.SeedAsync(target, temporary);
            lockStore = PublishedWorkspaceLockStore.Create("e2e-route-move-lock-store");
            return new PublishedRouteMoveWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore?.Dispose();
            PublishedRouteMoveSetup.DeleteArtifacts(temporary);
            temporary.Dispose();
            throw;
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotState() => temporary.SnapshotHashes();

    internal string Combine(string relativePath) => temporary.Combine(relativePath);

    internal string ReadText(string relativePath)
        => File.ReadAllText(Combine(relativePath), Encoding.UTF8);

    internal byte[] ReadBytes(string relativePath) => File.ReadAllBytes(Combine(relativePath));

    internal void AssertNoLockInfrastructure() => lockStore.AssertNoInfrastructure();

    internal void AssertPersistentExternalLock() => lockStore.AssertPersistentZeroByteLock(Path);

    internal void RemoveLifecycle() => File.Delete(Combine(".agents/open-forge.lifecycle.json"));

    internal void SeedOccupiedDestination()
        => File.WriteAllText(Combine(DestinationPath), "occupied\n", StrictUtf8NoBom);

    internal void SeedInvalidUtf8()
        => File.WriteAllBytes(Combine("invalid.md"), [0xff, 0xfe, 0xfd]);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            _ = lockStore.RemoveRecoveryArtifacts(Path);
            DeleteApplicationDestination(DestinationPath);
            DeleteApplicationDestination(DestinationOverwritePath);
            DeleteApplicationCategory(".agents/archive/topics");
            PublishedRouteMoveSetup.DeleteArtifacts(temporary);
        }
        finally
        {
            try
            {
                lockStore.Dispose();
            }
            finally
            {
                temporary.Dispose();
                disposed = true;
            }
        }
    }

    private void DeleteApplicationDestination(string relativePath)
    {
        var path = Combine(relativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private void DeleteApplicationCategory(string relativePath)
    {
        var path = Combine(relativePath);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
