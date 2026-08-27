using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace OpenForge.Cli.TestSupport;

/// <summary>
/// Owns a real temporary directory and removes it only while its ownership marker
/// still proves that the directory belongs to this instance.
/// </summary>
public sealed class TemporaryWorkspace : IDisposable
{
    private enum OwnedEntryKind
    {
        Directory,
        File,
        DirectoryLink,
        FileLink,
    }

    private const string OwnershipMarkerName = ".open-forge-test-workspace-owner";
    private const string WorkspaceNamePrefix = "open-forge-";
    private const int OwnershipTokenByteLength = 32;
    private const int WorkspacePurposeMaxLength = 48;
    private const int FileBufferSize = 4096;
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly string _ownershipToken;
    private readonly Dictionary<string, OwnedEntryKind> _ownedEntries = new(PathComparer);
    private bool _disposed;

    private TemporaryWorkspace(string path, string ownershipToken)
    {
        Path = path;
        _ownershipToken = ownershipToken;
    }

    /// <summary>
    /// Gets the absolute path of the owned temporary directory.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Creates a uniquely named, marker-owned directory below the operating
    /// system temporary directory.
    /// </summary>
    public static TemporaryWorkspace Create(string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        var safePurpose = SanitizePurpose(purpose);
        var ownershipToken = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(OwnershipTokenByteLength));
        var path = Directory.CreateTempSubdirectory(
            $"{WorkspaceNamePrefix}{safePurpose}-").FullName;
        try
        {
            EnsureOrdinaryDirectory(path);
            CreateOwnershipMarker(path, ownershipToken);
            return new TemporaryWorkspace(path, ownershipToken);
        }
        catch
        {
            TryDeleteFailedCreation(path, ownershipToken);
            throw;
        }
    }

    /// <summary>
    /// Combines one or more paths that are required to remain below this
    /// workspace. The returned path is lexical; it does not resolve links.
    /// </summary>
    public string Combine(params string[] relativeSegments)
    {
        ArgumentNullException.ThrowIfNull(relativeSegments);
        return ResolveRelativePath(relativeSegments);
    }

    /// <summary>
    /// Creates an ordinary directory below this workspace and returns its path.
    /// Missing parent directories are created as ordinary directories.
    /// </summary>
    public string CreateDirectory(string relativePath)
    {
        var path = ResolveRelativePath([relativePath]);
        CreateOwnedDirectories(path);
        return path;
    }

    /// <summary>
    /// Creates a UTF-8 file without a byte-order mark and returns
    /// its path.
    /// </summary>
    public string CreateFile(string relativePath, string contents = "")
    {
        WriteText(relativePath, contents);
        return Combine(relativePath);
    }

    /// <summary>
    /// Creates a byte file and returns its path.
    /// </summary>
    public string CreateFile(string relativePath, byte[] contents)
    {
        WriteBytes(relativePath, contents);
        return Combine(relativePath);
    }

    /// <summary>
    /// Writes a UTF-8 file without a byte-order mark.
    /// </summary>
    public void WriteText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var path = PrepareFilePath(relativePath);
        using var stream = CreateOwnedFile(path);
        using var writer = new StreamWriter(stream, StrictUtf8NoBom);
        writer.Write(contents);
    }

    /// <summary>
    /// Replaces an owned ordinary UTF-8 file without changing its ownership.
    /// </summary>
    public void ReplaceText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var path = ReadOwnedFilePath(relativePath);
        using var stream = OpenOwnedFileForReplacement(path);
        using var writer = new StreamWriter(stream, StrictUtf8NoBom);
        writer.Write(contents);
    }

    /// <summary>
    /// Writes a byte file.
    /// </summary>
    public void WriteBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var path = PrepareFilePath(relativePath);
        using var stream = CreateOwnedFile(path);
        stream.Write(contents);
    }

    /// <summary>
    /// Replaces an owned ordinary byte file without changing its ownership.
    /// </summary>
    public void ReplaceBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);

        var path = ReadOwnedFilePath(relativePath);
        using var stream = OpenOwnedFileForReplacement(path);
        stream.Write(contents);
    }

    /// <summary>
    /// Moves an owned ordinary file to one new contained path and transfers its
    /// ownership record.
    /// </summary>
    public string MoveFile(string sourceRelativePath, string destinationRelativePath)
    {
        var sourcePath = ReadOwnedFilePath(sourceRelativePath);
        var destinationPath = ResolveRelativePath([destinationRelativePath]);
        if (_ownedEntries.ContainsKey(destinationPath) || EntryExists(destinationPath))
        {
            throw new InvalidOperationException("The temporary workspace destination already exists.");
        }

        var parent = System.IO.Path.GetDirectoryName(destinationPath)
            ?? throw new InvalidOperationException("The temporary workspace destination has no parent directory.");
        CreateOwnedDirectories(parent);
        File.Move(sourcePath, destinationPath);
        _ownedEntries.Remove(sourcePath);
        _ownedEntries.Add(destinationPath, OwnedEntryKind.File);
        return destinationPath;
    }

    /// <summary>
    /// Creates a real directory symbolic link. The target is passed to the
    /// operating system unchanged and may be absolute, relative, or dangling.
    /// </summary>
    public string CreateDirectorySymbolicLink(string relativeLinkPath, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var linkPath = PrepareLinkPath(relativeLinkPath);
        Directory.CreateSymbolicLink(linkPath, targetPath);
        _ownedEntries.Add(linkPath, OwnedEntryKind.DirectoryLink);
        return linkPath;
    }

    /// <summary>
    /// Creates a real file symbolic link. The target is passed to the operating
    /// system unchanged and may be absolute, relative, or dangling.
    /// </summary>
    public string CreateFileSymbolicLink(string relativeLinkPath, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var linkPath = PrepareLinkPath(relativeLinkPath);
        File.CreateSymbolicLink(linkPath, targetPath);
        _ownedEntries.Add(linkPath, OwnedEntryKind.FileLink);
        return linkPath;
    }

    /// <summary>
    /// Attempts directory symbolic-link creation and reports false when the
    /// operating system or filesystem does not permit the operation.
    /// </summary>
    public bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        [NotNullWhen(true)] out string? linkPath)
    {
        try
        {
            linkPath = CreateDirectorySymbolicLink(relativeLinkPath, targetPath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            linkPath = null;
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            linkPath = null;
            return false;
        }
        catch (IOException)
        {
            linkPath = null;
            return false;
        }
    }

    /// <summary>
    /// Attempts file symbolic-link creation and reports false when the operating
    /// system or filesystem does not permit the operation.
    /// </summary>
    public bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        [NotNullWhen(true)] out string? linkPath)
    {
        try
        {
            linkPath = CreateFileSymbolicLink(relativeLinkPath, targetPath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            linkPath = null;
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            linkPath = null;
            return false;
        }
        catch (IOException)
        {
            linkPath = null;
            return false;
        }
    }

    /// <summary>
    /// Returns deterministic SHA-256 hashes for ordinary files below the
    /// workspace. Ownership metadata and symbolic links are not followed.
    /// </summary>
    public IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        VerifyOwnership();

        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        SnapshotDirectory(new DirectoryInfo(Path), Path, hashes);
        VerifyOwnership();
        return new ReadOnlyDictionary<string, string>(hashes);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        VerifyOwnership();
        DeleteOwnedEntries();
        _disposed = true;
    }

    private static string SanitizePurpose(string purpose)
    {
        var builder = new StringBuilder(purpose.Length);
        foreach (var character in purpose)
        {
            builder.Append(char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-');
        }

        var safePurpose = builder.ToString().Trim('-');
        if (safePurpose.Length == 0)
        {
            throw new ArgumentException(
                "The temporary workspace purpose must contain a valid path character.",
                nameof(purpose));
        }

        return safePurpose[..Math.Min(safePurpose.Length, WorkspacePurposeMaxLength)];
    }

    private static void CreateOwnershipMarker(string rootPath, string ownershipToken)
    {
        var markerPath = System.IO.Path.Combine(rootPath, OwnershipMarkerName);
        var bytes = StrictUtf8NoBom.GetBytes(ownershipToken);
        using var marker = new FileStream(
            markerPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        marker.Write(bytes);
    }

    private static void EnsureOrdinaryDirectory(string path)
    {
        var info = new DirectoryInfo(path);
        if (!info.Exists
            || (info.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
                != FileAttributes.Directory)
        {
            throw new InvalidOperationException("The temporary workspace root is not an owned ordinary directory.");
        }
    }

    private static void TryDeleteFailedCreation(string path, string ownershipToken)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            EnsureOrdinaryDirectory(path);
            var markerPath = System.IO.Path.Combine(path, OwnershipMarkerName);
            var entries = Directory.EnumerateFileSystemEntries(path).ToArray();
            if (entries.Length == 1
                && PathComparer.Equals(entries[0], markerPath)
                && MarkerMatches(markerPath, ownershipToken))
            {
                File.Delete(markerPath);
                Directory.Delete(path);
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
        }
    }

    private static bool MarkerMatches(string markerPath, string ownershipToken)
    {
        var marker = new FileInfo(markerPath);
        if (!marker.Exists
            || (marker.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0
            || marker.Length != StrictUtf8NoBom.GetByteCount(ownershipToken))
        {
            return false;
        }

        var expected = StrictUtf8NoBom.GetBytes(ownershipToken);
        var actual = File.ReadAllBytes(markerPath);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        var root = System.IO.Path.GetPathRoot(fullPath);
        if (root is not null && PathComparer.Equals(fullPath, root))
        {
            return root;
        }

        return fullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
    }

    private static void SnapshotDirectory(
        DirectoryInfo directory,
        string rootPath,
        IDictionary<string, string> hashes)
    {
        foreach (var entry in directory
                     .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                     .OrderBy(entry => entry.Name, StringComparer.Ordinal))
        {
            if (PathComparer.Equals(entry.FullName, System.IO.Path.Combine(rootPath, OwnershipMarkerName)))
            {
                continue;
            }

            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                continue;
            }

            if ((entry.Attributes & FileAttributes.Directory) != 0)
            {
                SnapshotDirectory((DirectoryInfo)entry, rootPath, hashes);
                continue;
            }

            if (entry is FileInfo && (entry.Attributes & FileAttributes.Device) == 0)
            {
                var relativePath = System.IO.Path.GetRelativePath(
                    rootPath,
                    entry.FullName).Replace('\\', '/');
                hashes[relativePath] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry.FullName)));
            }
        }
    }

    private void DeleteOwnedEntries()
    {
        foreach (var pair in _ownedEntries
                     .OrderByDescending(pair => pair.Key.Length)
                     .ThenByDescending(pair => pair.Key, PathComparer))
        {
            if (pair.Value is OwnedEntryKind.DirectoryLink or OwnedEntryKind.FileLink)
            {
                var target = pair.Value == OwnedEntryKind.DirectoryLink
                    ? new DirectoryInfo(pair.Key).LinkTarget
                    : new FileInfo(pair.Key).LinkTarget;
                if (target is null)
                {
                    if (EntryExists(pair.Key))
                    {
                        throw new InvalidOperationException("An owned temporary link was replaced.");
                    }

                    continue;
                }

                if (pair.Value == OwnedEntryKind.DirectoryLink)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Directory.Delete(pair.Key);
                    }
                    else
                    {
                        File.Delete(pair.Key);
                    }
                }
                else
                {
                    File.Delete(pair.Key);
                }

                continue;
            }

            if (!EntryExists(pair.Key))
            {
                continue;
            }

            var attributes = File.GetAttributes(pair.Key);
            switch (pair.Value)
            {
                case OwnedEntryKind.File:
                    if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    {
                        throw new InvalidOperationException("An owned temporary file was replaced.");
                    }

                    File.Delete(pair.Key);
                    break;
                case OwnedEntryKind.Directory:
                    if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint))
                        != FileAttributes.Directory)
                    {
                        throw new InvalidOperationException("An owned temporary directory was replaced.");
                    }

                    Directory.Delete(pair.Key, recursive: false);
                    break;
                case OwnedEntryKind.DirectoryLink or OwnedEntryKind.FileLink:
                    throw new InvalidOperationException("Owned links must be deleted through no-follow link handling.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(pair), pair.Value, "The owned entry kind is not defined.");
            }
        }

        var markerPath = System.IO.Path.Combine(Path, OwnershipMarkerName);
        var remaining = Directory.GetFileSystemEntries(Path);
        if (remaining.Length != 1 || !PathComparer.Equals(remaining[0], markerPath))
        {
            throw new InvalidOperationException("The temporary workspace contains an unowned entry.");
        }

        File.Delete(markerPath);
        Directory.Delete(Path, recursive: false);
    }

    private string PrepareFilePath(string relativePath)
    {
        var path = ResolveRelativePath([relativePath]);
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The temporary workspace file path has no parent directory.");
        CreateOwnedDirectories(parent);
        if (EntryExists(path))
        {
            throw new InvalidOperationException("Temporary workspace writes require a new ordinary file.");
        }

        return path;
    }

    private string ReadOwnedFilePath(string relativePath)
    {
        var path = ResolveRelativePath([relativePath]);
        if (!_ownedEntries.TryGetValue(path, out var kind)
            || kind != OwnedEntryKind.File
            || !EntryExists(path))
        {
            throw new InvalidOperationException("The temporary workspace path is not an owned ordinary file.");
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The owned temporary file was replaced.");
        }

        return path;
    }

    private static FileStream OpenOwnedFileForReplacement(string path)
    {
        return new FileStream(
            path,
            FileMode.Truncate,
            FileAccess.Write,
            FileShare.None,
            bufferSize: FileBufferSize,
            FileOptions.SequentialScan);
    }

    private FileStream CreateOwnedFile(string path)
    {
        var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: FileBufferSize,
            FileOptions.SequentialScan);
        try
        {
            _ownedEntries.Add(path, OwnedEntryKind.File);
            return stream;
        }
        catch
        {
            stream.Dispose();
            File.Delete(path);
            throw;
        }
    }

    private string PrepareLinkPath(string relativeLinkPath)
    {
        var linkPath = ResolveRelativePath([relativeLinkPath]);
        if (EntryExists(linkPath))
        {
            throw new InvalidOperationException("The symbolic-link path must not already exist.");
        }

        var parent = System.IO.Path.GetDirectoryName(linkPath)
            ?? throw new InvalidOperationException("The temporary workspace link path has no parent directory.");
        CreateOwnedDirectories(parent);
        return linkPath;
    }

    private void CreateOwnedDirectories(string directoryPath)
    {
        var relativePath = System.IO.Path.GetRelativePath(Path, directoryPath);
        if (relativePath == ".")
        {
            return;
        }

        var current = Path;
        foreach (var segment in relativePath.Split(
                     [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = System.IO.Path.Combine(current, segment);
            if (EntryExists(current))
            {
                if (!_ownedEntries.TryGetValue(current, out var existing)
                    || existing != OwnedEntryKind.Directory)
                {
                    throw new InvalidOperationException("Temporary workspace directories cannot adopt unowned entries.");
                }

                EnsureOrdinaryDirectory(current);
                continue;
            }

            CreateExclusiveDirectory(current);
            EnsureOrdinaryDirectory(current);
            _ownedEntries.Add(current, OwnedEntryKind.Directory);
        }
    }

    private static void CreateExclusiveDirectory(string destination)
    {
        var staging = Directory.CreateTempSubdirectory("open-forge-directory-stage-");
        try
        {
            Directory.Move(staging.FullName, destination);
        }
        catch
        {
            if (staging.Exists)
            {
                staging.Delete(recursive: false);
            }

            throw;
        }
    }

    private static bool EntryExists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private string ResolveRelativePath(IReadOnlyList<string> relativeSegments)
    {
        VerifyOwnership();
        if (relativeSegments.Count == 0)
        {
            throw new ArgumentException("At least one relative path segment is required.", nameof(relativeSegments));
        }

        foreach (var segment in relativeSegments)
        {
            ValidateRelativeSegment(segment);
        }

        var allSegments = new string[relativeSegments.Count + 1];
        allSegments[0] = Path;
        for (var index = 0; index < relativeSegments.Count; index++)
        {
            allSegments[index + 1] = relativeSegments[index];
        }

        var target = System.IO.Path.GetFullPath(System.IO.Path.Combine(allSegments));
        var relative = System.IO.Path.GetRelativePath(Path, target);
        if (IsOutside(relative))
        {
            throw new ArgumentException("The relative path leaves the owned temporary workspace.", nameof(relativeSegments));
        }

        if (PathComparer.Equals(target, System.IO.Path.Combine(Path, OwnershipMarkerName)))
        {
            throw new ArgumentException(
                "The relative path is reserved for temporary workspace ownership.",
                nameof(relativeSegments));
        }

        return target;
    }

    private static void ValidateRelativeSegment(string segment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);
        if (System.IO.Path.IsPathRooted(segment) || System.IO.Path.IsPathFullyQualified(segment))
        {
            throw new ArgumentException("Temporary workspace paths must be relative.", nameof(segment));
        }

        var pathSegments = segment.Split(
            [System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar],
            StringSplitOptions.None);
        if (pathSegments.Any(pathSegment => pathSegment.Length == 0 || pathSegment is "." or ".."))
        {
            throw new ArgumentException(
                "Temporary workspace paths cannot contain empty, dot, or traversal segments.",
                nameof(segment));
        }
    }

    private static bool IsOutside(string relativePath)
    {
        return System.IO.Path.IsPathRooted(relativePath)
            || relativePath == ".."
            || relativePath.StartsWith($"..{System.IO.Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || relativePath.StartsWith($"..{System.IO.Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private void VerifyOwnership()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TemporaryWorkspace));
        }

        ValidateOwnedRootShape(Path);
        EnsureOrdinaryDirectory(Path);

        var markerPath = System.IO.Path.Combine(Path, OwnershipMarkerName);
        if (!MarkerMatches(markerPath, _ownershipToken))
        {
            throw new InvalidOperationException("The temporary workspace ownership marker is missing or does not match.");
        }
    }

    private static void ValidateOwnedRootShape(string path)
    {
        var fullPath = NormalizeDirectoryPath(path);
        var temporaryDirectory = NormalizeDirectoryPath(System.IO.Path.GetTempPath());
        var parent = System.IO.Path.GetDirectoryName(fullPath);
        var name = System.IO.Path.GetFileName(fullPath);
        if (parent is null
            || !PathComparer.Equals(NormalizeDirectoryPath(parent), temporaryDirectory)
            || !name.StartsWith(WorkspaceNamePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The temporary workspace path has an unsafe shape.");
        }

        if (name.Length <= WorkspaceNamePrefix.Length)
        {
            throw new InvalidOperationException("The temporary workspace path has an unsafe shape.");
        }
    }
}
