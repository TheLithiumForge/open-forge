using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupStatusDoctorIntegrationTests
{
    [Fact(
        DisplayName = "Cleanup does not change Status or Doctor meaning while blocked recovery items remain"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task StatusAndDoctorRemainUnchangedAcrossCleanup()
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

        var statusBefore = await workspace.RunAsync(
            ["status", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        var doctorBefore = await workspace.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        Assert.Equal(CliOutputTarget.StandardOutput, statusBefore.PrimaryOutputTarget);
        Assert.Equal(CliOutputTarget.StandardOutput, doctorBefore.PrimaryOutputTarget);
        Assert.Equal(string.Empty, statusBefore.StandardError);
        Assert.Equal(string.Empty, doctorBefore.StandardError);

        var cleanup = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, cleanup.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, cleanup.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, cleanup.PrimaryOutputTarget);
        Assert.Equal(string.Empty, cleanup.StandardError);
        Assert.True(File.Exists(malformed));
        Assert.True(File.Exists(draft));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.False(workspace.LockInfrastructureExists);

        var statusAfter = await workspace.RunAsync(
            ["status", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        var doctorAfter = await workspace.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(statusBefore.ExitCode, statusAfter.ExitCode);
        Assert.Equal(statusBefore.Status, statusAfter.Status);
        Assert.Equal(statusBefore.StandardError, statusAfter.StandardError);
        Assert.Equal(statusBefore.StandardOutput, statusAfter.StandardOutput);
        Assert.Equal(doctorBefore.ExitCode, doctorAfter.ExitCode);
        Assert.Equal(doctorBefore.Status, doctorAfter.Status);
        Assert.Equal(doctorBefore.StandardError, doctorAfter.StandardError);
        Assert.Equal(doctorBefore.StandardOutput, doctorAfter.StandardOutput);
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
    }
}
