using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal sealed class RoutedOutputWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace = TemporaryWorkspace.Create("routed-command-output");

    internal RoutedOutputWorkspace()
    {
        _workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        _workspace.WriteText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Documents](docs/_docs.md) - #LoadNow #Docs"));
        _workspace.WriteText(".agents/docs/_docs.md", OpenForgeDocumentSeed.Metadata(
            description: "Documents", tags: ["Docs"],
            body: "# Documents\n\n## Entries\n\n- [Guide](guide.md) - #Docs #Guide\n- [Reference](reference.md) - #Docs\n"));
        _workspace.WriteText(".agents/docs/guide.md", OpenForgeDocumentSeed.Metadata(
            description: "Guide", tags: ["Docs", "Guide"],
            body: "# Guide\n\n## Rules\n\nRead the [Reference](reference.md#details).\n"));
        _workspace.WriteText(".agents/docs/reference.md", OpenForgeDocumentSeed.Metadata(
            description: "Reference", tags: ["Docs"],
            body: "# Reference\n\n## Details\n\nUse the guide.\n"));
    }

    internal string Path => _workspace.Path;
    internal void CollidingGuideId()
        => _workspace.WriteText(".agents/docs/guide/_guide.md", OpenForgeDocumentSeed.Metadata(
            description: "Guide category", tags: ["Docs"], body: "# Guide category\n\n## Entries\n\n- none - No entries - #Empty\n"));
    internal void AmbiguousRoot()
        => _workspace.WriteText(".agents/docs/index.md", OpenForgeDocumentSeed.Metadata(
            description: "Compatibility documents", tags: ["Docs"], body: "# Documents\n\n## Entries\n\n- none - No entries - #Empty\n"));
    internal void Write(string path, string text) => _workspace.WriteText(path, text);
    internal void Replace(string path, string text) => _workspace.ReplaceText(path, text);
    internal void ReplaceBytes(string path, byte[] bytes) => _workspace.ReplaceBytes(path, bytes);
    internal IReadOnlyDictionary<string, string> Snapshot() => _workspace.SnapshotHashes();
    public void Dispose() => _workspace.Dispose();
}
