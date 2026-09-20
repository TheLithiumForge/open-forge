using System.Runtime.ExceptionServices;
using System.Text;
using OpenForge.Cli.EndToEndTests;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal sealed class PublishedJourneyWorkspace : IDisposable
{
    private const string OwnershipMarkerName = ".open-forge-test-workspace-owner";
    private const string AgentsPath = "AGENTS.md";
    private const string ClaudePath = "CLAUDE.md";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly TemporaryWorkspace _workspace;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private readonly HashSet<string> _reservedFiles = new(PathComparer);
    private readonly HashSet<string> _reservedParentDirectories = new(PathComparer);
    private bool _disposed;

    private PublishedJourneyWorkspace(
        TemporaryWorkspace workspace,
        PublishedWorkspaceLockStore lockStore,
        PublishedExecutableTarget target)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        Target = target;
    }

    internal string Path => _workspace.Path;

    internal PublishedExecutableTarget Target { get; }

    internal IReadOnlyDictionary<string, string> ProcessEnvironment =>
        _lockStore.EnvironmentVariables;

    internal PublishedWorkspaceLockStore LockStore => _lockStore;

    internal static PublishedJourneyWorkspace Create(string purpose)
    {
        var workspace = TemporaryWorkspace.Create(purpose);
        PublishedWorkspaceLockStore? lockStore = null;
        try
        {
            lockStore = PublishedWorkspaceLockStore.Create($"{purpose}-lock-store");
            _ = lockStore.Track(workspace.Path);
            var target = PublishedExecutableTarget.Discover();
            return new PublishedJourneyWorkspace(workspace, lockStore, target);
        }
        catch
        {
            try
            {
                lockStore?.Dispose();
            }
            finally
            {
                workspace.Dispose();
            }

            throw;
        }
    }

    internal string Combine(params string[] segments)
    {
        var path = _workspace.Combine(segments);
        var markerPath = System.IO.Path.Combine(_workspace.Path, OwnershipMarkerName);
        if (PathComparer.Equals(path, _workspace.Path)
            || PathComparer.Equals(path, markerPath))
        {
            throw new ArgumentException(
                "The journey workspace path is reserved for workspace ownership.",
                nameof(segments));
        }

        ValidateExistingAncestors(path);
        return path;
    }

    internal void ExpectFiles(params string[] relativePaths)
    {
        ArgumentNullException.ThrowIfNull(relativePaths);
        foreach (var relativePath in relativePaths)
        {
            ReserveFile(Combine(relativePath));
        }
    }

    internal void ExpectCoreInstall()
    {
        ExpectFiles(
            PublishedInstallWorkspace.EmbeddedPayloadPaths
                .Append(AgentsPath)
                .Append(ClaudePath)
                .Append(OwnershipPath)
                .ToArray());
    }

    internal void WriteText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = PrepareWritePath(relativePath);
        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = PrepareWritePath(relativePath);
        File.WriteAllBytes(path, contents);
    }

    internal Task<ProcessRunResult> RunAsync(params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return PublishedJourneyProcess.RunAsync(
            Target,
            Path,
            arguments,
            ProcessEnvironment);
    }

    internal IReadOnlyDictionary<string, string> SnapshotState()
        => PublishedWorkspaceTreeSnapshot.Capture(Path);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Exception? failure = null;
        try
        {
            DeleteReservedFiles();
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        try
        {
            DeleteReservedParentDirectories();
        }
        catch (Exception exception)
        {
            failure = CombineFailures(failure, exception);
        }

        try
        {
            _workspace.Dispose();
        }
        catch (Exception exception)
        {
            failure = CombineFailures(failure, exception);
        }

        try
        {
            _lockStore.Dispose();
        }
        catch (Exception exception)
        {
            failure = CombineFailures(failure, exception);
        }

        if (failure is null)
        {
            _disposed = true;
            return;
        }

        ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private string PrepareWritePath(string relativePath)
    {
        var path = Combine(relativePath);
        ReserveFile(path);
        CreateMissingParentDirectories(path);

        if (TryGetAttributes(path, out var attributes) && !IsOrdinaryFile(attributes))
        {
            throw new InvalidOperationException(
                $"The journey workspace write destination is not an ordinary file: {path}");
        }

        return path;
    }

    private void ReserveFile(string path)
    {
        var parents = ParentDirectoryChain(path);
        if (_reservedParentDirectories.Contains(path))
        {
            throw new InvalidOperationException(
                $"The journey workspace path is already reserved as a parent directory: {path}");
        }

        foreach (var parent in parents)
        {
            if (_reservedFiles.Contains(parent))
            {
                throw new InvalidOperationException(
                    $"The journey workspace parent is already reserved as a file: {parent}");
            }
        }

        ValidateFileDestination(path);
        _reservedFiles.Add(path);
        foreach (var parent in parents)
        {
            _reservedParentDirectories.Add(parent);
        }
    }

    private void ValidateFileDestination(string path)
    {
        if (!TryGetAttributes(path, out var attributes)
            || IsOrdinaryFile(attributes)
            || IsFileSymbolicLink(path, attributes))
        {
            return;
        }

        throw new InvalidOperationException(
            $"The journey workspace destination is not an ordinary file or file link: {path}");
    }

    private void CreateMissingParentDirectories(string filePath)
    {
        foreach (var directory in ParentDirectoryChain(filePath))
        {
            if (TryGetAttributes(directory, out var attributes))
            {
                EnsureOrdinaryDirectory(directory, attributes);
                continue;
            }

            Directory.CreateDirectory(directory);
            if (!TryGetAttributes(directory, out attributes))
            {
                throw new InvalidOperationException(
                    $"The journey workspace parent directory was not created: {directory}");
            }

            EnsureOrdinaryDirectory(directory, attributes);
        }
    }

    private void DeleteReservedFiles()
    {
        // Revalidate the marker-owned root before deleting any child, not only
        // when TemporaryWorkspace eventually disposes the empty root.
        _ = _workspace.Combine("journey-ownership-check");
        foreach (var path in _reservedFiles
                     .OrderByDescending(path => path.Length)
                     .ThenByDescending(path => path, PathComparer))
        {
            ValidateExistingAncestors(path);
            if (!TryGetAttributes(path, out var attributes))
            {
                continue;
            }

            if (IsOrdinaryFile(attributes) || IsFileSymbolicLink(path, attributes))
            {
                File.Delete(path);
                continue;
            }

            throw new InvalidOperationException(
                $"The journey workspace cleanup target is not an ordinary file or file link: {path}");
        }
    }

    private void DeleteReservedParentDirectories()
    {
        _ = _workspace.Combine("journey-ownership-check");
        foreach (var path in _reservedParentDirectories
                     .OrderByDescending(path => path.Length)
                     .ThenByDescending(path => path, PathComparer))
        {
            ValidateExistingAncestors(path);
            if (!TryGetAttributes(path, out var attributes))
            {
                continue;
            }

            EnsureOrdinaryDirectory(path, attributes);
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                throw new InvalidOperationException(
                    $"The journey workspace parent directory is not empty: {path}");
            }

            Directory.Delete(path, recursive: false);
        }
    }

    private void ValidateExistingAncestors(string path)
    {
        foreach (var ancestor in ParentDirectoryChain(path))
        {
            if (!TryGetAttributes(ancestor, out var attributes))
            {
                break;
            }

            EnsureOrdinaryDirectory(ancestor, attributes);
        }
    }

    private IReadOnlyList<string> ParentDirectoryChain(string path)
    {
        var parents = new List<string>();
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The journey workspace path has no parent directory.");
        while (!PathComparer.Equals(parent, _workspace.Path))
        {
            parents.Add(parent);
            parent = System.IO.Path.GetDirectoryName(parent)
                ?? throw new InvalidOperationException(
                    "The journey workspace path escaped its owned root.");
        }

        parents.Reverse();
        return parents;
    }

    private static void EnsureOrdinaryDirectory(string path, FileAttributes attributes)
    {
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException(
                $"The journey workspace ancestor is not an ordinary directory: {path}");
        }
    }

    private static bool IsOrdinaryFile(FileAttributes attributes)
        => (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) == 0;

    private static bool IsFileSymbolicLink(string path, FileAttributes attributes)
        => (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint))
            == FileAttributes.ReparsePoint
            && GetLinkTarget(path) is not null;

    private static bool TryGetAttributes(string path, out FileAttributes attributes)
    {
        try
        {
            attributes = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return TryGetDanglingLinkAttributes(path, out attributes);
        }
        catch (DirectoryNotFoundException)
        {
            return TryGetDanglingLinkAttributes(path, out attributes);
        }
    }

    private static bool TryGetDanglingLinkAttributes(string path, out FileAttributes attributes)
    {
        if (GetLinkTarget(path) is null)
        {
            attributes = default;
            return false;
        }

        attributes = FileAttributes.ReparsePoint;
        return true;
    }

    private static string? GetLinkTarget(string path)
    {
        try
        {
            var fileTarget = new FileInfo(path).LinkTarget;
            return fileTarget ?? new DirectoryInfo(path).LinkTarget;
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

    private static Exception CombineFailures(Exception? first, Exception second)
        => first is null
            ? second
            : new AggregateException(
                "Published journey workspace cleanup failed.",
                first,
                second);
}
