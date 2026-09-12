using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupApplicationIntegrationTests
{
    [Fact(
        DisplayName = "Cleanup lease contention blocks every deletion and preserves the complete catalogue"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task ContendedLeaseStartsNoDeletion()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-application-contention");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup contention",
            Guid.Parse("77777777-7777-7777-7777-777777777777"));
        var draft = workspace.AddDraft(
            Guid.Parse("88888888-8888-8888-8888-888888888888"));
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();
        var lockBefore = workspace.SnapshotLockBytes();
        CleanupIntegrationRun run;
        using (var heldLease = workspace.HoldLock())
        {
            run = await workspace.RunAsync(
                ["cleanup", "--workspace", workspace.Path, "--json"],
                TestContext.Current.CancellationToken);
        }

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal("complete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("failed", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("verification").GetProperty("state").GetString());
        Assert.DoesNotContain(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("outcome").GetString() == "verified");
        Assert.DoesNotContain(
            result.GetProperty("residuals").EnumerateArray(),
            residual => residual.GetProperty("outcome").GetString() == "verified");
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        Assert.Equal("cleanup.workspace-lock-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal("blocked", finding.GetProperty("status").GetString());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.True(File.Exists(final));
        Assert.True(File.Exists(draft));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.True(workspace.LockInfrastructureExists);
    }

    [Fact(
        DisplayName = "Cleanup verifies exact deletion while preserving authored and unknown recovery content"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task SuccessfulApplicationPreservesNonCandidatesAndVerifiesAbsence()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-application-effects");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Repair,
            RecoveryBundleOperation.Repair,
            "cleanup application",
            Guid.Parse("99999999-9999-9999-9999-999999999999"));
        var draft = workspace.AddDraft(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var authoredPath = workspace.Combine("authored-content.md");
        workspace.WriteText("authored-content.md", "Authored content survives Cleanup.\n");
        var unknown = workspace.AddUnknown("operation-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.note");
        var alias = workspace.AddAlias("preserve-authored-alias.zip", authoredPath);
        var workspaceBefore = workspace.SnapshotWorkspace();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal("complete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("acquired", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("matched", result.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("verified", result.GetProperty("verification").GetProperty("state").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Empty(result.GetProperty("residuals").EnumerateArray());
        var effects = result.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, effects.Length);
        Assert.Equal(
            [final, draft],
            effects.Select(effect => effect.GetProperty("path").GetString()).ToArray());
        Assert.All(
            effects,
            effect =>
            {
                Assert.Equal("delete", effect.GetProperty("action").GetString());
                Assert.Equal("verified", effect.GetProperty("outcome").GetString());
                Assert.Equal("none", effect.GetProperty("residual").GetString());
                Assert.Equal("ordinary", effect.GetProperty("fileKind").GetString());
            });
        Assert.False(File.Exists(final));
        Assert.False(File.Exists(draft));
        Assert.True(File.Exists(unknown));
        Assert.True(File.Exists(alias));
        Assert.NotEqual((FileAttributes)0, File.GetAttributes(alias) & FileAttributes.ReparsePoint);
        Assert.Equal("Authored content survives Cleanup.\n", File.ReadAllText(authoredPath));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.True(workspace.LockInfrastructureExists);
        Assert.Empty(workspace.SnapshotLockBytes());
    }
}
