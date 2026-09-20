namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusReadableMetadataIntegrationTests
{
    private const string GeneratedTargetPath = ".agents/patterns/_patterns.md";
    private const string MalformedChildPath = ".agents/patterns/old-note.md";

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Status reports a readable malformed ordinary generated-navigation child as attention in text and JSON without writes"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task ReadableMalformedOrdinaryChildIsAttentionInTextAndJsonWithoutWrites()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-readable-malformed-metadata");
        try
        {
            File.WriteAllText(workspace.Combine(MalformedChildPath), """
                ---
                open-forge:
                  description: First description
                  description: Duplicate description
                  tags: [Pattern]
                ---

                # Old note
                """);
            var before = workspace.SnapshotHashes();
            var lockBefore = workspace.SnapshotLockBytes();
            var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

            var textRun = await StatusIntegrationApplication.RunAsync(
                workspace,
                "status",
                "--workspace",
                workspace.Path,
                "--format",
                "text",
                "--detail",
                "minimal");

            Assert.Equal(2, textRun.ExitCode);
            Assert.Equal(string.Empty, textRun.StandardError);
            Assert.Contains(MalformedChildPath, textRun.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("duplicate Open Forge metadata key", textRun.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Generated navigation metadata is malformed", textRun.StandardOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("status.generated-navigation-unavailable", textRun.StandardOutput, StringComparison.Ordinal);

            var jsonRun = await StatusIntegrationApplication.RunAsync(
                workspace,
                "status",
                "--workspace",
                workspace.Path,
                "--format",
                "json",
                "--detail",
                "full");

            Assert.Equal(2, jsonRun.ExitCode);
            Assert.Equal(string.Empty, jsonRun.StandardError);
            using var document = StatusIntegrationApplication.ParseJson(jsonRun);
            var root = document.RootElement;
            Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
            Assert.Equal("open-forge update", root.GetProperty("next").GetProperty("command").GetString());

            var findings = root.GetProperty("findings").EnumerateArray().ToArray();
            var finding = Assert.Single(
                findings,
                value => value.GetProperty("code").GetString() == "status.generated-navigation-metadata-invalid");
            Assert.Equal("warning", finding.GetProperty("severity").GetString());
            Assert.Equal("Generated navigation metadata is malformed", finding.GetProperty("title").GetString());
            Assert.Equal(
                MalformedChildPath,
                finding.GetProperty("subject").GetProperty("path").GetString());
            Assert.Contains(
                finding.GetProperty("evidence").EnumerateArray(),
                value => value.GetProperty("label").GetString() == "cause"
                    && value.GetProperty("value").GetString()!.Contains(
                        "duplicate Open Forge metadata key",
                        StringComparison.Ordinal));
            Assert.DoesNotContain(
                findings,
                value => value.GetProperty("code").GetString() == "status.generated-navigation-unavailable"
                    && value.GetProperty("subject").GetProperty("path").GetString() == GeneratedTargetPath);

            var entries = StatusJsonAssertions.Result(root).GetProperty("entriesSections").EnumerateArray();
            var target = Assert.Single(entries, value => value.GetProperty("path").GetString() == GeneratedTargetPath);
            Assert.Equal("unavailable", target.GetProperty("state").GetString());
            AssertNoWrites(workspace, before, lockBefore, recoveryBefore);
        }
        finally
        {
            File.Delete(workspace.Combine(MalformedChildPath));
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Status retains strict incomplete semantics for a real unreadable generated-navigation child"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task UnreadableGeneratedNavigationChildRemainsStrictlyIncomplete()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-unreadable-generated-metadata");
        try
        {
            File.WriteAllBytes(workspace.Combine(MalformedChildPath), [0xff, 0xfe, 0xfd]);
            var before = workspace.SnapshotHashes();
            var lockBefore = workspace.SnapshotLockBytes();
            var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

            var run = await StatusIntegrationApplication.RunAsync(
                workspace,
                "status",
                "--workspace",
                workspace.Path,
                "--format",
                "json",
                "--detail",
                "full");

            Assert.Equal(3, run.ExitCode);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = StatusIntegrationApplication.ParseJson(run);
            var root = document.RootElement;
            Assert.Equal("incomplete", root.GetProperty("status").GetString());
            var findings = root.GetProperty("findings").EnumerateArray().ToArray();
            Assert.DoesNotContain(
                findings,
                value => value.GetProperty("code").GetString() == "status.generated-navigation-metadata-invalid");
            Assert.Contains(
                findings,
                value => value.GetProperty("code").GetString() == "status.generated-navigation-unavailable"
                    && value.GetProperty("subject").GetProperty("path").GetString() == GeneratedTargetPath);
            var target = Assert.Single(
                StatusJsonAssertions.Result(root).GetProperty("entriesSections").EnumerateArray(),
                value => value.GetProperty("path").GetString() == GeneratedTargetPath);
            Assert.Equal("unavailable", target.GetProperty("state").GetString());
            AssertNoWrites(workspace, before, lockBefore, recoveryBefore);
        }
        finally
        {
            File.Delete(workspace.Combine(MalformedChildPath));
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Status retains strict blocked semantics for a real unsafe generated-navigation target"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task UnsafeGeneratedNavigationTargetRemainsStrictlyBlocked()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-unsafe-generated-metadata");
        using var outside = OpenForge.Cli.TestSupport.TemporaryWorkspace.Create(
            "status-unsafe-generated-metadata-outside");
        var outsidePath = outside.CreateFile("outside.md", "# Outside\n");
        if (!outside.TryCreateFileSymbolicLink("probe.md", outsidePath, out _))
        {
            Assert.Skip("This real-OS boundary requires symbolic-link support.");
        }

        workspace.DeleteOrReplaceWithLink(GeneratedTargetPath, outsidePath);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format",
            "json",
            "--detail",
            "full");

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        var findings = root.GetProperty("findings").EnumerateArray().ToArray();
        Assert.DoesNotContain(
            findings,
            value => value.GetProperty("code").GetString() == "status.generated-navigation-metadata-invalid");
        Assert.Contains(
            findings,
            value => value.GetProperty("code").GetString() == "status.generated-navigation-blocked"
                && value.GetProperty("subject").GetProperty("path").GetString() == GeneratedTargetPath);
        var target = Assert.Single(
            StatusJsonAssertions.Result(root).GetProperty("entriesSections").EnumerateArray(),
            value => value.GetProperty("path").GetString() == GeneratedTargetPath);
        Assert.Equal("blocked", target.GetProperty("state").GetString());
        Assert.Equal("# Outside\n", File.ReadAllText(outsidePath));
        AssertNoWrites(workspace, before, lockBefore, recoveryBefore);
    }

    private static void AssertNoWrites(
        StatusIntegrationWorkspace workspace,
        IReadOnlyDictionary<string, string> before,
        byte[] lockBefore,
        IReadOnlyDictionary<string, string> recoveryBefore)
    {
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }
}
