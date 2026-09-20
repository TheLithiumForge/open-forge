using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F10RemoveLinkedSourceJourneyTests
{
    private const string SourceId = "guidance/notes";
    private const string SourcePath = ".agents/guidance/notes.md";
    private const string SourceOverwritePath = ".agents/guidance/notes.overwrite.md";
    private const string IncomingPath = ".agents/patterns/incoming.md";
    private const string UnrelatedBytesPath = ".agents/patterns/unrelated.bin";

    [Fact(DisplayName = "F10 previews removal, preserves authored meaning, diagnoses, and repeats as a no-op"),
        Trait("Feature", "remove-route-preserve-meaning"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F10"),
        Trait("Scenarios", "C17-04,C17-02,C02-01,C17-05")]
    public async Task MainRemoveJourneyCarriesStateThroughDoctorAndRepeat()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateWorkspaceAsync("e2e-f10-main");
        var beforePreview = workspace.SnapshotState();
        var sourceBytes = ReadBytes(workspace, SourcePath);
        var overwriteBytes = ReadBytes(workspace, SourceOverwritePath);
        var incomingBefore = ReadText(workspace, IncomingPath);
        var incomingAfter = incomingBefore.Replace(
            "[Team notes](../guidance/notes.md)",
            "Team notes",
            StringComparison.Ordinal);
        var unrelatedBefore = ReadBytes(workspace, UnrelatedBytesPath);

        var preview = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", SourceId, "--dry-run"]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.NotEmpty(preview.StandardOutput);
        Assert.Contains("Would remove", preview.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No files were changed", preview.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(SourceId, preview.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforePreview, workspace.SnapshotState());
        Assert.Equal(sourceBytes, ReadBytes(workspace, SourcePath));
        Assert.Equal(overwriteBytes, ReadBytes(workspace, SourceOverwritePath));
        Assert.Equal(incomingBefore, ReadText(workspace, IncomingPath));
        Assert.Equal(unrelatedBefore, ReadBytes(workspace, UnrelatedBytesPath));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        var applied = await workspace.RunAsync(
            "route", "remove", SourceId, "--automatic");

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.NotEmpty(applied.StandardOutput);
        Assert.Contains(SourceId, applied.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(workspace.Combine(SourcePath)));
        Assert.False(File.Exists(workspace.Combine(SourceOverwritePath)));
        var expectedAfterApply = new HashSet<string>(beforePreview.Keys, StringComparer.Ordinal);
        Assert.True(expectedAfterApply.Remove(SourcePath));
        Assert.True(expectedAfterApply.Remove(SourceOverwritePath));
        Assert.Equal(
            expectedAfterApply.Order(StringComparer.Ordinal),
            workspace.SnapshotState().Keys.Order(StringComparer.Ordinal));
        Assert.Equal(incomingAfter, ReadText(workspace, IncomingPath));
        Assert.Equal(unrelatedBefore, ReadBytes(workspace, UnrelatedBytesPath));

        var guidanceIndex = ReadText(workspace, ".agents/guidance/_guidance.md");
        var patternsIndex = ReadText(workspace, ".agents/patterns/_patterns.md");
        Assert.DoesNotContain("notes.md", guidanceIndex, StringComparison.Ordinal);
        Assert.Contains("incoming.md", patternsIndex, StringComparison.Ordinal);
        Assert.DoesNotContain("guidance/notes", ReadText(workspace, IncomingPath), StringComparison.Ordinal);

        var beforeDoctor = workspace.SnapshotState();
        var doctor = await RunWithoutWritesAsync(
            target,
            workspace,
            ["doctor"]);

        Assert.Equal(0, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        Assert.NotEmpty(doctor.StandardOutput);
        Assert.Contains("No problems", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("No files were changed", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeDoctor, workspace.SnapshotState());

        var beforeRepeat = workspace.SnapshotState();
        var repeated = await RunWithoutWritesAsync(
            target,
            workspace,
            ["route", "remove", SourceId, "--automatic"]);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        Assert.NotEmpty(repeated.StandardOutput);
        Assert.Contains(SourceId, repeated.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            repeated.StandardOutput.Contains("nothing", StringComparison.OrdinalIgnoreCase)
                || repeated.StandardOutput.Contains("absent", StringComparison.OrdinalIgnoreCase)
                || repeated.StandardOutput.Contains("no longer exists", StringComparison.OrdinalIgnoreCase),
            "The repeated removal did not explain the approved completed no-op.");
        Assert.DoesNotContain("source-not-found", repeated.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeRepeat, workspace.SnapshotState());
        Assert.Equal(incomingAfter, ReadText(workspace, IncomingPath));
        Assert.Equal(unrelatedBefore, ReadBytes(workspace, UnrelatedBytesPath));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F10 blocks removal of the installed Framework-owned route and preserves ownership"),
        Trait("Feature", "remove-route-preserve-meaning"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F10"),
        Trait("Scenarios", "C17-06")]
    public async Task ManagedRouteCannotBeRemovedByAutomaticConfirmation()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await CreateManagedWorkspaceAsync("e2e-f10-managed");
        var before = workspace.SnapshotState();
        const string ManagedRoutePath = ".agents/guidance/_guidance.md";
        var managedSourceBefore = ReadBytes(workspace, ManagedRoutePath);
        var ownershipBefore = ReadBytes(workspace, ".agents/open-forge.lock.json");

        var result = await workspace.RunAsync(
            "route", "remove", ManagedRoutePath, "--automatic");

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains(ManagedRoutePath, result.StandardError, StringComparison.Ordinal);
        Assert.Contains("the Framework", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("open-forge update", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(managedSourceBefore, ReadBytes(workspace, ManagedRoutePath));
        Assert.Equal(ownershipBefore, ReadBytes(workspace, ".agents/open-forge.lock.json"));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static async Task<PublishedJourneyWorkspace> CreateWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                SourcePath,
                SourceOverwritePath,
                IncomingPath,
                UnrelatedBytesPath);

            await RunSetupAsync(workspace, "install", "--automatic");
            await RunSetupAsync(
                workspace,
                "route", "create", SourceId, "--description", "Team notes", "--tag=Guidance");
            await RunSetupAsync(
                workspace,
                "route", "create", "patterns/incoming", "--description", "Incoming links", "--tag=Pattern");

            workspace.WriteText(
                SourcePath,
                OpenForgeDocumentSeed.Metadata(
                    "Team notes",
                    ["Guidance"],
                    "\n# Team notes\n\n## Section\n\nAuthored notes body.\n"));
            workspace.WriteText(SourceOverwritePath, "Override notes bytes.\n");
            workspace.WriteText(
                IncomingPath,
                OpenForgeDocumentSeed.Metadata(
                    "Incoming links",
                    ["Pattern"],
                    "\n# Incoming\n\nBefore [Team notes](../guidance/notes.md) after.\n"));
            workspace.WriteBytes(UnrelatedBytesPath, [0x00, 0x22, 0xa5, 0xfe, 0xff]);
            await RunSetupAsync(workspace, "index");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task<PublishedJourneyWorkspace> CreateManagedWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            await RunSetupAsync(workspace, "install", "--automatic");
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
        Assert.Equal(0, result.ExitCode);
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

    private static string ReadText(PublishedJourneyWorkspace workspace, string relativePath)
        => File.ReadAllText(workspace.Combine(relativePath), Encoding.UTF8);

    private static byte[] ReadBytes(PublishedJourneyWorkspace workspace, string relativePath)
        => File.ReadAllBytes(workspace.Combine(relativePath));
}
