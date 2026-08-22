using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteSourceReferenceParserRedTests
{
    [Theory(DisplayName = "Shared source-reference parser preserves exact IDs without path normalization"),
        InlineData("memory/project alpha/工作%20note", "memory/project alpha/工作%20note"),
        InlineData("src/file.md", "src/file.md"),
        InlineData(".agents", ".agents"),
        InlineData("percent%2Fsegment", "percent%2Fsegment")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExactIdsRemainUnchanged(string input, string expectedId)
    {
        var result = RouteSourceReferenceParser.Parse(input);

        Assert.Equal(RouteSourceReferenceParseState.Valid, result.State);
        Assert.Equal(RouteSourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(expectedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Shared source-reference parser canonicalizes both accepted exact-path prefixes"),
        InlineData(".agents/memory/project alpha/工作%20note.md", ".agents/memory/project alpha/工作%20note.md"),
        InlineData("./.agents/memory/project alpha/工作%20note.md", ".agents/memory/project alpha/工作%20note.md"),
        InlineData(".agents/encoded%2Fname/file.md", ".agents/encoded%2Fname/file.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExactPathsPreserveUnicodeSpacesAndPercentText(string input, string expectedPath)
    {
        var result = RouteSourceReferenceParser.Parse(input);

        Assert.Equal(RouteSourceReferenceParseState.Valid, result.State);
        Assert.Equal(RouteSourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Shared source-reference parser rejects malformed and traversal IDs"),
        InlineData(""),
        InlineData("root/"),
        InlineData("root//child"),
        InlineData("root/./child"),
        InlineData("root/../child"),
        InlineData("root\\child"),
        InlineData("root/\0child"),
        InlineData("root/\u001Fchild")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InvalidIdsRetainTheirAttempt(string attemptedId)
    {
        var result = RouteSourceReferenceParser.Parse(attemptedId);

        Assert.Equal(RouteSourceReferenceParseState.Invalid, result.State);
        Assert.Equal(RouteSourceReferenceKind.SourceId, result.Kind);
        Assert.Equal(attemptedId, result.AttemptedId);
        Assert.Null(result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory(DisplayName = "Shared source-reference parser rejects malformed exact paths after prefix canonicalization"),
        InlineData(".agents/", ".agents/"),
        InlineData("./.agents/", ".agents/"),
        InlineData(".agents//root.md", ".agents//root.md"),
        InlineData(".agents/./root.md", ".agents/./root.md"),
        InlineData(".agents/../root.md", ".agents/../root.md"),
        InlineData(".agents/root\\root.md", ".agents/root\\root.md"),
        InlineData("./.agents/root/\u0001.md", ".agents/root/\u0001.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InvalidPathsRetainCanonicalAttemptedPath(string input, string expectedPath)
    {
        var result = RouteSourceReferenceParser.Parse(input);

        Assert.Equal(RouteSourceReferenceParseState.Invalid, result.State);
        Assert.Equal(RouteSourceReferenceKind.SourcePath, result.Kind);
        Assert.Null(result.AttemptedId);
        Assert.Equal(expectedPath, result.AttemptedPath);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Shared source-reference parser rejects a null value instead of selecting Loader roots")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void NullIsNotTheLoaderPolicy()
    {
        Assert.Throws<ArgumentNullException>(() => RouteSourceReferenceParser.Parse(null!));
    }

    [Theory(DisplayName = "Shared source-reference parser leaves Loader policy to the command resolver"),
        InlineData("loader", nameof(RouteSourceReferenceKind.SourceId)),
        InlineData(".agents/loader.md", nameof(RouteSourceReferenceKind.SourcePath))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void LoaderIsOnlyAnOrdinaryReferenceKind(
        string reference,
        string expectedKind)
    {
        var result = RouteSourceReferenceParser.Parse(reference);

        Assert.Equal(RouteSourceReferenceParseState.Valid, result.State);
        Assert.Equal(Enum.Parse<RouteSourceReferenceKind>(expectedKind), result.Kind);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Shared source-reference parser exposes the strict ID grammar"),
        InlineData("root", true),
        InlineData("project alpha/工作%20note", true),
        InlineData("", false),
        InlineData("root/", false),
        InlineData("root//child", false),
        InlineData("root/./child", false),
        InlineData("root/../child", false),
        InlineData("root\\child", false),
        InlineData(".agents/root.md", false)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IsValidIdUsesOnlyTheSharedGrammar(string value, bool expected)
    {
        Assert.Equal(expected, RouteSourceReferenceParser.IsValidId(value));
    }

    [Theory(DisplayName = "Shared source-reference parser exposes the strict canonical-path grammar"),
        InlineData(".agents/loader.md", true),
        InlineData(".agents/project alpha/工作.md", true),
        InlineData(".agents/", false),
        InlineData("./.agents/root.md", false),
        InlineData(".agents/root/../file.md", false),
        InlineData(".agents/root\\file.md", false),
        InlineData(".agents/root/\0.md", false)]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IsValidCanonicalPathRejectsUnsafeForms(string value, bool expected)
    {
        Assert.Equal(expected, RouteSourceReferenceParser.IsValidCanonicalPath(value));
    }
}
