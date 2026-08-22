using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteSourceIdentityListRegressionTests
{
    [Theory(DisplayName = "Route-list source identity derives the accepted automatic ID for every source form"),
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
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DeriveIdUsesCanonicalIdentityRules(string canonicalPath, string expectedId)
    {
        Assert.Equal(expectedId, RouteSourceIdentity.DeriveId(canonicalPath));
    }

    [Fact(DisplayName = "Route-list source identity rejects null, empty, outside, and traversal paths")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DeriveIdRejectsUnusablePaths()
    {
        Assert.Null(RouteSourceIdentity.DeriveId(null!));
        Assert.Null(RouteSourceIdentity.DeriveId(""));
        Assert.Null(RouteSourceIdentity.DeriveId("loader.md"));
        Assert.Null(RouteSourceIdentity.DeriveId("other/.agents/root.md"));
        Assert.Null(RouteSourceIdentity.DeriveId("./.agents/root.md"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents/"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents//root.md"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents/./root.md"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents/../root.md"));
        Assert.Null(RouteSourceIdentity.DeriveId(".agents/root\\root.md"));
    }
}
