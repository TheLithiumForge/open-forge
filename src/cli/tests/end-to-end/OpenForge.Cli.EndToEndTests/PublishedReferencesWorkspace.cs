using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedReferencesWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;

    private PublishedReferencesWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal static PublishedReferencesWorkspace CreateBare()
    {
        var workspace = TemporaryWorkspace.Create("e2e-references-bare");
        try
        {
            workspace.WriteText(
                ".agents/loader.md",
                "# Loader\n\n## Entries\n\n"
                    + "- docs - Docs - #CurrentTruth\n"
                );
            workspace.WriteText(
                ".agents/docs.md",
                "# Docs\n\n"
                + "[target](target.md#Overview)\n"
                + "[duplicate](target.md#Overview)\n"
                + "[space](<space file.md>)\n"
                + "[unicode](unicodé.md#Café)\n"
                + "[missing](missing.md#Absent)\n"
                + "[fragment](#Overview)\n"
                + "[external](https://example.invalid/reference)\n");
            workspace.WriteText(
                ".agents/docs.overwrite.md",
                "# Docs Override\n\n[overwrite](target.md#Overview)\n");
            workspace.WriteText(".agents/target.md", "# Target\n\n## Overview\n");
            workspace.WriteText(".agents/space file.md", "# Space\n");
            workspace.WriteText(".agents/unicodé.md", "# Café\n");
            workspace.WriteText(".agents/alpha.md", "# Alpha\n\n[docs](docs.md)\n");
            workspace.WriteText(".agents/beta.md", "# Beta\n\n[docs](docs.md)\n");
            workspace.WriteText(
                ".agents/reference-route/_references.md",
                "# References compatibility source\n\n[docs](../docs.md)\n");
            return new PublishedReferencesWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose() => _workspace.Dispose();
}
