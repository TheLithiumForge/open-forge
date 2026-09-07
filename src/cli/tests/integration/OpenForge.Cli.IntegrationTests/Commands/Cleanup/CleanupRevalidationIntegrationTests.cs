using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupRevalidationIntegrationTests
{
    [Fact(
        DisplayName = "Cleanup ignores a lookalike added after planning while deleting the unchanged exact candidate"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task UnknownAppearanceDoesNotChangeFilteredCatalogue()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-revalidation-lookalike");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup revalidation",
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
        var dryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);

        var unknown = workspace.AddUnknown("operation-dddddddddddddddddddddddddddddddd.backup");
        var unknownBytes = File.ReadAllBytes(unknown);
        var applied = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, applied.PrimaryOutputTarget);
        Assert.Equal(string.Empty, applied.StandardError);
        using var document = applied.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("matched", result.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal([final], CleanupJsonAssertions.Paths(result.GetProperty("effects")));
        Assert.False(File.Exists(final));
        Assert.True(File.Exists(unknown));
        Assert.Equal(unknownBytes, File.ReadAllBytes(unknown));
    }

    [Fact(
        DisplayName = "Cleanup blocks and preserves a final whose semantic bytes changed after planning"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task ChangedFinalCannotSatisfyThePlannedSemanticCondition()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-revalidation-semantic");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Repair,
            RecoveryBundleOperation.Repair,
            "cleanup semantic race",
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));
        var dryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        File.WriteAllBytes(final, "changed recovery bytes"u8.ToArray());
        var recoveryBefore = workspace.SnapshotRecovery();
        var workspaceBefore = workspace.SnapshotWorkspace();

        var applied = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, applied.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, applied.PrimaryOutputTarget);
        Assert.Equal(string.Empty, applied.StandardError);
        using var document = applied.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var candidate = CleanupJsonAssertions.Candidate(result, final);
        CleanupJsonAssertions.AssertCandidate(
            candidate,
            "final",
            "malformed",
            "ordinary",
            "blocked",
            "preserve");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "cleanup.recovery-final-malformed");
        Assert.Equal("not-requested", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.DoesNotContain(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("outcome").GetString() == "verified");
        Assert.True(File.Exists(final));
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
    }

    [Fact(
        DisplayName = "Cleanup blocks and preserves an exact final path that becomes a directory"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task ChangedFinalPathKindCannotSatisfyThePlannedCondition()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-revalidation-path-kind");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup path-kind race",
            Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        var dryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        File.Delete(final);
        Directory.CreateDirectory(final);
        var recoveryBefore = workspace.SnapshotRecovery();
        var workspaceBefore = workspace.SnapshotWorkspace();

        var applied = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, applied.Status);
        using var document = applied.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var candidate = CleanupJsonAssertions.Candidate(result, final);
        Assert.Equal("final", candidate.GetProperty("kind").GetString());
        Assert.Equal("unavailable", candidate.GetProperty("integrity").GetString());
        Assert.Equal("non-ordinary", candidate.GetProperty("fileKind").GetString());
        Assert.Equal("blocked", candidate.GetProperty("eligibility").GetString());
        Assert.Equal("preserve", candidate.GetProperty("action").GetString());
        Assert.True(Directory.Exists(final));
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
    }
}
