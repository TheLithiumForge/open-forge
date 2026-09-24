using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

internal sealed class RemoveRootIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private bool _disposed;

    private RemoveRootIntegrationWorkspace(TemporaryWorkspace temporary, WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }
    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;
    internal string Path => _temporary.Path;
    internal string Combine(string path) => _temporary.Combine(path);
    internal IReadOnlyDictionary<string, string> SnapshotHashes() => _temporary.SnapshotHashes();
    internal void WriteText(string path, string value) => _temporary.CreateFile(path, value);
    internal void WriteBytes(string path, byte[] value) => _temporary.CreateFile(path, value);
    internal bool TryCreateDirectoryLink(string path, string target) => _temporary.TryCreateDirectorySymbolicLink(path, target, out _);
    internal bool TryCreateFileLink(string path, string target) => _temporary.TryCreateFileSymbolicLink(path, target, out _);

    internal static RemoveRootIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            return new RemoveRootIntegrationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal async Task<CliProcessCompletion> RunAsync(
        IReadOnlyList<string> arguments,
        StringWriter output,
        StringWriter error,
        TextReader? standardInput = null,
        TextWriter? promptOutput = null)
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = standardInput ?? TextReader.Null,
                PromptOutput = promptOutput ?? TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = LockStoreRoot,
            });
        return await application.RunAsync(
            arguments.ToArray(),
            new CliProcessEnvironment(Path),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
    }

    internal string RecoveryDirectory()
        => RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Workspace);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            CleanupRecoveryArtifacts();
            RemoveTestEntry(Combine(".agents"));
        }
        finally
        {
            _lockStore.Dispose();
            _temporary.Dispose();
            _disposed = true;
        }
    }

    private void CleanupRecoveryArtifacts()
    {
        string directory;
        try
        {
            directory = RecoveryDirectory();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(directory).ToArray())
        {
            if (File.Exists(entry))
            {
                File.Delete(entry);
            }
            else if (Directory.Exists(entry))
            {
                Directory.Delete(entry, recursive: false);
            }
        }

        if (!Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private static void RemoveTestEntry(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            if ((attributes & FileAttributes.Directory) != 0)
            {
                Directory.Delete(path, recursive: false);
            }
            else
            {
                File.Delete(path);
            }
            return;
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            foreach (var child in Directory.EnumerateFileSystemEntries(path).ToArray())
            {
                RemoveTestEntry(child);
            }
            Directory.Delete(path, recursive: false);
            return;
        }

        File.Delete(path);
    }
}
