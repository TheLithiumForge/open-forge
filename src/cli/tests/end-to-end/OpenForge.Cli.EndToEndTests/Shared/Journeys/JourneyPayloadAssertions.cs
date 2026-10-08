using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal static class JourneyPayloadAssertions
{
    private const string EntriesMarker = "\n## Entries\n\n";
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static IReadOnlyDictionary<string, string> CanonicalSources(params string[] extensionIds)
    {
        var repository = PublishedExecutableTarget.FindRepositoryRoot();
        var sources = new Dictionary<string, string>(StringComparer.Ordinal);
        AddSources(Path.Combine(repository, "src", "open-forge"), sources);
        foreach (var id in extensionIds)
        {
            AddSources(Path.Combine(repository, "src", "extensions", id, "content"), sources);
        }
        return sources;
    }

    internal static void AssertDelivered(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> sources,
        JourneyFrontmatterForm form,
        params string[] authoredPaths)
    {
        var authored = authoredPaths.ToHashSet(StringComparer.Ordinal);
        var delivered = Directory.EnumerateFiles(workspace.Combine(".agents"), "*.md", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(workspace.Path, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(sources.Keys.Concat(authored).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal), delivered);
        foreach (var path in delivered.Where(path => !authored.Contains(path)))
        {
            var sourceBytes = File.ReadAllBytes(sources[path]);
            var actual = File.ReadAllBytes(workspace.Combine(path));
            if (Path.GetFileName(path) == "SKILL.md")
            {
                Assert.True(sourceBytes.SequenceEqual(actual), $"Native Skill differs from its canonical source: {path}");
                continue;
            }
            var expected = JourneyFrontmatter.RenderCanonical(StrictUtf8.GetString(sourceBytes), form);
            Assert.True(
                OutsideEntriesList(expected) == OutsideEntriesList(StrictUtf8.GetString(actual)),
                $"Delivered Markdown differs from canonical {JourneyFrontmatter.Flag(form)} bytes outside Entries: {path}");
        }
    }

    internal static void AssertEntries(PublishedJourneyWorkspace workspace, string path, params string[] expected)
        => Assert.Equal(expected, Entries(File.ReadAllText(workspace.Combine(path), Encoding.UTF8)));

    internal static void AssertCanonicalEntries(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> sources,
        params string[] projectedPaths)
    {
        var projected = projectedPaths.ToHashSet(StringComparer.Ordinal);
        foreach (var (path, source) in sources.Where(pair => !projected.Contains(pair.Key)))
        {
            var canonical = File.ReadAllText(source, StrictUtf8);
            if (canonical.Contains(EntriesMarker, StringComparison.Ordinal))
            {
                AssertEntries(workspace, path, Entries(canonical));
            }
        }
    }

    internal static string[] Entries(string text)
    {
        var start = text.IndexOf(EntriesMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, "Expected a canonical generated Entries section.");
        var listStart = start + EntriesMarker.Length;
        var listEnd = EntriesListEnd(text, listStart);
        return text[listStart..listEnd].Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    internal static void AssertForm(PublishedJourneyWorkspace workspace, JourneyFrontmatterForm form)
    {
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedInstallWorkspace.SettingsPath)));
        Assert.Equal(1, settings.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(JourneyFrontmatter.Flag(form), settings.RootElement.GetProperty("frontmatter").GetString());
    }

    internal static async Task<ProcessRunResult> RunWithoutWritesAsync(PublishedJourneyWorkspace workspace, params string[] arguments)
    {
        var before = workspace.SnapshotState();
        var externalBefore = SnapshotExternalStore(workspace);
        var result = await workspace.RunAsync(arguments);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(externalBefore, SnapshotExternalStore(workspace));
        return result;
    }

    internal static void AssertCompleted(ProcessRunResult result, int exitCode = 0)
    {
        Assert.True(result.ExitCode == exitCode,
            $"Expected exit {exitCode}, got {result.ExitCode}. Stdout: {result.StandardOutput} Stderr: {result.StandardError}");
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
    }

    private static void AddSources(string root, Dictionary<string, string> sources)
    {
        foreach (var path in Directory.EnumerateFiles(Path.Combine(root, ".agents"), "*.md", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            Assert.True(sources.TryAdd(relative, path), $"Duplicate canonical journey payload: {relative}");
        }
    }

    private static IReadOnlyDictionary<string, string> SnapshotExternalStore(PublishedJourneyWorkspace workspace)
    {
        var root = workspace.LockStore.LocalApplicationDataDirectory;
        if (!Directory.Exists(root)) return new Dictionary<string, string>(StringComparer.Ordinal);
        return PublishedWorkspaceTreeSnapshot.Capture(root);
    }

    private static string OutsideEntriesList(string text)
    {
        var start = text.IndexOf(EntriesMarker, StringComparison.Ordinal);
        if (start < 0) return text;
        var listStart = start + EntriesMarker.Length;
        return text[..listStart] + text[EntriesListEnd(text, listStart)..];
    }

    private static int EntriesListEnd(string text, int start)
    {
        while (text.AsSpan(start).StartsWith("- ", StringComparison.Ordinal))
        {
            var newline = text.IndexOf('\n', start);
            if (newline < 0) return text.Length;
            start = newline + 1;
        }
        return start;
    }
}
