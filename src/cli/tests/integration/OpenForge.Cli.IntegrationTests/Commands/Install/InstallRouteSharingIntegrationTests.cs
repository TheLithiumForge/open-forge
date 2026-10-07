using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "workspace-route-sharing"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallRouteSharingIntegrationTests
{
    private const string Working = ".agents/memory/working/_working.md";
    private const string PrivateNote = ".agents/memory/working/note.md";

    [Fact(DisplayName = "Force Install cannot publish private Entries from an invalid sharing lock")]
    public async Task InvalidPolicyDoesNotPermitForceInstall()
    {
        using var workspace = InstallOperationWorkspace.Create("force-sharing-policy");
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Private note", ["Private"], "# Private body\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        workspace.ReplaceInstalledText(InstallOperationWorkspace.OwnershipPath, "not json");
        var before = workspace.SnapshotHashes();
        var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(force: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.LifecycleBlocked);
        Assert.Empty(result.Facts.Effects);
        Assert.DoesNotContain("note.md", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Private metadata remains diagnosed without demanding private shared Entries")]
    public async Task PrivateMetadataDiagnosisRemainsLocal()
    {
        using var workspace = InstallOperationWorkspace.Create("health-sharing-metadata");
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Private note", ["Private"], "# Private body\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        workspace.ReplaceInstalledText(PrivateNote, "---\nopen-forge:\n  description: First\n  description: Duplicate\n  tags: [Private]\n---\n\n# Private note\n");
        var before = workspace.SnapshotHashes();
        var doctor = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        using var diagnosis = JsonDocument.Parse(doctor.Output);
        Assert.Contains(diagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("subject").GetProperty("path").GetString() == PrivateNote);
        AssertNoSharedRowFindings(diagnosis.RootElement);
        var status = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        using var health = JsonDocument.Parse(status.Output);
        AssertWorkingCurrent(health.RootElement);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Public unexposed notes retain Move and Remove refusal")]
    public async Task PublicUnexposedLeafStillRefused()
    {
        using var workspace = InstallOperationWorkspace.Create("public-unexposed-sharing");
        const string path = ".agents/memory/emerging/unlisted.md";
        workspace.WriteText(path, OpenForgeDocumentSeed.Metadata("Public unlisted note", ["Memory"], "# Note\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        workspace.ReplaceInstalledText(".agents/memory/emerging/_emerging.md",
            File.ReadAllText(workspace.Combine(".agents/memory/emerging/_emerging.md")).Replace("- [Public unlisted note](unlisted.md) - #Memory\n", string.Empty, StringComparison.Ordinal));
        var before = workspace.SnapshotHashes();
        var move = await CliHostCapture.RunAsync(["route", "move", "memory/emerging/unlisted", ".agents/memory/emerging/renamed.md", "--format", "json"], workspace.PhysicalPath);
        Assert.Equal(4, move.ExitCode);
        var remove = await CliHostCapture.RunAsync(["route", "remove", "memory/emerging/unlisted", "--automatic", "--format", "json"], workspace.PhysicalPath);
        Assert.Equal(4, remove.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "A second recognized host does not bypass shared-entrypoint ambiguity")]
    public async Task AmbiguousSharedEntrypointBlocksIndex()
    {
        using var workspace = InstallOperationWorkspace.Create("index-sharing-ambiguous");
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        const string extra = ".agents/memory/working/index.md";
        File.WriteAllText(workspace.Combine(extra), OpenForgeDocumentSeed.Metadata("Conflicting host", ["Memory"], "# Working\n\n## Entries\n"));
        try
        {
            var before = workspace.SnapshotHashes();
            var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(new(workspace.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        finally { File.Delete(workspace.Combine(extra)); }
    }

    [Fact(DisplayName = "Update preserves sharing records and does not reintroduce private Entries")]
    public async Task UpdateRetainsPolicyAndPrivateContents()
    {
        using var workspace = InstallOperationWorkspace.Create("update-sharing");
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Private note", ["Private"], "# Private body\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        var before = File.ReadAllBytes(workspace.Combine(PrivateNote));
        var policy = await Policy(workspace);
        workspace.ReplaceInstalledText(Working, File.ReadAllText(workspace.Combine(Working)) + "\n- [Private note](note.md) - #Private\n");
        var update = await CliHostCapture.RunAsync(["update", "--automatic", "--format", "json"], workspace.PhysicalPath);
        Assert.Equal(0, update.ExitCode);
        Assert.DoesNotContain("Private note", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(PrivateNote)));
        Assert.Equal(policy, await Policy(workspace));
    }

    [Fact(DisplayName = "Custom add remove and Gitignore restore the route sharing policy without deleting local notes")]
    public async Task AddRemoveAddKeepsLocalContent()
    {
        using var workspace = InstallOperationWorkspace.Create("install-sharing-cycle");
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Local note", ["Memory"], "# Local body\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        var before = File.ReadAllBytes(workspace.Combine(PrivateNote));
        foreach (var action in new[] { InstallRouteAction.Remove, InstallRouteAction.Add, InstallRouteAction.GitIgnore })
        {
            var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
                .ExecuteAsync(new(workspace.Workspace, InstallMode.Apply, false, true, false)
                { Setup = new(true, InstallPreset.Custom, [new("memory/working", action)]) }, TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(before, File.ReadAllBytes(workspace.Combine(PrivateNote)));
            Assert.True(workspace.Exists(Working));
            Assert.Equal(action == InstallRouteAction.GitIgnore ? 1 : 0, (await Policy(workspace)).Count);
        }
        Assert.DoesNotContain("Local note", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Create Update Move and Remove keep private notes out of shared Entries")]
    public async Task RouteWritersRespectSharing()
    {
        using var workspace = InstallOperationWorkspace.Create("route-sharing-writers");
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        const string moved = ".agents/memory/working/renamed.md";
        try
        {
            var create = await CliHostCapture.RunAsync(["route", "create", "memory/working/note", "--description", "Private note", "--tag", "Private", "--format", "json"], workspace.PhysicalPath);
            Assert.True(create.ExitCode == 0, create.Output);
            Assert.True(workspace.Exists(PrivateNote));
            Assert.DoesNotContain("note.md", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
            await AssertSharedNavigationCurrent(workspace);
            var update = await CliHostCapture.RunAsync(["route", "update", "memory/working/note", "--description", "Changed private note", "--format", "json"], workspace.PhysicalPath);
            Assert.Equal(0, update.ExitCode);
            Assert.DoesNotContain("Changed private note", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
            var move = await CliHostCapture.RunAsync(["route", "move", "memory/working/note", moved, "--format", "json"], workspace.PhysicalPath);
            Assert.True(move.ExitCode == 0, move.Output);
            Assert.True(workspace.Exists(moved));
            Assert.DoesNotContain("renamed.md", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
            await AssertSharedNavigationCurrent(workspace);
            var remove = await CliHostCapture.RunAsync(["route", "remove", "memory/working/renamed", "--automatic", "--format", "json"], workspace.PhysicalPath);
            Assert.Equal(0, remove.ExitCode);
            Assert.False(workspace.Exists(moved));
            Assert.DoesNotContain("Private note", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
            Assert.Equal(Working, Assert.Single(await Policy(workspace)).Entrypoint);
            await AssertSharedNavigationCurrent(workspace);
        }
        finally
        {
            if (File.Exists(workspace.Combine(PrivateNote))) File.Delete(workspace.Combine(PrivateNote));
            if (File.Exists(workspace.Combine(moved))) File.Delete(workspace.Combine(moved));
        }
    }

    [Fact(DisplayName = "Failed registration leaves deferred Gitignore unchanged after verified route restoration")]
    public async Task RegistrationFailureDoesNotApplyGitignore()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This write refusal requires Windows file sharing enforcement.");
        using var workspace = InstallOperationWorkspace.Create("install-sharing-lock-failure");
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        var before = File.ReadAllBytes(workspace.Combine(".gitignore"));
        using var held = new FileStream(workspace.Combine(InstallOperationWorkspace.OwnershipPath), FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(new(workspace.Workspace, InstallMode.Apply, false, true, false)
            { Setup = new(true, InstallPreset.FullCore, []) }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.True(workspace.Exists(".agents/guidance/_guidance.md"));
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(".gitignore")));
        Assert.Contains(result.Facts.Effects, effect => effect.Path == ".gitignore" && effect.Outcome == OpenForge.Cli.Core.Commands.Install.Models.Result.InstallEffectOutcome.NotStarted);
        Assert.Contains(result.Facts.Effects, effect => effect.Path == ".agents/guidance/_guidance.md" && effect.Outcome == OpenForge.Cli.Core.Commands.Install.Models.Result.InstallEffectOutcome.Verified);
    }

    [Fact(DisplayName = "Configuration registers shared entrypoints before applying Gitignore and repeat is quiet")]
    public async Task RegistrationPrecedesIgnoreAndQuietInstallPreservesPolicy()
    {
        using var workspace = InstallOperationWorkspace.Create("install-sharing-order");
        var result = await Install(workspace, configure: false);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var paths = result.Facts.Effects.Select(effect => effect.Path).ToArray();
        Assert.True(Array.IndexOf(paths, Working) < Array.IndexOf(paths, InstallOperationWorkspace.OwnershipPath));
        Assert.True(Array.IndexOf(paths, InstallOperationWorkspace.OwnershipPath) < Array.IndexOf(paths, ".gitignore"));
        var policy = await Policy(workspace);
        Assert.Equal(new SourceSharingRoute(".agents/memory/working", Working), Assert.Single(policy));
        var repeat = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Facts.Effects);
        Assert.Equal(policy, await Policy(workspace));
    }

    [Fact(DisplayName = "Configuration reuses one compatibility entrypoint and keeps private sources out of navigation")]
    public async Task CompatibilityEntrypointIsSharedWithoutCreatingCanonicalDuplicate()
    {
        using var workspace = InstallOperationWorkspace.Create("install-sharing-compatible");
        const string compatible = ".agents/memory/working/index.md";
        workspace.WriteText(compatible, OpenForgeDocumentSeed.Metadata("Team working rules", ["Memory"], "# Working\n\n## Axioms\n\n- Team rule.\n\n## Entries\n\n- stale\n"));
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Private note", ["Private"], "# Note\n"));
        var result = await Install(workspace, configure: true);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(workspace.Exists(Working));
        Assert.Equal(compatible, Assert.Single(await Policy(workspace)).Entrypoint);
        Assert.Contains($"!/{compatible}\n", File.ReadAllText(workspace.Combine(".gitignore")), StringComparison.Ordinal);
        var host = File.ReadAllText(workspace.Combine(compatible));
        Assert.Contains("- Team rule.", host, StringComparison.Ordinal);
        Assert.DoesNotContain("Private note", host, StringComparison.Ordinal);
        Assert.Contains("working/index.md", File.ReadAllText(workspace.Combine(".agents/memory/_memory.md")), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Index excludes private documents nested entrypoints and overwrites but local discovery reads them")]
    public async Task IndexPrivacyAndLocalDiscovery()
    {
        using var workspace = InstallOperationWorkspace.Create("index-sharing");
        workspace.WriteText(PrivateNote, OpenForgeDocumentSeed.Metadata("Private note", ["Private"], "# Private body\n"));
        const string nested = ".agents/memory/working/nested/_nested.md";
        workspace.WriteText(nested, OpenForgeDocumentSeed.Metadata("Private nested route", ["Private"], "# Nested\n\n## Entries\n\n- private stale\n"));
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        File.WriteAllText(workspace.Combine(".agents/memory/working/_working.overwrite.md"), OpenForgeDocumentSeed.Metadata("Private override", ["Private"], "# Local rules\n"));
        workspace.ReplaceInstalledText(Working, File.ReadAllText(workspace.Combine(Working)) + "\n- [Private note](note.md) - #Private\n");
        var privateBytes = File.ReadAllBytes(workspace.Combine(PrivateNote));
        var nestedBytes = File.ReadAllBytes(workspace.Combine(nested));
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(new(workspace.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Regions, region => region.Source.Path == Working);
        Assert.DoesNotContain(result.Regions, region => region.Source.Path == nested);
        Assert.DoesNotContain("Private", File.ReadAllText(workspace.Combine(Working)), StringComparison.Ordinal);
        Assert.Contains("working/_working.md", File.ReadAllText(workspace.Combine(".agents/memory/_memory.md")), StringComparison.Ordinal);
        Assert.Equal(privateBytes, File.ReadAllBytes(workspace.Combine(PrivateNote)));
        Assert.Equal(nestedBytes, File.ReadAllBytes(workspace.Combine(nested)));
        await AssertSharedNavigationCurrent(workspace, nested);
        var selected = await operation.ExecuteAsync(new(workspace.Workspace, ["memory/working/note"], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, selected.Status);
        Assert.Empty(selected.Regions);
        var find = await CliHostCapture.RunAsync(["find", "--tag", "Private", "--format", "json"], workspace.PhysicalPath);
        Assert.Equal(0, find.ExitCode);
        Assert.Contains(PrivateNote, find.Output, StringComparison.Ordinal);
        var context = await CliHostCapture.RunAsync(["context", "memory/working/note", "--format", "json"], workspace.PhysicalPath);
        Assert.Equal(0, context.ExitCode);
        Assert.Contains("Private body", context.Output, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Invalid sharing lock blocks Index without writes")]
    [InlineData("not json")]
    [InlineData("{\"framework\":{\"source\":{\"id\":\"framework\"},\"gitIgnoredRoutes\":[null]}}")]
    public async Task InvalidLockBlocks(string text)
    {
        using var workspace = InstallOperationWorkspace.Create("index-sharing-invalid");
        Assert.Equal(CliSemanticStatus.Complete, (await Install(workspace, configure: false)).Status);
        workspace.ReplaceInstalledText(InstallOperationWorkspace.OwnershipPath, text);
        var before = workspace.SnapshotHashes();
        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(new(workspace.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(IndexFindingCode.SourceUnsafe, Assert.Single(result.Findings).Code);
        var doctor = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        using var diagnosis = JsonDocument.Parse(doctor.Output);
        Assert.Equal("incomplete", diagnosis.RootElement.GetProperty("status").GetString());
        AssertNoSharedRowFindings(diagnosis.RootElement);
        var status = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        using var health = JsonDocument.Parse(status.Output);
        var working = Assert.Single(health.RootElement.GetProperty("data").GetProperty("entriesSections").EnumerateArray(),
            region => region.GetProperty("path").GetString() == Working);
        Assert.Equal("unavailable", working.GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task AssertSharedNavigationCurrent(InstallOperationWorkspace workspace, params string[] privateTargets)
    {
        var before = workspace.SnapshotHashes();
        var doctor = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.True(doctor.ExitCode == 0, doctor.Output);
        using var diagnosis = JsonDocument.Parse(doctor.Output);
        AssertNoSharedRowFindings(diagnosis.RootElement);
        var status = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        using var health = JsonDocument.Parse(status.Output);
        AssertWorkingCurrent(health.RootElement);
        var regions = health.RootElement.GetProperty("data").GetProperty("entriesSections").EnumerateArray();
        Assert.DoesNotContain(regions, region => privateTargets.Contains(region.GetProperty("path").GetString(), StringComparer.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void AssertNoSharedRowFindings(JsonElement result)
        => Assert.DoesNotContain(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() is "route.generated-region-stale" or "route.generated-entry-missing");

    private static void AssertWorkingCurrent(JsonElement result)
    {
        var working = Assert.Single(result.GetProperty("data").GetProperty("entriesSections").EnumerateArray(),
            region => region.GetProperty("path").GetString() == Working);
        Assert.Equal("current", working.GetProperty("state").GetString());
    }

    private static async Task<OpenForge.Cli.Core.Commands.Install.Models.Result.InstallResult> Install(InstallOperationWorkspace workspace, bool configure)
        => await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(new(workspace.Workspace, InstallMode.Apply, false, true, false)
            { Setup = new(configure, InstallPreset.Essentials, []) }, TestContext.Current.CancellationToken);

    private static async Task<IReadOnlyList<SourceSharingRoute>> Policy(InstallOperationWorkspace workspace)
    {
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        return Assert.IsType<FrameworkOwnership>(read.Document.Framework).GitIgnoredRoutes;
    }
}
