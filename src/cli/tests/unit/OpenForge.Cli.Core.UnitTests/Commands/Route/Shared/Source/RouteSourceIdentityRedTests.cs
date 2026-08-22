using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteSourceIdentityRedTests
{
    [Theory(DisplayName = "Shared source identity derives canonical IDs for every supported source form"),
        InlineData(".agents/loader.md", "loader"),
        InlineData(".agents/memory/_memory.md", "memory"),
        InlineData(".agents/memory/index.md", "memory"),
        InlineData(".agents/memory/_index.md", "memory"),
        InlineData(".agents/memory/references.md", "memory"),
        InlineData(".agents/memory/_references.md", "memory"),
        InlineData(".agents/skills/experience-design/SKILL.md", "skills/experience-design"),
        InlineData(".agents/memory/crystallized/documents/architecture.md", "memory/crystallized/documents/architecture"),
        InlineData(".agents/project alpha/工作 note.md", "project alpha/工作 note"),
        InlineData(".agents/project alpha/encoded%20name.md", "project alpha/encoded%20name"),
        InlineData(".agents/templates/prompt.yaml", "templates/prompt.yaml"),
        InlineData(".agents/guidance/style.overwrite.md", "guidance/style")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DeriveIdPreservesTheAcceptedIdentity(string canonicalPath, string expectedId)
    {
        Assert.Equal(expectedId, RouteSourceIdentity.DeriveId(canonicalPath));
    }

    [Theory(DisplayName = "Shared source identity rejects null, traversal, controls, and noncanonical paths"),
        InlineData(""),
        InlineData("loader.md"),
        InlineData("other/.agents/root.md"),
        InlineData("./.agents/root.md"),
        InlineData(".agents"),
        InlineData(".agents/"),
        InlineData(".agents//root.md"),
        InlineData(".agents/./root.md"),
        InlineData(".agents/../root.md"),
        InlineData(".agents/root\\root.md"),
        InlineData(".agents/root/\u001F.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DeriveIdReturnsNoIdentityForUnsafePaths(string path)
    {
        Assert.Null(RouteSourceIdentity.DeriveId(path));
    }

    [Fact(DisplayName = "Shared source identity rejects a null path without inventing an ID")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void NullPathHasNoIdentity()
    {
        Assert.Null(RouteSourceIdentity.DeriveId(null));
    }

    [Theory(DisplayName = "Shared source identity validates exact IDs without case or percent normalization"),
        InlineData("memory/project alpha/工作%20note", true),
        InlineData("src/file.md", true),
        InlineData("", false),
        InlineData("root/", false),
        InlineData("root//child", false),
        InlineData("root/./child", false),
        InlineData("root/../child", false),
        InlineData("root\\child", false),
        InlineData("root/\0child", false)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IsValidIdUsesTheExactGrammar(string id, bool expected)
    {
        Assert.Equal(expected, RouteSourceIdentity.IsValidId(id));
    }

    [Theory(DisplayName = "Shared source identity recognizes canonical and compatibility entrypoints only"),
        InlineData(".agents/root/_root.md", true),
        InlineData(".agents/root/index.md", true),
        InlineData(".agents/root/_index.md", true),
        InlineData(".agents/root/references.md", true),
        InlineData(".agents/root/_references.md", true),
        InlineData(".agents/root/SKILL.md", false),
        InlineData(".agents/root/leaf.md", false),
        InlineData(".agents/loader.md", false)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void EntrypointRecognitionIsFinite(string canonicalPath, bool expected)
    {
        Assert.Equal(expected, RouteSourceIdentity.IsRecognizedEntrypointPath(canonicalPath));
    }
}
