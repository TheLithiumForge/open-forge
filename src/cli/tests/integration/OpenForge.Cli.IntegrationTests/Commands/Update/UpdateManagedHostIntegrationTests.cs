using System.Text;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateManagedHostIntegrationTests
{
    private const string Prefix = "# Préface 🧭\r\n\r\nKeep this user-owned introduction.\r\n";
    private const string Suffix = "\r\n# User notes\r\nKeep this suffix exactly.\r\n";
    private const string StartMarker = "<!-- open-forge:start -->";

    [Theory(DisplayName = "Update ignores outside text around an installed managed root block"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task OutsideRootHostTextDoesNotCreateManagedDivergence(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-outside-no-op");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var block = workspace.ReadText(path);
        workspace.ReplaceText(path, $"{Prefix}{block}{Suffix}");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == UpdateFindingCode.ManagedDivergence);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Null(comparison.RegionIdentity);
    }

    [Theory(DisplayName = "Forced Update restores the installed root block while preserving outside bytes"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task ForceRestoresRootBlockPreservingOutsideBytesAndRepeatIsNoOp(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-force-preservation");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var block = workspace.ReadText(path);
        var expected = Encoding.UTF8.GetBytes($"{Prefix}{block}{Suffix}");
        var changed = block.Replace(StartMarker, $"{StartMarker}\n\nChanged managed content.", StringComparison.Ordinal);
        Assert.NotEqual(block, changed);
        workspace.ReplaceText(path, $"{Prefix}{changed}{Suffix}");

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        Assert.Single(result.Effects, effect => effect.Path == path);
        Assert.Equal(expected, workspace.ReadBytes(path));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Null(comparison.RegionIdentity);
        var after = workspace.SnapshotHashes();

        var repeat = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Update preserves Unicode host bytes around an explicitly recorded managed region"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md")]
    public async Task ExplicitManagedRegionPreservesUnicodePrefixAndSuffix(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-managed-host-unicode-region");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var block = workspace.ReadText(path);
        var expected = Encoding.UTF8.GetBytes($"{Prefix}{block}{Suffix}");
        var changed = block.Replace(StartMarker, $"{StartMarker}\n\nChanged managed content: café 🧭.", StringComparison.Ordinal);
        Assert.NotEqual(block, changed);
        workspace.ReplaceText(path, $"{Prefix}{changed}{Suffix}");
        workspace.SeedExplicitManagedHostRegion(path);

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(UpdateLifecycleTrust.Trusted, result.Lifecycle.Trust);
        var comparison = Assert.Single(result.Comparisons, comparison => comparison.RelativePath == path);
        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, comparison.Kind);
        Assert.Equal("managed", comparison.RegionIdentity);
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
}
