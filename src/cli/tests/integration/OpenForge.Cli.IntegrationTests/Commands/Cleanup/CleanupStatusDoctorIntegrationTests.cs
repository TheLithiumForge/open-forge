using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupStatusDoctorIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup preserves malformed recovery and authored workspace while Status and Doctor remain available"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task StatusAndDoctorRemainAvailableAfterIndependentCleanup()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-status-doctor");
        var malformed = workspace.AddMalformedFinal(
            Guid.Parse("12121212-1212-1212-1212-121212121212"));
        var draft = workspace.AddDraft(
            Guid.Parse("13131313-1313-1313-1313-131313131313"));
        workspace.WriteText(
            ".agents/authored-status-doctor.md",
            "Authored status and doctor content.\n");
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();
        var malformedBytes = File.ReadAllBytes(malformed);

        var statusBefore = await workspace.RunAsync(
            ["status", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);
        var doctorBefore = await workspace.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);
        Assert.Equal(CliOutputTarget.StandardOutput, statusBefore.PrimaryOutputTarget);
        Assert.Equal(CliOutputTarget.StandardOutput, doctorBefore.PrimaryOutputTarget);
        Assert.Equal(string.Empty, statusBefore.StandardError);
        Assert.Equal(string.Empty, doctorBefore.StandardError);

        var cleanup = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(2, cleanup.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, cleanup.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, cleanup.PrimaryOutputTarget);
        Assert.Equal(string.Empty, cleanup.StandardError);
        Assert.True(File.Exists(malformed));
        Assert.Equal(malformedBytes, File.ReadAllBytes(malformed));
        Assert.False(File.Exists(draft));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.NotEqual(recoveryBefore, workspace.SnapshotRecovery());
        Assert.True(workspace.LockInfrastructureExists);

        var statusAfter = await workspace.RunAsync(
            ["status", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);
        var doctorAfter = await workspace.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(statusBefore.ExitCode, statusAfter.ExitCode);
        Assert.Equal(statusBefore.Status, statusAfter.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, statusAfter.PrimaryOutputTarget);
        Assert.Equal(string.Empty, statusAfter.StandardError);
        Assert.Equal(doctorBefore.ExitCode, doctorAfter.ExitCode);
        Assert.Equal(doctorBefore.Status, doctorAfter.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, doctorAfter.PrimaryOutputTarget);
        Assert.Equal(string.Empty, doctorAfter.StandardError);
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.True(File.Exists(malformed));
        Assert.False(File.Exists(draft));
    }
}
