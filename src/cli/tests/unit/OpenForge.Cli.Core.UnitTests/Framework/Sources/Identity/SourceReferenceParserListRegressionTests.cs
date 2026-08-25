using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceReferenceParserListRegressionTests
{
    [Theory(DisplayName = "Neutral source-reference parser classifies every non-path value as an exact source ID"),
        InlineData("memory/crystallized", "memory/crystallized"),
        InlineData("src/file.md", "src/file.md"),
        InlineData("project alpha/工作.md", "project alpha/工作.md"),
        InlineData(".agents", ".agents")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void NonPathValuesRemainSourceIds(string input, string expectedId)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Valid, result.State);
        Assert.Equal(SourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(expectedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Neutral source-reference parser canonicalizes both exact path prefixes without decoding"),
        InlineData(".agents/memory/_memory.md", ".agents/memory/_memory.md"),
        InlineData("./.agents/project alpha/工作%20note.md", ".agents/project alpha/工作%20note.md"),
        InlineData(".agents/encoded%2Fname/file.md", ".agents/encoded%2Fname/file.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ExactPathsPreserveCanonicalText(string input, string expectedPath)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Valid, result.State);
        Assert.Equal(SourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Neutral source-reference parser rejects malformed IDs without normalization"),
        InlineData(""),
        InlineData("/root"),
        InlineData("root/"),
        InlineData("root//child"),
        InlineData("root/./child"),
        InlineData("root/../child"),
        InlineData("root\\child"),
        InlineData("root/\0child"),
        InlineData("root/\u0001child"),
        InlineData("root/\u001Fchild")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void InvalidIdsRemainAttemptedIds(string attemptedId)
    {
        var result = SourceReferenceParser.Parse(attemptedId);

        Assert.Equal(SourceReferenceParseState.Invalid, result.State);
        Assert.Equal(SourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(attemptedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory(DisplayName = "Neutral source-reference parser rejects every unsafe exact-path segment while retaining its canonical attempt"),
        InlineData(".agents/", ".agents/"),
        InlineData("./.agents/", ".agents/"),
        InlineData(".agents//root.md", ".agents//root.md"),
        InlineData(".agents/./root.md", ".agents/./root.md"),
        InlineData(".agents/../root.md", ".agents/../root.md"),
        InlineData(".agents/root\\root.md", ".agents/root\\root.md"),
        InlineData(".agents/root/\0.md", ".agents/root/\0.md"),
        InlineData("./.agents/root/\u0001.md", ".agents/root/\u0001.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void InvalidPathsRetainCanonicalAttemptedPath(string input, string expectedPath)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Invalid, result.State);
        Assert.Equal(SourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }
}
