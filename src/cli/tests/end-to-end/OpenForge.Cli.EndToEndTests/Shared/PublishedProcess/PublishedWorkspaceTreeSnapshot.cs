using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

internal static class PublishedWorkspaceTreeSnapshot
{
    internal static IReadOnlyDictionary<string, string> Capture(string rootPath)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        SnapshotEntry(new DirectoryInfo(rootPath), rootPath, ".", state);
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void SnapshotEntry(
        FileSystemInfo entry,
        string rootPath,
        string relativePath,
        IDictionary<string, string> state)
    {
        entry.Refresh();
        var attributes = entry.Attributes;
        var isReparsePoint = (attributes & FileAttributes.ReparsePoint) != 0;
        state[relativePath] = DescribeEntry(entry, relativePath, attributes, isReparsePoint);
        if (isReparsePoint || (attributes & FileAttributes.Directory) == 0)
        {
            return;
        }

        foreach (var child in ((DirectoryInfo)entry)
            .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
            .OrderBy(child => RelativePath(rootPath, child.FullName), StringComparer.Ordinal))
        {
            SnapshotEntry(child, rootPath, RelativePath(rootPath, child.FullName), state);
        }
    }

    private static string DescribeEntry(
        FileSystemInfo entry,
        string relativePath,
        FileAttributes attributes,
        bool isReparsePoint)
    {
        var isDirectory = (attributes & FileAttributes.Directory) != 0;
        var type = (isReparsePoint, isDirectory) switch
        {
            (true, true) => "directory-reparse",
            (true, false) => "file-reparse",
            (false, true) => "directory",
            (false, false) => "file",
        };

        var description = $"type={type};attributes={(int)attributes};creationUtcTicks={entry.CreationTimeUtc.Ticks};lastWriteUtcTicks={entry.LastWriteTimeUtc.Ticks}";
        if (isReparsePoint)
        {
            return $"{description};reparseIdentity={relativePath};linkTarget={entry.LinkTarget ?? "<null>"}";
        }

        if (entry is not FileInfo file)
        {
            return description;
        }

        var bytes = File.ReadAllBytes(file.FullName);
        return $"{description};length={file.Length};sha256={Convert.ToHexString(SHA256.HashData(bytes))}";
    }

    private static string RelativePath(string rootPath, string path)
        => System.IO.Path.GetRelativePath(rootPath, path).Replace('\\', '/');
}
