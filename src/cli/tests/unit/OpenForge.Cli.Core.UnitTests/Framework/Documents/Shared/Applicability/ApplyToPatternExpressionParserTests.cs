using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Shared.Applicability;

public sealed class ApplyToPatternExpressionParserTests
{
    [Theory(DisplayName = "Expressions split only top-level commas and preserve atomic pattern order")]
    [InlineData("src/*.cs, test/*.ts", new[] { "src/*.cs", "test/*.ts" })]
    [InlineData("{src,test}/**/*.{cs,ts},docs/*", new[] { "{src,test}/**/*.{cs,ts}", "docs/*" })]
    [InlineData("[,].cs,[a,b].ts", new[] { "[,].cs", "[a,b].ts" })]
    [InlineData("{[,],x}.cs,other", new[] { "{[,],x}.cs", "other" })]
    [InlineData("[] ,].cs,[!],].ts", new[] { "[] ,].cs", "[!],].ts" })]
    [InlineData("[{].cs,[}].cs,[[].cs", new[] { "[{].cs", "[}].cs", "[[].cs" })]
    [InlineData("src/a\\,b.cs,other", new[] { "src/a,b.cs", "other" })]
    [InlineData("[,\\,].cs,other", new[] { "[,,].cs", "other" })]
    [InlineData(" , \t src/*.cs ,, test/*.ts, \r\n", new[] { "src/*.cs", "test/*.ts" })]
    [InlineData("src/*.cs,src/*.cs", new[] { "src/*.cs", "src/*.cs" })]
    [InlineData("good,!important.md", new[] { "good", "!important.md" })]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ParsesAtomicFragments(string expression, string[] expected)
    {
        var result = ApplyToPatternExpressionParser.Parse(expression);

        Assert.Null(result.Failure);
        Assert.Equal(expected, result.Patterns.Select(pattern => pattern.Text));
    }

    [Theory(DisplayName = "Expression comma escapes decode before atomic brace alternatives are interpreted")]
    [InlineData("{a\\,b,c}", "{a,b,c}", new[] { "a", "b", "c" }, new[] { "a,b", "a,b,c" })]
    [InlineData("{a[,]b,c}", "{a[,]b,c}", new[] { "a,b", "c" }, new[] { "a", "b", "a,b,c" })]
    [InlineData("src/report\\,legacy.cs", "src/report,legacy.cs", new[] { "src/report,legacy.cs" }, new[] { "src/report", "legacy.cs" })]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void DecodedAtomicTextDeterminesCommaMatching(string expression, string expectedText, string[] matches, string[] nonmatches)
    {
        var result = ApplyToPatternExpressionParser.Parse(expression);

        Assert.Null(result.Failure);
        var pattern = Assert.Single(result.Patterns);
        Assert.Equal(expectedText, pattern.Text);
        Assert.Equal(expectedText.Split('/'), pattern.Segments);
        Assert.All(matches, path => Assert.True(ApplyToPatternMatcher.IsMatch(pattern, path)));
        Assert.All(nonmatches, path => Assert.False(ApplyToPatternMatcher.IsMatch(pattern, path)));
    }

    [Theory(DisplayName = "Empty expressions fail without returning any patterns")]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData(" , ,, , ")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RejectsAllEmptyFragments(string expression)
    {
        var result = ApplyToPatternExpressionParser.Parse(expression);

        Assert.Equal(ApplyToPatternFailure.Empty, result.Failure);
        Assert.Empty(result.Patterns);
    }

    [Theory(DisplayName = "Malformed expressions and invalid fragments never return partial success")]
    [InlineData("good,src\\\\file.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,src\\file.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,src\\\\,other", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,bad\\", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,[abc,other", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,{one,two", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,one,two}", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,{one,{two,three}}", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("good,{ok,../bad}", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("good,/bad", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("good,bad/", nameof(ApplyToPatternFailure.Empty))]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RejectsInvalidExpressions(string expression, string expectedFailure)
    {
        var result = ApplyToPatternExpressionParser.Parse(expression);

        Assert.Equal(Enum.Parse<ApplyToPatternFailure>(expectedFailure), result.Failure);
        Assert.Empty(result.Patterns);
    }

    [Fact(DisplayName = "Expression parse results enforce nonempty success and defined failure values")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ResultFactoriesEnforceTheirInvariant()
    {
        Assert.Throws<ArgumentException>(() => ApplyToPatternExpressionParseResult.Succeeded([]));
        Assert.Throws<ArgumentException>(() => ApplyToPatternExpressionParseResult.Succeeded(default(ImmutableArray<ApplyToPattern>)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ApplyToPatternExpressionParseResult.Failed((ApplyToPatternFailure)int.MaxValue));

        foreach (var failure in Enum.GetValues<ApplyToPatternFailure>())
        {
            var result = ApplyToPatternExpressionParseResult.Failed(failure);
            Assert.Equal(failure, result.Failure);
            Assert.Empty(result.Patterns);
        }
    }
}
