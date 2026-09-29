using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Shared.Applicability;

public sealed class ApplyToPatternMatcherTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "ApplyTo patterns preserve their authored text and slash-separated segments")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void ParsePreservesTextAndSegments()
    {
        const string text = " source files,generated/**/*.cs ";

        var result = ApplyToPatternMatcher.Parse(text);

        Assert.Null(result.Failure);
        var pattern = Assert.IsType<ApplyToPattern>(result.Pattern);
        Assert.Equal(text, pattern.Text);
        Assert.Equal([" source files,generated", "**", "*.cs "], pattern.Segments);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "ApplyTo patterns reject paths and unsupported syntax with typed failures")]
    [InlineData("", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("/src/file.cs", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("C:/src/file.cs", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("src//file.cs", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("src/", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("src/./file.cs", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("../file.cs", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("src\\file.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/{one,two}.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/[ab].cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("!src/*.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/ab**.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/***/file.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void ParseRejectsUnsupportedPatterns(string text, string expectedFailure)
    {
        var result = ApplyToPatternMatcher.Parse(text);

        Assert.Null(result.Pattern);
        Assert.Equal(Enum.Parse<ApplyToPatternFailure>(expectedFailure), result.Failure);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "ApplyTo patterns match simple wildcards within one case-sensitive path segment")]
    [InlineData("src/*.cs", "src/Order.cs", true)]
    [InlineData("src/*.cs", "src/nested/Order.cs", false)]
    [InlineData("src/Ord?r.cs", "src/Order.cs", true)]
    [InlineData("src/Order.cs", "src/order.cs", false)]
    [InlineData("planned/*.cs", "planned/NewFile.cs", true)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void MatchesSimpleWildcards(string text, string path, bool expected)
    {
        Assert.Equal(expected, IsMatch(text, path));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Standalone double-star matches zero or more complete path segments")]
    [InlineData("**/*.cs", "Root.cs", true)]
    [InlineData("**/*.cs", "src/Root.cs", true)]
    [InlineData("**/*.cs", "src/generated/Root.cs", true)]
    [InlineData("src/**/Tests/*.cs", "src/Tests/MatcherTests.cs", true)]
    [InlineData("src/**/Tests/*.cs", "src/unit/Tests/MatcherTests.cs", true)]
    [InlineData("src/**/Tests/*.cs", "tests/MatcherTests.cs", false)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void MatchesZeroOrMoreSegments(string text, string path, bool expected)
    {
        Assert.Equal(expected, IsMatch(text, path));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "ApplyTo matching rejects non-relative or traversing candidate paths")]
    [InlineData("*.cs", "/src/Root.cs")]
    [InlineData("*.cs", "C:/src/Root.cs")]
    [InlineData("*.cs", "src/../Root.cs")]
    [InlineData("*.cs", "src\\Root.cs")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void RejectsInvalidCandidatePaths(string text, string path)
    {
        Assert.False(IsMatch(text, path));
    }

    private static bool IsMatch(string text, string path)
    {
        var result = ApplyToPatternMatcher.Parse(text);
        return ApplyToPatternMatcher.IsMatch(Assert.IsType<ApplyToPattern>(result.Pattern), path);
    }
}
