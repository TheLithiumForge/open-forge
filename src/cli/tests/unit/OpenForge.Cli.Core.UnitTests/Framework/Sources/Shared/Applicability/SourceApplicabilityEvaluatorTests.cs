using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Applicability;

[Trait("Feature", "source-applicability"), Trait("Evidence", "Unit")]
public sealed class SourceApplicabilityEvaluatorTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Applicability is unconditioned when every ancestor omits applyTo")]
    public void ReturnsUnconditionedForAbsentConditions()
    {
        var conditions = new[]
        {
            new SourceApplyToCondition("_root.md", ApplyToMetadataFacts.Absent),
            new SourceApplyToCondition("scope/_scope.md", ApplyToMetadataFacts.Absent),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(conditions, []);

        Assert.Equal(SourceApplicabilityState.Unconditioned, result.State);
        Assert.Equal(conditions, result.Conditions);
        Assert.Empty(result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Applicability is pending when conditions have no known working paths")]
    public void ReturnsPendingWhenWorkingPathsAreEmpty()
    {
        var conditions = new[]
        {
            Condition("_root.md", "{src,test}/[a-z]*.cs"),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(conditions, []);

        Assert.Equal(SourceApplicabilityState.Pending, result.State);
        Assert.Equal(conditions, result.Conditions);
        Assert.Empty(result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Invalid metadata takes precedence over otherwise matching ancestor conditions")]
    public void InvalidMetadataWins()
    {
        var invalid = ApplyToMetadataFacts.Invalid(
            [],
            [],
            new ApplyToMetadataFailure(
                ApplyToMetadataFailureKind.InvalidPattern,
                null,
                ApplyToPatternFailure.Empty));
        var conditions = new[]
        {
            Condition("_root.md", "src/**/*.cs"),
            new SourceApplyToCondition("scope/_scope.md", invalid),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(conditions, ["src/file.cs"]);

        Assert.Equal(SourceApplicabilityState.Invalid, result.State);
        Assert.Equal(conditions, result.Conditions);
        Assert.Empty(result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A path must satisfy every ancestor condition on that same file")]
    public void DoesNotIntersectAncestorMatchesAcrossDifferentFiles()
    {
        var conditions = new[]
        {
            Condition("_root.md", "src/*.cs"),
            Condition("src/_src.md", "docs/*.md"),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(
            conditions,
            ["src/file.cs", "docs/file.md"]);

        Assert.Equal(SourceApplicabilityState.Unmatched, result.State);
        Assert.Empty(result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A working path matching every ancestor condition is applicable")]
    public void MatchesWhenOnePathSatisfiesEveryAncestorCondition()
    {
        var conditions = new[]
        {
            Condition("_root.md", "src/**"),
            Condition("src/_src.md", "**/*.cs"),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(conditions, ["src/file.cs"]);

        Assert.Equal(SourceApplicabilityState.Matched, result.State);
        Assert.Equal(conditions, result.Conditions);
        Assert.Equal(["src/file.cs"], result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A planned path must satisfy brace and class patterns on every ancestor")]
    public void MatchesBraceAndClassPatternsOnTheSamePlannedPath()
    {
        var conditions = new[]
        {
            Condition("_root.md", "{src,test}/[a-z]*.cs"),
            Condition("src/_src.md", "src/[a-z]*.cs"),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(
            conditions,
            ["src/planned.cs", "test/planned.cs"]);

        Assert.Equal(SourceApplicabilityState.Matched, result.State);
        Assert.Equal(conditions, result.Conditions);
        Assert.Equal(["src/planned.cs"], result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Applicability matching remains case-sensitive")]
    public void DoesNotMatchDifferentPathCase()
    {
        var conditions = new[] { Condition("_root.md", "src/*.cs") };

        var result = SourceApplicabilityEvaluator.Evaluate(conditions, ["Src/file.cs"]);

        Assert.Equal(SourceApplicabilityState.Unmatched, result.State);
        Assert.Empty(result.MatchingPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Pattern alternatives use OR and preserve unique matching path order")]
    public void AppliesPatternAlternativesAndDeduplicatesMatchingPaths()
    {
        var conditions = new[]
        {
            Condition("_root.md", "src/*.cs", "lib/*.cs"),
        };

        var result = SourceApplicabilityEvaluator.Evaluate(
            conditions,
            ["src/first.cs", "src/first.cs", "other/no.cs", "lib/second.cs"]);

        Assert.Equal(SourceApplicabilityState.Matched, result.State);
        Assert.Equal(["src/first.cs", "lib/second.cs"], result.MatchingPaths);
    }

    private static SourceApplyToCondition Condition(string sourcePath, params string[] patterns)
    {
        var parsedPatterns = patterns.Select(ParsePattern).ToArray();
        return new SourceApplyToCondition(
            sourcePath,
            ApplyToMetadataFacts.Valid(
                parsedPatterns,
                [new ApplyToDeclaration(
                    ApplyToMetadataLocation.Root,
                    new YamlTextSpan(0, 1),
                    new YamlTextSpan(2, 1),
                    [.. parsedPatterns])]));
    }

    private static ApplyToPattern ParsePattern(string text)
    {
        var result = ApplyToPatternMatcher.Parse(text);
        return result.Pattern
            ?? throw new InvalidOperationException($"Test pattern '{text}' was not accepted.");
    }
}
