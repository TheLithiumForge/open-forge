using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateRemovedFilesIntegrationTests
{
    private const string ExcludedOwnedPath = ".agents/patterns/_patterns.md";
    private const string ExcludedGeneratedPath = UpdateIntegrationWorkspace.GeneratedPath;

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update leaves an absent excluded owned file missing through force and prune, then restores it when exclusion is removed"), Trait("Evidence", "Integration")]
    public async Task MissingExcludedOwnedFileIsNotRestoredUntilOptIn()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-removed-files-missing-restore");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        Assert.True(workspace.Exists(ExcludedOwnedPath));
        var expected = workspace.ReadBytes(ExcludedOwnedPath);
        var ownership = workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath);
        workspace.RemoveFile(ExcludedOwnedPath);
        const string settings = """{"removedFiles":[".agents/patterns/_patterns.md"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var excluded = await workspace.ExecuteAsync(workspace.Request(force: true, prune: true, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.False(workspace.Exists(ExcludedOwnedPath));
        Assert.DoesNotContain(excluded.Effects, effect => effect.Path == ExcludedOwnedPath);
        Assert.Equal(ownership, workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));

        workspace.ReplaceText(".agents/open-forge.json", "{}");
        var optedIn = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, optedIn.Status);
        Assert.True(workspace.Exists(ExcludedOwnedPath));
        Assert.Equal(expected, workspace.ReadBytes(ExcludedOwnedPath));
        Assert.Contains(optedIn.Effects, effect => effect.Path == ExcludedOwnedPath);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update preserves excluded existing whole-file and generated-region bytes and ownership facts"), Trait("Evidence", "Integration")]
    public async Task PreservesExistingExcludedFileBytes()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-removed-files-byte-preservation");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        _ = workspace.SeedCoalescedAuthoredAndGeneratedChange();
        MutateManagedRegion(workspace, "AGENTS.md");
        MutateManagedRegion(workspace, "CLAUDE.md");
        var expected = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [ExcludedGeneratedPath] = workspace.ReadBytes(ExcludedGeneratedPath),
            ["AGENTS.md"] = workspace.ReadBytes("AGENTS.md"),
            ["CLAUDE.md"] = workspace.ReadBytes("CLAUDE.md"),
        };
        var ownership = workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath);
        const string settings = """{"removedFiles":[".agents/memory/_memory.md","AGENTS.md","CLAUDE.md"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var result = await workspace.ExecuteAsync(workspace.Request(force: true, prune: true, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        foreach (var (path, bytes) in expected)
        {
            Assert.Equal(bytes, workspace.ReadBytes(path));
            Assert.DoesNotContain(result.Effects, effect => effect.Path == path);
            Assert.DoesNotContain(result.Comparisons, comparison => comparison.RelativePath == path);
        }
        Assert.Equal(ownership, workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update blocks instead of restoring an excluded missing loader required by remaining routes"), Trait("Evidence", "Integration")]
    public async Task BlocksWhenMissingExcludedLoaderIsRequired()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-removed-loader-ancestor");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.RemoveFile(UpdateIntegrationWorkspace.ManagedPath);
        var ownership = workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath);
        const string settings = """{"removedFiles":[".agents/loader.md"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var result = await workspace.ExecuteAsync(workspace.Request(force: true, prune: true, automatic: true));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.TargetUnsafe
            && finding.Target == UpdateIntegrationWorkspace.ManagedPath);
        Assert.False(workspace.Exists(UpdateIntegrationWorkspace.ManagedPath));
        Assert.DoesNotContain(result.Effects, effect => effect.Path == UpdateIntegrationWorkspace.ManagedPath);
        Assert.Equal(ownership, workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update refuses malformed removed file settings without applying an effect"), Trait("Evidence", "Integration")]
    public async Task RefusesMalformedRemovedFileSettings()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-invalid-removed-files");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        const string settings = """{"removedFiles":[".agents/*.md"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(force: true, prune: true, automatic: true));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
    }

    private static void MutateManagedRegion(UpdateIntegrationWorkspace workspace, string path)
    {
        const string marker = "<!-- open-forge:start -->";
        var current = workspace.ReadText(path);
        var changed = current.Replace(marker, marker + "\n\nExcluded managed-region bytes.", StringComparison.Ordinal);
        Assert.NotEqual(current, changed);
        workspace.ReplaceText(path, changed);
    }
}
