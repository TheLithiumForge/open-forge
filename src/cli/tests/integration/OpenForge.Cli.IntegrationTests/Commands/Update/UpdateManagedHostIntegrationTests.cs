using System.Text;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateManagedHostIntegrationTests
{
    private const string Prefix = "\uFEFF# Préface 🧭\r\n\r\nKeep this user-owned introduction.\r\n";
    private const string Suffix = "\r\n## Exact Mechanical Execution Exception\r\nKeep this suffix exactly: café 🧭.\r\n";
    private const string StartMarker = "# Open Forge";
    private const string EndMarker = "**End of Open Forge managed section.**";
    private const string LegacyStartMarker = "<!-- open-forge:start -->";
    private const string LegacyEndMarker = "<!-- open-forge:end -->";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update ignores outside text around an installed managed root block"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task OutsideRootHostTextDoesNotCreateManagedDivergence(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-outside-no-op");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        workspace.ReplaceText(path, $"{Prefix}{canonicalPayload}{Suffix}");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == UpdateFindingCode.RetiredContentPreserved);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, comparison.Kind);
        Assert.Equal("open-forge", comparison.RegionIdentity);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Automatic Update recanonicalizes a soft-break footer and is then current"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task AutomaticUpdateRecanonicalizesSoftBreakFooterAndRepeatIsNoOp(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-soft-break-footer");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var softFooter = EndMarker.Replace(
            "managed section",
            "managed\nsection",
            StringComparison.Ordinal);
        var softBreakPayload = canonicalPayload.Replace(
            EndMarker,
            softFooter,
            StringComparison.Ordinal);
        Assert.NotEqual(canonicalPayload, softBreakPayload);
        var expectedBytes = Encoding.UTF8.GetBytes($"{Prefix}{canonicalPayload}{Suffix}");
        workspace.ReplaceText(path, $"{Prefix}{softBreakPayload}{Suffix}");

        var result = await workspace.ExecuteAsync(
            workspace.Request(force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(result.Effects, effect => effect.Path == path);
        Assert.Equal(expectedBytes, workspace.ReadBytes(path));
        var after = workspace.SnapshotHashes();
        var afterBytes = workspace.ReadBytes(path);

        var repeat = await workspace.ExecuteAsync(
            workspace.Request(force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, repeat.Lifecycle.Outcome);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
        Assert.Equal(afterBytes, workspace.ReadBytes(path));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Automatic Update migrates a legacy managed host to the canonical payload and is idempotent"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task AutomaticUpdateMigratesLegacyHostToCanonicalPayloadAndRepeatIsNoOp(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-legacy-migration");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var legacyPayload = CreateLegacyPayload(path, canonicalPayload, complete: true);
        var expected = Encoding.UTF8.GetBytes($"{Prefix}{canonicalPayload}{Suffix}");
        workspace.ReplaceText(path, $"{Prefix}{legacyPayload}{Suffix}");

        var result = await workspace.ExecuteAsync(workspace.Request(force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Empty(result.Findings);
        Assert.Single(result.Effects, effect => effect.Path == path);
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, comparison.Kind);
        Assert.Equal("open-forge", comparison.RegionIdentity);
        Assert.Equal(expected, workspace.ReadBytes(path));
        var after = workspace.SnapshotHashes();

        var repeat = await workspace.ExecuteAsync(workspace.Request(force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Equal(UpdateLifecycleOutcome.AlreadyCurrent, repeat.Lifecycle.Outcome);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
        Assert.Equal(expected, workspace.ReadBytes(path));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Dry-run Update plans legacy host migration without changing any bytes"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task DryRunPlansLegacyMigrationWithoutPersistentEffects(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-legacy-dry-run");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var legacyPayload = CreateLegacyPayload(path, canonicalPayload, complete: true);
        workspace.ReplaceText(path, $"{Prefix}{legacyPayload}{Suffix}");
        var before = workspace.SnapshotHashes();
        var legacyBytes = workspace.ReadBytes(path);

        var build = await workspace.BuildAsync(
            workspace.Request(mode: UpdateMode.DryRun, force: false, automatic: true));

        Assert.NotNull(build.Plan);
        Assert.False(build.Plan!.IsNoOp);
        Assert.Contains(build.Preview.Effects, effect => effect.Path == path);

        var result = await workspace.ExecuteAsync(
            workspace.Request(mode: UpdateMode.DryRun, force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateMode.DryRun, result.Mode);
        Assert.Contains(result.Effects, effect => effect.Path == path);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(legacyBytes, workspace.ReadBytes(path));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update blocks an incomplete legacy managed host without changing files"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task IncompleteLegacyBoundaryBlocksWithoutEffects(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-incomplete-legacy");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var incompleteLegacyPayload = CreateLegacyPayload(path, canonicalPayload, complete: false);
        workspace.ReplaceText(path, $"{Prefix}{incompleteLegacyPayload}{Suffix}");
        var expected = workspace.ReadBytes(path);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.SourceProvenanceInvalid
            && finding.Target == path);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(expected, workspace.ReadBytes(path));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update blocks an incomplete canonical managed host without changing files"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task IncompleteCanonicalBoundaryBlocksWithoutEffects(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-incomplete-canonical");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var incompleteCanonicalPayload = RemoveEndMarker(canonicalPayload);
        workspace.ReplaceText(path, $"{Prefix}{incompleteCanonicalPayload}{Suffix}");
        var expected = workspace.ReadBytes(path);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.SourceProvenanceInvalid
            && finding.Target == path);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(expected, workspace.ReadBytes(path));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update preserves Unicode and adjacent bytes for an explicitly recorded managed region"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task ExplicitManagedRegionPreservesUnicodePrefixAndSuffix(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-unicode-region");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var expected = Encoding.UTF8.GetBytes($"{Prefix}{canonicalPayload}{Suffix}");
        var changed = canonicalPayload.Replace(
            StartMarker,
            $"{StartMarker}\n\nChanged managed content: café 🧭.",
            StringComparison.Ordinal);
        Assert.NotEqual(canonicalPayload, changed);
        workspace.ReplaceText(path, $"{Prefix}{changed}{Suffix}");
        workspace.SeedExplicitManagedHostRegion(path);

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, comparison.Kind);
        Assert.Equal("open-forge", comparison.RegionIdentity);
        Assert.Single(result.Effects, effect => effect.Path == path);
        Assert.Equal(expected, workspace.ReadBytes(path));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        var after = workspace.SnapshotHashes();

        var repeat = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update leaves an excluded host byte-identical under normal update"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task ExcludedHostRemainsUnchangedUnderNormalUpdate(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-excluded");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var canonicalPayload = workspace.ReadText(path);
        var changed = canonicalPayload.Replace(
            StartMarker,
            $"{StartMarker}\n\nExcluded host bytes.",
            StringComparison.Ordinal);
        Assert.NotEqual(canonicalPayload, changed);
        workspace.ReplaceText(path, changed);
        workspace.ReplaceText(".agents/open-forge.json", $"{{\"removedFiles\":[\"{path}\"]}}");
        var before = workspace.SnapshotHashes();
        var expected = Encoding.UTF8.GetBytes(changed);

        var result = await workspace.ExecuteAsync(workspace.Request(force: false, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.DoesNotContain(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(expected, workspace.ReadBytes(path));
    }

    private static string CreateLegacyPayload(
        string path,
        string canonicalPayload,
        bool complete)
    {
        var legacyBody = path == "CLAUDE.md"
            ? RemoveCanonicalHeading(canonicalPayload)
            : canonicalPayload;
        var legacyEnd = complete ? LegacyEndMarker : string.Empty;
        if (!legacyBody.Contains(EndMarker, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The canonical host fixture must contain its named end boundary.");
        }

        legacyBody = legacyBody.Replace(EndMarker, legacyEnd, StringComparison.Ordinal);
        return $"{LegacyStartMarker}\n\n{legacyBody}";
    }

    private static string RemoveCanonicalHeading(string canonicalPayload)
    {
        var lfPrefix = $"{StartMarker}\n\n";
        if (canonicalPayload.StartsWith(lfPrefix, StringComparison.Ordinal))
        {
            return canonicalPayload[lfPrefix.Length..];
        }

        var crlfPrefix = $"{StartMarker}\r\n\r\n";
        if (canonicalPayload.StartsWith(crlfPrefix, StringComparison.Ordinal))
        {
            return canonicalPayload[crlfPrefix.Length..];
        }

        throw new InvalidOperationException("The canonical CLAUDE.md fixture must begin with the Open Forge heading.");
    }

    private static string RemoveEndMarker(string canonicalPayload)
    {
        if (!canonicalPayload.Contains(EndMarker, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The canonical host fixture must contain its named end boundary.");
        }

        return canonicalPayload.Replace(EndMarker, string.Empty, StringComparison.Ordinal);
    }
}
