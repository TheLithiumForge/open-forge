using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F03PlainNoteJourneyTests
{
    private const string NotePath = ".agents/guidance/my-note.md";
    private const string GuidanceEntrypointPath = ".agents/guidance/_guidance.md";
    private const string PlainNote =
        "# Team notes\n\n"
        + "Keep decisions close to the work.\n\n"
        + "- Check the result, not only the message.\n";
    private const string EnrichedNote =
        "---\n"
        + "open-forge:\n"
        + "  description: Team notes written by the maintainer\n"
        + "  tags: [Guidance, Team]\n"
        + "---\n\n"
        + PlainNote;
    private const string PartialDescriptionNote =
        "---\n"
        + "open-forge:\n"
        + "  description: Team notes written by the maintainer\n"
        + "custom-label: preserve-me\n"
        + "---\n\n"
        + "# Team notes\n\n"
        + "Keep this authored body unchanged.\n";
    private const string PartialTagsNote =
        "---\n"
        + "open-forge:\n"
        + "  tags: [Guidance, Team]\n"
        + "custom-label: preserve-me\n"
        + "---\n\n"
        + "# Team notes\n\n"
        + "Keep this authored body unchanged.\n";
    private const string MalformedNote =
        "---\n"
        + "open-forge:\n"
        + "  description: [unclosed\n";

    [Fact(
        DisplayName = "F03 carries a plain note through warning-only indexing, context, enrichment, and a no-op"),
        Trait("Feature", "plain-note-indexing-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F03")]
    public async Task PlainNoteJourneyRemainsUsableAndConvergesAfterOptionalEnrichment()
    {
        using var workspace = CreateInstalledWorkspace("e2e-f03-main", NotePath);
        await InstallW1Async(workspace);
        workspace.WriteText(NotePath, PlainNote);
        var authoredBytes = Encoding.UTF8.GetBytes(PlainNote);

        var beforeIndex = workspace.SnapshotState();
        var index = await workspace.RunAsync("index");

        AssertWarningWithOptionalMetadata(index);
        AssertOrdinaryFile(workspace.Combine(NotePath));
        Assert.Equal(authoredBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        var afterIndex = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeIndex, afterIndex, GuidanceEntrypointPath);
        Assert.NotEqual(beforeIndex[GuidanceEntrypointPath], afterIndex[GuidanceEntrypointPath]);
        var guidanceAfterPlainIndex = File.ReadAllText(workspace.Combine(GuidanceEntrypointPath), Encoding.UTF8);
        Assert.Contains("my-note", guidanceAfterPlainIndex, StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        var beforeDoctor = SnapshotState(workspace);
        var doctor = await RunWithoutWritesAsync(workspace, "doctor");
        Assert.Equal(2, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        Assert.Contains("metadata", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Framework update did not finish", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeDoctor, SnapshotState(workspace));

        var beforeContext = SnapshotState(workspace);
        var contextBeforeEnrichment = await RunWithoutWritesAsync(workspace, "context", "guidance/my-note");
        Assert.Equal(2, contextBeforeEnrichment.ExitCode);
        Assert.Equal(string.Empty, contextBeforeEnrichment.StandardError);
        Assert.Contains("# Team notes", contextBeforeEnrichment.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Keep decisions close to the work.", contextBeforeEnrichment.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("metadata", contextBeforeEnrichment.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeContext, SnapshotState(workspace));

        workspace.WriteText(NotePath, EnrichedNote);
        Assert.Equal(Encoding.UTF8.GetBytes(EnrichedNote), File.ReadAllBytes(workspace.Combine(NotePath)));

        var beforeEnrichedIndex = workspace.SnapshotState();
        var enrichedIndex = await workspace.RunAsync("index");

        Assert.Equal(0, enrichedIndex.ExitCode);
        Assert.Equal(string.Empty, enrichedIndex.StandardError);
        Assert.NotEmpty(enrichedIndex.StandardOutput);
        Assert.Equal(Encoding.UTF8.GetBytes(EnrichedNote), File.ReadAllBytes(workspace.Combine(NotePath)));
        var afterEnrichedIndex = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeEnrichedIndex, afterEnrichedIndex, GuidanceEntrypointPath);
        var guidanceAfterEnrichment = File.ReadAllText(workspace.Combine(GuidanceEntrypointPath), Encoding.UTF8);
        Assert.Contains("my-note", guidanceAfterEnrichment, StringComparison.Ordinal);
        Assert.Contains("Team notes written by the maintainer", guidanceAfterEnrichment, StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        var beforeContextAfterEnrichment = SnapshotState(workspace);
        var contextAfterEnrichment = await RunWithoutWritesAsync(workspace, "context", "guidance/my-note");
        Assert.Equal(0, contextAfterEnrichment.ExitCode);
        Assert.Equal(string.Empty, contextAfterEnrichment.StandardError);
        Assert.Contains("Team notes written by the maintainer", contextAfterEnrichment.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("# Team notes", contextAfterEnrichment.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Keep decisions close to the work.", contextAfterEnrichment.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforeContextAfterEnrichment, SnapshotState(workspace));

        var beforeRepeat = SnapshotState(workspace);
        var repeat = await RunWithoutWritesAsync(workspace, "index");

        Assert.Equal(0, repeat.ExitCode);
        Assert.Equal(string.Empty, repeat.StandardError);
        AssertContainsAny(repeat.StandardOutput, "current", "nothing to do", "no change");
        Assert.Equal(beforeRepeat, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    [Fact(
        DisplayName = "F03 plain and partial notes preview warning-only navigation without writes"),
        Trait("Feature", "plain-note-indexing-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F03")]
    public async Task PlainAndPartialNotesDryRunWithWarningIsReadOnly()
    {
        var fixtures = new[]
        {
            ("plain", PlainNote),
            ("partial-description", PartialDescriptionNote),
            ("partial-tags", PartialTagsNote),
        };

        foreach (var (name, contents) in fixtures)
        {
            using var workspace = CreateInstalledWorkspace($"e2e-f03-dry-run-{name}", NotePath);
            await InstallW1Async(workspace);
            workspace.WriteText(NotePath, contents);
            var before = SnapshotState(workspace);

            var dryRun = await RunWithoutWritesAsync(workspace, "index", "--dry-run");

            AssertWarningWithOptionalMetadata(dryRun);
            Assert.Contains("my-note", dryRun.StandardOutput, StringComparison.Ordinal);
            Assert.Equal(before, SnapshotState(workspace));
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
            Assert.Equal(contents, File.ReadAllText(workspace.Combine(NotePath), Encoding.UTF8));
        }
    }

    [Fact(
        DisplayName = "F03 malformed metadata is blocked as malformed and remains distinct from optional metadata"),
        Trait("Feature", "plain-note-indexing-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F03")]
    public async Task MalformedMetadataRemainsDistinctAndReadOnly()
    {
        using var workspace = CreateInstalledWorkspace("e2e-f03-malformed", NotePath);
        await InstallW1Async(workspace);
        workspace.WriteText(NotePath, MalformedNote);
        var before = SnapshotState(workspace);

        var index = await RunWithoutWritesAsync(workspace, "index");

        Assert.Equal(5, index.ExitCode);
        Assert.Equal(string.Empty, index.StandardOutput);
        Assert.Contains("frontmatter", index.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("invalid", index.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("optional metadata", index.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("read denied", index.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, SnapshotState(workspace));
        Assert.Equal(Encoding.UTF8.GetBytes(MalformedNote), File.ReadAllBytes(workspace.Combine(NotePath)));

        var beforeDoctor = SnapshotState(workspace);
        var doctor = await RunWithoutWritesAsync(workspace, "doctor");

        Assert.Equal(2, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        Assert.Contains("error", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("frontmatter", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("read denied", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Framework update did not finish", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeDoctor, SnapshotState(workspace));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(
        DisplayName = "F03 genuine Windows read denial is incomplete and distinct from malformed metadata"),
        Trait("Feature", "plain-note-indexing-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F03")]
    public async Task GenuineWindowsReadDenialRemainsDistinctAndReadOnly()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F03 X05 read-denial evidence requires Windows file-sharing semantics.");
        }

        using var workspace = CreateInstalledWorkspace("e2e-f03-read-denied", NotePath);
        await InstallW1Async(workspace);
        workspace.WriteText(NotePath, PlainNote);
        var before = SnapshotState(workspace);

        using (var locked = new FileStream(
                   workspace.Combine(NotePath),
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.None))
        {
            Assert.Throws<IOException>(() => File.ReadAllBytes(workspace.Combine(NotePath)));
            var index = await workspace.RunAsync("index");

            Assert.Equal(3, index.ExitCode);
            Assert.Equal(string.Empty, index.StandardError);
            Assert.Contains("read", index.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("frontmatter is invalid", index.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("malformed", index.StandardOutput, StringComparison.OrdinalIgnoreCase);

            Assert.Throws<IOException>(() => File.ReadAllBytes(workspace.Combine(NotePath)));
            var doctor = await workspace.RunAsync("doctor");

            Assert.Equal(3, doctor.ExitCode);
            Assert.Equal(string.Empty, doctor.StandardError);
            Assert.Contains("read", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("frontmatter is invalid", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Framework update did not finish", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(before, SnapshotState(workspace));
        Assert.Equal(Encoding.UTF8.GetBytes(PlainNote), File.ReadAllBytes(workspace.Combine(NotePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static PublishedJourneyWorkspace CreateInstalledWorkspace(
        string purpose,
        params string[] reservedFiles)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(reservedFiles);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task InstallW1Async(PublishedJourneyWorkspace workspace)
    {
        var install = await workspace.RunAsync("install", "--automatic");

        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.True(File.Exists(workspace.Combine("AGENTS.md")));
        Assert.True(File.Exists(workspace.Combine(GuidanceEntrypointPath)));
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotState(workspace),
            arguments,
            workspace.ProcessEnvironment);

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in workspace.SnapshotState())
        {
            state[$"workspace/{entry.Key}"] = entry.Value;
        }

        AddDirectorySnapshot(
            state,
            "data-home",
            workspace.LockStore.LocalApplicationDataDirectory);
        AddDirectorySnapshot(
            state,
            "recovery-workspace",
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void AddDirectorySnapshot(
        IDictionary<string, string> state,
        string prefix,
        string path)
    {
        if (!Directory.Exists(path))
        {
            state[prefix] = File.Exists(path)
                ? FileState(path)
                : "absent";
            return;
        }

        foreach (var entry in PublishedWorkspaceTreeSnapshot.Capture(path))
        {
            state[$"{prefix}/{entry.Key}"] = entry.Value;
        }
    }

    private static string FileState(string path)
        => $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";

    private static void AssertWarningWithOptionalMetadata(ProcessRunResult result)
    {
        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Contains("optional", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metadata", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        Assert.Equal(
            (FileAttributes)0,
            attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
    }

    private static void AssertOnlyPathsChanged(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        params string[] allowedPaths)
        => PublishedJourneyAssertions.AssertOnlyFileMutations(before, after, allowedPaths);

    private static void AssertContainsAny(string value, params string[] fragments)
    {
        Assert.True(
            fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase)),
            $"Expected one of [{string.Join(", ", fragments)}] in output:{Environment.NewLine}{value}");
    }
}
