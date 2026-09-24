using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Sources;

[Trait("Feature", "framework-payload-selection"), Trait("Evidence", "Unit")]
public sealed class FrameworkPayloadSelectionTests
{
    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "A removed file excludes only its exact concrete destination")]
    public void ExcludesExactDestinationOnly()
    {
        var settings = Settings([".agents/memory/working/_working.md"]);

        Assert.False(FrameworkPayloadSelection.IncludesPath(".agents/memory/working/_working.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(".agents/memory/working/local-note.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(".agents/memory/team/working/_working.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Removed file identity is case-sensitive")]
    public void MatchesCaseSensitively()
    {
        var settings = Settings([".agents/memory/working/_working.md"]);

        Assert.True(FrameworkPayloadSelection.IncludesPath(".agents/Memory/working/_working.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Removed root categories keep their existing prefix behavior")]
    public void RetainsCategoryExclusion()
    {
        var settings = Settings(categories: ["skills"]);

        Assert.False(FrameworkPayloadSelection.IncludesPath(".agents/skills", settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath(".agents/skills/use-workflow/SKILL.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(".agents/patterns/_patterns.md", settings));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "Framework payload selection honors removed directory boundaries")]
    public void ExcludesRemovedDirectoryAndDescendants()
    {
        var settings = Settings() with { RemovedDirectories = ["docs/archive"] };

        Assert.False(FrameworkPayloadSelection.IncludesPath("docs/archive", settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath("docs/archive/guide.md", settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath("docs/archived/guide.md", settings));
    }

    private static WorkspaceSettingsDocument Settings(
        ImmutableArray<string> files = default,
        ImmutableArray<string> categories = default)
        => WorkspaceSettingsDocument.Empty with
        {
            RemovedFiles = files.IsDefault ? [] : files,
            RemovedCategories = categories.IsDefault ? [] : categories,
        };
}
