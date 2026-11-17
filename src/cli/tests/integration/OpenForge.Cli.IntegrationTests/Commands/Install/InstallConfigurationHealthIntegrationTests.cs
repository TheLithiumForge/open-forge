using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallConfigurationHealthIntegrationTests
{
    private const string SkillPath = ".agents/skills/open-forge-cli/SKILL.md";
    private static readonly string[] SkillResources =
    [
        ".agents/skills/open-forge-cli/references/common.md",
        ".agents/skills/open-forge-cli/references/discovery.md",
        ".agents/skills/open-forge-cli/references/packages.md",
        ".agents/skills/open-forge-cli/references/routes.md",
    ];

    [Fact(DisplayName = "Omitted native Skill resources are not installed and existing user files remain untouched")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task OmittedSkillPreservesExistingUserFiles()
    {
        using var workspace = InstallOperationWorkspace.Create("omitted-skill-user-files");
        const string userFile = ".agents/skills/open-forge-cli/user-notes.txt";
        workspace.WriteText(userFile, "Keep my local notes.\n");
        workspace.WriteText(".agents/open-forge.json", """{"removedFiles":[".agents/skills/open-forge-cli/SKILL.md"]}""");
        var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("Keep my local notes.\n", File.ReadAllText(workspace.Combine(userFile)));
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path.StartsWith(".agents/skills/open-forge-cli/", StringComparison.Ordinal));
        Assert.All(SkillResources, path => Assert.False(workspace.Exists(path), path));
    }

    [Theory(DisplayName = "Essentials setup keeps full Doctor, Status and Context healthy with a supplied or intentionally omitted CLI Skill"), InlineData(false), InlineData(true)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task EssentialsHealth(bool omitSkill)
    {
        using var workspace = InstallOperationWorkspace.Create("install-essentials-health");
        if (omitSkill) workspace.WriteText(".agents/open-forge.json", """{"removedFiles":[".agents/skills/open-forge-cli/SKILL.md"]}""");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var request = new InstallRequest(workspace.Workspace, InstallMode.Apply, false, true, false)
        { Setup = new(false, InstallPreset.Essentials, []) };
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(!omitSkill, workspace.Exists(SkillPath));
        await AssertHealthyAsync(workspace, omitSkill);
    }

    [Theory(DisplayName = "Configured copied Working scaffolding composes clean Doctor, Status and Context and repeats without writes")]
    [InlineData(true, false), InlineData(false, false), InlineData(true, true), InlineData(false, true)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task CopiedWorkingHealth(bool retainLock, bool omitSkill)
    {
        using var workspace = InstallOperationWorkspace.Create("install-copied-working-health");
        if (omitSkill) workspace.WriteText(".agents/open-forge.json", """{"removedFiles":[".agents/skills/open-forge-cli/SKILL.md"]}""");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var initial = new InstallRequest(workspace.Workspace, InstallMode.Apply, false, true, false)
        { Setup = new(false, InstallPreset.Essentials, []) };
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(initial, TestContext.Current.CancellationToken)).Status);
        File.Delete(workspace.Combine(".agents/memory/working/_working.md"));
        if (!retainLock) File.Delete(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var configure = new InstallRequest(workspace.Workspace, InstallMode.Apply, false, true, false)
        { Setup = new(true, InstallPreset.Custom, []) };
        var restored = await operation.ExecuteAsync(configure, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, restored.Status);
        Assert.True(workspace.Exists(".agents/memory/working/_working.md"));
        Assert.Equal(!omitSkill, workspace.Exists(SkillPath));
        await AssertHealthyAsync(workspace, omitSkill);
        var before = workspace.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(configure, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task AssertHealthyAsync(InstallOperationWorkspace workspace, bool omitSkill)
    {
        var before = workspace.SnapshotHashes();
        var doctor = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.True(doctor.ExitCode == 0, doctor.Output + doctor.Error);
        using var diagnosis = JsonDocument.Parse(doctor.Output);
        Assert.All(diagnosis.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray(), category =>
            Assert.Equal("complete", category.GetProperty("coverage").GetString()));
        Assert.DoesNotContain(diagnosis.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("severity").GetString() is "warning" or "error");
        var status = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.True(status.ExitCode == 0, status.Output + status.Error);
        using var observed = JsonDocument.Parse(status.Output);
        Assert.Equal("completed", observed.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(observed.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("severity").GetString() is "warning" or "error");
        var context = await CliHostCapture.RunAsync(["context", "skills", "memory/working", "--follow-links=all", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.True(context.ExitCode == 0, context.Output + context.Error);
        using var selected = JsonDocument.Parse(context.Output);
        var sources = selected.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray().ToArray();
        Assert.Contains(sources, source => source.GetProperty("path").GetString() == ".agents/skills/_skills.md");
        Assert.Contains(sources, source => source.GetProperty("path").GetString() == ".agents/memory/working/_working.md");
        if (omitSkill)
        {
            Assert.All(SkillResources, path => Assert.False(workspace.Exists(path), path));
            Assert.DoesNotContain(sources, source => SkillResources.Contains(source.GetProperty("path").GetString(), StringComparer.Ordinal));
            Assert.DoesNotContain(sources, source => source.GetProperty("path").GetString() == SkillPath);
            using var settings = JsonDocument.Parse(File.ReadAllText(workspace.Combine(".agents/open-forge.json")));
            Assert.Contains(settings.RootElement.GetProperty("removedFiles").EnumerateArray(), path => path.GetString() == SkillPath);
            Assert.DoesNotContain("[open-forge-cli/SKILL.md]", File.ReadAllText(workspace.Combine(".agents/skills/_skills.md")), StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
