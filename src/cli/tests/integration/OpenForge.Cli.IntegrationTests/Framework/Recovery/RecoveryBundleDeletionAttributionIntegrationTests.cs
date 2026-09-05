using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleDeletionAttributionIntegrationTests
{
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
        var prepared = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var reader = new RecoveryBundleReader();
            var catalogue = new RecoveryBundleCatalogue(reader);
            var initial = await catalogue.ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);
            var candidate = Assert.Single(initial.Candidates, item => string.Equals(
                item.Path,
                preparation.BundlePath,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal));
            await ReplaceAttributionAsync(
                preparation.BundlePath,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Repair,
                    RecoveryBundleOperation.Repair,
                    workspace),
                TestContext.Current.CancellationToken);
            var lockResult = await lockStore.AcquireAsync(
                new WorkspaceLockRequest(workspace, input.Command, input.OperationId),
                TestContext.Current.CancellationToken);
            await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

            var deletion = await new RecoveryBundleDeletionGuard(catalogue, reader).DeleteAsync(
                lease,
                candidate,
                TestContext.Current.CancellationToken);

            Assert.Equal(RecoveryBundleDeletionState.Blocked, deletion.State);
            Assert.Equal(RecoveryBundleDisposition.Unknown, deletion.Disposition);
            Assert.Equal(preparation.BundlePath, deletion.ResidualPath);
            Assert.True(File.Exists(preparation.BundlePath));
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
        entry.Delete();
        var replacement = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
        await using var replacementStream = replacement.Open();
        await replacementStream.WriteAsync(
            RecoveryBundleManifestCodec.Serialize(document with
            {
                Attribution = RecoveryBundleAttributionCodec.Serialize(attribution),
            }),
            cancellationToken);
    }
}
