using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceIdentityListRegressionTests
{
    [Theory(DisplayName = "Neutral source identity derives the accepted automatic ID for every source form"),
        InlineData(".agents/loader.md", "loader"),
        InlineData(".agents/memory/_memory.md", "memory"),
        InlineData(".agents/memory/index.md", "memory"),
        InlineData(".agents/memory/_index.md", "memory"),
        InlineData(".agents/memory/references.md", "memory"),
        InlineData(".agents/memory/_references.md", "memory"),
        InlineData(".agents/skills/experience-design/SKILL.md", "skills/experience-design"),
        InlineData(".agents/memory/crystallized/documents/architecture.md", "memory/crystallized/documents/architecture"),
        InlineData(".agents/templates/prompt.yaml", "templates/prompt.yaml"),
        InlineData(".agents/project alpha/工作 note.md", "project alpha/工作 note"),
        InlineData(".agents/project alpha/encoded%20name.md", "project alpha/encoded%20name"),
        InlineData(".agents/guidance/style.overwrite.md", "guidance/style"),
        InlineData(".agents/memory/_memory.overwrite.md", "memory")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DeriveIdUsesCanonicalIdentityRules(string canonicalPath, string expectedId)
    {
        Assert.Equal(expectedId, SourceIdentity.DeriveId(canonicalPath));
    }

    [Fact(DisplayName = "Neutral source identity rejects null, empty, outside, and traversal paths")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DeriveIdRejectsUnusablePaths()
    {
        Assert.Null(SourceIdentity.DeriveId(null));
        Assert.Null(SourceIdentity.DeriveId(""));
        Assert.Null(SourceIdentity.DeriveId("loader.md"));
        Assert.Null(SourceIdentity.DeriveId("other/.agents/root.md"));
        Assert.Null(SourceIdentity.DeriveId("./.agents/root.md"));
        Assert.Null(SourceIdentity.DeriveId(".agents"));
        Assert.Null(SourceIdentity.DeriveId(".agents/"));
        Assert.Null(SourceIdentity.DeriveId(".agents//root.md"));
        Assert.Null(SourceIdentity.DeriveId(".agents/./root.md"));
        Assert.Null(SourceIdentity.DeriveId(".agents/../root.md"));
        Assert.Null(SourceIdentity.DeriveId(".agents/root\\root.md"));
    }
}
