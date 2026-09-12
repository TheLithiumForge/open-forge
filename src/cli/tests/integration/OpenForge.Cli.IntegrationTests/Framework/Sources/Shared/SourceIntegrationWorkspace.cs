using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

internal sealed class SourceIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private SourceIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static SourceIntegrationWorkspace Create(string purpose)
    {
        return new SourceIntegrationWorkspace(TemporaryWorkspace.Create(purpose));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _temporary.WriteBytes(relativePath, contents);
    }

    internal string CreateDirectory(string relativePath)
    {
        return _temporary.CreateDirectory(relativePath);
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _temporary.SnapshotHashes();
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}
