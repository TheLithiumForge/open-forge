using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedLibraryWorkspace : IDisposable
{
    internal const string Id = "team-knowledge";
    internal const string SourceRoot = "shared/team-knowledge";
    internal const string RecordPath = OwnershipPath;
    internal const string OwnershipPath = ".agents/open-forge.lock.json";
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
            - [Directives](directives/_directives.md) - #Directive
            """);
        Write(".agents/directives/_directives.md", """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            """);
    }

    internal void Record(params string[] paths)
        => RecordAt(".", paths);

    internal void RecordAt(string destinationRoot, params string[] paths)
    {
        OwnershipAt(destinationRoot, paths);
    }

    private void OwnershipAt(string destinationRoot, params string[] paths)
    {
        var orderedPaths = paths.Order(StringComparer.Ordinal).ToArray();
        var pathLines = string.Join(Environment.NewLine,
            orderedPaths.Select((path, index) => $"        \"{path}\"{(index == orderedPaths.Length - 1 ? string.Empty : ",")}"));
        Write(OwnershipPath, string.Join(Environment.NewLine,
        [
            "{",
            "  \"$schema\": \"https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v1/open-forge.lock.schema.json\",",
            "  \"schemaVersion\": 1,",
            "  \"extensions\": [],",
            "  \"libraries\": [",
            "    {",
            "      \"id\": \"team-knowledge\",",
            "      \"sourceRoot\": \"shared/team-knowledge\",",
            $"      \"destinationRoot\": \"{destinationRoot}\",",
            "      \"paths\": [",
            pathLines,
            "      ]",
            "    }",
            "  ]",
            "}"]));
    }

    internal void GrantDocs()
        => Write(".agents/open-forge.json", """
            {"allowInstallPaths":["docs"]}
            """);

    internal void MappedLink(string path, string target) => _workspace.CreateFileSymbolicLink(path, target);

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
        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
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

        var ownership = new FileInfo(Combine(OwnershipPath));
        if (ownership.Exists)
        {
            if ((ownership.Attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("The published Library ownership cleanup target is not an ordinary file.");
            }

            File.Delete(ownership.FullName);
        }

        var parent = Combine(".agents/directives");
        if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
        {
            Directory.Delete(parent);
        }

        _workspace.Dispose();
    }
}
