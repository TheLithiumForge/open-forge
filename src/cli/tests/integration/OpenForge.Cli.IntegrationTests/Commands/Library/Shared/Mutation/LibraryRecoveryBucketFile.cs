using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

internal sealed class LibraryRecoveryBucketFile : IDisposable
{
    private readonly string _path;
    private readonly byte[] _sentinel;

    internal LibraryRecoveryBucketFile(LibraryMutationWorkspace workspace)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("The recovery bucket fixture requires a local application-data root.");
        Directory.CreateDirectory(storeRoot);
        _path = RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, workspace.Workspace.PhysicalRoot);
        _sentinel = Guid.NewGuid().ToByteArray();
        using var stream = new FileStream(_path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(_sentinel);
    }

    internal void AssertUnchanged()
    {
        Assert.Equal((FileAttributes)0, File.GetAttributes(_path) & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
        Assert.Equal(_sentinel, File.ReadAllBytes(_path));
    }

    public void Dispose()
    {
        AssertUnchanged();
        File.Delete(_path);
    }
}
