using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

internal sealed class SourceDocumentReaderIntegrationWorkspace : IDisposable
{
    private readonly SourceIntegrationWorkspace _workspace;

    private SourceDocumentReaderIntegrationWorkspace(SourceIntegrationWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal CliWorkspace Workspace => _workspace.Workspace;

    internal static SourceDocumentReaderIntegrationWorkspace Create()
    {
        return new SourceDocumentReaderIntegrationWorkspace(
            SourceIntegrationWorkspace.Create("source-document-reader-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _workspace.Write(relativePath, contents);
    }

    internal string Absolute(string relativePath)
    {
        return _workspace.Absolute(relativePath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _workspace.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _workspace.SnapshotHashes();
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }
}
