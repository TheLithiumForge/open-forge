using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedIndexWorkspace : IDisposable
{
    private const string StaleEntries = "- stale";

    internal const string RootPath = ".agents/root/_root.md";
    internal const string ExpectedEntry = "- [Child](child.md) - #Docs";
    internal const string RootPrefix = "# Root\n\nUnrelated prose.";

    private readonly TemporaryWorkspace _workspace;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private readonly List<string> _generatedFiles = [];

    private PublishedIndexWorkspace(
        TemporaryWorkspace workspace,
        PublishedWorkspaceLockStore lockStore)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        _ = _lockStore.Track(_workspace.Path);
    }

    internal string Path => _workspace.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment =>
        _lockStore.EnvironmentVariables;

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal Task<string> ReadRootAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(_workspace.Combine(RootPath), cancellationToken);

    internal string ReadText(string path) => File.ReadAllText(_workspace.Combine(path));
    internal void WriteText(string path, string text) => _workspace.WriteText(path, text);
    internal void TrackGeneratedFiles(params string[] paths) => _generatedFiles.AddRange(paths);

    internal static PublishedIndexWorkspace CreateRooted()
    {
        var workspace = Create();
        workspace.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Root](root/_root.md) - #Workspace\n");
        workspace._workspace.ReplaceText(RootPath, OpenForgeDocumentSeed.Metadata("Root", ["Workspace"],
            "# Root\n\n## Entries\n\n- [Child](child.md) - #Docs\n"));
        return workspace;
    }

    internal static PublishedIndexWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("e2e-index");
        var lockStore = PublishedWorkspaceLockStore.Create("e2e-index-lock-store");
        try
        {
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
            return new PublishedIndexWorkspace(workspace, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal void AssertPersistentExternalLock()
        => _lockStore.AssertPersistentZeroByteLock(Path);

    internal void AssertNoLockInfrastructure()
        => _lockStore.AssertNoInfrastructure();

    public void Dispose()
    {
        foreach (var path in _generatedFiles.Select(path => _workspace.Combine(path)))
        {
            if (!File.Exists(path)) continue;
            Assert.Equal((FileAttributes)0, File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            File.Delete(path);
        }
        _lockStore.Dispose();
        _workspace.Dispose();
    }
}
