using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;

internal sealed class RecoveryFinalIdentityFixture
{
    private readonly CliWorkspace _workspace;
    private readonly RecoveryBundlePreparation _preparation;
    private readonly (string Name, byte[] Bytes)[] _entries;
    private readonly (string Path, byte[]? Bytes)[] _targets;

    private RecoveryFinalIdentityFixture(
        CliWorkspace workspace,
        RecoveryBundlePreparation preparation,
        (string Name, byte[] Bytes)[] entries,
        (string Path, byte[]? Bytes)[] targets)
    {
        _workspace = workspace;
        _preparation = preparation;
        _entries = entries;
        _targets = targets;
    }

    internal static async ValueTask<RecoveryFinalIdentityFixture> ObserveAsync(
        CliWorkspace workspace,
        RecoveryBundlePreparation preparation)
    {
        var entries = await ReadEntriesAsync(preparation.BundlePath);
        Assert.Equal("manifest.json", entries[0].Name);
        Assert.NotEmpty(entries.Skip(1));
        var targets = new List<(string Path, byte[]? Bytes)>();
        foreach (var entry in preparation.Entries)
        {
            var path = Path.Combine(workspace.LexicalRoot, entry.TargetPath);
            var bytes = File.Exists(path)
                ? await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)
                : null;
            targets.Add((path, bytes));
            if (entry.PriorPayload is { } payload)
            {
                Assert.NotNull(bytes);
                Assert.Equal(bytes, Assert.Single(entries, item => item.Name == payload).Bytes);
            }
        }

        var fixture = new RecoveryFinalIdentityFixture(
            workspace: workspace,
            preparation: preparation,
            entries: entries,
            targets: [.. targets]);
        _ = await fixture.AssertAdmissionAsync(preparation.Command, preparation.Attribution);
        return fixture;
    }

    internal async ValueTask ChangeAttributionAsync(RecoveryBundleAttribution attribution)
    {
        var document = await ReadManifestAsync();
        await WriteManifestAsync(document with
        {
            Attribution = RecoveryBundleAttributionCodec.Serialize(attribution),
        });
    }

    internal async ValueTask ChangeCommandAsync(string command)
    {
        var document = await ReadManifestAsync();
        await WriteManifestAsync(document with { Command = command });
    }

    internal async ValueTask<byte[]> AssertAdmissionAsync(
        string expectedCommand,
        RecoveryBundleAttribution expectedAttribution)
    {
        var read = await RecoveryBundleReader.ReadFinalAsync(
            _workspace,
            _preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, read.State);
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified);
        Assert.Equal(expectedCommand, verified.Command);
        Assert.Equal(expectedAttribution, verified.Attribution);
        Assert.Equal(_preparation.BundlePath, verified.BundlePath);
        Assert.Equal(_preparation.WorkspacePhysicalPath, verified.WorkspacePhysicalPath);
        Assert.Equal(_preparation.WorkspaceKey, verified.WorkspaceKey);
        Assert.Equal(_preparation.OperationId, verified.OperationId);
        Assert.Equal(_preparation.Entries, verified.Entries);
        var entries = await ReadEntriesAsync(_preparation.BundlePath);
        Assert.Equal(_entries.Select(entry => entry.Name), entries.Select(entry => entry.Name));
        for (var index = 1; index < entries.Length; index++)
        {
            Assert.Equal(_entries[index].Bytes, entries[index].Bytes);
        }

        await AssertTargetsUnchangedAsync();
        return await File.ReadAllBytesAsync(_preparation.BundlePath, TestContext.Current.CancellationToken);
    }

    internal async ValueTask AssertTargetsUnchangedAsync()
    {
        foreach (var (path, bytes) in _targets)
        {
            if (bytes is null)
            {
                Assert.False(File.Exists(path));
            }
            else
            {
                Assert.Equal(bytes, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
            }
        }
    }

    private async ValueTask<RecoveryBundleManifestV1> ReadManifestAsync()
    {
        using var archive = ZipFile.OpenRead(_preparation.BundlePath);
        await using var stream = Assert.Single(archive.Entries, entry => entry.FullName == "manifest.json").Open();
        var decoded = await RecoveryBundleManifestCodec.DecodeAsync(stream, TestContext.Current.CancellationToken);
        return Assert.IsType<RecoveryBundleManifestV1>(decoded.Document);
    }

    private async ValueTask WriteManifestAsync(RecoveryBundleManifestV1 document)
    {
        using var archive = ZipFile.Open(_preparation.BundlePath, ZipArchiveMode.Update);
        var entry = Assert.Single(archive.Entries, item => item.FullName == "manifest.json");
        await using var stream = entry.Open();
        stream.SetLength(0);
        await stream.WriteAsync(RecoveryBundleManifestCodec.Serialize(document), TestContext.Current.CancellationToken);
    }

    private static async ValueTask<(string Name, byte[] Bytes)[]> ReadEntriesAsync(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var entries = new List<(string Name, byte[] Bytes)>();
        foreach (var entry in archive.Entries)
        {
            await using var stream = entry.Open();
            using var bytes = new MemoryStream();
            await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
            entries.Add((entry.FullName, bytes.ToArray()));
        }

        return [.. entries];
    }
}
