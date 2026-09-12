using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

internal sealed class IndexOperationWorkspace : IDisposable
{
    private const string StaleEntries = "stale";

    internal const string RootPath = ".agents/root/_root.md";
    internal const string ChildPath = ".agents/root/child.md";
    internal const string ExpectedEntry = "- [Child](child.md) - #Docs";
    internal const string RootPrefix = "# Root\n\nUnrelated prose.";
    internal const string AlphaPath = ".agents/alpha/_alpha.md";
    internal const string AlphaChildPath = ".agents/alpha/child.md";
    internal const string AlphaExpectedEntry = "- [Alpha Child](child.md) - #Alpha";
    internal const string AlphaPrefix = "# Alpha\n\nAlpha unrelated prose.";
    internal const string BetaPath = ".agents/beta/_beta.md";
    internal const string BetaChildPath = ".agents/beta/child.md";
    internal const string BetaExpectedEntry = "- [Beta Child](child.md) - #Beta";
    internal const string BetaPrefix = "# Beta\n\nBeta unrelated prose.";

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;

    private IndexOperationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = _lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal string RootPhysicalPath => _temporary.Combine(RootPath);

    internal static IndexOperationWorkspace Create(
        string purpose,
        string generatedEntries = StaleEntries)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            temporary.WriteText(
                RootPath,
                OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Entries = generatedEntries,
                    Prefix = RootPrefix,
                }));
            temporary.WriteText(
                ChildPath,
                OpenForgeDocumentSeed.Metadata(
                    description: "Child",
                    tags: ["Docs"],
                    body: "# Child\n"));
            return new IndexOperationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal static IndexOperationWorkspace CreateMultiTarget(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            WriteTarget(
                temporary: temporary,
                seed: new TargetSeed
                {
                    TargetPath = AlphaPath,
                    ChildPath = AlphaChildPath,
                    Prefix = AlphaPrefix,
                    Description = "Alpha Child",
                    Tag = "Alpha",
                });
            WriteTarget(
                temporary: temporary,
                seed: new TargetSeed
                {
                    TargetPath = BetaPath,
                    ChildPath = BetaChildPath,
                    Prefix = BetaPrefix,
                    Description = "Beta Child",
                    Tag = "Beta",
                });
            return new IndexOperationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal IndexRequest Request(IndexMode mode)
        => new(
            Workspace,
            [RootPath],
            mode);

    internal IndexRequest MultiTargetRequest(IndexMode mode)
        => new(
            Workspace,
            [BetaPath, AlphaPath],
            mode);

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal void ReplaceRootBytes(byte[] bytes)
        => _temporary.ReplaceBytes(RootPath, bytes);

    internal void ReplaceRootText(string text)
        => _temporary.ReplaceText(RootPath, text);

    internal FileStream HoldLock()
        => _lockStore.OpenExclusive(Workspace);

    internal Task<string> ReadRootAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(RootPhysicalPath, cancellationToken);

    internal Task<string> ReadTargetAsync(
        string canonicalPath,
        CancellationToken cancellationToken)
        => File.ReadAllTextAsync(_temporary.Combine(canonicalPath), cancellationToken);

    internal async ValueTask<int> ReadRecoveryCandidateCountAsync(
        CancellationToken cancellationToken)
    {
        var result = await RecoveryBundleCatalogue.ReadAsync(Workspace, cancellationToken);
        return result.State switch
        {
            RecoveryBundleCatalogueState.Available => result.Candidates.Length,
            RecoveryBundleCatalogueState.Unavailable => throw new InvalidOperationException(
                "The Index integration recovery catalogue was unavailable."),
            RecoveryBundleCatalogueState.Cancelled => throw new InvalidOperationException(
                "The Index integration recovery catalogue was cancelled."),
            _ => throw new ArgumentOutOfRangeException(
                paramName: null,
                actualValue: result.State,
                message: "The recovery catalogue state is not defined."),
        };
    }

    public void Dispose()
    {
        DeleteRecoveryArtifacts();
        _lockStore.Dispose();
        _temporary.Dispose();
    }

    private static void WriteTarget(
        TemporaryWorkspace temporary,
        TargetSeed seed)
    {
        temporary.WriteText(
            seed.TargetPath,
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = StaleEntries,
                Prefix = seed.Prefix,
            }));
        temporary.WriteText(
            seed.ChildPath,
            OpenForgeDocumentSeed.Metadata(
                description: seed.Description,
                tags: [seed.Tag],
                body: $"# {seed.Description}\n"));
    }

    private sealed record TargetSeed
    {
        public required string TargetPath { get; init; }

        public required string ChildPath { get; init; }

        public required string Prefix { get; init; }

        public required string Description { get; init; }

        public required string Tag { get; init; }
    }

    private void DeleteRecoveryArtifacts()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return;
        }

        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            Workspace.PhysicalRoot);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFileSystemEntries(
                     directory,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            File.Delete(path);
        }

        Directory.Delete(directory);
    }
}
