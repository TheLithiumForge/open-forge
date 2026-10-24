using System.Text;
using System.Text.Json;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests.Shared.MainRoot;

internal sealed class PublishedMainRootWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace = TemporaryWorkspace.Create(nameof(PublishedMainRootWorkspace));
    private readonly PublishedWorkspaceLockStore _store = PublishedWorkspaceLockStore.Create(nameof(PublishedMainRootWorkspace));
    private readonly HashSet<string> _trees = new(StringComparer.Ordinal);
    private readonly PublishedExecutableTarget _target = PublishedExecutableTarget.Discover();

    internal PublishedMainRootWorkspace(string mainRoot)
    {
        MainRoot = mainRoot;
        _store.Track(Path);
        OwnTree(PublishedTokens.Sources.AgentsRoot);
        OwnTree(mainRoot);
    }

    internal string MainRoot { get; }
    internal string Loader => MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    internal string Path => _workspace.Path;
    internal string Combine(string relative) => _workspace.Combine(relative);

    internal void OwnTree(string relative)
    {
        _ = Combine(relative);
        _trees.Add(relative);
        if (System.IO.Path.GetDirectoryName(relative) is { Length: > 0 } parent) _workspace.CreateDirectory(parent);
    }

    internal void Write(string relative, string content)
    {
        if (!_trees.Any(tree => relative.StartsWith(tree + "/", StringComparison.Ordinal)))
            throw new InvalidOperationException("Fixture writes must stay in a declared tree.");
        var path = Combine(relative);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(false));
    }

    internal byte[] Read(string relative) => File.ReadAllBytes(Combine(relative));

    internal string[] EntryTargets(string source)
        => Markdown.Parse(File.ReadAllText(Combine(source))).Descendants<ListItemBlock>()
            .SelectMany(item => item.Descendants<LinkInline>())
            .Select(link => System.IO.Path.GetFullPath(Uri.UnescapeDataString(link.Url!), System.IO.Path.GetDirectoryName(Combine(source))!)).ToArray();

    internal Dictionary<string, byte[]> Bytes()
        => _workspace.SnapshotHashes().Keys.ToDictionary(path => path, Read, StringComparer.Ordinal);

    internal void AssertBytes(Dictionary<string, byte[]> expected)
    {
        var actual = Bytes();
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in expected) Assert.Equal(bytes, actual[path]);
    }

    internal Task<ProcessRunResult> RunRaw(params string[] arguments)
        => PublishedProcessTestSupport.RunAsync(_target, Path, arguments, _store.EnvironmentVariables);

    internal async Task<JsonDocument> Run(params string[] arguments)
    {
        var result = await RunRaw([.. arguments, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        return JsonDocument.Parse(result.StandardOutput);
    }

    internal async Task Install(string form = PublishedTokens.Settings.RootFrontmatter)
    {
        using var result = await Run(PublishedTokens.Install.Command, PublishedTokens.Install.MainRoot, MainRoot,
            PublishedTokens.Install.Automatic, PublishedTokens.Install.Frontmatter, form);
        using var settings = JsonDocument.Parse(Read(PublishedTokens.Settings.RelativePath));
        Assert.Equal(PublishedTokens.Settings.SchemaVersion, settings.RootElement.GetProperty(PublishedTokens.Settings.SchemaVersionProperty).GetInt32());
        if (MainRoot != PublishedTokens.Sources.AgentsRoot)
            Assert.Equal(MainRoot, settings.RootElement.GetProperty(PublishedTokens.Settings.MainRootProperty).GetString());
    }

    internal string Normalize(string text)
        => text.Replace(Path.Replace("\\", "\\\\", StringComparison.Ordinal), "<workspace>", StringComparison.Ordinal)
            .Replace(Path, "<workspace>", StringComparison.Ordinal);

    public void Dispose()
    {
        _store.RemoveRecoveryArtifacts(Path);
        foreach (var tree in _trees.OrderByDescending(path => path.Length)) DeleteEntry(Combine(tree));
        foreach (var file in new[] { PublishedTokens.Layout.AgentHost, PublishedTokens.Layout.ClaudeHost, PublishedTokens.Layout.GitIgnore })
            DeleteEntry(Combine(file));
        _store.Dispose();
        _workspace.Dispose();
    }

    private static void DeleteEntry(string path)
    {
        if (!PathExists(path)) return;
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.Directory) == 0) { File.Delete(path); return; }
        if ((attributes & FileAttributes.ReparsePoint) == 0)
            foreach (var entry in Directory.EnumerateFileSystemEntries(path)) DeleteEntry(entry);
        Directory.Delete(path);
    }

    private static bool PathExists(string path) => File.Exists(path) || Directory.Exists(path) || new FileInfo(path).LinkTarget is not null;
}
