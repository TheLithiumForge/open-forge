using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedLibraryWorkspace : IDisposable
{
    internal const string Id = "team-knowledge";
    internal const string SourceRoot = "shared/team-knowledge";
    internal const string RecordPath = ".agents/open-forge.libraries.json";
    internal const string ReviewPath = ".agents/directives/review.md";
    internal const string RawReviewTarget = "../../shared/team-knowledge/.agents/directives/review.md";
    internal const string SourceBody = "---\nopen-forge:\n  description: Review\n  tags: [Directive]\n---\n# Review\n\nSource-owned bytes.\n";

    private readonly TemporaryWorkspace _workspace = TemporaryWorkspace.Create("published-library");
    private readonly PublishedWorkspaceLockStore _store = PublishedWorkspaceLockStore.Create("published-library-store");

    internal PublishedLibraryWorkspace()
    {
        _workspace.CreateDirectory(".agents");
        _workspace.CreateDirectory($"{SourceRoot}/.agents");
        _store.Track(Path);
    }

    internal string Path => _workspace.Path;
    internal string Combine(string path) => _workspace.Combine(path);
    internal void Write(string path, string body) => _workspace.WriteText(path, body);
    internal void Source(string path = ReviewPath) => Write($"{SourceRoot}/{path}", SourceBody);
    internal void Link(string path = ReviewPath)
    {
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("A projection needs a parent.");
        var target = System.IO.Path.GetRelativePath(parent, $"{SourceRoot}/{path}").Replace('\\', '/');
        _workspace.CreateFileSymbolicLink(path, target);
    }

    internal void ConsumerRoute()
    {
        Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        Write(".agents/loader.md", """
            # Loader
            ## Entries
            <!-- open-forge:generated-index:start -->
            - [Directives](directives/_directives.md) - #Directive
            <!-- open-forge:generated-index:end -->
            """);
        Write(".agents/directives/_directives.md", """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            <!-- open-forge:generated-index:start -->
            <!-- open-forge:generated-index:end -->
            """);
    }

    internal void Record(params string[] paths)
    {
        var pathArray = string.Join(",", paths.Order(StringComparer.Ordinal).Select(path => $"\"{path}\""));
        Write(RecordPath, $$"""
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","paths":[{{pathArray}}]}]}
            """);
    }

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _workspace.SnapshotHashes())
        {
            snapshot.Add(pair.Key, pair.Value);
        }

        AddEntries(new DirectoryInfo(Path), snapshot);
        return snapshot;
    }

    private void AddEntries(DirectoryInfo directory, IDictionary<string, string> snapshot)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            var relative = System.IO.Path.GetRelativePath(Path, entry.FullName).Replace('\\', '/');
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                snapshot[relative] = $"link:{entry.LinkTarget}";
            }
            else if (entry is DirectoryInfo child)
            {
                snapshot[relative] = "directory";
                AddEntries(child, snapshot);
            }
        }
    }
    internal Task<ProcessRunResult> RunAsync(PublishedExecutableTarget target, params string[] arguments)
        => PublishedProcessTestSupport.RunAsync(target, Path, arguments, _store.EnvironmentVariables);
    internal Task<ProcessRunResult> ReadOnlyAsync(PublishedExecutableTarget target, params string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, Path, Snapshot, arguments, _store.EnvironmentVariables);
    internal void AssertNoInfrastructure() => _store.AssertNoInfrastructure();
    internal void AssertSource() => Assert.Equal(SourceBody, File.ReadAllText(Combine($"{SourceRoot}/{ReviewPath}")));
    internal void AssertAppliedInfrastructure()
    {
        _store.AssertPersistentZeroByteLock(Path);
        _store.AssertNoRecoveryArtifacts(Path);
    }

    internal static JsonDocument Result(ProcessRunResult result, string status, int exitCode = 0)
    {
        Assert.True(result.ExitCode == exitCode,
            $"Expected exit {exitCode}, actual {result.ExitCode}.\nStandard error:\n{result.StandardError}\nStandard output:\n{result.StandardOutput}");
        Assert.Equal(string.Empty, result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(status, document.RootElement.GetProperty("status").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        return document;
    }

    public void Dispose()
    {
        _store.Dispose();
        var projection = new FileInfo(Combine(ReviewPath));
        if (projection.LinkTarget == RawReviewTarget)
        {
            File.Delete(projection.FullName);
        }

        var record = new FileInfo(Combine(RecordPath));
        if (record.Exists && (record.Attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory)) == 0)
        {
            File.Delete(record.FullName);
        }

        var parent = Combine(".agents/directives");
        if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
        {
            Directory.Delete(parent);
        }

        _workspace.Dispose();
    }
}
