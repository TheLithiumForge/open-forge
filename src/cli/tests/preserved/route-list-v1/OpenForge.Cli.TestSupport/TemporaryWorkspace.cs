using System.Security.Cryptography;
using System.Text;

namespace OpenForge.Cli.TestSupport;

internal sealed class TemporaryWorkspace : IDisposable
{
    private const string OwnershipMarkerName = ".open-forge-test-workspace-owner";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly string _ownershipToken;
    private bool _cleanupCompleted;

    private TemporaryWorkspace(string path, string ownershipToken)
    {
        Path = path;
        _ownershipToken = ownershipToken;
    }

    internal string Path { get; }

    internal static TemporaryWorkspace Create(string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        var safePurpose = SanitizePurpose(purpose);
        while (true)
        {
            var uniqueId = Guid.NewGuid().ToString("N");
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"open-forge-{safePurpose}-{uniqueId}");
            if (Directory.Exists(path) || File.Exists(path))
            {
                continue;
            }

            var ownershipToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            Directory.CreateDirectory(path);
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
    }

    internal string Combine(params string[] relativeSegments)
    {
        return ResolveRelativePath(relativeSegments);
    }

    internal string CreateDirectory(string relativePath)
    {
        var path = ResolveRelativePath([relativePath]);
        Directory.CreateDirectory(path);
        return path;
    }

    internal void WriteText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = PrepareFilePath(relativePath);
        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        File.WriteAllBytes(PrepareFilePath(relativePath), contents);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        VerifyOwnership();
        var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        SnapshotDirectory(new DirectoryInfo(Path), Path, isRoot: true, hashes);
        VerifyOwnership();
        return hashes;
    }

    public void Dispose()
    {
        if (_cleanupCompleted)
        {
            return;
        }

        VerifyOwnership();
        DeleteOwnedDirectory(new DirectoryInfo(Path), isRoot: true);
        _cleanupCompleted = true;
    }

    private static string SanitizePurpose(string purpose)
    {
        var safePurpose = new string(purpose
            .Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-')
            .ToArray())
            .Trim('-');
        if (safePurpose.Length == 0)
        {
            throw new ArgumentException("The temporary workspace purpose must contain a valid path character.", nameof(purpose));
        }

        return safePurpose[..Math.Min(safePurpose.Length, 48)];
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
            || (info.Attributes & FileAttributes.Directory) == 0
            || (info.Attributes & FileAttributes.ReparsePoint) != 0)
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
            var entries = Directory.EnumerateFileSystemEntries(path).ToArray();
            if (entries.Length == 0)
            {
                Directory.Delete(path);
                return;
            }

            var markerPath = System.IO.Path.Combine(path, OwnershipMarkerName);
            if (entries.Length == 1
                && string.Equals(
                    entries[0],
                    markerPath,
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                && File.ReadAllText(markerPath, StrictUtf8NoBom) == ownershipToken)
            {
                File.Delete(markerPath);
                Directory.Delete(path);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
        }
    }

    private static void SnapshotDirectory(
        DirectoryInfo directory,
        string rootPath,
        bool isRoot,
        IDictionary<string, string> hashes)
    {
        foreach (var entry in directory
                     .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                     .OrderBy(entry => entry.Name, StringComparer.Ordinal))
        {
            if ((isRoot && string.Equals(
                    entry.FullName,
                    System.IO.Path.Combine(rootPath, OwnershipMarkerName),
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                || (entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                continue;
            }

            if ((entry.Attributes & FileAttributes.Directory) != 0)
            {
                SnapshotDirectory((DirectoryInfo)entry, rootPath, isRoot: false, hashes);
                continue;
            }

            if (entry is FileInfo && (entry.Attributes & FileAttributes.Device) == 0)
            {
                var relativePath = System.IO.Path.GetRelativePath(rootPath, entry.FullName).Replace('\\', '/');
                hashes.Add(relativePath, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry.FullName))));
            }
        }
    }

    private static void DeleteOwnedDirectory(DirectoryInfo directory, bool isRoot)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
        {
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                DeleteReparseEntry(entry);
                continue;
            }

            if ((entry.Attributes & FileAttributes.Directory) != 0)
            {
                DeleteOwnedDirectory((DirectoryInfo)entry, isRoot: false);
                continue;
            }

            File.Delete(entry.FullName);
        }

        if (!isRoot || !Directory.EnumerateFileSystemEntries(directory.FullName).Any())
        {
            Directory.Delete(directory.FullName);
            return;
        }

        throw new InvalidOperationException("The owned temporary workspace could not be emptied safely.");
    }

    private static void DeleteReparseEntry(FileSystemInfo entry)
    {
        if ((entry.Attributes & FileAttributes.Directory) != 0)
        {
            Directory.Delete(entry.FullName);
        }
        else
        {
            File.Delete(entry.FullName);
        }
    }

    private string PrepareFilePath(string relativePath)
    {
        var path = ResolveRelativePath([relativePath]);
        var parent = System.IO.Path.GetDirectoryName(path);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }

        return path;
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

        var combined = System.IO.Path.Combine(allSegments);
        var target = System.IO.Path.GetFullPath(combined);
        var relative = System.IO.Path.GetRelativePath(Path, target);
        if (System.IO.Path.IsPathRooted(relative)
            || relative == ".."
            || relative.StartsWith($"..{System.IO.Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || relative.StartsWith($"..{System.IO.Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("The relative path leaves the owned temporary workspace.", nameof(relativeSegments));
        }

        if (string.Equals(
            target,
            System.IO.Path.Combine(Path, OwnershipMarkerName),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new ArgumentException("The relative path is reserved for temporary workspace ownership.", nameof(relativeSegments));
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
            throw new ArgumentException("Temporary workspace paths cannot contain empty, dot, or traversal segments.", nameof(segment));
        }
    }

    private void VerifyOwnership()
    {
        if (_cleanupCompleted)
        {
            throw new ObjectDisposedException(nameof(TemporaryWorkspace));
        }

        if (!Directory.Exists(Path))
        {
            throw new InvalidOperationException("The owned temporary workspace root is missing.");
        }

        EnsureOrdinaryDirectory(Path);
        var markerPath = System.IO.Path.Combine(Path, OwnershipMarkerName);
        var marker = new FileInfo(markerPath);
        if (!marker.Exists
            || (marker.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0
            || marker.Length != _ownershipToken.Length)
        {
            throw new InvalidOperationException("The temporary workspace ownership marker is missing or unsafe.");
        }

        string ownershipToken;
        try
        {
            ownershipToken = File.ReadAllText(markerPath, StrictUtf8NoBom);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidOperationException("The temporary workspace ownership marker is invalid.", exception);
        }

        if (!CryptographicOperations.FixedTimeEquals(
                StrictUtf8NoBom.GetBytes(ownershipToken),
                StrictUtf8NoBom.GetBytes(_ownershipToken)))
        {
            throw new InvalidOperationException("The temporary workspace ownership token does not match.");
        }
    }
}
