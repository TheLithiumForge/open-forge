using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F08RepairRenamedReferenceJourneyTests
{
    private const string NotesPath = ".agents/guidance/notes.md";
    private const string TargetPath = ".agents/guidance/target.md";
    private const string RenamedTargetPath = ".agents/guidance/renamed-target.md";
    private const string GuidanceEntrypointPath = ".agents/guidance/_guidance.md";
    private const string LibraryId = "f08-library";
    private const string LibrarySourceRoot = "f08-library-source";
    private const string LibrarySourcePath = "f08-library-source/library-note.md";
    private const string LibraryDestinationRoot = ".agents/library-drift";
    private const string LibraryDestinationPath = ".agents/library-drift/library-note.md";
    private const string CandidateAPath = ".agents/guidance/renamed-target-a.md";
    private const string CandidateBPath = ".agents/guidance/renamed-target-b.md";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    [Fact(
        DisplayName = "F08 carries a real rename from references through diagnosis, exact relink, and no-op repair"),
        Trait("Feature", "repair-renamed-reference-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F08"),
        Trait("Scenarios", "C10-01,C10-05,C02-03,C06-05,C10-03,C06-01")]
    public async Task ManualRenameIsDiagnosedRepairedAndConverges()
    {
        using var workspace = await CreateWorkspaceAsync("f08-main");
        var targetBytes = File.ReadAllBytes(workspace.Combine(TargetPath));
        var navigationBytes = File.ReadAllBytes(workspace.Combine(GuidanceEntrypointPath));

        var initial = await RunReadOnlyAsync(workspace, "references", "guidance/target");
        Assert.Equal(0, initial.ExitCode);
        Assert.Equal(string.Empty, initial.StandardError);
        Assert.Contains("notes.md", initial.StandardOutput, StringComparison.Ordinal);

        using (var initialJson = await RunJsonAsync(workspace, 0, "references", "guidance/target"))
        {
            Assert.Equal("completed", initialJson.RootElement.GetProperty("status").GetString());
            var incoming = Assert.Single(
                initialJson.RootElement.GetProperty("data").GetProperty("incoming").EnumerateArray());
            Assert.Equal(NotesPath, incoming.GetProperty("path").GetString());
        }

        File.Move(workspace.Combine(TargetPath), workspace.Combine(RenamedTargetPath));
        Assert.False(File.Exists(workspace.Combine(TargetPath)));
        Assert.True(File.Exists(workspace.Combine(RenamedTargetPath)));
        Assert.Equal(targetBytes, File.ReadAllBytes(workspace.Combine(RenamedTargetPath)));

        var notesPath = workspace.Combine(NotesPath);
        var notesBefore = File.ReadAllBytes(notesPath);
        var notesTextBefore = StrictUtf8NoBom.GetString(notesBefore);
        var measured = MeasureLink(notesBefore, "target.md");
        var expectedNotes = RewriteDestination(notesTextBefore, measured.ExpectedDestination, "renamed-target.md");
        var expectedNotesBytes = StrictUtf8NoBom.GetBytes(expectedNotes);

        var broken = await RunReadOnlyAsync(
            workspace,
            "references",
            "guidance/notes",
            "--direction=out");
        Assert.Equal(2, broken.ExitCode);
        Assert.Equal(string.Empty, broken.StandardError);
        Assert.Contains("target.md", broken.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(measured.ReferenceLocation, broken.StandardOutput, StringComparison.Ordinal);

        using (var brokenJson = await RunJsonAsync(
                   workspace,
                   2,
                   "references",
                   "guidance/notes",
                   "--direction=out"))
        {
            Assert.Equal("completed-with-warnings", brokenJson.RootElement.GetProperty("status").GetString());
            var occurrence = Assert.Single(
                brokenJson.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray());
            Assert.Equal(measured.ReferenceLocation, occurrence.GetProperty("location").GetString());
            Assert.Equal("target.md", occurrence.GetProperty("destination").GetString());
            Assert.Equal("missing", occurrence.GetProperty("state").GetString());
            Assert.Equal($"{TargetPath}", occurrence.GetProperty("resolvedPath").GetString());
        }

        var doctor = await RunReadOnlyAsync(workspace, "doctor");
        Assert.Equal(2, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        Assert.Contains("notes.md", doctor.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("target.md", doctor.StandardOutput, StringComparison.Ordinal);

        var beforePreview = workspace.SnapshotState();
        var preview = await RunReadOnlyAsync(
            workspace,
            "repair",
            "--relink",
            measured.RepairLocation,
            measured.ExpectedDestination,
            RenamedTargetPath,
            "--dry-run");
        Assert.True(preview.ExitCode == 0, $"Expected exit 0, got {preview.ExitCode}. Stdout: {preview.StandardOutput} Stderr: {preview.StandardError}");
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Contains("target.md", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforePreview, workspace.SnapshotState());
        Assert.Equal(notesBefore, File.ReadAllBytes(notesPath));

        var beforeApply = workspace.SnapshotState();
        var applied = await workspace.RunAsync(
            "repair",
            "--automatic",
            "--relink",
            measured.RepairLocation,
            measured.ExpectedDestination,
            RenamedTargetPath);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.NotEmpty(applied.StandardOutput);
        AssertOnlyWorkspacePathChanged(beforeApply, workspace.SnapshotState(), NotesPath);
        Assert.Equal(expectedNotesBytes, File.ReadAllBytes(notesPath));
        Assert.Equal(targetBytes, File.ReadAllBytes(workspace.Combine(RenamedTargetPath)));
        Assert.Equal(navigationBytes, File.ReadAllBytes(workspace.Combine(GuidanceEntrypointPath)));
        Assert.Contains("[Team target](renamed-target.md)", expectedNotes, StringComparison.Ordinal);
        Assert.DoesNotContain("[Team target](target.md)", expectedNotes, StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        var resolved = await RunReadOnlyAsync(
            workspace,
            "references",
            "guidance/notes",
            "--direction=out");
        Assert.Equal(0, resolved.ExitCode);
        Assert.Equal(string.Empty, resolved.StandardError);
        Assert.Contains("renamed-target.md", resolved.StandardOutput, StringComparison.Ordinal);

        using (var resolvedJson = await RunJsonAsync(
                   workspace,
                   0,
                   "references",
                   "guidance/notes",
                   "--direction=out"))
        {
            Assert.Equal("completed", resolvedJson.RootElement.GetProperty("status").GetString());
            var occurrence = Assert.Single(
                resolvedJson.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray());
            Assert.Equal(measured.ReferenceLocation, occurrence.GetProperty("location").GetString());
            Assert.Equal("renamed-target.md", occurrence.GetProperty("destination").GetString());
            Assert.Equal($"{RenamedTargetPath}", occurrence.GetProperty("resolvedPath").GetString());
            Assert.Equal("complete", occurrence.GetProperty("state").GetString());
        }

        var beforeNoOp = workspace.SnapshotState();
        var noOp = await RunReadOnlyAsync(workspace, "repair", "--automatic");
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("Nothing to repair", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforeNoOp, workspace.SnapshotState());
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(
        DisplayName = "F08 preserves an ambiguous broken occurrence until an explicit candidate is chosen"),
        Trait("Feature", "repair-renamed-reference-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F08"),
        Trait("Scenarios", "C06-03")]
    public async Task AutomaticRepairLeavesTwoEquallyValidCandidatesUnchanged()
    {
        using var workspace = await CreateWorkspaceAsync(
            "f08-ambiguous",
            includeTarget: false,
            includeAmbiguousCandidates: true);
        var notesBefore = File.ReadAllBytes(workspace.Combine(NotesPath));
        var before = workspace.SnapshotState();

        var text = await RunReadOnlyAsync(workspace, "repair", "--automatic");
        Assert.Equal(2, text.ExitCode);
        Assert.Equal(string.Empty, text.StandardError);
        Assert.Contains("target.md", text.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(notesBefore, File.ReadAllBytes(workspace.Combine(NotesPath)));

        using var json = await RunJsonAsync(workspace, 2, "repair", "--automatic");
        Assert.Equal("completed-with-warnings", json.RootElement.GetProperty("status").GetString());
        var remaining = Assert.Single(
            json.RootElement.GetProperty("data").GetProperty("remaining").EnumerateArray());
        Assert.Equal("guided", remaining.GetProperty("kind").GetString());
        var candidates = remaining.GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(4, candidates.Length);
        Assert.Equal(
            [GuidanceEntrypointPath, NotesPath, CandidateAPath, CandidateBPath],
            candidates.Select(candidate => candidate.GetProperty("path").GetString()).Order(StringComparer.Ordinal));
    }

    [Fact(
        DisplayName = "F08 applies the chosen relink despite unrelated Library drift and retains that diagnosis"),
        Trait("Feature", "repair-renamed-reference-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F08"),
        Trait("Scenarios", "X16")]
    public async Task ExactRelinkCompletesWithinScopeWhileLibraryDriftRemains()
    {
        using var workspace = await CreateWorkspaceAsync("f08-x16-library", includeLibrary: true);
        var targetBytes = File.ReadAllBytes(workspace.Combine(TargetPath));
        File.Move(workspace.Combine(TargetPath), workspace.Combine(RenamedTargetPath));
        var notesBytes = File.ReadAllBytes(workspace.Combine(NotesPath));
        var notesText = StrictUtf8NoBom.GetString(notesBytes);
        var measured = MeasureLink(notesBytes, "target.md");
        var expectedNotes = RewriteDestination(notesText, measured.ExpectedDestination, "renamed-target.md");

        var attached = await workspace.RunAsync(
            "library",
            "attach",
            LibraryId,
            LibrarySourceRoot,
            "--to",
            LibraryDestinationRoot,
            "--automatic");
        AssertSetupSuccess(attached, "library attach");
        var driftPath = workspace.Combine(LibraryDestinationPath);
        Assert.NotNull(new FileInfo(driftPath).LinkTarget);
        var librarySourceBytes = File.ReadAllBytes(workspace.Combine(LibrarySourcePath));
        File.Delete(driftPath);
        var driftBytes = Encoding.UTF8.GetBytes("Unrelated user-owned Library drift.\n");
        workspace.WriteBytes(LibraryDestinationPath, driftBytes);
        AssertOrdinaryFile(driftPath);
        Assert.Equal(librarySourceBytes, File.ReadAllBytes(workspace.Combine(LibrarySourcePath)));

        var doctorBefore = await RunReadOnlyAsync(workspace, "doctor");
        Assert.Equal(2, doctorBefore.ExitCode);
        Assert.Equal(string.Empty, doctorBefore.StandardError);
        Assert.Contains("library-note.md", doctorBefore.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("target.md", doctorBefore.StandardOutput, StringComparison.Ordinal);

        var beforePreview = workspace.SnapshotState();
        var preview = await RunReadOnlyAsync(
            workspace,
            "repair",
            "--automatic",
            "--relink",
            measured.RepairLocation,
            measured.ExpectedDestination,
            RenamedTargetPath,
            "--dry-run");
        Assert.Equal(2, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Contains("notes.md", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("library-note.md", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforePreview, workspace.SnapshotState());

        var applied = await workspace.RunAsync(
            "repair",
            "--automatic",
            "--relink",
            measured.RepairLocation,
            measured.ExpectedDestination,
            RenamedTargetPath);
        Assert.Equal(2, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("notes.md", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("library-note.md", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(StrictUtf8NoBom.GetBytes(expectedNotes), File.ReadAllBytes(workspace.Combine(NotesPath)));
        Assert.Equal(targetBytes, File.ReadAllBytes(workspace.Combine(RenamedTargetPath)));
        Assert.Equal(driftBytes, File.ReadAllBytes(driftPath));
        Assert.Equal(librarySourceBytes, File.ReadAllBytes(workspace.Combine(LibrarySourcePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        var resolved = await RunReadOnlyAsync(
            workspace,
            "references",
            "guidance/notes",
            "--direction=out");
        Assert.Equal(0, resolved.ExitCode);
        Assert.Contains("renamed-target.md", resolved.StandardOutput, StringComparison.Ordinal);

        var doctorAfter = await RunReadOnlyAsync(workspace, "doctor");
        Assert.Equal(2, doctorAfter.ExitCode);
        Assert.Equal(string.Empty, doctorAfter.StandardError);
        Assert.Contains("library-note.md", doctorAfter.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(
        DisplayName = "F08 blocks an exact relink when its required guidance navigation source is unsafe"),
        Trait("Feature", "repair-renamed-reference-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F08"),
        Trait("Scenarios", "X16")]
    public async Task UnsafeGuidanceNavigationBlocksExactRelinkWithoutWrites()
    {
        using var workspace = await CreateWorkspaceAsync("f08-x16-unsafe");
        File.Move(workspace.Combine(TargetPath), workspace.Combine(RenamedTargetPath));
        var notesBytes = File.ReadAllBytes(workspace.Combine(NotesPath));
        var notesText = StrictUtf8NoBom.GetString(notesBytes);
        var measured = MeasureLink(notesBytes, "target.md");
        var navigationPath = workspace.Combine(GuidanceEntrypointPath);
        var navigationBytes = File.ReadAllBytes(navigationPath);

        File.Delete(navigationPath);
        Directory.CreateDirectory(navigationPath);
        Assert.True((File.GetAttributes(navigationPath) & FileAttributes.Directory) != 0);
        try
        {
            var before = workspace.SnapshotState();
            var blocked = await RunReadOnlyAsync(
                workspace,
                "repair",
                "--automatic",
                "--relink",
                measured.RepairLocation,
                measured.ExpectedDestination,
                RenamedTargetPath);
            Assert.Equal(5, blocked.ExitCode);
            Assert.Equal(string.Empty, blocked.StandardOutput);
            Assert.NotEmpty(blocked.StandardError);
            Assert.Contains("guidance", blocked.StandardError, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, workspace.SnapshotState());
            Assert.Equal(
                notesText,
                StrictUtf8NoBom.GetString(File.ReadAllBytes(workspace.Combine(NotesPath))));
        }
        finally
        {
            Directory.Delete(navigationPath, recursive: false);
            workspace.WriteBytes(GuidanceEntrypointPath, navigationBytes);
        }
    }

    private static async Task<PublishedJourneyWorkspace> CreateWorkspaceAsync(
        string purpose,
        bool includeTarget = true,
        bool includeAmbiguousCandidates = false,
        bool includeLibrary = false)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            var reserved = new List<string>
            {
                NotesPath,
                TargetPath,
                RenamedTargetPath,
            };
            if (includeAmbiguousCandidates)
            {
                reserved.Add(CandidateAPath);
                reserved.Add(CandidateBPath);
            }

            if (includeLibrary)
            {
                reserved.Add(LibrarySourcePath);
                reserved.Add(LibraryDestinationPath);
            }

            workspace.ExpectFiles([.. reserved]);
            var install = await workspace.RunAsync("install", "--automatic");
            AssertSetupSuccess(install, "install");
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));

            workspace.WriteText(
                NotesPath,
                OpenForgeDocumentSeed.Metadata(
                    "Notes",
                    ["Reference"],
                    "\n# Notes\n\nThe maintainer 👩‍💻 keeps one exact link: [Team target](target.md)\n"));
            if (includeTarget)
            {
                workspace.WriteText(
                    TargetPath,
                    OpenForgeDocumentSeed.Metadata(
                        "Target",
                        ["Reference"],
                        "\n# Target\n\nDistinctive target body for the rename journey.\n"));
            }

            if (includeAmbiguousCandidates)
            {
                var candidateBody = "\n# Target\n\nEqually valid replacement body.\n";
                workspace.WriteText(
                    CandidateAPath,
                    OpenForgeDocumentSeed.Metadata("Target", ["Reference"], candidateBody));
                workspace.WriteText(
                    CandidateBPath,
                    OpenForgeDocumentSeed.Metadata("Target", ["Reference"], candidateBody));
            }

            if (includeLibrary)
            {
                workspace.WriteText(
                    LibrarySourcePath,
                    OpenForgeDocumentSeed.Metadata(
                        "Library note",
                        ["Reference"],
                        "\n# Library note\n\nUnrelated Library source bytes.\n"));
            }

            var indexed = await workspace.RunAsync("index");
            AssertSetupSuccess(indexed, "index");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static async Task<JsonDocument> RunJsonAsync(
        PublishedJourneyWorkspace workspace,
        int expectedExitCode,
        params string[] arguments)
    {
        var result = await RunReadOnlyAsync(workspace, [.. arguments, "--format=json", "--detail=full"]);
        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        return JsonDocument.Parse(result.StandardOutput);
    }

    private static LinkMeasurement MeasureLink(byte[] sourceBytes, string expectedDestination)
    {
        var source = StrictUtf8NoBom.GetString(sourceBytes);
        var linkUse = MeasureLocation(source, "[Team target]", "authored link-use");
        var destination = MeasureLocation(source, expectedDestination, "authored expected destination");
        return new LinkMeasurement(
            $"{NotesPath}@{destination.Line}:{destination.Column}",
            $":{linkUse.Line}:{linkUse.Column}",
            expectedDestination);
    }

    private static SourceLocation MeasureLocation(string source, string literal, string description)
    {
        var start = source.IndexOf(literal, StringComparison.Ordinal);
        Assert.True(start >= 0, $"The {description} literal was not found.");
        var line = 1 + source[..start].Count(character => character == '\n');
        var lineStart = source.LastIndexOf('\n', start == 0 ? 0 : start - 1);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;
        var column = source[lineStart..start].EnumerateRunes().Count() + 1;
        return new SourceLocation(line, column);
    }

    private static string RewriteDestination(
        string source,
        string expectedDestination,
        string selectedDestination)
    {
        var destination = source.IndexOf(expectedDestination, StringComparison.Ordinal);
        Assert.True(destination >= 0, "The authored expected destination was not found for rewriting.");
        return source[..destination]
            + selectedDestination
            + source[(destination + expectedDestination.Length)..];
    }

    private static void AssertOnlyWorkspacePathChanged(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        string expectedPath)
    {
        PublishedJourneyAssertions.AssertOnlyFileMutations(before, after, expectedPath);
        Assert.NotEqual(before[expectedPath], after[expectedPath]);
    }

    private static void AssertSetupSuccess(ProcessRunResult result, string operation)
    {
        Assert.True(
            result.ExitCode == 0,
            $"F08 {operation} setup failed with exit {result.ExitCode}.\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        Assert.Equal(
            0,
            (int)(attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)));
    }

    private sealed record LinkMeasurement(
        string RepairLocation,
        string ReferenceLocation,
        string ExpectedDestination);

    private sealed record SourceLocation(int Line, int Column);
}
