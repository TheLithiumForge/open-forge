using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.EndToEndTests.Shared.Route;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteRemoveWorkspace : IDisposable
{
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal const string ParentPath = ".agents/guidance/_guidance.md";
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";

    private readonly TemporaryWorkspace temporary;
    private readonly PublishedWorkspaceLockStore lockStore;
    private bool disposed;

    private PublishedRouteRemoveWorkspace(
        TemporaryWorkspace temporary,
        PublishedWorkspaceLockStore lockStore)
    {
        this.temporary = temporary;
        this.lockStore = lockStore;
        _ = lockStore.Track(temporary.Path);
    }

    internal string Path => temporary.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment
        => lockStore.EnvironmentVariables;

    internal static async Task<PublishedRouteRemoveWorkspace> CreateAsync(
        PublishedExecutableTarget target)
    {
        var temporary = TemporaryWorkspace.Create("e2e-route-remove");
        PublishedWorkspaceLockStore? lockStore = null;
        try
        {
            await PublishedRouteWorkspaceSeed.SeedAsync(target, temporary);
            File.WriteAllText(
                temporary.Combine("README.md"),
                "Prefix [Old guide](.agents/guidance/old%20guide.md#section) suffix.\n",
                StrictUtf8NoBom);
            var definitionsPath = temporary.Combine("definitions.md");
            var definitions = File.ReadAllText(definitionsPath, StrictUtf8NoBom);
            File.WriteAllText(
                definitionsPath,
                definitions.Replace(
                    "[target]: .agents/guidance/old%20guide.md#caf%C3%A9\n",
                    "[target]: .agents/archive/_archive.md#caf%C3%A9\n",
                    StringComparison.Ordinal),
                StrictUtf8NoBom);
            lockStore = PublishedWorkspaceLockStore.Create("e2e-route-remove-lock-store");
            return new PublishedRouteRemoveWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore?.Dispose();
            PublishedRouteMoveSetup.DeleteArtifacts(temporary);
            temporary.Dispose();
            throw;
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotState()
        => temporary.SnapshotHashes();

    internal string Combine(string relativePath)
        => temporary.Combine(relativePath);

    internal string ReadText(string relativePath)
        => File.ReadAllText(Combine(relativePath), Encoding.UTF8);

    internal byte[] ReadBytes(string relativePath)
        => File.ReadAllBytes(Combine(relativePath));

    internal void AssertNoLockInfrastructure()
        => lockStore.AssertNoInfrastructure();

    internal void AssertPersistentExternalLock()
        => lockStore.AssertPersistentZeroByteLock(Path);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            _ = lockStore.RemoveRecoveryArtifacts(Path);
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
}
