using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Selection;

public sealed class RouteListSourceReferenceParserTests
{
    [Fact(DisplayName = "Route-list source-reference parser classifies an omitted reference as Loader roots")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OmittedReferenceSelectsLoaderRoots()
    {
        var result = RouteListSourceReferenceParser.Parse(null);

        Assert.Equal(RouteListSourceReferenceParseState.Valid, result.State);
        Assert.Equal(RouteListSourceReferenceKind.LoaderRoots, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Route-list source-reference parser classifies every non-path value as an exact source ID"),
        InlineData("memory/crystallized", "memory/crystallized"),
        InlineData("src/file.md", "src/file.md"),
        InlineData("project alpha/工作.md", "project alpha/工作.md"),
        InlineData(".agents", ".agents")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void NonPathValuesRemainSourceIds(string input, string expectedId)
    {
        var result = RouteListSourceReferenceParser.Parse(input);

        Assert.Equal(RouteListSourceReferenceParseState.Valid, result.State);
        Assert.Equal(RouteListSourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(expectedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Route-list source-reference parser canonicalizes both exact path prefixes without decoding"),
        InlineData(".agents/memory/_memory.md", ".agents/memory/_memory.md"),
        InlineData("./.agents/project alpha/工作%20note.md", ".agents/project alpha/工作%20note.md"),
        InlineData(".agents/encoded%2Fname/file.md", ".agents/encoded%2Fname/file.md")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ExactPathsPreserveCanonicalText(string input, string expectedPath)
    {
        var result = RouteListSourceReferenceParser.Parse(input);

        Assert.Equal(RouteListSourceReferenceParseState.Valid, result.State);
        Assert.Equal(RouteListSourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Route-list source-reference parser rejects malformed IDs without normalization"),
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
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InvalidIdsRemainAttemptedIds(string attemptedId)
    {
        var result = RouteListSourceReferenceParser.Parse(attemptedId);

        Assert.Equal(RouteListSourceReferenceParseState.Invalid, result.State);
        Assert.Equal(RouteListSourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(attemptedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory(DisplayName = "Route-list source-reference parser rejects every unsafe exact-path segment while retaining its canonical attempt"),
        InlineData(".agents/", ".agents/"),
        InlineData("./.agents/", ".agents/"),
        InlineData(".agents//root.md", ".agents//root.md"),
        InlineData(".agents/./root.md", ".agents/./root.md"),
        InlineData(".agents/../root.md", ".agents/../root.md"),
        InlineData(".agents/root\\root.md", ".agents/root\\root.md"),
        InlineData(".agents/root/\0.md", ".agents/root/\0.md"),
        InlineData("./.agents/root/\u0001.md", ".agents/root/\u0001.md")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InvalidPathsRetainCanonicalAttemptedPath(string input, string expectedPath)
    {
        var result = RouteListSourceReferenceParser.Parse(input);

        Assert.Equal(RouteListSourceReferenceParseState.Invalid, result.State);
        Assert.Equal(RouteListSourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }
}
