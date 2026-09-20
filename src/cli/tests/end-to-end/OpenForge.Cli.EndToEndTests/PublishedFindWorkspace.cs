using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedFindWorkspace : IDisposable
{
    internal const string FirstUnavailableFrontmatterPath = ".agents/first-unavailable.md";
    internal const string SecondUnavailableFrontmatterPath = ".agents/second-unavailable.md";

    private readonly TemporaryWorkspace _workspace;
    private readonly FileStream? _lockedFile;
    private readonly IReadOnlyDictionary<string, string>? _lockedSnapshot;
    private bool _lockedSnapshotRead;

    private PublishedFindWorkspace(
        TemporaryWorkspace workspace,
        FileStream? lockedFile = null,
        IReadOnlyDictionary<string, string>? lockedSnapshot = null)
    {
        _workspace = workspace;
        _lockedFile = lockedFile;
        _lockedSnapshot = lockedSnapshot;
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotState()
    {
        if (_lockedSnapshot is null)
        {
            return PublishedWorkspaceTreeSnapshot.Capture(_workspace.Path);
        }

        if (!_lockedSnapshotRead)
        {
            _lockedSnapshotRead = true;
            return _lockedSnapshot;
        }

        _lockedFile?.Dispose();
        return PublishedWorkspaceTreeSnapshot.Capture(_workspace.Path);
    }

    internal void AddUnavailableFrontmatterSources()
    {
        const string malformedDocument = "---\nopen-forge: [unterminated\n---\n# Unavailable\n";
        _workspace.WriteText(FirstUnavailableFrontmatterPath, malformedDocument);
        _workspace.WriteText(SecondUnavailableFrontmatterPath, malformedDocument);
    }

    internal static PublishedFindWorkspace CreateBare()
    {
        var workspace = TemporaryWorkspace.Create("e2e-find-bare");
        try
        {
            WriteBaseDocuments(workspace);
            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedFindWorkspace CreateAttention()
    {
        var workspace = TemporaryWorkspace.Create("e2e-find-attention");
        try
        {
            WriteBaseDocuments(workspace);
            workspace.WriteText(
                ".agents/docs/_docs.md",
                "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\nCollision entrypoint\n");
            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedFindWorkspace CreateIncomplete(bool invalidEncoding)
    {
        var workspace = TemporaryWorkspace.Create(invalidEncoding
            ? "e2e-find-invalid-encoding"
            : "e2e-find-incomplete");
        FileStream? lockedFile = null;
        try
        {
            WriteBaseDocuments(workspace);
            if (invalidEncoding)
            {
                workspace.WriteBytes(".agents/bad.md", [0xFF, 0xFE, 0x00, 0x01]);
            }
            else
            {
                workspace.WriteText(".agents/unreadable.md", "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n");
                var snapshot = PublishedWorkspaceTreeSnapshot.Capture(workspace.Path);
                lockedFile = new FileStream(
                    workspace.Combine(".agents/unreadable.md"),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);
                return new PublishedFindWorkspace(workspace, lockedFile, snapshot);
            }

            return new PublishedFindWorkspace(workspace);
        }
        catch
        {
            lockedFile?.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _lockedFile?.Dispose();
        _workspace.Dispose();
    }

    private static void WriteBaseDocuments(TemporaryWorkspace workspace)
    {
        workspace.WriteText(
            ".agents/loader.md",
            "---\nopen-forge:\n  description: Loader\n  tags: [LoadNow]\n---\n# Loader\n\n"
            + "## Entries\n\n"
            + "- none - No entries - #Empty\n"
            );
        workspace.WriteText(
            ".agents/docs.md",
            "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nTarget body\n");
        workspace.WriteText(
            ".agents/docs.overwrite.md",
            "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nOverwrite body\n");
        workspace.WriteText(
            ".agents/guide.md",
            "---\nopen-forge:\n  description: Guide\n  tags: [Reference]\n---\n# Guide\n\n## Details\n\nGuide body\n");
    }

}
