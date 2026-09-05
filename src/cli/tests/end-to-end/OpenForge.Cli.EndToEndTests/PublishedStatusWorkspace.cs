using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedStatusWorkspace : IDisposable
{
    private const string RecoveryApplicationDirectory = "OpenForge";
    private const string RecoveryDirectory = "recovery";
    private const string RecoveryVersionDirectory = "v1";
    private const string RecoveryDraftFileName = "operation-00000000000000000000000000000001.draft";
    private static readonly byte[] RecoveryDraftBytes = Encoding.UTF8.GetBytes("pending recovery draft bytes\n");

    private readonly TemporaryWorkspace _workspace;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private readonly string _lockPath;
    private readonly string _recoveryStoreRoot;
    private readonly string _recoveryWorkspaceDirectory;
    private readonly string _recoveryDraftPath;
    private bool _disposed;

    private PublishedStatusWorkspace(
        TemporaryWorkspace workspace,
        PublishedWorkspaceLockStore lockStore,
        string lockPath,
        string recoveryStoreRoot,
        string recoveryWorkspaceDirectory)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        _lockPath = lockPath;
        _recoveryStoreRoot = recoveryStoreRoot;
        _recoveryWorkspaceDirectory = recoveryWorkspaceDirectory;
        _recoveryDraftPath = System.IO.Path.Combine(
            recoveryWorkspaceDirectory,
            RecoveryDraftFileName);
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal string RecoveryDraftPath => _recoveryDraftPath;

    internal string WorkspaceLockPath => _lockPath;

    internal string RecoveryStoreRoot => _recoveryStoreRoot;

    internal string RecoveryWorkspaceDirectory => _recoveryWorkspaceDirectory;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment => _lockStore.EnvironmentVariables;

    internal IReadOnlyDictionary<string, string> SnapshotWorkspaceFiles() => _workspace.SnapshotHashes();

    internal static PublishedStatusWorkspace Create()
        => Create(includeRecoveryDraft: false);

    internal static PublishedStatusWorkspace CreateIncompleteRecovery()
        => Create(includeRecoveryDraft: true);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _lockStore.RemoveRecoveryArtifacts(_workspace.Path);
        }
        finally
        {
            try
            {
                _lockStore.Dispose();
            }
            finally
            {
                _workspace.Dispose();
                _disposed = true;
            }
        }
    }

    private static PublishedStatusWorkspace Create(bool includeRecoveryDraft)
    {
        var workspace = TemporaryWorkspace.Create("e2e-status");
        var lockStore = PublishedWorkspaceLockStore.Create("e2e-status-lock-store");
        try
        {
            SeedWorkspace(workspace);
            var lockPath = lockStore.Track(workspace.Path);
            var recoveryStoreRoot = ResolveRecoveryStoreRoot(lockStore.EnvironmentVariables);
            var recoveryWorkspaceDirectory = System.IO.Path.Combine(
                recoveryStoreRoot,
                WorkspaceKey(workspace.Path));
            var recoveryDraftPath = System.IO.Path.Combine(
                recoveryWorkspaceDirectory,
                RecoveryDraftFileName);

            if (includeRecoveryDraft)
            {
                Directory.CreateDirectory(recoveryWorkspaceDirectory);
                using var draft = new FileStream(
                    recoveryDraftPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);
                draft.Write(RecoveryDraftBytes);
            }

            return new PublishedStatusWorkspace(
                workspace,
                lockStore,
                lockPath,
                recoveryStoreRoot,
                recoveryWorkspaceDirectory);
        }
        catch
        {
            try
            {
                lockStore.RemoveRecoveryArtifacts(workspace.Path);
            }
            finally
            {
                lockStore.Dispose();
                workspace.Dispose();
            }

            throw;
        }
    }

    private static void SeedWorkspace(TemporaryWorkspace workspace)
    {
        workspace.WriteText(
            "AGENTS.md",
            "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build(
                "- [Status](status/_status.md) - #Status"));
        workspace.WriteText(
            ".agents/status/_status.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Status",
                tags: ["Status"],
                body: "\n# Status\n\nStatus evidence.\n"));
    }

    private static string ResolveRecoveryStoreRoot(IReadOnlyDictionary<string, string> environment)
    {
        string localApplicationData;
        if (OperatingSystem.IsWindows())
        {
            localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.DoNotVerify);
        }
        else if (environment.TryGetValue("XDG_DATA_HOME", out var configuredLocalApplicationData))
        {
            localApplicationData = configuredLocalApplicationData;
        }
        else
        {
            throw new InvalidOperationException(
                "The published status fixture requires an isolated local application-data path.");
        }

        if (string.IsNullOrWhiteSpace(localApplicationData)
            || !System.IO.Path.IsPathFullyQualified(localApplicationData))
        {
            throw new InvalidOperationException(
                "The published status fixture requires an absolute local application-data path.");
        }

        return System.IO.Path.Combine(
            System.IO.Path.GetFullPath(localApplicationData),
            RecoveryApplicationDirectory,
            RecoveryDirectory,
            RecoveryVersionDirectory);
    }

    private static string WorkspaceKey(string workspacePath)
    {
        var normalized = System.IO.Path.TrimEndingDirectorySeparator(
            System.IO.Path.GetFullPath(workspacePath));
        var identity = OperatingSystem.IsWindows()
            ? normalized.ToUpperInvariant()
            : normalized;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }
}
