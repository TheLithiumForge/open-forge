using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteUpdateWorkspace : IDisposable
{
    private const int VerificationBodyLength = 16 * 1024 * 1024;

    internal const string ParentPath = ".agents/memory/project-alpha/_project-alpha.md";
    internal const string TargetId = "memory/project-alpha/overview";
    internal const string TargetPath = ".agents/memory/project-alpha/overview.md";
    internal const string TemplateId = "templates/route";
    internal const string TemplatePath = ".agents/templates/route.md";
    internal const string TemplateBody = "# Exact Template\n\nKeep {tokens} exactly.\n";
    internal const string ExpectedDescription = "After overview";

    private readonly TemporaryWorkspace _temporary;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private bool _disposed;

    private PublishedRouteUpdateWorkspace(
        TemporaryWorkspace temporary,
        PublishedWorkspaceLockStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        _ = _lockStore.Track(temporary.Path);
    }

    internal string Path => _temporary.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment
        => _lockStore.EnvironmentVariables;

    internal static PublishedRouteUpdateWorkspace Create()
    {
        var temporary = TemporaryWorkspace.Create("e2e-route-update");
        var lockStore = PublishedWorkspaceLockStore.Create(
            "e2e-route-update-lock-store");
        try
        {
            temporary.WriteText(
                ".agents/loader.md",
                GeneratedLoaderDocumentBuilder.Build(
                    "- [Project Alpha](memory/project-alpha/_project-alpha.md) - #Project\n"
                    + "- [Templates](templates/_templates.md) - #Template"));
            temporary.WriteText(
                ParentPath,
                ParentDocument("- [Before overview](overview.md) - #Before #Memory"));
            temporary.WriteText(TargetPath, TargetDocument());
            temporary.WriteText(
                ".agents/templates/_templates.md",
                OpenForgeDocumentSeed.Metadata(
                    "Templates",
                    ["Template"],
                    "\n" + OpenForgeDocumentSeed.GeneratedEntries(
                        "- [Route Template](route.md) - #Template")));
            temporary.WriteText(
                TemplatePath,
                OpenForgeDocumentSeed.Metadata(
                    "Route Template",
                    ["Template"],
                    TemplateBody));
            return new PublishedRouteUpdateWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotState()
        => _temporary.SnapshotHashes();

    internal Task<string> ReadTargetAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(
            _temporary.Combine(TargetPath),
            Encoding.UTF8,
            cancellationToken);

    internal Task<string> ReadParentAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(
            _temporary.Combine(ParentPath),
            Encoding.UTF8,
            cancellationToken);

    internal string ReadParent()
        => File.ReadAllText(_temporary.Combine(ParentPath), Encoding.UTF8);

    internal void RemoveTemplate() => File.Delete(_temporary.Combine(TemplatePath));

    internal void RemoveAgentsRoot()
        => Directory.Delete(_temporary.Combine(".agents"), recursive: true);

    internal void SeedAmbiguousTarget()
        => _temporary.WriteText(
            ".agents/memory/project-alpha/overview/_overview.md",
            TargetDocument());

    internal void SeedEmptyBodyTarget()
        => _temporary.ReplaceText(
            TargetPath,
            "---\nopen-forge:\n  description: Before overview\n"
            + "  responsibility: Owns the overview\n"
            + "  tags: [Before, Memory]\n---\n");

    internal void SeedVerificationWindow()
        => _temporary.ReplaceText(
            TargetPath,
            TargetDocument() + new string('x', VerificationBodyLength) + "\n");

    internal void MutateTargetAfterApplication()
        => _temporary.ReplaceText(
            TargetPath,
            TargetDocument().Replace(
                "Before overview",
                "Concurrent overview",
                StringComparison.Ordinal));

    internal IReadOnlyList<string> RemoveRetainedRecoveryArtifacts()
        => _lockStore.RemoveRecoveryArtifacts(Path);

    internal void AssertNoLockInfrastructure() => _lockStore.AssertNoInfrastructure();

    internal void AssertPersistentExternalLock()
        => _lockStore.AssertPersistentZeroByteLock(Path);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private static string TargetDocument()
        => "---\nopen-forge:\n  description: Before overview\n"
            + "  responsibility: Owns the overview\n"
            + "  tags: [Before, Memory]\n---\n\n"
            + "# Authored body\n\nPreserve this body.\n";

    private static string ParentDocument(string entry)
        => OpenForgeDocumentSeed.Metadata(
            "Project Alpha",
            ["Project"],
            "\n" + OpenForgeDocumentSeed.GeneratedEntries(entry));
}
