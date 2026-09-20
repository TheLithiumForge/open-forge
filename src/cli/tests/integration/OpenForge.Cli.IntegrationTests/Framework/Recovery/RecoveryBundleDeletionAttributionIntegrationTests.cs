using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleDeletionAttributionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Recovery deletion rejects a final whose schema-v1 attribution changed after selection")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task DeletionGuardRejectsChangedAttributionAndRetainsFinal()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-deletion-attribution-change");
        using var lockStore = WorkspaceLockTestStore.Create("recovery-deletion-attribution-change-lock-store");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var input = RecoveryBundleStoreIntegrationTests.DeleteInput(
            temporary,
            workspace,
            Guid.NewGuid());
        var prepared = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var initial = await RecoveryBundleCatalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);
            var candidate = Assert.Single(initial.Candidates, item => string.Equals(
                item.Path,
                preparation.BundlePath,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));
            var selected = Assert.IsType<RecoveryBundleVerifiedRead>(candidate.Verified);
            var targetPath = Assert.Single(input.Targets).Change.LogicalPath;
            var targetBytes = await File.ReadAllBytesAsync(targetPath, TestContext.Current.CancellationToken);
            var originalEntries = await ReadEntriesAsync(preparation.BundlePath, TestContext.Current.CancellationToken);
            Assert.Equal(["manifest.json", "payloads/00000000.bin"], originalEntries.Select(entry => entry.Name));
            Assert.Equal("prior"u8.ToArray(), originalEntries[1].Bytes);
            Assert.Equal(originalEntries[1].Bytes, targetBytes);
            var changedAttribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair,
                RecoveryBundleOperation.Repair,
                workspace);
            Assert.NotEqual(selected.Attribution, changedAttribution);

            await ReplaceAttributionAsync(
                preparation.BundlePath,
                changedAttribution,
                TestContext.Current.CancellationToken);
            var read = await RecoveryBundleReader.ReadFinalAsync(
                workspace,
                preparation.BundlePath,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleReadState.Valid, read.State);
            var observed = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
            Assert.Equal(changedAttribution, observed.Attribution);
            Assert.NotEqual(selected.Attribution, observed.Attribution);
            Assert.Equal(selected.BundlePath, observed.BundlePath);
            Assert.Equal(selected.WorkspacePhysicalPath, observed.WorkspacePhysicalPath);
            Assert.Equal(selected.WorkspaceKey, observed.WorkspaceKey);
            Assert.Equal(selected.Command, observed.Command);
            Assert.Equal(selected.OperationId, observed.OperationId);
            Assert.Equal(selected.Entries, observed.Entries);
            var changedEntries = await ReadEntriesAsync(preparation.BundlePath, TestContext.Current.CancellationToken);
            Assert.Equal(originalEntries.Select(entry => entry.Name), changedEntries.Select(entry => entry.Name));
            Assert.False(originalEntries[0].Bytes.SequenceEqual(changedEntries[0].Bytes));
            Assert.Equal(originalEntries[1].Bytes, changedEntries[1].Bytes);
            var changedBundleBytes = await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken);
            var lockResult = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

            var deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                lease,
                candidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Blocked, deletion.State);
            Assert.Equal(RecoveryBundleDisposition.Unknown, deletion.Disposition);
            Assert.Equal(preparation.BundlePath, deletion.ResidualPath);
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.Equal(changedBundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
            Assert.Equal(targetBytes, await File.ReadAllBytesAsync(targetPath, TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.BundlePath);
        }
    }

    private static async ValueTask ReplaceAttributionAsync(
        string path,
        RecoveryBundleAttribution attribution,
        CancellationToken cancellationToken)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Update);
        var entry = archive.GetEntry(RecoveryBundleFormatV1.ManifestEntryName)
            ?? throw new InvalidOperationException("The prepared recovery bundle requires its manifest.");
        RecoveryBundleManifestDecodeResult decoded;
        await using (var stream = entry.Open())
        {
            decoded = await RecoveryBundleManifestCodec.DecodeAsync(
                stream,
                cancellationToken);
        }

        var document = decoded.Document
            ?? throw new InvalidOperationException("The prepared recovery manifest must remain valid before attribution changes.");
        await using var replacementStream = entry.Open();
        replacementStream.SetLength(0);
        await replacementStream.WriteAsync(
            RecoveryBundleManifestCodec.Serialize(document with
            {
                Attribution = RecoveryBundleAttributionCodec.Serialize(attribution),
            }),
            cancellationToken);
    }

    private static async ValueTask<(string Name, byte[] Bytes)[]> ReadEntriesAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var archive = ZipFile.OpenRead(path);
        var entries = new List<(string Name, byte[] Bytes)>();
        foreach (var entry in archive.Entries)
        {
            await using var stream = entry.Open();
            using var bytes = new MemoryStream();
            await stream.CopyToAsync(bytes, cancellationToken);
            entries.Add((entry.FullName, bytes.ToArray()));
        }

        return [.. entries];
    }
}
