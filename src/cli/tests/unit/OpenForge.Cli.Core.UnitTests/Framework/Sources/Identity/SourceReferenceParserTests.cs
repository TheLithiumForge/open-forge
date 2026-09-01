using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceReferenceParserTests
{
    [Theory(DisplayName = "Neutral source-reference parsing preserves exact valid IDs"),
        InlineData("memory/project alpha/工作%20note"),
        InlineData("src/file.md"),
        InlineData(".agents"),
        InlineData("percent%2Fsegment")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ExactIdsRemainUnchanged(string input)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Valid, result.State);
        Assert.Equal(SourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(input, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Neutral source-reference parsing canonicalizes both accepted exact-path prefixes"),
        InlineData(".agents/memory/project alpha/工作%20note.md", ".agents/memory/project alpha/工作%20note.md"),
        InlineData("./.agents/memory/project alpha/工作%20note.md", ".agents/memory/project alpha/工作%20note.md"),
        InlineData(".agents/encoded%2Fname/file.md", ".agents/encoded%2Fname/file.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ExactPathsPreserveUnicodeSpacesAndPercentText(
        string input,
        string expectedPath)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Valid, result.State);
        Assert.Equal(SourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Neutral source-reference parsing retains malformed ID attempts and causes"),
        InlineData(""),
        InlineData("root/"),
        InlineData("root//child"),
        InlineData("root/./child"),
        InlineData("root/../child"),
        InlineData("root\\child"),
        InlineData("root/\0child"),
        InlineData("root/\u001Fchild")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void InvalidIdsRetainTheirAttempt(string attemptedId)
    {
        var result = SourceReferenceParser.Parse(attemptedId);

        Assert.Equal(SourceReferenceParseState.Invalid, result.State);
        Assert.Equal(SourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(attemptedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory(DisplayName = "Neutral source-reference parsing rejects malformed exact paths after prefix canonicalization"),
        InlineData(".agents/", ".agents/"),
        InlineData("./.agents/", ".agents/"),
        InlineData(".agents//root.md", ".agents//root.md"),
        InlineData(".agents/./root.md", ".agents/./root.md"),
        InlineData(".agents/../root.md", ".agents/../root.md"),
        InlineData(".agents/root\\root.md", ".agents/root\\root.md"),
        InlineData("./.agents/root/\u0001.md", ".agents/root/\u0001.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void InvalidPathsRetainCanonicalAttemptedPath(
        string input,
        string expectedPath)
    {
        var result = SourceReferenceParser.Parse(input);

        Assert.Equal(SourceReferenceParseState.Invalid, result.State);
        Assert.Equal(SourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }
}
