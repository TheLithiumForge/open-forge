using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed partial class StatusIntegrationWorkspace : IDisposable
{
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string GeneratedTargetPath = ".agents/memory/_memory.md";
    internal const string ExtensionTargetPath = ".agents/status-extension.md";
    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private readonly HashSet<string> _transformedTargets = new(StringComparer.Ordinal);
    private bool _hasInstallArtifacts;
    private bool _disposed;
    private StatusIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        LockPath = _lockStore.Track(Workspace);
    }
    internal CliWorkspace Workspace { get; }
    internal string Path => _temporary.Path;
    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;
    internal string LockPath { get; }

    internal static StatusIntegrationWorkspace Create(
        string purpose,
        bool withEntry = true)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            var workspace = new StatusIntegrationWorkspace(temporary, lockStore);
            if (withEntry)
            {
                workspace.WriteText("AGENTS.md", "# Workspace\n");
            }

            return workspace;
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }
    internal static async Task<StatusIntegrationWorkspace> CreateInstalledAsync(string purpose)
    {
        var workspace = Create(purpose, withEntry: false);
        try
        {
            workspace._hasInstallArtifacts = true;
            using var input = new StringReader(string.Empty);
            using var prompt = new StringWriter();
            var result = await InstallOperationFactory.Create(
                    new CliInteractiveSession(input, prompt, canPrompt: false),
                    workspace.LockStoreRoot)
                .ExecuteAsync(
                    new OpenForge.Cli.Core.Commands.Install.Models.Request.InstallRequest(
                        workspace.Workspace,
                        OpenForge.Cli.Core.Commands.Install.Models.Request.InstallMode.Apply,
                        force: false,
                        automatic: true,
                        allowsInteractiveConfirmation: false),
                    TestContext.Current.CancellationToken);
            if (result.Status != CliSemanticStatus.Complete)
            {
                throw new InvalidOperationException($"The installed Status fixture did not complete: {result.Status}.");
            }

            var lifecycle = StatusLifecycleFixture.Read(workspace);
            StatusLifecycleFixture.WriteSections(
                workspace,
                lifecycle.Framework,
                StatusLifecycleFixture.ExtensionSection(
                    StatusLifecycleFixture.Extensions([], [])));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal string Combine(string relativePath)
        => _temporary.Combine(relativePath);

    internal void WriteText(string relativePath, string contents)
        => _temporary.WriteText(relativePath, contents);

    internal void WriteBytes(string relativePath, byte[] contents)
        => _temporary.WriteBytes(relativePath, contents);

    internal void ReplaceText(string relativePath, string contents)
        => _temporary.ReplaceText(relativePath, contents);

    internal void ReplaceBytes(string relativePath, byte[] contents)
        => _temporary.ReplaceBytes(relativePath, contents);

    internal string CreateDirectory(string relativePath)
        => _temporary.CreateDirectory(relativePath);

    internal bool TryCreateFileSymbolicLink(
        string relativePath,
        string targetPath,
        out string? linkPath)
        => _temporary.TryCreateFileSymbolicLink(relativePath, targetPath, out linkPath);

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal byte[] SnapshotLockBytes()
        => File.Exists(LockPath) ? File.ReadAllBytes(LockPath) : [];

    internal void SeedLockBytes(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var parent = System.IO.Path.GetDirectoryName(LockPath)
            ?? throw new InvalidOperationException("The Status lock path requires a parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(LockPath, bytes);
    }

    internal string RecoveryDirectory()
        => RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(Workspace);

    internal void DeleteOrReplaceWithDirectory(string relativePath)
    {
        var path = Combine(relativePath);
        File.Delete(path);
        Directory.CreateDirectory(path);
        _transformedTargets.Add(path);
    }

    internal void DeleteOrReplaceWithLink(string relativePath, string targetPath)
    {
        var path = Combine(relativePath);
        File.Delete(path);
        File.CreateSymbolicLink(path, targetPath);
        _transformedTargets.Add(path);
    }

    internal void DeleteTarget(string relativePath)
    {
        File.Delete(Combine(relativePath));
        _transformedTargets.Add(Combine(relativePath));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _transformedTargets.OrderByDescending(path => path.Length))
        {
            DeleteTransformedPath(path);
        }

        DeletePostInstallAgentFiles();
        if (_hasInstallArtifacts)
        {
            StatusInstalledWorkspaceArtifacts.Delete(_temporary);
        }

        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private static void DeleteTransformedPath(string path)
    {
        if (new FileInfo(path).LinkTarget is not null)
        {
            File.Delete(path);
            return;
        }

        if (File.Exists(path))
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                File.Delete(path);
            }
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: false);
        }
    }

}
