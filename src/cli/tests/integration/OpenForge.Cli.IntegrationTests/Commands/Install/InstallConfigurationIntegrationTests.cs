using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallConfigurationIntegrationTests
{
    [Fact(DisplayName = "Essentials preview and apply have complete equal effects and repeat is quiet")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task EssentialsPreviewApplyRepeat()
    {
        using var workspace = InstallOperationWorkspace.Create("install-essentials");
        var operation = Create(workspace);
        var before = workspace.SnapshotHashes();
        var preview = await operation.ExecuteAsync(Request(workspace, InstallPreset.Essentials, dryRun: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var result = await operation.ExecuteAsync(Request(workspace, InstallPreset.Essentials), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(preview.Facts.Effects.Select(effect => effect.Path), result.Facts.Effects.Select(effect => effect.Path));
        Assert.False(workspace.Exists(".agents/guidance"));
        Assert.False(workspace.Exists(".agents/maps"));
        Assert.False(workspace.Exists(".agents/templates"));
        Assert.False(workspace.Exists(".agents/memory/archived"));
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.True(workspace.Exists(".agents/skills/open-forge-cli/SKILL.md"));
        Assert.Contains("/.agents/memory/working/", File.ReadAllText(workspace.Combine(".gitignore")), StringComparison.Ordinal);
        var completed = workspace.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(Request(workspace, InstallPreset.Essentials, configure: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Facts.Effects);
        Assert.Equal(completed, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Full Core restores omitted defaults and retains unknown settings and narrower omissions")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task FullCoreRestoresNarrowly()
    {
        using var workspace = InstallOperationWorkspace.Create("install-full-core-configure");
        workspace.WriteText(".agents/open-forge.json", """{"foreign":{"future":true},"removedFiles":[".agents/skills/open-forge-cli/SKILL.md"]}""");
        workspace.WriteText(".gitignore", "# authored\r\n*.local\r\n");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Essentials), TestContext.Current.CancellationToken)).Status);
        var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.FullCore, configure: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(workspace.Exists(".agents/guidance/_guidance.md"));
        Assert.True(workspace.Exists(".agents/memory/archived/_archived.md"));
        Assert.False(workspace.Exists(".agents/skills/open-forge-cli/SKILL.md"));
        Assert.Equal("# authored\r\n*.local\r\n", File.ReadAllText(workspace.Combine(".gitignore")));
        using var settings = JsonDocument.Parse(File.ReadAllText(workspace.Combine(".agents/open-forge.json")));
        Assert.True(settings.RootElement.GetProperty("foreign").GetProperty("future").GetBoolean());
        Assert.Equal(".agents/skills/open-forge-cli/SKILL.md", Assert.Single(settings.RootElement.GetProperty("removedFiles").EnumerateArray()).GetString());
    }

    [Fact(DisplayName = "Remove retains edited defaults and companions and releases only their Framework receipts")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task RemoveKeepsAuthoredContent()
    {
        using var workspace = InstallOperationWorkspace.Create("install-remove-retains");
        var operation = Create(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        const string path = ".agents/skills/open-forge-cli/SKILL.md";
        var edited = File.ReadAllText(workspace.Combine(path)) + "\nAuthored instruction retained.\n";
        workspace.ReplaceInstalledText(path, edited);
        File.WriteAllText(workspace.Combine(".agents/skills/_skills.overwrite.md"), "---\nopen-forge:\n  description: Authored companion\n  tags: [Skill]\n---\n\n# Companion\n\nPreserve this companion.\n");
        var companion = File.ReadAllBytes(workspace.Combine(".agents/skills/_skills.overwrite.md"));
        var result = await operation.ExecuteAsync(Request(workspace, InstallPreset.Custom, configure: true,
            overrides: [new("skills", InstallRouteAction.Remove)]), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(edited, File.ReadAllText(workspace.Combine(path)));
        Assert.Equal(companion, File.ReadAllBytes(workspace.Combine(".agents/skills/_skills.overwrite.md")));
        Assert.Contains("skills/_skills.md", File.ReadAllText(workspace.Combine(".agents/loader.md")), StringComparison.Ordinal);
        using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
        Assert.DoesNotContain(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray(), value => value.GetString() == path);
        Assert.DoesNotContain(ownership.RootElement.GetProperty("framework").GetProperty("regions").EnumerateArray(), value => value.GetProperty("path").GetString() == ".agents/skills/_skills.md");
    }

    [Theory(DisplayName = "Custom recreates ignored copied Working scaffolding with and without a lock"), InlineData(true), InlineData(false)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CopiedWorkingKeepsIgnoreChoice(bool retainLock)
    {
        using var workspace = InstallOperationWorkspace.Create("install-copied-working");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Essentials), TestContext.Current.CancellationToken)).Status);
        File.Delete(workspace.Combine(".agents/memory/working/_working.md"));
        if (!retainLock) File.Delete(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Custom, configure: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var configuration = Assert.IsType<InstallConfiguration>(result.Input.Configuration);
        Assert.Equal(InstallRouteAction.GitIgnore, configuration.Routes.Single(row => row.Id == "memory/working").Action);
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.Contains("/.agents/memory/working/", File.ReadAllText(workspace.Combine(".gitignore")), StringComparison.Ordinal);
        if (!retainLock)
        {
            using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
            Assert.DoesNotContain(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray(), path => path.GetString() == ".agents/skills/open-forge-cli/SKILL.md");
        }
    }

    [Theory(DisplayName = "Stale configuration settings and ignore bytes prevent every planned effect"), InlineData(".agents/open-forge.json", "{\"foreign\":true}")]
    [InlineData(".gitignore", "# concurrent user rule\n"), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task StaleConfigurationStopsBeforeEffects(string path, string replacement)
    {
        using var workspace = InstallOperationWorkspace.Create("install-configure-stale");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Essentials), TestContext.Current.CancellationToken)).Status);
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(observe: (_, _) => File.WriteAllText(workspace.Combine(path), replacement)), workspace.LockStoreRoot);
        var request = new InstallRequest(workspace.Workspace, InstallMode.Apply, false, false, true)
        { Setup = new(true, InstallPreset.FullCore, []) };
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.False(workspace.Exists(".agents/guidance/_guidance.md"));
        Assert.Equal(replacement, File.ReadAllText(workspace.Combine(path)));
        Assert.All(result.Facts.Effects, effect => Assert.Equal(InstallEffectOutcome.NotStarted, effect.Outcome));
    }

    [Fact(DisplayName = "Managed loader edits retain the Update boundary during configuration"), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task ManagedAnchorDivergenceBlocks()
    {
        using var workspace = InstallOperationWorkspace.Create("install-configure-anchor");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        workspace.ReplaceInstalledText(".agents/loader.md", File.ReadAllText(workspace.Combine(".agents/loader.md")) + "\nChanged loader rule.\n");
        var before = workspace.SnapshotHashes();
        var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Essentials, configure: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Unsafe or ambiguous setup observations block and unavailable bytes remain incomplete"), InlineData("loader-directory"), InlineData("ignore-directory")]
    [InlineData("ambiguous-ignore"), InlineData("unavailable-ignore"), InlineData("invalid-ownership")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task SetupObservationBoundary(string fixture)
    {
        var (status, code) = fixture switch
        {
            "unavailable-ignore" => (CliSemanticStatus.Incomplete, InstallFindingCode.LifecycleUnavailable),
            "invalid-ownership" => (CliSemanticStatus.Blocked, InstallFindingCode.LifecycleBlocked),
            _ => (CliSemanticStatus.Blocked, InstallFindingCode.TargetUnsafe),
        };
        using var workspace = InstallOperationWorkspace.Create("install-setup-boundary");
        FileStream? unavailable = null;
        try
        {
            if (fixture == "loader-directory") workspace.CreateDirectory(".agents/loader.md");
            if (fixture == "ignore-directory") workspace.CreateDirectory(".gitignore");
            if (fixture == "ambiguous-ignore") workspace.WriteText(".gitignore", "# BEGIN OPEN FORGE INSTALL\n");
            if (fixture == "invalid-ownership") workspace.WriteText(InstallOperationWorkspace.OwnershipPath, "{ invalid }");
            if (fixture == "unavailable-ignore")
            {
                workspace.WriteText(".gitignore", "# authored\n");
                unavailable = new FileStream(workspace.Combine(".gitignore"), FileMode.Open, FileAccess.Read, FileShare.None);
            }
            var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Essentials, configure: true), TestContext.Current.CancellationToken);
            Assert.Equal(status, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == code);
            Assert.Empty(result.Facts.Effects);
            Assert.False(workspace.Exists(".agents/directives/_directives.md"));
        }
        finally { unavailable?.Dispose(); }
    }

    [Fact(DisplayName = "Configuration replaces a broad Memory omission with explicit selected states")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task BroadMemoryExclusionPreservesOtherStateOmissions()
    {
        using var workspace = InstallOperationWorkspace.Create("install-memory-state-configuration");
        workspace.WriteText(".agents/open-forge.json", """{"removedCategories":["memory"],"future":123}""");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Custom, configure: true,
            overrides: [new("memory/working", InstallRouteAction.GitIgnore)]), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(workspace.Exists(".agents/memory/_memory.md"));
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.False(workspace.Exists(".agents/memory/crystallized/_crystallized.md"));
        Assert.False(workspace.Exists(".agents/memory/emerging/_emerging.md"));
        Assert.False(workspace.Exists(".agents/memory/archived/_archived.md"));
        using var settings = JsonDocument.Parse(File.ReadAllText(workspace.Combine(".agents/open-forge.json")));
        Assert.Equal(123, settings.RootElement.GetProperty("future").GetInt32());
        Assert.DoesNotContain(settings.RootElement.GetProperty("removedCategories").EnumerateArray(), value => value.GetString() == "memory");
        Assert.Equal(3, settings.RootElement.GetProperty("removedDirectories").GetArrayLength());
    }

    [Fact(DisplayName = "Omitting supplied defaults preserves scoped Framework and other managers' receipts")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task OtherReceiptsSurviveOmission()
    {
        using var workspace = InstallOperationWorkspace.Create("install-omit-receipts");
        Assert.Equal(CliSemanticStatus.Complete, (await Create(workspace).ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        var framework = Assert.IsType<FrameworkOwnership>(read.Document.Framework);
        const string scoped = ".agents/skills/team/_team.md";
        var extension = new ExtensionOwnership("example", "1", null, [], [".agents/skills/local/ext.md"], []);
        var library = new LibraryOwnership("notes", "vendor/notes", ".agents/skills/vendor", ["note.md"]);
        var amended = read.Document with
        {
            Framework = framework with { Paths = framework.Paths.Add(scoped), Regions = framework.Regions.Add(new(scoped, "entries")) },
            Extensions = [extension],
            Libraries = [library],
        };
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(amended));
        var result = await Create(workspace).ExecuteAsync(Request(workspace, InstallPreset.Custom, configure: true,
            overrides: [new("skills", InstallRouteAction.Remove)]), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var after = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceOwnershipCodec.Write(amended with { Framework = null }),
            WorkspaceOwnershipCodec.Write(after.Document with { Framework = null }));
        var afterFramework = Assert.IsType<FrameworkOwnership>(after.Document.Framework);
        Assert.Contains(scoped, afterFramework.Paths);
        Assert.Contains(new OwnedRegion(scoped, "entries"), afterFramework.Regions);
        Assert.DoesNotContain(".agents/skills/open-forge-cli/SKILL.md", afterFramework.Paths);
    }

    private static InstallOperation Create(InstallOperationWorkspace workspace)
        => InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);

    private static InstallRequest Request(InstallOperationWorkspace workspace, InstallPreset preset,
        bool configure = false, bool dryRun = false, ImmutableArray<InstallRouteSelection> overrides = default)
        => new(workspace.Workspace, dryRun ? InstallMode.DryRun : InstallMode.Apply, false, true, false)
        { Setup = new(configure, preset, overrides.IsDefault ? [] : overrides) };
}
