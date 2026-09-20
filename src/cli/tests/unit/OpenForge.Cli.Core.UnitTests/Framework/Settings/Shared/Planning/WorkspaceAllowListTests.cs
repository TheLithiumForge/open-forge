using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Planning;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceAllowListTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An entry admits itself and everything beneath it")]
    [InlineData("docs", "docs")]
    [InlineData("docs", "docs/shared.md")]
    [InlineData("docs", "docs/team/deep/note.md")]
    [InlineData("docs/shared.md", "docs/shared.md")]
    [InlineData("docs/team", "docs/team/note.md")]
    public void Admits(string entry, string path)
        => Assert.True(WorkspaceAllowList.Admits([entry], path));

    /// <summary>
    /// The boundary that matters. A sibling whose name merely starts with an
    /// admitted entry is a different directory, and admitting it would turn one
    /// authored entry into a prefix wildcard the author never wrote.
    /// </summary>
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "A sibling sharing a name prefix is not admitted")]
    [InlineData("docs", "docs-evil")]
    [InlineData("docs", "docs-evil/shared.md")]
    [InlineData("docs", "documents/note.md")]
    [InlineData("docs/team", "docs/teamwork/note.md")]
    [InlineData("docs/shared.md", "docs/shared.md.bak")]
    public void RejectsPrefixSiblings(string entry, string path)
        => Assert.False(WorkspaceAllowList.Admits([entry], path));

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An unrelated path is not admitted")]
    [InlineData("docs", "elsewhere")]
    [InlineData("docs", "elsewhere/note.md")]
    [InlineData("docs/team", "docs/note.md")]
    [InlineData("docs/shared.md", "docs")]
    public void RejectsUnrelated(string entry, string path)
        => Assert.False(WorkspaceAllowList.Admits([entry], path));

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Any one entry admits, and none admits nothing")]
    public void ConsidersEveryEntry()
    {
        ImmutableArray<string> allowed = ["tools", "docs", "site/public"];

        Assert.True(WorkspaceAllowList.Admits(allowed, "docs/a.md"));
        Assert.True(WorkspaceAllowList.Admits(allowed, "site/public/index.md"));
        Assert.False(WorkspaceAllowList.Admits(allowed, "site/private/index.md"));
        Assert.False(WorkspaceAllowList.Admits([], "docs/a.md"));
        Assert.False(WorkspaceAllowList.Admits(default, "docs/a.md"));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An empty or whitespace entry admits nothing")]
    [InlineData("")]
    [InlineData("   ")]
    public void IgnoresEmptyEntries(string entry)
    {
        Assert.False(WorkspaceAllowList.Admits([entry], "docs/a.md"));
        Assert.False(WorkspaceAllowList.Admits([entry], "a.md"));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "An absent path is never admitted")]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsEmptyPath(string path)
        => Assert.False(WorkspaceAllowList.Admits(["docs"], path));

    [Trait("Boundary", "Processing")]
    [Fact]
    public void EvaluationSharesPathsAcrossAllCallersAndDeduplicatesRequirements()
    {
        var result = WorkspaceAllowList.Evaluate(["docs"], ["docs/a.md", "other/a.md", "docs/a.md"]);
        Assert.Equal(WorkspacePermissionDecision.Required, result.Decision);
        Assert.Equal(["docs/a.md", "other/a.md"], result.Required);
        Assert.Equal(["other/a.md"], result.Missing);
        Assert.Equal(WorkspacePermissionDecision.Granted, WorkspaceAllowList.Evaluate(["docs"], ["docs/a.md"]).Decision);
        Assert.Equal(WorkspacePermissionDecision.NotRequired, WorkspaceAllowList.Evaluate([], []).Decision);
    }
}
