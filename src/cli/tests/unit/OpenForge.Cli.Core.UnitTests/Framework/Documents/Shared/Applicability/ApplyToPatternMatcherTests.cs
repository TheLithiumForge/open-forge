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
    [InlineData("src/\nfile.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/{one,{two,three}}.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/{one,two.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/one,two}.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("src/[ab.cs", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("[]", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("[!]", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("[^]", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void ParseRejectsUnsupportedPatterns(string text, string expectedFailure)
    {
        var result = ApplyToPatternMatcher.Parse(text);

        Assert.Null(result.Pattern);
        Assert.Equal(Enum.Parse<ApplyToPatternFailure>(expectedFailure), result.Failure);
    }

    [Theory(DisplayName = "ApplyTo patterns treat leading exclamation marks as literal path characters")]
    [InlineData("!important.md", "!important.md", true)]
    [InlineData("!important.md", "important.md", false)]
    [InlineData("docs/!draft/*.md", "docs/!draft/a.md", true)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit")]
    public void MatchesLeadingExclamationMarksAsLiterals(string text, string path, bool expected)
    {
        Assert.Equal(expected, IsMatch(text, path));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "ApplyTo patterns match simple wildcards within one case-sensitive path segment")]
    [InlineData("src/*.cs", "src/Order.cs", true)]
    [InlineData("src/*.cs", "src/nested/Order.cs", false)]
    [InlineData("src/Ord?r.cs", "src/Order.cs", true)]
    [InlineData("src/Order.cs", "src/order.cs", false)]
    [InlineData("planned/*.cs", "planned/NewFile.cs", true)]
    [InlineData("a**b", "ab", true)]
    [InlineData("a**b", "axyzb", true)]
    [InlineData("a**b", "a/x/b", false)]
    [InlineData("src/***/file.cs", "src/one/file.cs", true)]
    [InlineData("src/***/file.cs", "src/file.cs", false)]
    [InlineData("src/***/file.cs", "src/one/two/file.cs", false)]
    [InlineData("*", ".gitignore", true)]
    [InlineData("**/*.cs", ".hidden/.File.cs", true)]
    [InlineData("*.cs", "src/File.cs", false)]
    [InlineData("?", "a", true)]
    [InlineData("?", "ab", false)]
    [InlineData("a,b.cs", "a,b.cs", true)]
    [InlineData("a,b.cs", "b.cs", false)]
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
    [InlineData("src/**", "src", true)]
    [InlineData("**/**/file.cs", "file.cs", true)]
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

    [Theory(DisplayName = "Brace groups expand textually across segments and multiply their alternatives")]
    [InlineData("{src,test}/**/*.{cs,ts}", "src/deep/File.cs", true)]
    [InlineData("{src,test}/**/*.{cs,ts}", "test/File.ts", true)]
    [InlineData("{src,test}/**/*.{cs,ts}", "other/File.cs", false)]
    [InlineData("{src/test,test/unit}/*.cs", "test/unit/File.cs", true)]
    [InlineData("{src/,}*.cs", "File.cs", true)]
    [InlineData("{src/,}*.cs", "src/File.cs", true)]
    [InlineData("{,src/}*.cs", "File.cs", true)]
    [InlineData("pre{,fix}.cs", "pre.cs", true)]
    [InlineData("pre{,fix}.cs", "prefix.cs", true)]
    [InlineData("{src}/*.cs", "src/File.cs", true)]
    [InlineData("src/{}*.cs", "src/File.cs", true)]
    [InlineData("{[,],x}.cs", ",.cs", true)]
    [InlineData("{[,],x}.cs", "x.cs", true)]
    [InlineData("{[{],[}]}.cs", "{.cs", true)]
    [InlineData("{[{],[}]}.cs", "}.cs", true)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void MatchesBraceAlternatives(string text, string path, bool expected)
    {
        Assert.Equal(expected, IsMatch(text, path));
    }

    [Theory(DisplayName = "Every brace alternative must satisfy all workspace-relative path rules")]
    [InlineData("{safe,}", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("{,safe}", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("{safe,/root}", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("{safe,C:/root}", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("{safe,C:root}", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("{C,D}:/root", nameof(ApplyToPatternFailure.AbsolutePath))]
    [InlineData("{safe,./root}", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("{safe,../root}", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("src/{safe,..}/file", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("src/{safe,.}/file", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("src/{safe,}/file", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("{safe,src/}", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("{safe,src//file}", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("{safe,src\\file}", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("{safe,src/\nfile}", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RejectsInvalidExpandedPaths(string text, string expectedFailure)
    {
        var result = ApplyToPatternMatcher.Parse(text);

        Assert.Null(result.Pattern);
        Assert.Equal(Enum.Parse<ApplyToPatternFailure>(expectedFailure), result.Failure);
    }

    [Fact(DisplayName = "Brace alternatives preserve authored text and raw segments")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BraceExpansionPreservesAuthoredIdentity()
    {
        const string text = "{src/test,unit}/**/*.{cs,ts}";

        var pattern = Assert.IsType<ApplyToPattern>(ApplyToPatternMatcher.Parse(text).Pattern);

        Assert.Equal(text, pattern.Text);
        Assert.Equal(["{src", "test,unit}", "**", "*.{cs,ts}"], pattern.Segments);
    }

    [Theory(DisplayName = "Brace expansion accepts 1000 alternatives and rejects larger sums or products")]
    [InlineData(1000, 1, true)]
    [InlineData(1001, 1, false)]
    [InlineData(10, 100, true)]
    [InlineData(10, 101, false)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BoundsAlternativeCount(int firstCount, int secondCount, bool succeeds)
    {
        var first = string.Join(',', Enumerable.Range(0, firstCount));
        var second = string.Join(',', Enumerable.Range(0, secondCount));

        var result = ApplyToPatternMatcher.Parse($"{{{first}}}/{{{second}}}");

        if (succeeds)
        {
            Assert.Null(result.Failure);
            var pattern = Assert.IsType<ApplyToPattern>(result.Pattern);
            Assert.True(ApplyToPatternMatcher.IsMatch(pattern, $"{firstCount - 1}/{secondCount - 1}"));
        }
        else
        {
            Assert.Null(result.Pattern);
            Assert.Equal(ApplyToPatternFailure.UnsupportedSyntax, result.Failure);
        }
    }

    [Theory(DisplayName = "Character classes match ordinal members, ranges, negation and bracket literals")]
    [InlineData("[abc].cs", "b.cs", true)]
    [InlineData("[abc].cs", "d.cs", false)]
    [InlineData("[a-c0-2].cs", "b.cs", true)]
    [InlineData("[a-c0-2].cs", "2.cs", true)]
    [InlineData("[a-c0-2].cs", "3.cs", false)]
    [InlineData("[a-c].cs", "B.cs", false)]
    [InlineData("[!a-c].cs", "d.cs", true)]
    [InlineData("[!a-c].cs", "a.cs", false)]
    [InlineData("[^a-c].cs", "d.cs", true)]
    [InlineData("[^a-c].cs", "c.cs", false)]
    [InlineData("[]a].cs", "].cs", true)]
    [InlineData("[]a].cs", "a.cs", true)]
    [InlineData("[!]a].cs", "].cs", false)]
    [InlineData("[!]a].cs", "b.cs", true)]
    [InlineData("[^]a].cs", "].cs", false)]
    [InlineData("[^]a].cs", "b.cs", true)]
    [InlineData("[-a].cs", "-.cs", true)]
    [InlineData("[a-].cs", "-.cs", true)]
    [InlineData("[-a].cs", "b.cs", false)]
    [InlineData("[!a-].cs", "-.cs", false)]
    [InlineData("[*].cs", "*.cs", true)]
    [InlineData("[*].cs", "x.cs", false)]
    [InlineData("[?].cs", "?.cs", true)]
    [InlineData("[[].cs", "[.cs", true)]
    [InlineData("[{].cs", "{.cs", true)]
    [InlineData("[,].cs", ",.cs", true)]
    [InlineData("[a]", "aa", false)]
    [InlineData("a[!x]b", "a/b", false)]
    [InlineData("[!x]*", ".hidden", true)]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void MatchesCharacterClasses(string text, string path, bool expected)
    {
        Assert.Equal(expected, IsMatch(text, path));
    }

    [Fact(DisplayName = "Repeated stars and double-star segments handle a late mismatch without backtracking")]
    [Trait("Feature", "apply-to-patterns"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void RepeatedWildcardsHandleLateMismatch()
    {
        var segmentPattern = string.Concat(Enumerable.Repeat("*a", 100));
        Assert.False(IsMatch($"{segmentPattern}b", new string('a', 200)));

        var pathPattern = string.Concat(Enumerable.Repeat("**/a/", 40));
        var path = string.Join('/', Enumerable.Repeat("a", 80));
        Assert.False(IsMatch($"{pathPattern}b", path));
    }

    private static bool IsMatch(string text, string path)
    {
        var result = ApplyToPatternMatcher.Parse(text);
        return ApplyToPatternMatcher.IsMatch(Assert.IsType<ApplyToPattern>(result.Pattern), path);
    }
}
