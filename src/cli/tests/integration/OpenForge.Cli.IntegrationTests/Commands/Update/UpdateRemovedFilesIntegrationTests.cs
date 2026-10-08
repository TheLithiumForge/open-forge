using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateRemovedFilesIntegrationTests
{
    private const string ExcludedOwnedPath = ".agents/patterns/_patterns.md";
    private const string ExcludedGeneratedPath = UpdateIntegrationWorkspace.GeneratedPath;
    private const string CanonicalHostHeading = "# Open Forge";
    private const string CanonicalHostFooter = "**End of Open Forge managed section.**";
    private const string LegacyStartMarker = "<!-- open-forge:start -->";
    private const string LegacyEndMarker = "<!-- open-forge:end -->";
    private const string LegacyHostPrefix = "User-owned introduction.\n\n";
    private const string LegacyHostSuffix = "\n\n# User-owned notes\nKeep this suffix exactly.\n";
    private const string LegacyAgentsBody =
        "# Open Forge\n\n"
        + "Open Forge provides the working rules and context for this workspace.\n\n"
        + "Before starting a task, read `.agents/loader.md`.\n"
        + "Use it to select every relevant scope, including nested scopes.\n"
        + "Follow the loaded rules throughout the task.\n";
    private const string LegacyClaudeBody = "@AGENTS.md\n@.agents/loader.md\n";

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
        const string settings = """{"frontmatter":"scoped","removedFiles":[".agents/patterns/_patterns.md"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var excluded = await workspace.ExecuteAsync(workspace.Request(force: true, prune: true, automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.False(workspace.Exists(ExcludedOwnedPath));
        Assert.DoesNotContain(excluded.Effects, effect => effect.Path == ExcludedOwnedPath);
        Assert.Equal(ownership, workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));

        workspace.ReplaceText(".agents/open-forge.json", "{\"frontmatter\":\"scoped\"}");
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
    [Theory(DisplayName = "Update preserves an excluded root host's legacy HTML bytes without conversion"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    public async Task PreservesExcludedLegacyRootHostBytes(string path)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-removed-files-legacy-host");
        workspace.WriteText(".agents/open-forge.json", "{}");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var legacy = LegacyHostContents(path);
        Assert.DoesNotContain(CanonicalHostFooter, legacy, StringComparison.Ordinal);
        if (path == "CLAUDE.md")
        {
            Assert.DoesNotContain(CanonicalHostHeading, legacy, StringComparison.Ordinal);
        }

        workspace.ReplaceText(path, legacy);
        var expected = workspace.ReadBytes(path);
        var settings = $$"""{"removedFiles":["{{path}}"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expected, workspace.ReadBytes(path));
        Assert.DoesNotContain(result.Effects, effect => effect.Path == path);
    }

    private static string LegacyHostContents(string path)
    {
        var body = path switch
        {
            "AGENTS.md" => LegacyAgentsBody,
            "CLAUDE.md" => LegacyClaudeBody,
            _ => throw new ArgumentOutOfRangeException(nameof(path), path, "Unsupported managed host."),
        };

        return $"{LegacyHostPrefix}{LegacyStartMarker}\n\n{body}{LegacyEndMarker}\n{LegacyHostSuffix}";
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
        var current = workspace.ReadText(path);
        var changed = current.Replace(CanonicalHostHeading, CanonicalHostHeading + "\n\nExcluded managed-region bytes.", StringComparison.Ordinal);
        Assert.NotEqual(current, changed);
        workspace.ReplaceText(path, changed);
    }
}
