using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceIdentityTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source identity derives IDs for every accepted source form"),
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
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DeriveIdPreservesTheAcceptedIdentity(
        string canonicalPath,
        string expectedId)
    {
        Assert.Equal(expectedId, SourceIdentity.DeriveId(canonicalPath));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source identity rejects unsafe and noncanonical paths"),
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
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DeriveIdDoesNotInventIdentityForUnsafePaths(string canonicalPath)
    {
        Assert.Null(SourceIdentity.DeriveId(canonicalPath));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source identity rejects a null path without inventing an ID")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void NullPathHasNoIdentity()
    {
        Assert.Null(SourceIdentity.DeriveId(null));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source identity validates exact IDs without normalization"),
        InlineData("memory/project alpha/工作%20note", true),
        InlineData("src/file.md", true),
        InlineData("", false),
        InlineData("root/", false),
        InlineData("root//child", false),
        InlineData("root/./child", false),
        InlineData("root/../child", false),
        InlineData("root\\child", false),
        InlineData("root/\0child", false)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void IsValidIdUsesTheExactGrammar(
        string id,
        bool expected)
    {
        Assert.Equal(expected, SourceIdentity.IsValidId(id));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source identity recognizes canonical and compatibility entrypoints only"),
        InlineData(".agents/root/_root.md", true),
        InlineData(".agents/root/index.md", true),
        InlineData(".agents/root/_index.md", true),
        InlineData(".agents/root/references.md", true),
        InlineData(".agents/root/_references.md", true),
        InlineData(".agents/root/SKILL.md", false),
        InlineData(".agents/root/leaf.md", false),
        InlineData(".agents/loader.md", false)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntrypointRecognitionIsFinite(
        string canonicalPath,
        bool expected)
    {
        Assert.Equal(expected, SourceIdentity.IsRecognizedEntrypointPath(canonicalPath));
    }
}
