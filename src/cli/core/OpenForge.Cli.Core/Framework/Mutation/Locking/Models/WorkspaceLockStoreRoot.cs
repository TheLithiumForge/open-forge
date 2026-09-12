using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed record WorkspaceLockStoreRoot
{
    private WorkspaceLockStoreRoot(string path)
    {
        Path = FileExpectation.NormalizeAbsolutePath(path, nameof(path));
    }

    internal string Path { get; }

    internal static WorkspaceLockStoreRoot FromAbsolutePath(string path)
        => new(path);

    internal static WorkspaceLockStoreRoot? ResolveForCurrentUser(
        Environment.SpecialFolderOption option)
    {
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            option);
        return string.IsNullOrWhiteSpace(localApplicationData)
            || !System.IO.Path.IsPathFullyQualified(localApplicationData)
                ? null
                : new WorkspaceLockStoreRoot(System.IO.Path.GetFullPath(localApplicationData));
    }
}
