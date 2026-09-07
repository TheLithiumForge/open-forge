using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupCatalogueIntegrationTests
{
    [Fact(
        DisplayName = "Cleanup retains every accepted producer attribution as typed eligible provenance"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task EveryAcceptedProducerIdentityIsCatalogued()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-producers");
        var expected = new Dictionary<string, (string Producer, string Operation)>(StringComparer.Ordinal)
        {
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Framework,
                RecoveryBundleOperation.Install,
                "cleanup framework install",
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"))] = ("framework", "install"),
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Extension,
                RecoveryBundleOperation.Remove,
                "cleanup extension remove",
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"))] = ("extension", "remove"),
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                "cleanup index",
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"))] = ("index", "index"),
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Route,
                RecoveryBundleOperation.Move,
                "cleanup route move",
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"))] = ("route", "move"),
            [await workspace.AddVerifiedFinalAsync(
                RecoveryBundleProducer.Repair,
                RecoveryBundleOperation.Repair,
                "cleanup repair",
                Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"))] = ("repair", "repair"),
        };
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var candidates = result.GetProperty("catalogue").GetProperty("candidates");
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), CleanupJsonAssertions.Paths(candidates));
        Assert.All(
            candidates.EnumerateArray(),
            candidate =>
            {
                var path = candidate.GetProperty("path").GetString()
                    ?? throw new Xunit.Sdk.XunitException("A Cleanup candidate path is required.");
                Assert.Equal("final", candidate.GetProperty("kind").GetString());
                Assert.Equal("verified", candidate.GetProperty("integrity").GetString());
                Assert.Equal("ordinary", candidate.GetProperty("fileKind").GetString());
                Assert.Equal("eligible", candidate.GetProperty("eligibility").GetString());
                Assert.Equal("delete", candidate.GetProperty("action").GetString());
                var provenance = candidate.GetProperty("provenance");
                Assert.Equal(expected[path].Producer, provenance.GetProperty("producer").GetString());
                Assert.Equal(expected[path].Operation, provenance.GetProperty("operation").GetString());
                Assert.Equal("workspace", provenance.GetProperty("subject").GetProperty("kind").GetString());
                Assert.Equal(
                    WorkspaceIdentity.Key(workspace.Workspace.PhysicalRoot),
                    provenance.GetProperty("subject").GetProperty("identity").GetString());
            });
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Empty(workspace.SnapshotLockBytes());
        Assert.False(workspace.LockInfrastructureExists);
    }

    [Fact(
        DisplayName = "Cleanup catalogues strict finals and drafts while preserving blocked and unknown recovery items"),
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
        var malformed = workspace.AddMalformedFinal(
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"));
        var unsupported = await workspace.AddUnsupportedFinalAsync(
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));
        var unavailable = workspace.AddUnavailableFinal(
            Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"));
        var unsafeDraft = workspace.AddUnsafeDraft(
            Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var mismatched = workspace.AddMismatchedFinal(
            Guid.Parse("22222222-2222-2222-2222-222222222222"));
        var authoredPath = workspace.Combine("authored-note.md");
        workspace.WriteText("authored-note.md", "Authored content must remain untouched.\n");
        var unknown = workspace.AddUnknown("operation-33333333333333333333333333333333.backup");
        var unknownAlias = workspace.AddAlias("preserve-alias.zip", authoredPath);
        var supportBytes = "Nested support bytes must remain untouched.\n"u8.ToArray();
        var supportSentinel = workspace.AddUnknownSupportFile(
            "support-material",
            "nested",
            "sentinel.bin",
            supportBytes);
        var missingAttribution = workspace.AddMissingAttributionFinal(
            Guid.Parse("33333333-3333-3333-3333-333333333333"));
        var invalidAttribution = workspace.AddInvalidAttributionFinal(
            Guid.Parse("44444444-4444-4444-4444-444444444444"));
        var payloadLengthMismatch = workspace.AddPayloadLengthMismatchFinal(
            Guid.Parse("55555555-5555-5555-5555-555555555555"));
        var payloadHashMismatch = workspace.AddPayloadHashMismatchFinal(
            Guid.Parse("66666666-6666-6666-6666-666666666666"));
        var malformedVariants = new[]
        {
            missingAttribution,
            invalidAttribution,
            payloadLengthMismatch,
            payloadHashMismatch,
        };
        var malformedVariantBytes = malformedVariants.ToDictionary(
            path => path,
            File.ReadAllBytes,
            StringComparer.Ordinal);
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();
        Assert.Equal("directory", recoveryBefore["support-material"]);
        Assert.Equal("directory", recoveryBefore["support-material/nested"]);
        Assert.Equal(
            Convert.ToHexString(SHA256.HashData(supportBytes)),
            recoveryBefore["support-material/nested/sentinel.bin"]);
        Assert.All(malformedVariants, path => Assert.True(File.Exists(path)));

        var run = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        CleanupJsonAssertions.RootPropertyOrder(document.RootElement);
        var result = CleanupJsonAssertions.Result(document);
        CleanupJsonAssertions.ResultPropertyOrder(result);
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("complete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("blocked", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("not-requested", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("verification").GetProperty("state").GetString());

        var expectedPaths = new[]
        {
            indexFinal,
            repairFinal,
            draft,
            malformed,
            unsupported,
            unavailable,
            unsafeDraft,
            mismatched,
            missingAttribution,
            invalidAttribution,
            payloadLengthMismatch,
            payloadHashMismatch,
        }.Order(StringComparer.Ordinal).ToArray();
        var catalogue = result.GetProperty("catalogue");
        Assert.Equal(expectedPaths, CleanupJsonAssertions.Paths(catalogue.GetProperty("candidates")));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(catalogue.GetProperty("candidates")),
            path => string.Equals(path, unknown, StringComparison.Ordinal));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(catalogue.GetProperty("candidates")),
            path => string.Equals(path, unknownAlias, StringComparison.Ordinal));

        var indexCandidate = CleanupJsonAssertions.Candidate(result, indexFinal);
        CleanupJsonAssertions.AssertCandidate(
            indexCandidate,
            "final",
            "verified",
            "ordinary",
            "eligible",
            "delete");
        Assert.Equal("index", indexCandidate.GetProperty("provenance").GetProperty("producer").GetString());
        Assert.Equal("index", indexCandidate.GetProperty("provenance").GetProperty("operation").GetString());
        Assert.Equal(
            WorkspaceIdentity.Key(workspace.Workspace.PhysicalRoot),
            indexCandidate.GetProperty("provenance").GetProperty("workspaceKey").GetString());

        var repairCandidate = CleanupJsonAssertions.Candidate(result, repairFinal);
        CleanupJsonAssertions.AssertCandidate(
            repairCandidate,
            "final",
            "verified",
            "ordinary",
            "eligible",
            "delete");
        Assert.Equal("repair", repairCandidate.GetProperty("provenance").GetProperty("producer").GetString());
        Assert.Equal("repair", repairCandidate.GetProperty("provenance").GetProperty("operation").GetString());

        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, draft),
            "draft",
            "incomplete",
            "ordinary",
            "eligible",
            "delete");
        Assert.Equal(
            JsonValueKind.Null,
            CleanupJsonAssertions.Candidate(result, draft).GetProperty("provenance").ValueKind);

        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, malformed),
            "final",
            "malformed",
            "ordinary",
            "blocked",
            "preserve");
        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, unsupported),
            "final",
            "unsupported",
            "ordinary",
            "blocked",
            "preserve");
        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, unavailable),
            "final",
            "unavailable",
            "non-ordinary",
            "blocked",
            "preserve");
        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, unsafeDraft),
            "draft",
            "unavailable",
            "non-ordinary",
            "blocked",
            "preserve");
        CleanupJsonAssertions.AssertCandidate(
            CleanupJsonAssertions.Candidate(result, mismatched),
            "final",
            "malformed",
            "ordinary",
            "blocked",
            "preserve");
        Assert.All(
            malformedVariants,
            path => CleanupJsonAssertions.AssertCandidate(
                CleanupJsonAssertions.Candidate(result, path),
                "final",
                "malformed",
                "ordinary",
                "blocked",
                "preserve"));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(result.GetProperty("plan").GetProperty("entries")),
            path => malformedVariants.Contains(path, StringComparer.Ordinal));
        Assert.DoesNotContain(
            CleanupJsonAssertions.Paths(result.GetProperty("effects")),
            path => malformedVariants.Contains(path, StringComparer.Ordinal));

        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.DoesNotContain(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("outcome").GetString() == "verified");
        Assert.Equal(supportBytes, File.ReadAllBytes(supportSentinel));
        foreach (var (path, bytes) in malformedVariantBytes)
        {
            Assert.Equal(bytes, File.ReadAllBytes(path));
        }
        Assert.Equal("Authored content must remain untouched.\n", File.ReadAllText(authoredPath));
        Assert.True(File.Exists(unknown));
        Assert.True(File.Exists(unknownAlias));
    }

    [Fact(
        DisplayName = "Cleanup preserves an exact-name recovery alias as a non-ordinary blocked candidate"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task ExactNameAliasCannotBecomeADeletionTarget()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-catalogue-alias");
        var authoredPath = workspace.Combine("authored-alias-target.md");
        workspace.WriteText("authored-alias-target.md", "Do not delete through an alias.\n");
        var aliasPath = workspace.AddAlias(
            RecoveryBundleFormatV1.FinalFileName(
                Guid.Parse("44444444-4444-4444-4444-444444444444")),
            authoredPath);
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        var candidate = CleanupJsonAssertions.Candidate(result, aliasPath);
        CleanupJsonAssertions.AssertCandidate(
            candidate,
            "final",
            "unavailable",
            "non-ordinary",
            "blocked",
            "preserve");
        Assert.Equal("complete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("blocked", result.GetProperty("plan").GetProperty("safety").GetString());
        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.True(File.Exists(aliasPath));
        Assert.NotEqual((FileAttributes)0, File.GetAttributes(aliasPath) & FileAttributes.ReparsePoint);
        Assert.Equal("Do not delete through an alias.\n", File.ReadAllText(authoredPath));
    }

    [Fact(
        DisplayName = "Cleanup reports a selected recovery bucket collision as incomplete without treating it as empty"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task RecoveryBucketCollisionIsIncompleteAndPreserved()
    {
        using var workspace = CleanupIntegrationWorkspace.Create(
            "cleanup-catalogue-incomplete",
            withEntry: false);
        var bucket = workspace.BlockRecoveryDirectory();
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var run = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = run.ParseJson();
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("incomplete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Empty(result.GetProperty("catalogue").GetProperty("candidates").EnumerateArray());
        Assert.Equal("blocked", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("not-requested", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        Assert.Equal("cleanup.catalogue-incomplete", finding.GetProperty("code").GetString());
        Assert.True(File.Exists(bucket));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.Equal(recoveryBefore, workspace.SnapshotRecovery());
        Assert.False(workspace.LockInfrastructureExists);
    }
}
