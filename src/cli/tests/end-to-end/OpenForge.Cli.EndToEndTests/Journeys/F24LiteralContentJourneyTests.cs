using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F24LiteralContentJourneyTests
{
    private const string Feature = "f24-literal-content-journey";
    private const string LoaderPath = ".agents/loader.md";
    private const string ParentPath = ".agents/guidance/_guidance.md";
    private const string NotesPath = ".agents/guidance/notes.md";
    private const string SpaceNotesPath = ".agents/guidance/space note.md";
    private const string ChildPath = ".agents/guidance/child.md";
    private const string LaterAuthoredParagraph = "Authored paragraph after the generated list.";
    private const string LaterAuthoredList = "- This later authored list stays byte-identical.";

    [Fact(DisplayName = "F24 X23 C15-01 X07 X19 reads literal content, updates metadata, indexes one child, and repeats")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F24")]
    public async Task X23C15X07X19CarriesAuthoredBytesAcrossReadUpdateAndIndex()
    {
        using var workspace = await CreateFixtureAsync("f24-main");
        var notesBefore = File.ReadAllBytes(workspace.Combine(NotesPath));
        var notesSpans = LocateDocumentSpans(notesBefore);
        var parentBeforeContext = File.ReadAllBytes(workspace.Combine(ParentPath));
        var contextBefore = CaptureJourneyState(workspace);

        var context = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => CaptureJourneyState(workspace),
            ["context", "guidance/notes", "--additions-only", "--content=body,section:Examples"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, context.ExitCode);
        Assert.Equal(string.Empty, context.StandardError);
        Assert.Contains("Café — literal lifecycle payload", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("## Examples", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("```text", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("printf", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Ω", context.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u00E9", context.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(contextBefore, CaptureJourneyState(workspace));
        Assert.Equal(notesBefore, File.ReadAllBytes(workspace.Combine(NotesPath)));
        Assert.Equal(parentBeforeContext, File.ReadAllBytes(workspace.Combine(ParentPath)));

        var routeStateBefore = CaptureJourneyState(workspace);
        var parentBeforeRouteUpdate = File.ReadAllBytes(workspace.Combine(ParentPath));
        var routeUpdate = await workspace.RunAsync(
            "route",
            "update",
            "guidance/notes",
            "--description",
            "Current team operating notes");

        Assert.Equal(0, routeUpdate.ExitCode);
        Assert.Equal(string.Empty, routeUpdate.StandardError);
        Assert.Contains("guidance/notes", routeUpdate.StandardOutput, StringComparison.Ordinal);

        var notesAfterRouteUpdate = File.ReadAllBytes(workspace.Combine(NotesPath));
        var notesAfterRouteSpans = LocateDocumentSpans(notesAfterRouteUpdate);
        Assert.Equal(
            notesBefore[notesSpans.BodyStart..],
            notesAfterRouteUpdate[notesAfterRouteSpans.BodyStart..]);
        Assert.Equal(
            notesBefore[..notesSpans.DescriptionLine.Start],
            notesAfterRouteUpdate[..notesAfterRouteSpans.DescriptionLine.Start]);
        Assert.Equal(
            notesBefore[notesSpans.DescriptionLine.End..notesSpans.BodyStart],
            notesAfterRouteUpdate[notesAfterRouteSpans.DescriptionLine.End..notesAfterRouteSpans.BodyStart]);
        Assert.Contains(
            "description: Current team operating notes",
            Encoding.UTF8.GetString(notesAfterRouteUpdate[..notesAfterRouteSpans.BodyStart]),
            StringComparison.Ordinal);
        Assert.Contains(
            "unrelated: preserve-me",
            Encoding.UTF8.GetString(notesAfterRouteUpdate[..notesAfterRouteSpans.BodyStart]),
            StringComparison.Ordinal);
        Assert.Contains("notes.md", Encoding.UTF8.GetString(File.ReadAllBytes(workspace.Combine(ParentPath))), StringComparison.Ordinal);
        AssertOnlyManagedListChanged(
            parentBeforeRouteUpdate,
            File.ReadAllBytes(workspace.Combine(ParentPath)));
        AssertEqualExcept(
            workspace,
            routeStateBefore,
            CaptureJourneyState(workspace),
            "workspace/" + ParentPath,
            "workspace/" + NotesPath);

        workspace.WriteBytes(ChildPath, ChildBytes());
        var indexStateBefore = CaptureJourneyState(workspace);
        var parentBeforeIndex = File.ReadAllBytes(workspace.Combine(ParentPath));
        var loaderBeforeIndex = File.ReadAllBytes(workspace.Combine(LoaderPath));

        var indexed = await workspace.RunAsync("index");

        Assert.Equal(0, indexed.ExitCode);
        Assert.Equal(string.Empty, indexed.StandardError);
        Assert.Contains("Entries", indexed.StandardOutput, StringComparison.Ordinal);
        var parentAfterIndex = File.ReadAllBytes(workspace.Combine(ParentPath));
        AssertOnlyManagedListChanged(parentBeforeIndex, parentAfterIndex);
        var parentAfterIndexText = Encoding.UTF8.GetString(parentAfterIndex);
        Assert.Contains("notes.md", parentAfterIndexText, StringComparison.Ordinal);
        Assert.Contains("child.md", parentAfterIndexText, StringComparison.Ordinal);
        Assert.Contains(LaterAuthoredParagraph, parentAfterIndexText, StringComparison.Ordinal);
        Assert.Contains(LaterAuthoredList, parentAfterIndexText, StringComparison.Ordinal);
        var loaderAfterIndex = File.ReadAllBytes(workspace.Combine(LoaderPath));
        AssertOnlyManagedListChanged(loaderBeforeIndex, loaderAfterIndex);
        Assert.Contains(
            "- [Guidance notes](guidance/_guidance.md) - #Guidance",
            Encoding.UTF8.GetString(loaderAfterIndex),
            StringComparison.Ordinal);
        AssertEqualExcept(
            workspace,
            indexStateBefore,
            CaptureJourneyState(workspace),
            "workspace/" + ParentPath,
            "workspace/" + LoaderPath);

        var converged = CaptureJourneyState(workspace);
        var parentAtConvergence = File.ReadAllBytes(workspace.Combine(ParentPath));
        var repeated = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => CaptureJourneyState(workspace),
            ["index"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        Assert.Contains("Nothing to do", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(converged, CaptureJourneyState(workspace));
        Assert.Equal(parentAtConvergence, File.ReadAllBytes(workspace.Combine(ParentPath)));
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
    }

    [Fact(DisplayName = "F24 X15 context preserves an exact quoted path argument and literal sibling content")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F24")]
    public async Task X15QuotedPathUsesArgumentArrayIdentityWithoutWrites()
    {
        using var workspace = await CreateFixtureAsync("f24-quoted-path");
        var before = CaptureJourneyState(workspace);

        var result = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => CaptureJourneyState(workspace),
            ["context", SpaceNotesPath, "--content=body"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Space path payload", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("café", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, CaptureJourneyState(workspace));
        Assert.True(File.Exists(workspace.Combine(SpaceNotesPath)));
    }

    private static async Task<PublishedJourneyWorkspace> CreateFixtureAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                "workspace-note.md",
                ParentPath,
                NotesPath,
                SpaceNotesPath,
                ChildPath);
            workspace.WriteText("workspace-note.md", "Preserve this authored workspace file.\n");

            var install = await workspace.RunAsync("install", "--automatic");
            Assert.Equal(0, install.ExitCode);
            Assert.Equal(string.Empty, install.StandardError);
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

            workspace.WriteBytes(ParentPath, ParentBytes());
            workspace.WriteBytes(NotesPath, NotesBytes());
            workspace.WriteBytes(SpaceNotesPath, SpaceNotesBytes());
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static IReadOnlyDictionary<string, string> CaptureJourneyState(
        PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in workspace.SnapshotState())
        {
            state[$"workspace/{pair.Key}"] = pair.Value;
        }

        var localApplicationData = workspace.LockStore.LocalApplicationDataDirectory;
        AddTree(
            state,
            "catalogue",
            Path.Combine(localApplicationData, "OpenForge", "locks"));
        AddTree(state, "recovery", workspace.LockStore.RecoveryStoreRoot);
        return state;
    }

    private static void AddTree(IDictionary<string, string> state, string prefix, string root)
    {
        if (!Directory.Exists(root))
        {
            state[$"{prefix}/<root>"] = "absent";
            return;
        }

        foreach (var pair in PublishedWorkspaceTreeSnapshot.Capture(root))
        {
            state[$"{prefix}/{pair.Key}"] = pair.Value;
        }
    }

    private static void AssertOnlyManagedListChanged(byte[] before, byte[] after)
    {
        var beforeSpan = LocateFirstEntriesList(before);
        var afterSpan = LocateFirstEntriesList(after);
        Assert.Equal(before[..beforeSpan.Start], after[..afterSpan.Start]);
        Assert.Equal(before[beforeSpan.End..], after[afterSpan.End..]);
    }

    private static void AssertEqualExcept(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        params string[] allowedKeys)
    {
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            SelectStateEntries(before, "workspace/"),
            SelectStateEntries(after, "workspace/"),
            allowedKeys);

        var expectedRecoveryDirectories = ExpectedRecoveryDirectoryKeys(workspace);
        AssertExactEntries(before, after, "catalogue/");

        var exactRecoveryExclusions = expectedRecoveryDirectories
            .Where(path => path.StartsWith("recovery/", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
        exactRecoveryExclusions.Add("recovery/<root>");
        AssertExactEntries(before, after, "recovery/", exactRecoveryExclusions);

        var recoveryRootCreated = AssertRecoveryRootSentinel(before, after);
        foreach (var path in expectedRecoveryDirectories.Order(StringComparer.Ordinal))
        {
            if (recoveryRootCreated || before.ContainsKey(path) || after.ContainsKey(path))
            {
                AssertRecoveryDirectoryEntry(before, after, path);
            }
        }

        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static IReadOnlyDictionary<string, string> SelectStateEntries(
        IReadOnlyDictionary<string, string> state,
        string prefix)
    {
        var selected = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in state)
        {
            if (pair.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                selected.Add(pair.Key, pair.Value);
            }
        }

        return selected;
    }

    private static void AssertExactEntries(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        string prefix,
        IReadOnlySet<string>? excludedKeys = null)
    {
        var beforeEntries = before
            .Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)
                && (excludedKeys is null || !excludedKeys.Contains(pair.Key)))
            .ToArray();
        var afterEntries = after
            .Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)
                && (excludedKeys is null || !excludedKeys.Contains(pair.Key)))
            .ToArray();
        Assert.Equal(beforeEntries, afterEntries);
    }

    private static IReadOnlySet<string> ExpectedRecoveryDirectoryKeys(
        PublishedJourneyWorkspace workspace)
    {
        var expected = new HashSet<string>(StringComparer.Ordinal);
        AddDirectoryChain(
            expected,
            "recovery/",
            workspace.LockStore.RecoveryStoreRoot,
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return expected;
    }

    private static void AddDirectoryChain(
        ISet<string> expected,
        string prefix,
        string root,
        string leaf)
    {
        expected.Add(prefix + ".");
        var relative = Path.GetRelativePath(root, leaf).Replace('\\', '/');
        Assert.False(
            relative == ".." || relative.StartsWith("../", StringComparison.Ordinal),
            $"The recovery workspace path escaped its expected root: {leaf}");

        for (var current = relative; !string.IsNullOrEmpty(current) && current != "."; current =
                 Path.GetDirectoryName(current)?.Replace('\\', '/') ?? ".")
        {
            expected.Add(prefix + current);
        }
    }

    private static void AssertRecoveryDirectoryEntry(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        string path)
    {
        Assert.True(after.TryGetValue(path, out var current), $"Expected recovery directory was not observed: {path}");
        Assert.StartsWith("type=directory;", current, StringComparison.Ordinal);
        if (before.TryGetValue(path, out var previous))
        {
            Assert.StartsWith("type=directory;", previous, StringComparison.Ordinal);
            Assert.True(PublishedJourneyAssertions.DirectoryMetadataMatchesAfterChildMutation(previous, current));
        }
    }

    private static bool AssertRecoveryRootSentinel(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
    {
        const string sentinelPath = "recovery/<root>";
        var hasBefore = before.TryGetValue(sentinelPath, out var previous);
        var hasAfter = after.TryGetValue(sentinelPath, out var current);
        if (!hasBefore)
        {
            Assert.False(hasAfter, $"Unexpected recovery root sentinel was added: {sentinelPath}");
            return false;
        }

        if (string.Equals(previous, "absent", StringComparison.Ordinal) && !hasAfter)
        {
            return true;
        }

        Assert.True(hasAfter, $"Unexpected recovery root sentinel removal: {sentinelPath}");
        Assert.Equal(previous, current);
        return false;
    }

    private static DocumentSpans LocateDocumentSpans(byte[] bytes)
    {
        var openingEnd = LineEnd(bytes, 0);
        Assert.Equal("---", Encoding.UTF8.GetString(bytes[..ContentEnd(bytes, openingEnd)]));
        var closingStart = FindExactLine(bytes, openingEnd, "---");
        var closingEnd = LineEnd(bytes, closingStart);
        return new DocumentSpans(
            BodyStart: closingEnd,
            DescriptionLine: FindLineContaining(bytes, openingEnd, closingStart, "  description:"));
    }

    private static ByteSpan LocateFirstEntriesList(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var heading = text.IndexOf("## Entries", StringComparison.Ordinal);
        Assert.True(heading >= 0, "The authored parent fixture must contain an Entries heading.");
        var position = text.IndexOf('\n', heading);
        Assert.True(position >= 0, "The authored Entries heading must have a following line.");
        position++;

        var firstListStart = -1;
        var firstListEnd = -1;
        while (position < text.Length)
        {
            var contentEnd = text.IndexOf('\n', position);
            var lineEnd = contentEnd < 0 ? text.Length : contentEnd + 1;
            var line = text[position..(contentEnd < 0 ? text.Length : contentEnd)].TrimEnd('\r');
            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                firstListStart = firstListStart < 0 ? position : firstListStart;
                firstListEnd = lineEnd;
                position = lineEnd;
                continue;
            }

            if (firstListStart >= 0)
            {
                break;
            }

            position = lineEnd;
        }

        Assert.True(firstListStart >= 0, "The authored parent fixture must contain a managed Entries list.");
        return new ByteSpan(
            Encoding.UTF8.GetByteCount(text[..firstListStart]),
            Encoding.UTF8.GetByteCount(text[..firstListEnd]));
    }

    private static ByteSpan FindLineContaining(
        byte[] bytes,
        int start,
        int end,
        string needle)
    {
        var needleBytes = Encoding.UTF8.GetBytes(needle);
        for (var lineStart = start; lineStart < end; lineStart = LineEnd(bytes, lineStart))
        {
            var lineEnd = Math.Min(LineEnd(bytes, lineStart), end);
            if (Contains(bytes[lineStart..ContentEnd(bytes, lineEnd)], needleBytes))
            {
                return new ByteSpan(lineStart, lineEnd);
            }
        }

        throw new InvalidOperationException($"The authored fixture does not contain metadata line '{needle}'.");
    }

    private static int FindExactLine(byte[] bytes, int start, string value)
    {
        var expected = Encoding.UTF8.GetBytes(value);
        for (var lineStart = start; lineStart < bytes.Length; lineStart = LineEnd(bytes, lineStart))
        {
            var lineEnd = LineEnd(bytes, lineStart);
            if (bytes[lineStart..ContentEnd(bytes, lineEnd)].SequenceEqual(expected))
            {
                return lineStart;
            }
        }

        throw new InvalidOperationException($"The authored fixture does not contain delimiter '{value}'.");
    }

    private static int LineEnd(byte[] bytes, int start)
    {
        var newline = Array.IndexOf(bytes, (byte)'\n', start);
        return newline < 0 ? bytes.Length : newline + 1;
    }

    private static int ContentEnd(byte[] bytes, int lineEnd)
    {
        var contentEnd = lineEnd;
        if (contentEnd > 0 && bytes[contentEnd - 1] == (byte)'\n')
        {
            contentEnd--;
        }

        if (contentEnd > 0 && bytes[contentEnd - 1] == (byte)'\r')
        {
            contentEnd--;
        }

        return contentEnd;
    }

    private static bool Contains(byte[] value, byte[] search)
    {
        for (var start = 0; start <= value.Length - search.Length; start++)
        {
            if (value[start..(start + search.Length)].SequenceEqual(search))
            {
                return true;
            }
        }

        return false;
    }

    private static byte[] ParentBytes()
        => Encoding.UTF8.GetBytes(
            "---\n"
            + "open-forge:\n"
            + "  description: Guidance notes\n"
            + "  tags: [Guidance]\n"
            + "  unrelated: preserve-parent\n"
            + "---\n\n"
            + "# Guidance\n\n"
            + "Authored paragraph before Entries.\n\n"
            + "## Entries\n\n"
            + "- none - No entries - #Empty\n\n"
            + LaterAuthoredParagraph + "\n\n"
            + LaterAuthoredList + "\n"
            + "- Keep this later line too.\n");

    private static byte[] NotesBytes()
        => Encoding.UTF8.GetBytes(
            "---\r\n"
            + "open-forge:\r\n"
            + "  description: Literal notes\r\n"
            + "  tags: [Guidance, Examples]\n"
            + "  unrelated: preserve-me\r\n"
            + "---\n\n"
            + "# Notes\r\n\n"
            + "Café — literal lifecycle payload\r\n\n"
            + "## Examples\n\n"
            + "```text\r\n"
            + "printf '\tCafé — payload'\r\n"
            + "```\n\n"
            + "- Unicode: naïve — Ω");

    private static byte[] SpaceNotesBytes()
        => Encoding.UTF8.GetBytes(
            "---\n"
            + "open-forge:\n"
            + "  description: Space path note\n"
            + "  tags: [Guidance]\n"
            + "---\n\n"
            + "# Space path note\n\n"
            + "Space path payload — café.\n");

    private static byte[] ChildBytes()
        => Encoding.UTF8.GetBytes(
            "---\n"
            + "open-forge:\n"
            + "  description: Child note\n"
            + "  tags: [Guidance]\n"
            + "---\n\n"
            + "# Child note\n\n"
            + "Child authored body.\n");

    private readonly record struct ByteSpan(int Start, int End);

    private readonly record struct DocumentSpans(int BodyStart, ByteSpan DescriptionLine);
}
