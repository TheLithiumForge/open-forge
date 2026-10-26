using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Sources;

[Trait("Feature", "framework-payload-selection"), Trait("Evidence", "Unit")]
public sealed class FrameworkPayloadSelectionTests
{
    private const string CustomRoot = "handbook/context";
    private const string PackageGuide = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;
    private const string InstalledGuide = CustomRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;

    [Theory(DisplayName = "Installed exclusions remain literal while package exclusions follow the selected root"), Trait("Boundary", "Selection")]
    [InlineData(PackageGuide), InlineData(InstalledGuide)]
    public void DistinguishesInstalledAndPackagePaths(string removed)
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = Settings([removed]);

        Assert.Equal(removed != InstalledGuide, FrameworkPayloadSelection.IncludesPath(layout, PackageGuide, settings));
        Assert.Equal(removed != InstalledGuide, FrameworkPayloadSelection.IncludesInstalledPath(layout, InstalledGuide, settings));
        Assert.Equal(removed != PackageGuide, FrameworkPayloadSelection.IncludesInstalledPath(layout, PackageGuide, settings));
    }

    [Fact(DisplayName = "Installed directory exclusions use literal ordinal boundaries"), Trait("Boundary", "Selection")]
    public void InstalledDirectoryBoundaries()
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = Settings() with { RemovedDirectories = [WorkspaceLayout.DefaultMainRoot] };

        Assert.False(FrameworkPayloadSelection.IncludesInstalledPath(layout, PackageGuide, settings));
        Assert.True(FrameworkPayloadSelection.IncludesInstalledPath(layout, InstalledGuide, settings));
        Assert.True(FrameworkPayloadSelection.IncludesInstalledPath(layout, PackageGuide.ToUpperInvariant(), settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(layout, PackageGuide, settings));
    }

    [Fact(DisplayName = "Omitting a native Skill host suppresses dependent Framework resources without broadening exact removal"), Trait("Boundary", "Selection")]
    public void OmittedSkillSuppressesPackagedResources()
    {
        var settings = Settings([".agents/skills/cli/SKILL.md"]);
        const string resource = ".agents/skills/cli/references/commands.md";
        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, resource, settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/skills/cli/assets/example.json", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/skills/cli-extra/references/commands.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/skills/CLI/references/commands.md", settings));
        Assert.False(WorkspaceRemovals.IsPathRemoved(DefaultWorkspaceLayout, resource, settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "A removed file excludes only its exact concrete destination")]
    public void ExcludesExactDestinationOnly()
    {
        var settings = Settings([".agents/memory/working/_working.md"]);

        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/memory/working/_working.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/memory/working/local-note.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/memory/team/working/_working.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Removed file identity is case-sensitive")]
    public void MatchesCaseSensitively()
    {
        var settings = Settings([".agents/memory/working/_working.md"]);

        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/Memory/working/_working.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Removed root categories keep their existing prefix behavior")]
    public void RetainsCategoryExclusion()
    {
        var settings = Settings(categories: ["skills"]);

        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/skills", settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/skills/use-workflow/SKILL.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, ".agents/patterns/_patterns.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Framework payload selection honors removed directory boundaries")]
    public void ExcludesRemovedDirectoryAndDescendants()
    {
        var settings = Settings() with { RemovedDirectories = ["docs/archive"] };

        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, "docs/archive", settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, "docs/archive/guide.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(DefaultWorkspaceLayout, "docs/archived/guide.md", settings));
    }

    private static WorkspaceSettingsDocument Settings(
        ImmutableArray<string> files = default,
        ImmutableArray<string> categories = default)
        => WorkspaceSettingsDocument.Empty with
        {
            RemovedFiles = files.IsDefault ? [] : files,
            RemovedCategories = categories.IsDefault ? [] : categories,
        };

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(FrameworkPayloadSelectionTests)),
                Path.GetFullPath(nameof(FrameworkPayloadSelectionTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
