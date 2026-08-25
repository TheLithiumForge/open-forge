using System.Collections.ObjectModel;
using System.Security.Cryptography;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedFindWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly FileStream? _lockedFile;
    private readonly IReadOnlyDictionary<string, string>? _lockedSnapshot;
    private bool _lockedSnapshotRead;

    private PublishedFindWorkspace(
        TemporaryWorkspace workspace,
        FileStream? lockedFile = null,
        IReadOnlyDictionary<string, string>? lockedSnapshot = null)
    {
        _workspace = workspace;
        _lockedFile = lockedFile;
        _lockedSnapshot = lockedSnapshot;
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotState()
    {
        if (_lockedSnapshot is null)
        {
            return SnapshotState(_workspace);
        }

        if (!_lockedSnapshotRead)
        {
            _lockedSnapshotRead = true;
            return _lockedSnapshot;
        }

        _lockedFile?.Dispose();
        return SnapshotState(_workspace);
    }

    internal static PublishedFindWorkspace CreateBare()
    {
        var workspace = TemporaryWorkspace.Create("e2e-find-bare");
        try
        {
            WriteBaseDocuments(workspace);
            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedFindWorkspace CreateAttention()
    {
        var workspace = TemporaryWorkspace.Create("e2e-find-attention");
        try
        {
            WriteBaseDocuments(workspace);
            workspace.WriteText(
                ".agents/docs/_docs.md",
                "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\nCollision entrypoint\n");
            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedFindWorkspace CreateIncomplete(bool invalidEncoding)
    {
        var workspace = TemporaryWorkspace.Create(invalidEncoding
            ? "e2e-find-invalid-encoding"
            : "e2e-find-incomplete");
        FileStream? lockedFile = null;
        try
        {
            WriteBaseDocuments(workspace);
            if (invalidEncoding)
            {
                workspace.WriteBytes(".agents/bad.md", [0xFF, 0xFE, 0x00, 0x01]);
            }
            else
            {
                workspace.WriteText(".agents/unreadable.md", "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n");
                var snapshot = SnapshotState(workspace);
                lockedFile = new FileStream(
                    workspace.Combine(".agents/unreadable.md"),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);
                return new PublishedFindWorkspace(workspace, lockedFile, snapshot);
            }

            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            lockedFile?.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _lockedFile?.Dispose();
        _workspace.Dispose();
    }

    private static void WriteBaseDocuments(TemporaryWorkspace workspace)
    {
        workspace.WriteText(
            ".agents/loader.md",
            "---\nopen-forge:\n  description: Loader\n  tags: [LoadNow]\n---\n# Loader\n\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- none - No entries - #Empty\n"
            + "<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(
            ".agents/docs.md",
            "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nTarget body\n");
        workspace.WriteText(
            ".agents/docs.overwrite.md",
            "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nOverwrite body\n");
        workspace.WriteText(
            ".agents/guide.md",
            "---\nopen-forge:\n  description: Guide\n  tags: [Reference]\n---\n# Guide\n\n## Details\n\nGuide body\n");
    }

    private static IReadOnlyDictionary<string, string> SnapshotState(TemporaryWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var root = new DirectoryInfo(workspace.Path);
        SnapshotEntry(root, workspace.Path, ".", state);
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
            .OrderBy(child => GetRelativePath(rootPath, child.FullName), StringComparer.Ordinal))
        {
            SnapshotEntry(
                child,
                rootPath,
                GetRelativePath(rootPath, child.FullName),
                state);
        }
    }

    private static string DescribeEntry(
        FileSystemInfo entry,
        string relativePath,
        FileAttributes attributes,
        bool isReparsePoint)
    {
        var type = isReparsePoint
            ? (attributes & FileAttributes.Directory) != 0 ? "directory-reparse" : "file-reparse"
            : (attributes & FileAttributes.Directory) != 0 ? "directory" : "file";
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

    private static string GetRelativePath(string rootPath, string path)
        => System.IO.Path.GetRelativePath(rootPath, path).Replace('\\', '/');
}
