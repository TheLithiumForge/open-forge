using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedExtensionListWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly TemporaryWorkspace _source;

    private PublishedExtensionListWorkspace(
        TemporaryWorkspace workspace,
        TemporaryWorkspace source)
    {
        _workspace = workspace;
        _source = source;
    }

    internal string Path => _workspace.Path;

    internal string SourcePath => _source.Path;

    internal string MissingSourcePath => _source.Combine("missing-source");

    internal IReadOnlyDictionary<string, string> SnapshotState() => SnapshotTree(_workspace.Path);

    internal IReadOnlyDictionary<string, string> SnapshotSource() => SnapshotTree(_source.Path);

    internal static PublishedExtensionListWorkspace Create(
        bool trustedInstalled = false,
        bool opaqueFrameworkDuplicates = false)
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-list");
        var source = TemporaryWorkspace.Create("e2e-extension-source");
        try
        {
            workspace.WriteText(
                ".agents/open-forge.lifecycle.json",
                Lifecycle(workspace.Path, trustedInstalled, opaqueFrameworkDuplicates));
            source.WriteText(
                "extension.json",
                """
                {
                  "id": "local-toolkit",
                  "name": "Local Toolkit",
                  "description": "A local Extension package.",
                  "version": "1.2.3",
                  "dependencies": []
                }
                """);
            return new PublishedExtensionListWorkspace(
                workspace: workspace,
                source: source);
        }
        catch
        {
            source.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal string CreateMalformedSource()
    {
        _source.WriteText("malformed/extension.json", "{ \"id\": \"invalid\", \"id\": \"duplicate\" }");
        return _source.Combine("malformed");
    }

    public void Dispose()
    {
        _source.Dispose();
        _workspace.Dispose();
    }

    private static string Lifecycle(
        string workspacePath,
        bool trustedInstalled,
        bool opaqueFrameworkDuplicates)
    {
        var packages = trustedInstalled
            ? """
              [{
                "id": "development-toolkit",
                "version": "0.1.0",
                "source": "embedded catalogue",
                "dependencies": [],
                "paths": [".agents/workflows/architecture.md"]
              }]
              """
            : "[]";
        var paths = trustedInstalled
            ? """
              [{
                "path": ".agents/workflows/architecture.md",
                "owners": ["development-toolkit"],
                "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                "fingerprintKind": "semantic"
              }]
              """
            : "[]";
        var framework = opaqueFrameworkDuplicates
            ? """
              {
                "settings": { "enabled": true, "enabled": false },
                "settings": null
              }
              """
            : "null";
        return $$"""
            {
              "schemaVersion": 1,
              "fingerprintPolicy": "open-forge-markdown-v1",
              "workspacePath": "{{JsonEncodedText.Encode(System.IO.Path.GetFullPath(workspacePath))}}",
              "framework": {{framework}},
              "extensions": {
                "coverage": "complete",
                "packages": {{packages}},
                "paths": {{paths}}
              }
            }
            """;
    }

    private static IReadOnlyDictionary<string, string> SnapshotTree(string rootPath)
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
