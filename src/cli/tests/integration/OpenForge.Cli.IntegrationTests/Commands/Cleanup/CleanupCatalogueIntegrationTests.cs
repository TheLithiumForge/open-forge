using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupCatalogueIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup preserves accepted producer attribution in standard item data"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task EveryAcceptedProducerIdentityIsCatalogued()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-producers");
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Framework,
                RecoveryBundleOperation.Install,
                "cleanup framework install",
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"))] = "cleanup framework install",
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Extension,
                RecoveryBundleOperation.Remove,
                "cleanup extension remove",
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"))] = "cleanup extension remove",
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                "cleanup index",
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"))] = "cleanup index",
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Route,
                RecoveryBundleOperation.Move,
                "cleanup route move",
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"))] = "cleanup route move",
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Repair,
                RecoveryBundleOperation.Repair,
                "cleanup repair",
                Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"))] = "cleanup repair",
        };
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--format", "json", "--detail", "standard"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var items = result.GetProperty("items");
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), CleanupJsonAssertions.Paths(items));
        Assert.All(
            items.EnumerateArray(),
            item =>
            {
                var path = item.GetProperty("path").GetString()
                    ?? throw new Xunit.Sdk.XunitException("A Cleanup item path is required.");
                CleanupJsonAssertions.PropertyOrder(item, "path", "kind", "outcome", "origin", "integrity");
                Assert.Equal("bundle", item.GetProperty("kind").GetString());
                Assert.Equal("would-be-removed", item.GetProperty("outcome").GetString());
                Assert.Equal(expected[path], item.GetProperty("origin").GetString());
                Assert.Equal("verified", item.GetProperty("integrity").GetString());
            });
        Assert.Empty(result.GetProperty("notEligible").EnumerateArray());
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Empty(workspace.SnapshotLockBytes());
        Assert.False(workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup removes independently eligible recovery items while retaining malformed bytes with attention"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task MalformedFinalDoesNotBlockIndependentDeletion()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-malformed-partial");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup malformed partial final",
            Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var draft = workspace.AddDraft(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var malformed = workspace.AddMalformedFinal(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        var malformedBytes = File.ReadAllBytes(malformed);
        var workspaceBefore = workspace.SnapshotWorkspace();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--format", "json", "--detail", "standard"],
            TestContext.Current.CancellationToken);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal(
            new[] { final, draft }.Order(StringComparer.Ordinal),
            CleanupJsonAssertions.Paths(result.GetProperty("items")));
        Assert.All(
            result.GetProperty("items").EnumerateArray(),
            item => Assert.Equal("removed", item.GetProperty("outcome").GetString()));
        Assert.Equal([malformed], CleanupJsonAssertions.Paths(result.GetProperty("notEligible")));
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("cleanup.recovery-final-malformed", finding.GetProperty("code").GetString());
        var headline = document.RootElement.GetProperty("summary").GetProperty("headline").GetString()
            ?? throw new Xunit.Sdk.XunitException("Cleanup attention headline is required.");
        Assert.Contains("removed", headline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.GetFileName(malformed), run.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(final));
        Assert.False(File.Exists(draft));
        Assert.True(File.Exists(malformed));
        Assert.Equal(malformedBytes, File.ReadAllBytes(malformed));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.True(workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup catalogues eligible items while preserving blocked and unknown recovery items"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task CatalogueIsCompleteOrderedTypedAndPreserving()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-complete");
        var indexFinal = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup catalogue index",
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var repairFinal = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Repair,
            RecoveryBundleOperation.Repair,
            "cleanup catalogue repair",
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        var draft = workspace.AddDraft(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
        var malformed = workspace.AddMalformedFinal(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));
        var unsupported = await workspace.AddUnsupportedFinalAsync(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));
        var unavailable = workspace.AddUnavailableFinal(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        var unsafeDraft = workspace.AddUnsafeDraft(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var mismatched = workspace.AddMismatchedFinal(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var authoredPath = workspace.Combine("authored-note.md");
        workspace.WriteText("authored-note.md", "Authored content must remain untouched.\n");
        var unknown = workspace.AddUnknown("operation-33333333333333333333333333333333.backup");
        var unknownAlias = workspace.AddAlias("preserve-alias.zip", authoredPath);
        var supportBytes = "Nested support bytes must remain untouched.\n"u8.ToArray();
        var supportSentinel = workspace.AddUnknownSupportFile("support-material", "nested", "sentinel.bin", supportBytes);
        var missingAttribution = workspace.AddMissingAttributionFinal(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        var invalidAttribution = workspace.AddInvalidAttributionFinal(Guid.Parse("44444444-4444-4444-4444-444444444444"));
        var payloadLengthMismatch = workspace.AddPayloadLengthMismatchFinal(Guid.Parse("55555555-5555-5555-5555-555555555555"));
        var payloadHashMismatch = workspace.AddPayloadHashMismatchFinal(Guid.Parse("66666666-6666-6666-6666-666666666666"));
        var blocked = new[]
        {
            malformed,
            unsupported,
            unavailable,
            unsafeDraft,
            mismatched,
            missingAttribution,
            invalidAttribution,
            payloadLengthMismatch,
            payloadHashMismatch,
        };
        var blockedBytes = blocked
            .Where(File.Exists)
            .ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--format", "json", "--detail", "standard"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        CleanupJsonAssertions.RootPropertyOrder(document.RootElement);
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal([indexFinal, repairFinal, draft], CleanupJsonAssertions.Paths(result.GetProperty("items")));
        Assert.All(
            result.GetProperty("items").EnumerateArray(),
            item => Assert.Equal("would-be-removed", item.GetProperty("outcome").GetString()));
        Assert.Equal(
            blocked.Order(StringComparer.Ordinal),
            CleanupJsonAssertions.Paths(result.GetProperty("notEligible")));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(result.GetProperty("notEligible")),
            path => string.Equals(path, unknown, StringComparison.Ordinal));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(result.GetProperty("notEligible")),
            path => string.Equals(path, unknownAlias, StringComparison.Ordinal));
        var findingCodes = document.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Select(finding => finding.GetProperty("code").GetString())
            .ToArray();
        Assert.Contains("cleanup.recovery-final-malformed", findingCodes);
        Assert.Contains("cleanup.recovery-final-unsupported", findingCodes);
        Assert.Contains("cleanup.recovery-draft-unsafe", findingCodes);

        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.Equal(supportBytes, File.ReadAllBytes(supportSentinel));
        foreach (var (path, bytes) in blockedBytes)
        {
            Assert.Equal(bytes, File.ReadAllBytes(path));
        }

        Assert.Equal("Authored content must remain untouched.\n", File.ReadAllText(authoredPath));
        Assert.True(File.Exists(unknown));
        Assert.True(File.Exists(unknownAlias));
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup preserves an exact-name recovery alias as a non-ordinary blocked item"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task ExactNameAliasCannotBecomeADeletionTarget()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-alias");
        var authoredPath = workspace.Combine("authored-alias-target.md");
        workspace.WriteText("authored-alias-target.md", "Do not delete through an alias.\n");
        var aliasPath = workspace.AddAlias(
            RecoveryBundleFormatV1.FinalFileName(Guid.Parse("44444444-4444-4444-4444-444444444444")),
            authoredPath);
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--format", "json", "--detail", "standard"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var item = CleanupJsonAssertions.NotEligible(result, aliasPath);
        Assert.Equal("not recognized", item.GetProperty("reason").GetString());
        var headline = document.RootElement.GetProperty("summary").GetProperty("headline").GetString()
            ?? throw new Xunit.Sdk.XunitException("Cleanup blocked headline is required.");
        Assert.StartsWith("Cannot clean up: ", headline, StringComparison.Ordinal);
        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.True(File.Exists(aliasPath));
        Assert.NotEqual((FileAttributes)0, File.GetAttributes(aliasPath) & FileAttributes.ReparsePoint);
        Assert.Equal("Do not delete through an alias.\n", File.ReadAllText(authoredPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Cleanup reports a selected recovery bucket collision as incomplete without treating it as empty"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task RecoveryBucketCollisionIsIncompleteAndPreserved()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-incomplete", withEntry: false);
        var bucket = workspace.BlockRecoveryDirectory();
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--format", "json", "--detail", "standard"],
            TestContext.Current.CancellationToken);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Empty(result.GetProperty("items").EnumerateArray());
        Assert.Empty(result.GetProperty("notEligible").EnumerateArray());
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("cleanup.catalogue-incomplete", finding.GetProperty("code").GetString());
        Assert.True(File.Exists(bucket));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.False(workspace.LockInfrastructureExists);
    }
}
