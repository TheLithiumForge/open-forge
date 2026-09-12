using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.Deletion;

internal sealed class RecoveryDeletionSessionWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("recovery-deletion-session");
    private readonly WorkspaceLockTestStore _locks = WorkspaceLockTestStore.Create("recovery-deletion-session-lock");
    private readonly List<string> _ownedPaths = [];

    internal RecoveryDeletionSessionWorkspace()
    {
        Workspace = RecoveryBundleStoreIntegrationTests.Workspace(_temporary);
    }

    internal CliWorkspace Workspace { get; }

    internal async Task<string> AddFinalAsync()
    {
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(_temporary, Workspace, Guid.NewGuid());
        var prepared = await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        _ownedPaths.Add(preparation.BundlePath);
        return preparation.BundlePath;
    }

    internal string AddDraft()
        => AddFile(RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()));

    internal string AddUnknown()
        => AddFile("keep-unknown.zip");

    internal async Task<RecoveryBundleCatalogueResult> FreezeAsync(int count)
    {
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
        Assert.Equal(count, catalogue.Candidates.Length);
        Assert.All(catalogue.Candidates, candidate =>
        {
            Assert.True(File.Exists(candidate.Path));
            Assert.Equal((FileAttributes)0, File.GetAttributes(candidate.Path) & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint));
            Assert.True(candidate.Integrity is RecoveryBundleIntegrity.Verified or RecoveryBundleIntegrity.Incomplete);
        });
        return catalogue;
    }

    internal async Task<WorkspaceLockLease> AcquireAsync()
    {
        var result = await _locks.AcquireAsync(
            new WorkspaceLockRequest(Workspace, "cleanup", Guid.NewGuid()), TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
        Assert.True(lease.IsHeldFor(Workspace));
        return lease;
    }

    internal async Task ChangeAttributionAsync(string path)
    {
        var entries = new List<(string Name, byte[] Content)>();
        RecoveryBundleManifestV1 document;
        using (var archive = ZipFile.OpenRead(path))
        {
            var entry = archive.GetEntry(RecoveryBundleFormatV1.ManifestEntryName)
                ?? throw new InvalidOperationException("The final fixture requires its manifest.");
            await using (var stream = entry.Open())
            {
                var decoded = await RecoveryBundleManifestCodec.DecodeAsync(stream, TestContext.Current.CancellationToken);
                document = Assert.IsType<RecoveryBundleManifestV1>(decoded.Document);
            }

            foreach (var payload in archive.Entries.Skip(1))
            {
                await using var source = payload.Open();
                using var destination = new MemoryStream();
                await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
                entries.Add((payload.FullName, destination.ToArray()));
            }
        }

        var manifest = RecoveryBundleManifestCodec.Serialize(document with
        {
            Attribution = RecoveryBundleAttributionCodec.Serialize(RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair, Workspace)),
        });
        File.Delete(path);
        using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
            await using (var destination = entry.Open())
            {
                await destination.WriteAsync(manifest, TestContext.Current.CancellationToken);
            }

            foreach (var (name, content) in entries)
            {
                var replacement = archive.CreateEntry(name);
                await using var destination = replacement.Open();
                await destination.WriteAsync(content, TestContext.Current.CancellationToken);
            }
        }

        var read = await RecoveryBundleReader.ReadFinalAsync(Workspace, path, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, read.State);
        Assert.Equal(RecoveryBundleProducer.Repair, Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified).Attribution.Producer);
    }

    public void Dispose()
    {
        foreach (var path in _ownedPaths)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: false);
            }
            else
            {
                File.Delete(path);
            }
        }

        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Workspace);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: false);
        }

        _locks.Dispose();
        _temporary.Dispose();
    }

    private string AddFile(string name)
    {
        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Workspace);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, name);
        File.WriteAllBytes(path, [1, 2, 3]);
        _ownedPaths.Add(path);
        return path;
    }
}
