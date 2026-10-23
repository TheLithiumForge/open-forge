using System.Diagnostics;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.IntegrationTests.Commands.Cleanup.Shared.Processes;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport.Processes;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

[Collection("Recovery store environment")]
[Trait("Feature", "cleanup-c1-storage"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class CleanupLinkedStorageIntegrationTests
{
    [Theory(DisplayName = "Cleanup never opens an external FIFO through a linked recovery bucket")]
    [InlineData("linux", false, false), InlineData("linux", false, true)]
    [InlineData("linux", true, false), InlineData("linux", true, true)]
    [InlineData("macos", false, false), InlineData("macos", false, true)]
    [InlineData("macos", true, false), InlineData("macos", true, true)]
    public async Task LinkedBucketIsRejectedBeforeOpeningCandidates(string platform, bool legacy, bool dryRun)
    {
        if (!(platform == "linux" && OperatingSystem.IsLinux()) && !(platform == "macos" && OperatingSystem.IsMacOS()))
        {
            Assert.Skip($"This FIFO evidence requires {platform}.");
        }

        using var files = TemporaryWorkspace.Create("cleanup-linked-fifo");
        using var external = TemporaryWorkspace.Create("cleanup-external-fifo");
        var storage = external.CreateDirectory("storage");
        using var home = TestDataHome.Create("cleanup-linked-fifo-home");
        var prior = Environment.GetEnvironmentVariable(TestDataHome.Variable);
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
        string? bucket = null;
        var fifo = external.CreateFile(Path.Combine("storage", RecoveryBundleFormatV1.FinalFileName(Guid.NewGuid())));
        File.Delete(fifo);
        try
        {
            Environment.SetEnvironmentVariable(TestDataHome.Variable, home.Path);
            var location = legacy ? RecoveryStoreLocator.Legacy(workspace) : RecoveryStoreLocator.Local(workspace);
            Assert.NotNull(location);
            bucket = location.WorkspaceDirectory;
            Directory.CreateDirectory(Path.GetDirectoryName(bucket)!);
            Directory.CreateSymbolicLink(bucket, storage);
            var start = new ProcessStartInfo("mkfifo") { UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add(fifo);
            using (var create = Process.Start(start) ?? throw new InvalidOperationException("The FIFO fixture did not start."))
            {
                await create.WaitForExitAsync(TestContext.Current.CancellationToken);
                Assert.Equal(0, create.ExitCode);
            }

            var lines = await IntegrationProbeProcess.RunAsync(
                [CleanupStorageProbe.ChildArgument, files.Path, dryRun ? "dry-run" : "apply", legacy ? "legacy" : "workspace"]);

            Assert.Equal(["Incomplete", "0", "NotRequested"], lines);
            Assert.Equal(storage, new DirectoryInfo(bucket).LinkTarget);
            Assert.Equal(fifo, Assert.Single(Directory.EnumerateFileSystemEntries(storage)));
            Assert.False(File.Exists(files.Combine(".open-forge-state/workspace.lock")));
        }
        finally
        {
            if (bucket is not null && new DirectoryInfo(bucket).LinkTarget is not null) Directory.Delete(bucket);
            File.Delete(fifo);
            Environment.SetEnvironmentVariable(TestDataHome.Variable, prior);
        }
    }
}
