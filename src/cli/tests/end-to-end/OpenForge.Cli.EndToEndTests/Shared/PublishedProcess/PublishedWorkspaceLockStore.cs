using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

internal sealed class PublishedWorkspaceLockStore : IDisposable
{
    private const int MaximumFriendlyNameLength = 48;
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly SemaphoreSlim WindowsCatalogueGate = new(
        initialCount: 1,
        maxCount: 1);
    private readonly TemporaryWorkspace _temporary;
    private readonly Dictionary<string, string> _lockPaths = new(PathComparer);
    private readonly HashSet<string> _testCreatedCatalogueDirectories = new(PathComparer);
    private readonly bool _ownsWindowsCatalogueGate;
    private bool _cataloguePreconditionsRecorded;
    private bool _disposed;

    private PublishedWorkspaceLockStore(
        TemporaryWorkspace temporary,
        bool ownsWindowsCatalogueGate)
    {
        _temporary = temporary;
        _ownsWindowsCatalogueGate = ownsWindowsCatalogueGate;
        var localApplicationData = LocalApplicationDataPath();
        EnvironmentVariables = OperatingSystem.IsWindows()
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["XDG_DATA_HOME"] = localApplicationData,
                ["LOCALAPPDATA"] = localApplicationData,
                ["HOME"] = temporary.Path,
            };
    }

    internal IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

    internal string LocalApplicationDataDirectory => LocalApplicationDataPath();

    internal string RecoveryStoreRoot => Path.Combine(LocalApplicationDataDirectory, "OpenForge", "recovery", "v1");

    internal string RecoveryWorkspaceDirectory(string workspacePath)
        => Path.Combine(RecoveryStoreRoot, RecoveryWorkspaceKey(workspacePath));

    internal static string RecoveryWorkspaceKey(string workspacePath)
        => WorkspaceKey(Normalize(workspacePath));

    internal static PublishedWorkspaceLockStore Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var ownsWindowsCatalogueGate = false;
        try
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsCatalogueGate.Wait();
                ownsWindowsCatalogueGate = true;
            }

            return new PublishedWorkspaceLockStore(
                temporary: temporary,
                ownsWindowsCatalogueGate: ownsWindowsCatalogueGate);
        }
        catch
        {
            if (ownsWindowsCatalogueGate)
            {
                WindowsCatalogueGate.Release();
            }

            temporary.Dispose();
            throw;
        }
    }

    internal string Track(string workspacePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var normalized = Normalize(workspacePath);
        if (_lockPaths.TryGetValue(normalized, out var trackedPath))
        {
            return trackedPath;
        }

        RecordCataloguePreconditions();
        var lockPath = LockPath(normalized);
        RequireAbsent(lockPath);
        _lockPaths.Add(normalized, lockPath);
        return lockPath;
    }

    internal void AssertPersistentZeroByteLock(string workspacePath)
    {
        var lockPath = Track(workspacePath);
        var attributes = AttributesIfPresent(lockPath);
        Assert.NotNull(attributes);
        Assert.True(IsOrdinaryFile(attributes.Value));
        Assert.Equal(0, new FileInfo(lockPath).Length);
        Assert.False(File.Exists(Path.Combine(workspacePath, ".agents", "open-forge.lock")));
    }

    internal void AssertNoRecoveryArtifacts(string workspacePath)
    {
        var recoveryWorkspaceDirectory = RecoveryWorkspaceDirectory(workspacePath);
        var attributes = AttributesIfPresent(recoveryWorkspaceDirectory);
        if (attributes is null)
        {
            return;
        }

        Assert.True(IsOrdinaryDirectory(attributes.Value));
        Assert.Empty(Directory.EnumerateFileSystemEntries(recoveryWorkspaceDirectory));
    }

    internal void AssertNoInfrastructure()
    {
        if (OperatingSystem.IsWindows())
        {
            foreach (var lockPath in _lockPaths.Values)
            {
                RequireAbsent(lockPath);
            }

            return;
        }

        Assert.False(Directory.Exists(LocalApplicationDataPath()));
    }

    internal IReadOnlyList<string> RemoveRecoveryArtifacts(string workspacePath)
    {
        var workspaceDirectory = RecoveryWorkspaceDirectory(workspacePath);
        var attributes = AttributesIfPresent(workspaceDirectory);
        if (attributes is null)
        {
            return [];
        }

        if (!IsOrdinaryDirectory(attributes.Value))
        {
            throw new InvalidOperationException(
                "The published-test recovery workspace path is not an ordinary directory.");
        }

        var artifacts = Directory.EnumerateFiles(workspaceDirectory)
            .Order(StringComparer.Ordinal)
            .ToArray();
        foreach (var artifact in artifacts)
        {
            var fileAttributes = AttributesIfPresent(artifact);
            if (fileAttributes is null || !IsOrdinaryFile(fileAttributes.Value))
            {
                throw new InvalidOperationException(
                    "The published-test recovery artifact is not an ordinary file.");
            }

            File.Delete(artifact);
        }

        DeleteEmptyDirectory(workspaceDirectory);
        if (!OperatingSystem.IsWindows())
        {
            DeleteEmptyDirectoryTree(Path.Combine(LocalApplicationDataPath(), "OpenForge", "recovery"));
        }

        return artifacts;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            foreach (var lockPath in _lockPaths.Values)
            {
                DeleteExactOwnedLock(lockPath);
            }

            if (!OperatingSystem.IsWindows())
            {
                DeleteEmptyDirectoryTree(Path.Combine(LocalApplicationDataPath(), "OpenForge"));
            }

            foreach (var directory in _testCreatedCatalogueDirectories.OrderByDescending(
                         path => path.Length))
            {
                DeleteEmptyOwnedDirectory(directory);
            }

            if (!OperatingSystem.IsWindows())
            {
                DeleteEmptyDirectory(LocalApplicationDataPath());
            }

            if (OperatingSystem.IsMacOS())
            {
                DeleteEmptyDirectory(Path.Combine(_temporary.Path, "Library"));
            }
        }
        finally
        {
            try
            {
                _temporary.Dispose();
            }
            finally
            {
                if (_ownsWindowsCatalogueGate)
                {
                    WindowsCatalogueGate.Release();
                }

                _disposed = true;
            }
        }
    }

    private string LockPath(string normalizedWorkspacePath)
        => Path.Combine(
            StoreDirectory(),
            $"{FriendlyName(normalizedWorkspacePath)}-{WorkspaceKey(normalizedWorkspacePath)}.lock");

    private string StoreDirectory()
        => Path.Combine(LocalApplicationDataPath(), "OpenForge", "locks", "v1");

    private string LocalApplicationDataPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.DoNotVerify);
            if (string.IsNullOrWhiteSpace(localApplicationData)
                || !Path.IsPathFullyQualified(localApplicationData))
            {
                throw new InvalidOperationException(
                    "The Windows published test requires an absolute LocalApplicationData Known Folder.");
            }

            return Path.GetFullPath(localApplicationData);
        }

        return OperatingSystem.IsMacOS()
            ? Path.Combine(_temporary.Path, "Library", "Application Support")
            : _temporary.Combine("application-data");
    }

    private void RecordCataloguePreconditions()
    {
        if (_cataloguePreconditionsRecorded)
        {
            return;
        }

        foreach (var directory in CatalogueDirectories())
        {
            var attributes = AttributesIfPresent(directory);
            if (attributes is null)
            {
                _testCreatedCatalogueDirectories.Add(directory);
                continue;
            }

            if (!IsOrdinaryDirectory(attributes.Value))
            {
                throw new InvalidOperationException(
                    $"The published lock catalogue ancestor is not an ordinary directory: {directory}");
            }
        }

        _cataloguePreconditionsRecorded = true;
    }

    private IReadOnlyList<string> CatalogueDirectories()
    {
        var openForge = Path.Combine(LocalApplicationDataPath(), "OpenForge");
        var locks = Path.Combine(openForge, "locks");
        return [openForge, locks, Path.Combine(locks, "v1")];
    }

    private static string Normalize(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var normalized = Path.TrimEndingDirectorySeparator(fullPath);
        return string.IsNullOrEmpty(normalized)
            ? Path.GetPathRoot(fullPath)
                ?? throw new InvalidOperationException("The published workspace requires a rooted identity.")
            : normalized;
    }

    private static string WorkspaceKey(string normalizedPath)
    {
        var identity = OperatingSystem.IsWindows()
            ? normalizedPath.ToUpperInvariant()
            : normalizedPath;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private static string FriendlyName(string normalizedPath)
    {
        var builder = new StringBuilder();
        var separatorPending = false;
        foreach (var character in Path.GetFileName(normalizedPath))
        {
            var lowered = char.ToLowerInvariant(character);
            if (lowered is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                var requiredLength = separatorPending && builder.Length > 0 ? 2 : 1;
                if (builder.Length + requiredLength > MaximumFriendlyNameLength)
                {
                    break;
                }

                if (separatorPending && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(lowered);
                separatorPending = false;
            }
            else
            {
                separatorPending = builder.Length > 0;
            }
        }

        while (builder.Length > 0 && builder[^1] == '-')
        {
            builder.Length--;
        }

        return builder.Length == 0 ? "workspace" : builder.ToString();
    }

    private static void RequireAbsent(string path)
    {
        if (AttributesIfPresent(path) is not null)
        {
            throw new InvalidOperationException(
                $"The exact published-test workspace lock path must be absent before execution: {path}");
        }
    }

    private static void DeleteExactOwnedLock(string path)
    {
        var attributes = AttributesIfPresent(path);
        if (attributes is null)
        {
            return;
        }

        if (!IsOrdinaryFile(attributes.Value) || new FileInfo(path).Length != 0)
        {
            throw new InvalidOperationException(
                $"The exact published-test workspace lock is not an owned zero-byte ordinary file; cleanup left it visible: {path}");
        }

        File.Delete(path);
    }

    private static void DeleteEmptyOwnedDirectory(string path)
    {
        var attributes = AttributesIfPresent(path);
        if (attributes is null)
        {
            return;
        }

        if (!IsOrdinaryDirectory(attributes.Value))
        {
            throw new InvalidOperationException(
                $"The test-created lock catalogue path is no longer an ordinary directory; cleanup left it visible: {path}");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path, recursive: false);
        }
    }

    private static void DeleteEmptyDirectory(string path)
    {
        var attributes = AttributesIfPresent(path);
        if (attributes is null)
        {
            return;
        }

        if (!IsOrdinaryDirectory(attributes.Value))
        {
            throw new InvalidOperationException(
                $"The isolated published-test directory is not an ordinary directory: {path}");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path, recursive: false);
        }
    }

    private static void DeleteEmptyDirectoryTree(string path)
    {
        var attributes = AttributesIfPresent(path);
        if (attributes is null)
        {
            return;
        }

        if (!IsOrdinaryDirectory(attributes.Value))
        {
            throw new InvalidOperationException(
                $"The isolated published-test directory is not an ordinary directory: {path}");
        }

        foreach (var childPath in Directory.EnumerateDirectories(path))
        {
            DeleteEmptyDirectoryTree(childPath);
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path, recursive: false);
        }
    }

    private static FileAttributes? AttributesIfPresent(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }

    private static bool IsOrdinaryFile(FileAttributes attributes)
        => (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;

    private static bool IsOrdinaryDirectory(FileAttributes attributes)
        => (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint))
            == FileAttributes.Directory;
}
