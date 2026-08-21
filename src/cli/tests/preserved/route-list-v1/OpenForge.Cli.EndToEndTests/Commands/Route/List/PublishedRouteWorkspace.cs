using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Commands.Route.List;

internal sealed class PublishedRouteWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;

    private PublishedRouteWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal static PublishedRouteWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("route-list-end-to-end");
        workspace.CreateDirectory(".agents/root");
        workspace.WriteText(
            ".agents/loader.md",
            "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - #Root\n<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(
            ".agents/root/_root.md",
            "---\nopen-forge:\n  description: Root\n  tags: [Root]\n---\n\n# Root\n");
        workspace.WriteText(
            ".agents/root/child.md",
            "---\nopen-forge:\n  description: Child\n  tags: [Child]\n---\n\n# Child\n");
        return new PublishedRouteWorkspace(workspace);
    }

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        return _workspace.SnapshotHashes();
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        _workspace.WriteBytes(relativePath, contents);
    }

    internal void WriteText(string relativePath, string contents)
    {
        _workspace.WriteText(relativePath, contents);
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }
}
