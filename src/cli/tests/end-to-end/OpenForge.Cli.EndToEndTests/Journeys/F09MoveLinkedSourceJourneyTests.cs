using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F09MoveLinkedSourceJourneyTests
{
    private const string SourceId = "guidance/notes";
    private const string SourcePath = ".agents/guidance/notes.md";
    private const string SourceOverwritePath = ".agents/guidance/notes.overwrite.md";
    private const string DestinationId = "guidance/team/moved-note";
    private const string DestinationPath = ".agents/guidance/team/moved-note.md";
    private const string DestinationOverwritePath = ".agents/guidance/team/moved-note.overwrite.md";
    private const string CategoryPrefix = ".agents/guidance/team";
    private const string CategoryDestinationPrefix = ".agents/guidance/archive/team";
    private const string IncomingPath = ".agents/patterns/incoming.md";
    private const string OtherPath = ".agents/guidance/other.md";
    private const string UnrelatedBytesPath = ".agents/guidance/unrelated.bin";

    [Fact(DisplayName = "F09 moves a linked leaf through dry-run, apply, references, and context"),
        Trait("Feature", "move-route-preserve-links"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F09"),
        Trait("Scenarios", "C16-04,C16-02,C10-01,C08-02")]
    public async Task MainMoveJourneyCarriesStateThroughReadback()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateLeafWorkspaceAsync("e2e-f09-main");
        string[] arguments = ["route", "move", SourceId, DestinationId];

        var beforePreview = workspace.SnapshotState();
        var preview = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "move", SourceId, DestinationId, "--dry-run"]);

        AssertCompleted(preview, SourceId, DestinationId);
        Assert.Contains("Would move", preview.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforePreview, workspace.SnapshotState());
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        Assert.False(File.Exists(workspace.Combine(DestinationPath)));

        var beforeApply = workspace.SnapshotState();
        var overwriteBytes = ReadBytes(workspace, SourceOverwritePath);
        var incomingBefore = ReadText(workspace, IncomingPath);
        var otherBefore = ReadText(workspace, OtherPath);
        var notesBefore = ReadText(workspace, SourcePath);
        var unrelatedBefore = ReadBytes(workspace, UnrelatedBytesPath);

        var applied = await workspace.RunAsync(arguments);

        AssertCompleted(applied, SourceId, DestinationId);
        Assert.False(File.Exists(workspace.Combine(SourcePath)));
        Assert.False(File.Exists(workspace.Combine(SourceOverwritePath)));
        Assert.Equal(overwriteBytes, ReadBytes(workspace, DestinationOverwritePath));
        Assert.Equal(
            notesBefore.Replace("team/_team.md", "_team.md", StringComparison.Ordinal),
            Encoding.UTF8.GetString(ReadBytes(workspace, DestinationPath)));
        Assert.Equal(
            incomingBefore.Replace(
                "../guidance/notes.md#section",
                "../guidance/team/moved-note.md#section",
                StringComparison.Ordinal),
            ReadText(workspace, IncomingPath));
        Assert.Equal(
            otherBefore.Replace(
                "notes.md#section",
                "team/moved-note.md#section",
                StringComparison.Ordinal),
            ReadText(workspace, OtherPath));
        Assert.Equal(unrelatedBefore, ReadBytes(workspace, UnrelatedBytesPath));
        Assert.Contains("Team notes", ReadText(workspace, IncomingPath), StringComparison.Ordinal);
        Assert.Contains("#section", ReadText(workspace, IncomingPath), StringComparison.Ordinal);
        Assert.DoesNotContain("guidance/notes", ReadText(workspace, IncomingPath), StringComparison.Ordinal);
        Assert.DoesNotContain("notes.md", ReadText(workspace, OtherPath), StringComparison.Ordinal);

        AssertFullLeafPathSetMoved(beforeApply, workspace.SnapshotState());
        var guidanceIndex = ReadText(workspace, ".agents/guidance/_guidance.md");
        var teamIndex = ReadText(workspace, ".agents/guidance/team/_team.md");
        Assert.Contains("team", guidanceIndex, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("notes.md", guidanceIndex, StringComparison.Ordinal);
        Assert.Contains("moved-note.md", teamIndex, StringComparison.Ordinal);
        Assert.DoesNotContain("notes.md", teamIndex, StringComparison.Ordinal);

        var references = await workspace.RunAsync("references", DestinationId);
        AssertCompleted(references, DestinationId);
        Assert.Contains(IncomingPath, references.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(OtherPath, references.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(SourcePath, references.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("_guidance.md", references.StandardOutput, StringComparison.Ordinal);

        var context = await workspace.RunAsync("context", DestinationId);
        AssertCompleted(context, DestinationId);
        Assert.Contains("Team notes", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Section", context.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(SourceId, context.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(UnrelatedBytesPath, context.StandardOutput, StringComparison.OrdinalIgnoreCase);

        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F09 blocks an occupied destination without changing links or bytes"),
        Trait("Feature", "move-route-preserve-links"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F09"),
        Trait("Scenarios", "C16-05")]
    public async Task OccupiedDestinationIsBlockedWithoutWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateLeafWorkspaceAsync("e2e-f09-occupied");
        const string occupied = "occupied destination bytes\n";
        workspace.WriteText(DestinationPath, occupied);
        var before = workspace.SnapshotState();
        var sourceBytes = ReadBytes(workspace, SourcePath);
        var sourceOverwriteBytes = ReadBytes(workspace, SourceOverwritePath);
        var incomingBytes = ReadBytes(workspace, IncomingPath);
        var otherBytes = ReadBytes(workspace, OtherPath);

        var result = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "move", SourceId, DestinationId]);

        Assert.Equal(5, result.ExitCode);
        Assert.NotEmpty(result.StandardError);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains(DestinationId, result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exists", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(sourceBytes, ReadBytes(workspace, SourcePath));
        Assert.Equal(sourceOverwriteBytes, ReadBytes(workspace, SourceOverwritePath));
        Assert.Equal(incomingBytes, ReadBytes(workspace, IncomingPath));
        Assert.Equal(otherBytes, ReadBytes(workspace, OtherPath));
        Assert.Equal(occupied, ReadText(workspace, DestinationPath));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F09 moves a complete category and maintains outside references"),
        Trait("Feature", "move-route-preserve-links"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F09"),
        Trait("Scenarios", "C16-03")]
    public async Task CategoryMoveCarriesEveryDescendantAndReference()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateCategoryWorkspaceAsync("e2e-f09-category");
        var before = workspace.SnapshotState();
        var childBefore = ReadText(workspace, ".agents/guidance/team/child.md");
        var childOverwriteBefore = ReadBytes(workspace, ".agents/guidance/team/child.overwrite.md");
        var nestedCategoryBefore = ReadBytes(workspace, ".agents/guidance/team/sub/_sub.md");
        var supportBefore = ReadBytes(workspace, ".agents/guidance/team/support.bin");
        var notesBefore = ReadText(workspace, SourcePath);
        var otherBefore = ReadText(workspace, OtherPath);

        var result = await workspace.RunAsync(
            "route", "move", "guidance/team", "guidance/archive/team");

        AssertCompleted(result, "guidance/team", "guidance/archive/team");
        var after = workspace.SnapshotState();
        AssertCategoryPathSetMoved(before, after);
        Assert.False(Directory.Exists(workspace.Combine(CategoryPrefix)));
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/archive/team/_team.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/archive/team/sub/_sub.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/archive/team/child.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/archive/team/child.overwrite.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/guidance/archive/team/support.bin")));
        Assert.Equal(nestedCategoryBefore, ReadBytes(workspace, ".agents/guidance/archive/team/sub/_sub.md"));
        Assert.Equal(childOverwriteBefore, ReadBytes(workspace, ".agents/guidance/archive/team/child.overwrite.md"));
        Assert.Equal(supportBefore, ReadBytes(workspace, ".agents/guidance/archive/team/support.bin"));
        Assert.Equal(
            childBefore.Replace("../notes.md#section", "../../notes.md#section", StringComparison.Ordinal),
            ReadText(workspace, ".agents/guidance/archive/team/child.md"));
        Assert.Equal(
            notesBefore.Replace(
                "team/_team.md",
                "archive/team/_team.md",
                StringComparison.Ordinal),
            ReadText(workspace, SourcePath));
        Assert.Equal(
            otherBefore.Replace(
                "team/child.md#detail",
                "archive/team/child.md#detail",
                StringComparison.Ordinal),
            ReadText(workspace, OtherPath));

        var guidanceIndex = ReadText(workspace, ".agents/guidance/_guidance.md");
        var archiveIndex = ReadText(workspace, ".agents/guidance/archive/_archive.md");
        var movedCategoryIndex = ReadText(workspace, ".agents/guidance/archive/team/_team.md");
        Assert.Contains("archive", guidanceIndex, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("guidance/team/_team.md", guidanceIndex, StringComparison.Ordinal);
        Assert.Contains("team", archiveIndex, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("child.md", movedCategoryIndex, StringComparison.Ordinal);
        Assert.Contains("sub", movedCategoryIndex, StringComparison.OrdinalIgnoreCase);

        var references = await workspace.RunAsync(
            "references", "guidance/archive/team/child");
        AssertCompleted(references, "guidance/archive/team/child");
        Assert.Contains(OtherPath, references.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("guidance/team/child.md", references.StandardOutput, StringComparison.Ordinal);

        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F09 reports an incomplete reference scan without moving anything"),
        Trait("Feature", "move-route-preserve-links"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F09"),
        Trait("Scenarios", "C16-11")]
    public async Task DeniedIncomingScanLeavesTheMoveUnapplied()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("C16-11 requires Windows FileShare.None read denial.");
        }

        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateLeafWorkspaceAsync("e2e-f09-denied-incoming");
        var incomingPath = workspace.Combine(IncomingPath);
        var before = workspace.SnapshotState();
        var incomingBefore = ReadBytes(workspace, IncomingPath);
        using (var held = new FileStream(
                   incomingPath,
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.None))
        {
            var readFailure = Assert.ThrowsAny<Exception>(() =>
            {
                using var readAttempt = new FileStream(
                    incomingPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
            });
            Assert.True(
                readFailure is IOException or UnauthorizedAccessException,
                $"The denied-read proof returned an unexpected exception: {readFailure.GetType().FullName}.");

            var result = await workspace.RunAsync(
                "route", "move", SourceId, "guidance/team/moved-note");

            Assert.Equal(3, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
            Assert.NotEmpty(result.StandardOutput);
            Assert.Contains("Nothing was changed", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("scan", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(workspace.Combine(SourcePath)));
            Assert.False(File.Exists(workspace.Combine(DestinationPath)));
        }

        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(incomingBefore, ReadBytes(workspace, IncomingPath));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static async Task<PublishedJourneyWorkspace> CreateLeafWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                SourcePath,
                SourceOverwritePath,
                DestinationPath,
                DestinationOverwritePath,
                ".agents/guidance/team/_team.md",
                ".agents/guidance/other.md",
                IncomingPath,
                UnrelatedBytesPath);

            await RunSetupAsync(workspace, "install", "--automatic");
            await RunSetupAsync(
                workspace,
                "route", "init", "guidance/team", "--description", "Team", "--tag=Team");
            await RunSetupAsync(
                workspace,
                "route", "create", SourceId, "--description", "Team notes", "--tag=Guidance");
            await RunSetupAsync(
                workspace,
                "route", "create", "guidance/other", "--description", "Other guidance", "--tag=Guidance");
            await RunSetupAsync(
                workspace,
                "route", "create", "patterns/incoming", "--description", "Incoming links", "--tag=Pattern");

            workspace.WriteText(
                SourcePath,
                OpenForgeDocumentSeed.Metadata(
                    "Team notes",
                    ["Guidance"],
                    "\n# Team notes\n\n## Section\n\n[Team home](team/_team.md)\n"));
            workspace.WriteText(SourceOverwritePath, "Override notes bytes.\n");
            workspace.WriteText(
                OtherPath,
                OpenForgeDocumentSeed.Metadata(
                    "Other guidance",
                    ["Guidance"],
                    "\n# Other\n\n[Team notes from guidance](notes.md#section)\n"));
            workspace.WriteText(
                IncomingPath,
                OpenForgeDocumentSeed.Metadata(
                    "Incoming links",
                    ["Pattern"],
                    "\n# Incoming\n\n[Team notes](../guidance/notes.md#section)\n"));
            workspace.WriteBytes(UnrelatedBytesPath, [0x00, 0x11, 0xa5, 0xfe, 0xff]);
            await RunSetupAsync(workspace, "index");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task<PublishedJourneyWorkspace> CreateCategoryWorkspaceAsync(string purpose)
    {
        var workspace = await CreateLeafWorkspaceAsync(purpose);
        try
        {
            workspace.ExpectFiles(
                ".agents/guidance/archive/_archive.md",
                ".agents/guidance/team/sub/_sub.md",
                ".agents/guidance/team/child.md",
                ".agents/guidance/team/child.overwrite.md",
                ".agents/guidance/team/support.bin",
                ".agents/guidance/archive/team/_team.md",
                ".agents/guidance/archive/team/sub/_sub.md",
                ".agents/guidance/archive/team/child.md",
                ".agents/guidance/archive/team/child.overwrite.md",
                ".agents/guidance/archive/team/support.bin");

            await RunSetupAsync(
                workspace,
                "route", "init", "guidance/archive", "--description", "Archive", "--tag=Archive");
            await RunSetupAsync(
                workspace,
                "route", "init", "guidance/team/sub", "--description", "Nested team", "--tag=Team");
            await RunSetupAsync(
                workspace,
                "route", "create", "guidance/team/child", "--description", "Nested child", "--tag=Guidance");

            workspace.WriteText(
                ".agents/guidance/team/child.md",
                OpenForgeDocumentSeed.Metadata(
                    "Nested child",
                    ["Guidance"],
                    "\n# Nested child\n\n## Detail\n\n[Team notes](../notes.md#section)\n"));
            workspace.WriteText(
                ".agents/guidance/team/child.overwrite.md",
                "Child override authored bytes.\n");
            workspace.WriteBytes(
                ".agents/guidance/team/support.bin",
                [0x00, 0x10, 0xfe, 0xff]);
            workspace.WriteText(
                OtherPath,
                OpenForgeDocumentSeed.Metadata(
                    "Other guidance",
                    ["Guidance"],
                    "\n# Other\n\n[Team notes from guidance](notes.md#section)\n\n[Team child](team/child.md#detail)\n"));
            await RunSetupAsync(workspace, "index");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task RunSetupAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
    {
        var result = await workspace.RunAsync(arguments);
        Assert.True(result.ExitCode == 0, $"Expected exit 0, got {result.ExitCode}. Stdout: {result.StandardOutput} Stderr: {result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedJourneyWorkspace workspace,
        IReadOnlyList<string> arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertCompleted(ProcessRunResult result, params string[] expectedText)
    {
        Assert.True(result.ExitCode == 0, $"Expected exit 0, got {result.ExitCode}. Stdout: {result.StandardOutput} Stderr: {result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
        foreach (var text in expectedText)
        {
            Assert.Contains(text, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertFullLeafPathSetMoved(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
    {
        var expected = new HashSet<string>(before.Keys, StringComparer.Ordinal);
        Assert.True(expected.Remove(SourcePath));
        Assert.True(expected.Remove(SourceOverwritePath));
        expected.Add(DestinationPath);
        expected.Add(DestinationOverwritePath);
        Assert.Equal(
            expected.Order(StringComparer.Ordinal),
            after.Keys.Order(StringComparer.Ordinal));
    }

    private static void AssertCategoryPathSetMoved(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
    {
        var sourceEntries = before.Keys
            .Where(path => path.Equals(CategoryPrefix, StringComparison.Ordinal)
                || path.StartsWith(CategoryPrefix + "/", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(sourceEntries);

        var expected = new HashSet<string>(before.Keys, StringComparer.Ordinal);
        foreach (var sourceEntry in sourceEntries)
        {
            Assert.True(expected.Remove(sourceEntry));
            expected.Add(CategoryDestinationPrefix + sourceEntry[CategoryPrefix.Length..]);
        }

        Assert.Equal(
            expected.Order(StringComparer.Ordinal),
            after.Keys.Order(StringComparer.Ordinal));
    }

    private static string ReadText(PublishedJourneyWorkspace workspace, string relativePath)
        => File.ReadAllText(workspace.Combine(relativePath), Encoding.UTF8);

    private static byte[] ReadBytes(PublishedJourneyWorkspace workspace, string relativePath)
        => File.ReadAllBytes(workspace.Combine(relativePath));
}
