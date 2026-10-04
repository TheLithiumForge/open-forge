using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

[Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitCanonicalRestorationIntegrationTests
{
    [Theory(DisplayName = "Canonical restoration adds a missing Core root or Memory state and repeats as a verified no-op")]
    [InlineData("patterns"), InlineData("skills"), InlineData("memory/archived")]
    public async Task RestoresSelectedPayloadAndConverges(string target)
    {
        using var workspace = await TrustedAsync();
        var directory = ".agents/" + target;
        workspace.Delete(directory);
        var before = workspace.SnapshotHashes();
        var preview = await ExecuteAsync(workspace, target, RouteInitMode.DryRun);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.NotEmpty(preview.Effects);
        var applied = await ExecuteAsync(workspace, target);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteInitVerificationState.Verified, applied.Verification);
        Assert.Equal(preview.Effects.Select(effect => (effect.Path, effect.Kind, effect.Action)),
            applied.Effects.Select(effect => (effect.Path, effect.Kind, effect.Action)));
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload;
        Assert.NotNull(payload);
        foreach (var asset in payload.Assets.Where(asset => asset.Path.StartsWith(directory + "/", StringComparison.Ordinal)))
        {
            Assert.True(workspace.Exists(asset.Path), asset.Path);
            if (!SourceFormClassifier.TryClassify(asset.Path, out var form) || !SourceFormClassifier.IsEntrypoint(form))
            {
                Assert.Equal(asset.Bytes.ToArray(), await File.ReadAllBytesAsync(workspace.Combine(asset.Path), TestContext.Current.CancellationToken));
            }
        }
        if (target == "skills")
        {
            Assert.Contains(applied.Effects, effect => effect.Kind == RouteInitEffectKind.Payload
                && effect.Path == ".agents/skills/open-forge-cli/SKILL.md");
            Assert.DoesNotContain(applied.Entrypoints, entry => entry.Path.EndsWith("SKILL.md", StringComparison.Ordinal));
            Assert.Contains("open-forge-cli/SKILL.md", workspace.ReadText(".agents/skills/_skills.md"), StringComparison.Ordinal);
        }
        var after = workspace.SnapshotHashes();
        var repeated = await ExecuteAsync(workspace, target);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Canonical restoration clears exact exclusions and retains descendant omissions and unknown settings")]
    public async Task ClearsOnlySelectedExclusions()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/skills");
        workspace.WriteText(".agents/open-forge.json", """
            {"future":{"keep":true},"removedCategories":["skills","templates"],
             "removedDirectories":[".agents/skills",".agents/skills/private"],
             "removedFiles":[".agents/skills/_skills.md",".agents/skills/open-forge-cli/SKILL.md"]}
            """);
        var result = await ExecuteAsync(workspace, "skills");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Effects, effect => effect.Kind == RouteInitEffectKind.Settings);
        Assert.False(workspace.Exists(".agents/skills/open-forge-cli/SKILL.md"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.True(settings.RootElement.GetProperty("future").GetProperty("keep").GetBoolean());
        Assert.Equal(["templates"], settings.RootElement.GetProperty("removedCategories").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/skills/private"], settings.RootElement.GetProperty("removedDirectories").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/skills/open-forge-cli/SKILL.md"], settings.RootElement.GetProperty("removedFiles").EnumerateArray().Select(value => value.GetString()));
    }

    [Fact(DisplayName = "Restoring initially omitted Skills retains an unowned CLI Skill omission with healthy Doctor and Context")]
    public async Task OmittedSkillRestorationLeavesHealthyContext()
    {
        using var workspace = RouteInitFrameworkIntegrationWorkspace.CreateEmpty("omitted-skills-restoration-health");
        workspace.WriteText(".agents/open-forge.json", """
            {"removedCategories":["skills"],"removedFiles":[".agents/skills/open-forge-cli/SKILL.md"]}
            """);
        var installed = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.InstallRequest(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);
        Assert.False(workspace.Exists(".agents/skills"));
        var originalOwnership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain(".agents/skills/open-forge-cli/SKILL.md", originalOwnership.Document.Framework?.Paths ?? []);

        var restored = await ExecuteAsync(workspace, "skills");
        Assert.Equal(CliSemanticStatus.Complete, restored.Status);
        Assert.False(workspace.Exists(".agents/skills/open-forge-cli/SKILL.md"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Empty(settings.RootElement.GetProperty("removedCategories").EnumerateArray());
        Assert.Equal([".agents/skills/open-forge-cli/SKILL.md"],
            settings.RootElement.GetProperty("removedFiles").EnumerateArray().Select(value => value.GetString()));

        var skills = workspace.ReadText(".agents/skills/_skills.md");
        Assert.Contains("When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads", skills, StringComparison.Ordinal);
        Assert.Contains("Respect an intentional omission and use the loader's CLI summary instead.", skills, StringComparison.Ordinal);
        Assert.DoesNotContain(new MarkdownDocumentParser().Parse(skills).Links,
            link => link.RawDestination.Contains("open-forge-cli/SKILL.md", StringComparison.Ordinal));

        var beforeReadOnlyCommands = workspace.SnapshotHashes();
        var doctor = await CliHostCapture.RunAsync(
            ["doctor", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.Equal(0, doctor.ExitCode);
        Assert.Empty(doctor.Error);
        using var diagnosis = JsonDocument.Parse(doctor.Output);
        Assert.Equal("completed", diagnosis.RootElement.GetProperty("status").GetString());
        Assert.All(diagnosis.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray(), category =>
            Assert.Equal("complete", category.GetProperty("coverage").GetString()));
        Assert.DoesNotContain(diagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("severity").GetString() is "warning" or "error");

        var context = await CliHostCapture.RunAsync(
            ["context", "skills", "--follow-links=all", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.Equal(0, context.ExitCode);
        Assert.Empty(context.Error);
        using var contextDocument = JsonDocument.Parse(context.Output);
        Assert.Equal("completed", contextDocument.RootElement.GetProperty("status").GetString());
        var sources = contextDocument.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray().ToArray();
        Assert.Contains(sources, source => source.GetProperty("path").GetString() == ".agents/skills/_skills.md");
        Assert.DoesNotContain(sources, source => source.GetProperty("path").GetString() == ".agents/skills/open-forge-cli/SKILL.md");
        Assert.Equal(beforeReadOnlyCommands, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/skills/open-forge-cli/SKILL.md"));
    }

    [Fact(DisplayName = "A broader excluded ancestor blocks child restoration without enabling its siblings or writing effects")]
    public async Task BroaderExcludedAncestorBlocks()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/memory");
        workspace.WriteText(".agents/open-forge.json", "{\"removedCategories\":[\"memory\"]}");
        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(workspace, "memory/archived");
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Contains("route init memory --framework", Assert.Single(result.Findings).Cause, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Restoration preserves existing edited payload bytes and overwrite companions without adopting them")]
    public async Task ExistingOccupantsRemainUserOwned()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/open-forge.lock.json");
        workspace.WriteText(".agents/skills/open-forge-cli/SKILL.md", """
            ---
            name: open-forge-cli
            description: My edited CLI skill
            ---
            My authored skill body.
            """);
        workspace.WriteText(".agents/skills/_skills.overwrite.md", "My skills override.\n");
        var edited = workspace.ReadText(".agents/skills/open-forge-cli/SKILL.md");
        var result = await ExecuteAsync(workspace, "skills");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(edited, workspace.ReadText(".agents/skills/open-forge-cli/SKILL.md"));
        Assert.Equal("My skills override.\n", workspace.ReadText(".agents/skills/_skills.overwrite.md"));
        Assert.All(result.Effects, effect => Assert.Equal(RouteInitEffectKind.GeneratedRegion, effect.Kind));
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.Empty(ownership.Document.Framework?.Paths ?? []);
        Assert.DoesNotContain(ownership.Document.Framework?.Regions ?? [], region =>
            region.Path == ".agents/skills/open-forge-cli/SKILL.md" || region.Path.EndsWith(".overwrite.md", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Restoring a Memory state adds only necessary ancestors when its root is absent")]
    public async Task MissingMemoryRootDoesNotRestoreSiblingStates()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/memory");
        var result = await ExecuteAsync(workspace, "memory/archived");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(workspace.Exists(".agents/memory/_memory.md"));
        Assert.True(workspace.Exists(".agents/memory/archived/_archived.md"));
        Assert.False(workspace.Exists(".agents/memory/working"));
        Assert.False(workspace.Exists(".agents/memory/crystallized"));
        Assert.Contains("archived/_archived.md", workspace.ReadText(".agents/memory/_memory.md"), StringComparison.Ordinal);
        Assert.DoesNotContain("working/_working.md", workspace.ReadText(".agents/memory/_memory.md"), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Explicit Memory root restoration retains excluded descendant state omissions")]
    public async Task RootRestorationRetainsDescendantStateOmissions()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/memory");
        workspace.WriteText(".agents/open-forge.json", "{\"removedCategories\":[\"memory\"],\"removedDirectories\":[\".agents/memory/archived\"]}");
        var result = await ExecuteAsync(workspace, "memory");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.True(workspace.Exists(".agents/memory/crystallized/_crystallized.md"));
        Assert.False(workspace.Exists(".agents/memory/archived"));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Empty(settings.RootElement.GetProperty("removedCategories").EnumerateArray());
        Assert.Equal(".agents/memory/archived", Assert.Single(settings.RootElement.GetProperty("removedDirectories").EnumerateArray()).GetString());
    }

    [Fact(DisplayName = "An orphan overwrite companion blocks restoration and preserves its exact bytes")]
    public async Task OrphanCompanionIsBlockedAndPreserved()
    {
        using var workspace = await TrustedAsync();
        workspace.WriteText(".agents/skills/_skills.overwrite.md", "My orphaned skills override.\n");
        workspace.Delete(".agents/skills/_skills.md");
        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(workspace, "skills");
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Contains("overwrite companion", Assert.Single(result.Findings).Cause, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Restoration refuses malformed settings and absent Loader without effects")]
    public async Task UnsafeRequiredInputsBlock()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/patterns");
        workspace.WriteText(".agents/open-forge.json", "{\"removedFiles\":false}");
        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(workspace, "patterns");
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.Delete(".agents/open-forge.json");
        workspace.Delete(".agents/loader.md");
        before = workspace.SnapshotHashes();
        result = await ExecuteAsync(workspace, "patterns");
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Restoration revalidation rejects changed settings and preserved payload snapshots")]
    public async Task StaleSnapshotsInvalidateCompletePlan()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/skills/_skills.md");
        var builder = new RouteInitPlanBuilder();
        var build = await builder.BuildAsync(workspace.Request("skills"), TestContext.Current.CancellationToken);
        var plan = build.Plan;
        Assert.NotNull(plan);
        workspace.WriteText(".agents/open-forge.json", "{\"future\":true}");
        var revalidated = await new RouteInitPlanRevalidator(builder).RevalidateAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInitPlanRevalidationState.Changed, revalidated.State);
        workspace.Delete(".agents/open-forge.json");
        workspace.WriteText(".agents/skills/open-forge-cli/SKILL.md", workspace.ReadText(".agents/skills/open-forge-cli/SKILL.md") + "\nChanged body.\n");
        revalidated = await new RouteInitPlanRevalidator(builder).RevalidateAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInitPlanRevalidationState.Changed, revalidated.State);
    }

    [Fact(DisplayName = "Restoration revalidation rejects changed paired overwrite bytes")]
    public async Task ChangedCompanionInvalidatesPlan()
    {
        using var workspace = await TrustedAsync();
        workspace.Delete(".agents/patterns");
        workspace.WriteText(".agents/skills/_skills.overwrite.md", "Original companion.\n");
        var builder = new RouteInitPlanBuilder();
        var plan = (await builder.BuildAsync(workspace.Request("patterns"), TestContext.Current.CancellationToken)).Plan;
        Assert.NotNull(plan);
        workspace.WriteText(".agents/skills/_skills.overwrite.md", "Changed companion.\n");
        var result = await new RouteInitPlanRevalidator(builder).RevalidateAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInitPlanRevalidationState.Changed, result.State);
        Assert.False(workspace.Exists(".agents/patterns"));
    }

    private static Task<RouteInitFrameworkIntegrationWorkspace> TrustedAsync()
        => RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync("canonical-restoration", TestContext.Current.CancellationToken);

    private static ValueTask<RouteInitResult> ExecuteAsync(RouteInitFrameworkIntegrationWorkspace workspace, string target, RouteInitMode mode = RouteInitMode.Apply)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request(target, mode), TestContext.Current.CancellationToken);
}
