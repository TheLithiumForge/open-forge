using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedIndexWorkspace : IDisposable
{
    private const string WorkspaceLockPath = ".agents/open-forge.lock";
    private const string StaleEntries = "stale";

    internal const string RootPath = ".agents/root/_root.md";
    internal const string ExpectedEntry = "- [Child](child.md) - #Docs";
    internal const string RootPrefix = "# Root\n\nUnrelated prose.";

    private readonly TemporaryWorkspace _workspace;

    private PublishedIndexWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal Task<string> ReadRootAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(_workspace.Combine(RootPath), cancellationToken);

    internal static PublishedIndexWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("e2e-index");
        try
        {
            workspace.WriteText(WorkspaceLockPath, "available");
            workspace.WriteText(
                RootPath,
                OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = StaleEntries,
                    Prefix = RootPrefix,
                }));
            workspace.WriteText(
                ".agents/root/child.md",
                OpenForgeDocumentSeed.Metadata(
                    description: "Child",
                    tags: ["Docs"],
                    body: "\n# Child\n"));
            return new PublishedIndexWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose() => _workspace.Dispose();
}
