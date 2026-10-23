using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexRecoveryStorageIntegrationTests
{
    [Theory(DisplayName = "Index rejects linked recovery ancestors before archives or target effects")]
    [InlineData(false), InlineData(true)]
    public async Task LinkedRecoveryAncestorStopsBeforeEffects(bool versionDirectory)
    {
        using var workspace = IndexOperationWorkspace.Create("index-linked-recovery");
        using var external = TemporaryWorkspace.Create("index-recovery-external");
        var storage = external.CreateDirectory("storage");
        var root = RecoveryStoreLocator.Local(workspace.Workspace).RootPath;
        var link = versionDirectory ? root : Path.GetDirectoryName(root)!;
        Directory.CreateDirectory(Path.GetDirectoryName(link)!);
        Directory.CreateSymbolicLink(link, storage);
        try
        {
            var before = workspace.SnapshotHashes();
            var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

            Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(IndexRecoveryState.NotCreated, result.Recovery.State);
            Assert.Null(result.Recovery.ResidualPath);
            Assert.Equal(0, result.Counts.Applied);
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Empty(Directory.EnumerateFileSystemEntries(storage));
            Assert.Equal(storage, new DirectoryInfo(link).LinkTarget);
        }
        finally
        {
            Directory.Delete(link);
        }
    }
}
