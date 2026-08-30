using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedExtensionCreateWorkspace : IDisposable
{
    internal const string StableId = "development-toolkit";

    private readonly TemporaryWorkspace _catalogue;
    private readonly TemporaryWorkspace _workspace;

    private PublishedExtensionCreateWorkspace(
        TemporaryWorkspace catalogue,
        TemporaryWorkspace workspace)
    {
        _catalogue = catalogue;
        _workspace = workspace;
    }

    internal string CataloguePath => _catalogue.Path;

    internal string WorkspacePath => _workspace.Path;

    internal string DestinationPath => _catalogue.Combine(StableId);

    internal string ManifestPath => _catalogue.Combine(StableId, "extension.json");

    internal string PayloadAgentsPath => _catalogue.Combine(StableId, "payload", ".agents");

    internal IReadOnlyDictionary<string, string> SnapshotCatalogue() => _catalogue.SnapshotHashes();

    internal IReadOnlyDictionary<string, string> SnapshotWorkspace() => _workspace.SnapshotHashes();

    internal static PublishedExtensionCreateWorkspace Create()
    {
        var catalogue = TemporaryWorkspace.Create("e2e-extension-create-catalogue");
        var workspace = TemporaryWorkspace.Create("e2e-extension-create-workspace");
        try
        {
            catalogue.CreateFile("unrelated.txt", "preserve catalogue sibling");
            workspace.CreateFile("workspace-note.txt", "preserve workspace");
            workspace.CreateFile(".agents/open-forge.lifecycle.json", "preserve lifecycle");
            workspace.CreateFile(".agents/recovery-sentinel.zip", "preserve recovery evidence");
            return new PublishedExtensionCreateWorkspace(catalogue, workspace);
        }
        catch
        {
            workspace.Dispose();
            catalogue.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(DestinationPath))
        {
            Directory.Delete(DestinationPath, recursive: true);
        }
        else if (File.Exists(DestinationPath))
        {
            File.Delete(DestinationPath);
        }

        _workspace.Dispose();
        _catalogue.Dispose();
    }
}
