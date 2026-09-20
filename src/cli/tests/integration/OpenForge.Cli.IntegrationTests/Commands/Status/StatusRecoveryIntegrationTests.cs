namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusRecoveryIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Recovery catalogue reports only exact candidate path kind and integrity without lock or fallback effects"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RecoveryCatalogueReportsOnlyExactCandidatePathKindAndIntegrityWithoutTargetLockOrFallbackEffects()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-recovery-catalogue");
        workspace.SeedLockBytes([0x71, 0x72, 0x73, 0x74]);
        using var recovery = StatusRecoveryFixture.Create(workspace);
        using var exclusions = StatusRecoveryExclusions.Create(workspace);
        var verified = await recovery.AddVerifiedFinalAsync();
        var draft = recovery.AddDraft();
        var malformed = recovery.AddMalformedFinal();
        var unsupported = await recovery.AddUnsupportedFinalAsync();
        var unavailable = recovery.AddUnavailableFinal();
        var lookalike = exclusions.AddLookalike();
        var adjacent = exclusions.AddAdjacentLookalike();
        var differentWorkspace = exclusions.AddDifferentWorkspaceCandidate();
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(recovery.WorkspaceDirectory);
        var externalBefore = exclusions.SnapshotExternalBytes();

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--format", "json", "--detail", "full");

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var result = StatusJsonAssertions.Result(root);
        StatusJsonAssertions.AvailableValue(root.GetProperty("counts").GetProperty("recoveryBundles"), 1);
        StatusJsonAssertions.AvailableValue(root.GetProperty("counts").GetProperty("recoveryDrafts"), 1);
        var candidates = result.GetProperty("recovery").GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(5, candidates.Length);
        Assert.All(candidates, item => StatusJsonAssertions.PropertyOrder(item, "path", "kind", "integrity"));
        var expected = new[]
        {
            $"{verified}|final|verified",
            $"{draft}|draft|incomplete",
            $"{malformed}|final|malformed",
            $"{unsupported}|final|unsupported",
            $"{unavailable}|final|unavailable",
        }.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var actual = candidates.Select(item =>
            $"{item.GetProperty("path").GetString()}|"
            + $"{item.GetProperty("kind").GetString()}|"
            + item.GetProperty("integrity").GetString()).ToArray();
        Assert.Equal(expected, actual);
        Assert.DoesNotContain(candidates, item => item.GetProperty("path").GetString() == lookalike);
        Assert.DoesNotContain(candidates, item => item.GetProperty("path").GetString() == adjacent);
        Assert.DoesNotContain(candidates, item => item.GetProperty("path").GetString() == differentWorkspace);
        foreach (var code in new[]
        {
            "status.recovery-candidate-verified",
            "status.recovery-draft-incomplete",
            "status.recovery-final-malformed",
            "status.recovery-final-unsupported",
            "status.recovery-final-unavailable",
        })
        {
            Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == code);
        }

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(recovery.WorkspaceDirectory));
        var externalAfter = exclusions.SnapshotExternalBytes();
        Assert.Equal(externalBefore.Keys, externalAfter.Keys);
        foreach (var path in externalBefore.Keys)
        {
            Assert.Equal(externalBefore[path], externalAfter[path]);
        }
    }
}
