using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedCleanupWorkspace : IDisposable
{
    private const string EligibleFinalName = "operation-11111111111111111111111111111111.zip";
    private const string EligibleDraftName = "operation-22222222222222222222222222222222.draft";
    private const string ForeignBlockedName = "operation-33333333333333333333333333333333.zip";
    private const string UnknownName = "unknown-support.txt";
    private const string NestedUnknownDirectoryName = "unknown-support";
    private const string NestedSentinelName = "sentinel.txt";
    private static readonly byte[] DraftBytes = Encoding.UTF8.GetBytes("incomplete cleanup draft\n");
    private static readonly byte[] ForeignBlockedBytes = Encoding.UTF8.GetBytes("malformed recovery archive\n");
    private static readonly byte[] NestedSentinelBytes = Encoding.UTF8.GetBytes("nested support sentinel\n");
    private readonly TemporaryWorkspace _workspace;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private readonly string _localApplicationData;
    private readonly string _recoveryStoreRoot;
    private readonly string _selectedRecoveryDirectory;
    private readonly string _foreignRecoveryDirectory;
    private readonly string _lockPath;
    private bool _disposed;

    private PublishedCleanupWorkspace(
        TemporaryWorkspace workspace,
        PublishedWorkspaceLockStore lockStore)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        _lockPath = _lockStore.Track(workspace.Path);
        _localApplicationData = ResolveLocalApplicationData(lockStore.EnvironmentVariables);
        _recoveryStoreRoot = System.IO.Path.Combine(_localApplicationData, "OpenForge", "recovery", "v1");
        _selectedRecoveryDirectory = System.IO.Path.Combine(_recoveryStoreRoot, WorkspaceKey(workspace.Path));
        _foreignRecoveryDirectory = System.IO.Path.Combine(
            _recoveryStoreRoot,
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("foreign-workspace"))));
        EligibleFinalPath = System.IO.Path.Combine(_selectedRecoveryDirectory, EligibleFinalName);
        EligibleDraftPath = System.IO.Path.Combine(_selectedRecoveryDirectory, EligibleDraftName);
        UnknownPath = System.IO.Path.Combine(_selectedRecoveryDirectory, UnknownName);
        ForeignBlockedPath = System.IO.Path.Combine(_foreignRecoveryDirectory, ForeignBlockedName);
        NestedUnknownDirectoryPath = System.IO.Path.Combine(
            _selectedRecoveryDirectory,
            NestedUnknownDirectoryName);
        NestedSentinelPath = System.IO.Path.Combine(NestedUnknownDirectoryPath, NestedSentinelName);
    }

    internal string Path => _workspace.Path;

    internal string EligibleFinalPath { get; }

    internal string EligibleDraftPath { get; }

    internal string UnknownPath { get; }

    internal string ForeignBlockedPath { get; }

    private string NestedUnknownDirectoryPath { get; }

    internal string NestedSentinelPath { get; }

    private static string NestedSentinelStateKey
        => $"recovery/selected/{NestedUnknownDirectoryName}/{NestedSentinelName}";

    internal IReadOnlyDictionary<string, string> ProcessEnvironment => _lockStore.EnvironmentVariables;

    internal static PublishedCleanupWorkspace CreateEmpty()
    {
        var workspace = TemporaryWorkspace.Create("e2e-cleanup-workspace");
        var lockStore = PublishedWorkspaceLockStore.Create("e2e-cleanup-lock-store");
        PublishedCleanupWorkspace? result = null;
        try
        {
            result = new PublishedCleanupWorkspace(workspace, lockStore);
            workspace.WriteText("workspace-note.md", "preserve workspace content\n");
            return result;
        }
        catch
        {
            if (result is not null)
            {
                result.Dispose();
            }
            else
            {
                lockStore.Dispose();
                workspace.Dispose();
            }

            throw;
        }
    }

    internal static PublishedCleanupWorkspace CreateWithArtifacts()
    {
        var result = CreateEmpty();
        try
        {
            result.SeedArtifacts();
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    internal string Combine(params string[] segments) => _workspace.Combine(segments);

    internal IReadOnlyDictionary<string, string> SnapshotState()
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, hash) in _workspace.SnapshotHashes())
        {
            state[$"workspace/{path}"] = hash;
        }

        state["infrastructure/local-application-data"] = PathState(_localApplicationData);
        var openForge = System.IO.Path.Combine(_localApplicationData, "OpenForge");
        state["infrastructure/openforge"] = PathState(openForge);
        state["infrastructure/locks"] = PathState(System.IO.Path.Combine(openForge, "locks"));
        state["infrastructure/locks-v1"] = PathState(System.IO.Path.Combine(openForge, "locks", "v1"));
        state["infrastructure/lock"] = LockPathState(_lockPath);
        CaptureDirectory(state, "recovery/store", _recoveryStoreRoot);
        CaptureDirectory(state, "recovery/selected", _selectedRecoveryDirectory);
        CaptureDirectory(state, "recovery/foreign", _foreignRecoveryDirectory);
        return state;
    }

    internal static string HashFile(string path)
        => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));

    internal static void AssertNestedSupportSnapshot(
        IReadOnlyDictionary<string, string> snapshot,
        string expectedHash)
    {
        Assert.Equal("directory", snapshot[$"recovery/selected/{NestedUnknownDirectoryName}"]);
        Assert.Equal($"file:{expectedHash}", snapshot[NestedSentinelStateKey]);
    }

    internal void AssertNoLockInfrastructure()
    {
        Assert.False(File.Exists(_lockPath));
        Assert.False(File.Exists(_workspace.Combine(".agents", "open-forge.lock")));
    }

    internal void AssertPersistentExternalLock() => _lockStore.AssertPersistentZeroByteLock(Path);

    internal FileStream HoldWorkspaceLease()
        => new(_lockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            DeleteFixtureFile(EligibleFinalPath);
            DeleteFixtureFile(EligibleDraftPath);
            DeleteFixtureFile(UnknownPath);
            DeleteFixtureFile(ForeignBlockedPath);
            DeleteFixtureFile(NestedSentinelPath);
            DeleteFixtureDirectory(NestedUnknownDirectoryPath);
            DeleteFixtureDirectory(_selectedRecoveryDirectory);
            DeleteFixtureDirectory(_foreignRecoveryDirectory);
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

    private void SeedArtifacts()
    {
        Directory.CreateDirectory(_selectedRecoveryDirectory);
        Directory.CreateDirectory(_foreignRecoveryDirectory);
        Directory.CreateDirectory(NestedUnknownDirectoryPath);
        File.WriteAllBytes(EligibleDraftPath, DraftBytes);
        File.WriteAllBytes(UnknownPath, Encoding.UTF8.GetBytes("preserve unknown support\n"));
        File.WriteAllBytes(ForeignBlockedPath, ForeignBlockedBytes);
        File.WriteAllBytes(NestedSentinelPath, NestedSentinelBytes);
        WriteFinal(EligibleFinalPath, Path, Guid.ParseExact("11111111111111111111111111111111", "N"));
    }

    private static void WriteFinal(string path, string workspacePath, Guid operationId)
    {
        var priorBytes = Encoding.UTF8.GetBytes("previous target bytes\n");
        var workspaceKey = WorkspaceKey(workspacePath);
        var manifest = $$"""
            {
              "schemaVersion": 1,
              "command": "route move",
              "operationId": "{{operationId:N}}",
              "workspacePath": "{{JsonEncodedText.Encode(workspacePath)}}",
              "workspaceKey": "{{workspaceKey}}",
              "attribution": {
                "producer": "route",
                "operation": "move",
                "subject": {
                  "kind": "workspace",
                  "identity": "{{workspaceKey}}"
                }
              },
              "entries": [{
                "ordinal": 0,
                "target": "cleanup-target.md",
                "kind": "delete",
                "priorLength": {{priorBytes.Length}},
                "priorSha256": "{{Hash(priorBytes)}}",
                "payload": "payloads/00000000.bin",
                "intendedAbsent": true,
                "intendedLength": null,
                "intendedSha256": null
              }]
            }
            """;
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false);
        var manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.NoCompression);
        using (var manifestStream = manifestEntry.Open())
        {
            var manifestBytes = Encoding.UTF8.GetBytes(manifest);
            manifestStream.Write(manifestBytes);
        }

        var payloadEntry = archive.CreateEntry("payloads/00000000.bin", CompressionLevel.NoCompression);
        using var payloadStream = payloadEntry.Open();
        payloadStream.Write(priorBytes);
    }

    private static string WorkspaceKey(string workspacePath)
    {
        var fullPath = System.IO.Path.GetFullPath(workspacePath);
        var normalized = System.IO.Path.TrimEndingDirectorySeparator(fullPath);
        normalized = string.IsNullOrEmpty(normalized)
            ? System.IO.Path.GetPathRoot(fullPath)
                ?? throw new InvalidOperationException("The cleanup fixture requires a rooted workspace.")
            : normalized;
        var identity = OperatingSystem.IsWindows() ? normalized.ToUpperInvariant() : normalized;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private static string Hash(byte[] bytes)
        => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static string ResolveLocalApplicationData(IReadOnlyDictionary<string, string> environment)
    {
        string localApplicationData;
        if (OperatingSystem.IsWindows())
        {
            localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.DoNotVerify);
        }
        else if (environment.TryGetValue("XDG_DATA_HOME", out var configured))
        {
            localApplicationData = configured;
        }
        else
        {
            throw new InvalidOperationException("The cleanup fixture requires isolated local application data.");
        }

        if (string.IsNullOrWhiteSpace(localApplicationData)
            || !System.IO.Path.IsPathFullyQualified(localApplicationData))
        {
            throw new InvalidOperationException(
                "The cleanup fixture requires an absolute local application-data path.");
        }

        return System.IO.Path.GetFullPath(localApplicationData);
    }

    private static void CaptureDirectory(
        IDictionary<string, string> state,
        string prefix,
        string directory)
    {
        if (!TryGetAttributes(directory, out var attributes))
        {
            state[prefix] = "absent";
            return;
        }

        state[prefix] = PathState(directory, attributes);
        if (!IsOrdinaryDirectory(attributes))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(directory).Order(StringComparer.Ordinal))
        {
            var childPrefix = $"{prefix}/{System.IO.Path.GetFileName(entry)}";
            CaptureDirectory(state, childPrefix, entry);
        }
    }

    private static string PathState(string path, FileAttributes attributes)
    {
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            return "reparse-point";
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            return "directory";
        }

        if ((attributes & FileAttributes.Device) != 0)
        {
            return "special";
        }

        return $"file:{Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)))}";
    }

    private static string PathState(string path)
    {
        return TryGetAttributes(path, out var attributes)
            ? PathState(path, attributes)
            : "absent";
    }

    private static bool TryGetAttributes(string path, out FileAttributes attributes)
    {
        try
        {
            attributes = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            attributes = default;
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            attributes = default;
            return false;
        }
    }

    private static bool IsOrdinaryDirectory(FileAttributes attributes)
        => (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            == FileAttributes.Directory;

    private static string LockPathState(string path)
    {
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(path);
        }
        catch (FileNotFoundException)
        {
            return "absent";
        }
        catch (DirectoryNotFoundException)
        {
            return "absent";
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            return "directory";
        }

        return $"file-length:{new FileInfo(path).Length}";
    }

    private static void DeleteFixtureFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException($"The cleanup fixture file is not ordinary: {path}");
        }

        File.Delete(path);
    }

    private static void DeleteFixtureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException($"The cleanup fixture directory is not ordinary: {path}");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
