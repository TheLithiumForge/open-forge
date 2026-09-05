using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleStoreIntegrationTests
{
    [Fact(DisplayName = "Recovery store closes reopens and verifies one exact multi-target bundle")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PreparePublishesOneExactStreamedOperationBundle()
    {
        const int LargePayloadBufferCount = 32;
        const int LargePayloadRemainder = 17;
        const int RandomSeed = 17;
        using var temporary = TemporaryWorkspace.Create("recovery-bundle-store");
        var workspace = Workspace(temporary);
        var operationId = Guid.NewGuid();
        var binaryBytes = new byte[] { 0, 255, 1, 128, 13, 10, 0 };
        var largeBytes = new byte[
            (RecoveryBundleFormatV1.StreamBufferSize * LargePayloadBufferCount)
            + LargePayloadRemainder];
        new Random(RandomSeed).NextBytes(largeBytes);
        var emptyPath = temporary.CreateFile("empty.bin", []);
        var binaryPath = temporary.CreateFile("binary.bin", binaryBytes);
        var largePath = temporary.CreateFile("large.bin", largeBytes);
        var createPath = temporary.Combine("created.bin");
        var empty = FileStateSnapshot.File(emptyPath, emptyPath, []);
        var binary = FileStateSnapshot.File(binaryPath, binaryPath, binaryBytes);
        var large = FileStateSnapshot.File(largePath, largePath, largeBytes);
        var input = RecoveryBundleInput.Create(
            workspace,
            command: "index",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId,
            targets:
            [
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Replace(empty.Expectation, [1]),
                    empty),
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Delete(binary.Expectation),
                    binary),
                RecoveryBundleTarget.Create(
                    PlannedFileChange.ReplaceGeneratedRegion(large.Expectation, "new"u8),
                    large),
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Create(FileExpectation.Missing(createPath), "create"u8),
                    FileStateSnapshot.Missing(createPath)),
            ]);

        RecoveryBundlePreparation? preparation = null;
        try
        {
            var result = await Store().PrepareAsync(
                input,
                TestContext.Current.CancellationToken);

            Assert.True(result.State == RecoveryBundlePreparationState.Prepared, result.Cause);
            preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
            Assert.Equal(operationId, preparation.OperationId);
            Assert.Equal(3, preparation.Entries.Length);
            Assert.Equal(
                ["empty.bin", "binary.bin", "large.bin"],
                preparation.Entries.Select(entry => entry.TargetPath));
            Assert.False(File.Exists(DraftPath(workspace, operationId)));

            using var archive = ZipFile.OpenRead(preparation.BundlePath);
            Assert.Equal(
                [
                    RecoveryBundleFormatV1.ManifestEntryName,
                    RecoveryBundleFormatV1.PayloadName(0),
                    RecoveryBundleFormatV1.PayloadName(1),
                    RecoveryBundleFormatV1.PayloadName(2),
                ],
                archive.Entries.Select(entry => entry.FullName));
            Assert.Equal(Array.Empty<byte>(), await ReadEntryAsync(archive.Entries[1]));
            Assert.Equal(binaryBytes, await ReadEntryAsync(archive.Entries[2]));
            Assert.Equal(largeBytes, await ReadEntryAsync(archive.Entries[3]));

            var reopened = await new RecoveryBundleReader().ReadFinalAsync(
                workspace,
                preparation.BundlePath,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleReadState.Valid, reopened.State);
            Assert.Equal(operationId, reopened.Verified?.OperationId);
        }
        finally
        {
            DeleteOwned(preparation?.BundlePath);
        }
    }

    [Fact(DisplayName = "Recovery store and observer leave create-only operations bundle free")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CreateOnlyNeedsNoBundleAndObserverCreatesNothing()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-bundle-create-only");
        var workspace = Workspace(temporary);
        var path = temporary.Combine("created.bin");
        var input = RecoveryBundleInput.Create(
            workspace,
            command: "extension create",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId: Guid.NewGuid(),
            targets:
            [
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Create(FileExpectation.Missing(path), "created"u8),
                    FileStateSnapshot.Missing(path)),
            ]);
        var workspaceDirectory = WorkspaceDirectory(workspace);

        var prepared = await Store().PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var catalogue = await new RecoveryBundleCatalogue(new RecoveryBundleReader()).ReadAsync(
            workspace,
            TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryBundlePreparationState.NotNeeded, prepared.State);
        Assert.Null(prepared.Preparation);
        Assert.Null(prepared.ResidualPath);
        Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
        Assert.Empty(catalogue.Candidates);
        Assert.False(Directory.Exists(workspaceDirectory));
    }

    [Fact(DisplayName = "Recovery store reports deterministic collision and cancellation paths honestly")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CollisionAndCancellationReportActualResidualPaths()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-bundle-results");
        var workspace = Workspace(temporary);
        var input = DeleteInput(temporary, workspace, Guid.NewGuid());
        RecoveryBundlePreparation? preparation = null;
        try
        {
            var first = await Store().PrepareAsync(input, TestContext.Current.CancellationToken);
            preparation = Assert.IsType<RecoveryBundlePreparation>(first.Preparation);
            var collision = await Store().PrepareAsync(input, TestContext.Current.CancellationToken);
            using var readbackCancellation = new CancellationTokenSource();
            readbackCancellation.Cancel();
            var cancelledReadback = await new RecoveryBundleReader().ReadExpectedFinalAsync(
                input,
                preparation.BundlePath,
                readbackCancellation.Token);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var cancelled = await Store().PrepareAsync(
                DeleteInput(temporary, workspace, Guid.NewGuid()),
                cancellation.Token);

            Assert.Equal(RecoveryBundlePreparationState.Blocked, collision.State);
            Assert.Equal(preparation.BundlePath, collision.ResidualPath);
            Assert.True(File.Exists(collision.ResidualPath));
            Assert.Equal(RecoveryBundleReadState.Cancelled, cancelledReadback.Read.State);
            Assert.Null(cancelledReadback.Preparation);
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.Equal(RecoveryBundlePreparationState.Cancelled, cancelled.State);
            Assert.Null(cancelled.ResidualPath);
        }
        finally
        {
            DeleteOwned(preparation?.BundlePath);
        }
    }

    internal static RecoveryBundleStore Store()
        => new(new RecoveryBundleReader());

    internal static RecoveryBundleInput DeleteInput(
        TemporaryWorkspace temporary,
        CliWorkspace workspace,
        Guid operationId)
    {
        var relativePath = $"target-{operationId:N}.bin";
        var path = temporary.CreateFile(relativePath, "prior"u8.ToArray());
        var before = FileStateSnapshot.File(path, path, "prior"u8);
        return RecoveryBundleInput.Create(
            workspace,
            command: "cleanup evidence",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId,
            targets:
            [
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Delete(before.Expectation),
                    before),
            ]);
    }

    internal static string WorkspaceDirectory(CliWorkspace workspace)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("LocalApplicationData must be observable for Integration evidence.");
        return RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, workspace.PhysicalRoot);
    }

    internal static void DeleteOwned(string? path)
    {
        if (path is null)
        {
            return;
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("A recovery bundle requires a workspace directory.");
        if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    internal static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static string DraftPath(CliWorkspace workspace, Guid operationId)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("The prepared store root must be observable.");
        return RecoveryBundlePathIdentity.DraftPath(storeRoot, workspace.PhysicalRoot, operationId);
    }

    private static async ValueTask<byte[]> ReadEntryAsync(ZipArchiveEntry entry)
    {
        await using var source = entry.Open();
        using var destination = new MemoryStream();
        await source.CopyToAsync(destination, TestContext.Current.CancellationToken);
        return destination.ToArray();
    }
}
