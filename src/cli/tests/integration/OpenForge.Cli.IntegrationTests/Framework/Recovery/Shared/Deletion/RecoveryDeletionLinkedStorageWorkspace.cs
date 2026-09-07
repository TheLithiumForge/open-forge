using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery.Shared.Deletion;

internal sealed class RecoveryDeletionLinkedStorageWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _external = TemporaryWorkspace.Create("cleanup-external-storage");
    private readonly List<string> _drafts = [];
    private bool _linked;

    internal RecoveryDeletionLinkedStorageWorkspace()
    {
        Source = new RecoveryDeletionSessionWorkspace();
        Locks = WorkspaceLockTestStore.Create("cleanup-linked-storage-lock");
        _ = Locks.Track(Source.Workspace);
        Bucket = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Source.Workspace);
        ExternalDirectory = _external.CreateDirectory("external-artifacts");
    }

    internal RecoveryDeletionSessionWorkspace Source { get; }

    internal WorkspaceLockTestStore Locks { get; }

    internal string Bucket { get; }

    internal string ExternalDirectory { get; }

    internal string AddDraft()
    {
        var draft = Source.AddDraft();
        _drafts.Add(draft);
        return draft;
    }

    internal string ExternalPath(string draft) => Path.Combine(ExternalDirectory, Path.GetFileName(draft));

    internal void LinkBucketToExternal()
    {
        Assert.False(_linked);
        Assert.NotEmpty(_drafts);
        Assert.Equal(_drafts.Count(File.Exists), Directory.GetFileSystemEntries(Bucket).Length);
        Directory.CreateDirectory(ExternalDirectory);
        foreach (var draft in _drafts)
        {
            if (!File.Exists(draft))
            {
                continue;
            }

            _external.CreateFile(Path.Combine("external-artifacts", Path.GetFileName(draft)), File.ReadAllBytes(draft));
            File.Delete(draft);
        }

        Directory.Delete(Bucket, recursive: false);
        Directory.CreateSymbolicLink(Bucket, ExternalDirectory);
        _linked = true;
        Assert.Equal(ExternalDirectory, new DirectoryInfo(Bucket).LinkTarget);
        Assert.NotEqual((FileAttributes)0, File.GetAttributes(Bucket) & FileAttributes.ReparsePoint);
    }

    internal async Task QualifyLinkedDraftAsync(string draft)
    {
        Assert.True(_linked);
        Assert.True(Path.IsPathFullyQualified(Bucket));
        Assert.Equal(Bucket, Path.GetDirectoryName(draft));
        Assert.True(RecoveryBundleFormatV1.TryParseCandidateFileName(Path.GetFileName(draft), out _, out var kind));
        Assert.Equal(RecoveryBundleCandidateKind.Draft, kind);
        Assert.Equal((FileAttributes)0, File.GetAttributes(ExternalPath(draft))
            & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint));
        Assert.Equal((FileAttributes)0, File.GetAttributes(draft)
            & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint));
        Assert.Equal<byte>([1, 2, 3], File.ReadAllBytes(ExternalPath(draft)));
        var catalogue = await Source.Catalogue.ReadAsync(Source.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleCatalogueState.Available, catalogue.State);
        var candidate = Assert.Single(catalogue.Candidates, item => item.Path == draft);
        Assert.Equal(RecoveryBundleIntegrity.Incomplete, candidate.Integrity);
        Assert.Null(candidate.Verified);
        Assert.False(Locks.InfrastructureExists);
    }

    public void Dispose()
    {
        if (_linked)
        {
            Directory.Delete(Bucket, recursive: false);
            Directory.CreateDirectory(Bucket);
        }

        Source.Dispose();
        Locks.Dispose();
        _external.Dispose();
    }
}
