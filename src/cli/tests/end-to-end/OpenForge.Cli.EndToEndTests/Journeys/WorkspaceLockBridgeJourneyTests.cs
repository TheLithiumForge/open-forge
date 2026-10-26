using System.Security.AccessControl;
using System.Security.Principal;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class WorkspaceLockBridgeJourneyTests
{
    private const string Feature = "workspace-lock-bridge";
    private const string NoteName = "bridge-note.md";
    private const string Note = Tokens.Sources.AgentsRoot + "/guidance/" + NoteName;
    private const string GuidanceEntrypoint = Tokens.Sources.AgentsRoot + "/guidance/_guidance.md";
    private const string HolderFile = "holder.txt";
    private const string RelativeHome = "relative-home";

    [Fact(DisplayName = "Local contention crosses legacy data homes and preserves a nonempty sentinel")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task LocalContentionCrossesDataHomesAndPreservesBytes()
    {
        using var workspace = await CreateWriterAsync("bridge-local");
        using var otherHome = PublishedWorkspaceLockStore.Create("bridge-other-home");
        _ = otherHome.Track(workspace.Path);
        var sentinel = TestDataHome.LocalWorkspaceLockPath(workspace.Path);
        var bytes = new byte[] { 0, 1, 255, 13, 10, 42 };
        File.WriteAllBytes(sentinel, bytes);
        workspace.ExpectFiles(HolderFile);
        using var holder = await PublishedLockHolder.StartAsync(sentinel, workspace.Combine(HolderFile));
        var before = workspace.SnapshotState();
        var blocked = await PublishedJourneyProcess.RunAsync(
            workspace.Target, workspace.Path, [Tokens.Index.Command], otherHome.EnvironmentVariables);
        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(before, workspace.SnapshotState());
        await holder.ReleaseAsync();
        var retry = await PublishedJourneyProcess.RunAsync(
            workspace.Target, workspace.Path, [Tokens.Index.Command], otherHome.EnvironmentVariables);
        Assert.Equal(0, retry.ExitCode);
        Assert.Equal(bytes, File.ReadAllBytes(sentinel));
        Assert.Contains(NoteName, File.ReadAllText(workspace.Combine(GuidanceEntrypoint)), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Legacy holders exclude a published writer and a busy bridge releases its local handle")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task LegacyHolderBlocksWriterAndReleasesLocal(bool readOnly)
    {
        using var workspace = await CreateWriterAsync("bridge-legacy");
        var legacy = workspace.LockStore.Track(workspace.Path);
        workspace.ExpectFiles(HolderFile);
        using var holder = await PublishedLockHolder.StartAsync(legacy, workspace.Combine(HolderFile), readOnly);
        var before = workspace.SnapshotState();
        var blocked = await workspace.RunAsync(Tokens.Index.Command);
        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(before, workspace.SnapshotState());
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment) { [Tokens.DataHome.Variable] = RelativeHome };
        var localOnly = await PublishedJourneyProcess.RunAsync(workspace.Target, workspace.Path, [Tokens.Index.Command], environment);
        Assert.Equal(0, localOnly.ExitCode);
        await holder.ReleaseAsync();
    }

    [Theory(DisplayName = "Published writers resume after either legacy holder releases")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task WriterResumesAfterLegacyHolderReleases(bool readOnly)
    {
        using var workspace = await CreateWriterAsync("bridge-directions");
        var legacy = workspace.LockStore.Track(workspace.Path);
        workspace.ExpectFiles(HolderFile);
        using var holder = await PublishedLockHolder.StartAsync(legacy, workspace.Combine(HolderFile), readOnly);
        Assert.Equal(5, (await workspace.RunAsync(Tokens.Index.Command)).ExitCode);
        await holder.ReleaseAsync();
        Assert.Equal(0, (await workspace.RunAsync(Tokens.Index.Command)).ExitCode);
    }

    [Theory(DisplayName = "Windows denied legacy writes permit a published writer with an existing or absent lock")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task DeniedLegacyWriteUsesReadOnlyOrObservedAbsence(bool existing)
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("Windows ACL qualification requires Windows access control.");
            return;
        }

        using var workspace = await CreateWriterAsync("bridge-denied");
        var legacy = workspace.LockStore.Track(workspace.Path);
        if (!existing)
        {
            File.Delete(legacy);
        }

        var directory = new DirectoryInfo(Path.GetDirectoryName(legacy) ?? throw new InvalidOperationException());
        FileSystemSecurity? original = null;
        var aclApplied = false;
        var file = new FileInfo(legacy);
        try
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var user = identity.User ?? throw new InvalidOperationException("The Windows identity has no SID.");
                if (existing)
                {
                    original = file.GetAccessControl();
                    var denied = file.GetAccessControl();
                    denied.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.WriteData, AccessControlType.Deny));
                    file.SetAccessControl(denied);
                    aclApplied = true;
                }
                else
                {
                    original = directory.GetAccessControl();
                    var denied = directory.GetAccessControl();
                    denied.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.CreateFiles, AccessControlType.Deny));
                    directory.SetAccessControl(denied);
                    aclApplied = true;
                }
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or System.Security.SecurityException)
            {
                throw new InvalidOperationException("The process cannot set the required Windows ACL.", exception);
            }

            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                using var denied = new FileStream(legacy, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            });
            var result = await workspace.RunAsync(Tokens.Index.Command);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(existing, File.Exists(legacy));
            Assert.Contains(NoteName, File.ReadAllText(workspace.Combine(GuidanceEntrypoint)), StringComparison.Ordinal);
        }
        finally
        {
            if (aclApplied && original is FileSecurity fileSecurity)
            {
                file.SetAccessControl(fileSecurity);
            }
            else if (aclApplied && original is DirectorySecurity directorySecurity)
            {
                directory.SetAccessControl(directorySecurity);
            }
        }
    }

    private static async Task<PublishedJourneyWorkspace> CreateWriterAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
            workspace.WriteText(Note, OpenForgeDocumentSeed.Metadata(
                description: "Bridge journey note", tags: ["Guidance"], body: "\n# Bridge note\n"));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }
}
