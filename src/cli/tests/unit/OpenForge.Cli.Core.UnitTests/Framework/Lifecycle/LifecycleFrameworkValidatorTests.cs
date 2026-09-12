using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Shared.Validation;
using OpenForge.Cli.Core.Framework.Lifecycle;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class LifecycleFrameworkValidatorTests
{
    private const string Fingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact(DisplayName = "Framework lifecycle validation accepts complete ordered identity")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateAcceptsCompleteOrderedIdentity()
    {
        var result = LifecycleFrameworkValidator.Validate(Framework());

        Assert.Equal(LifecycleSectionValidationState.Valid, result.State);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Framework lifecycle validation accepts a derived generated target with null source provenance")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateAcceptsGeneratedTargetWithoutSourceProvenance()
    {
        const string path = ".agents/loader.md";
        const string region = "entries";
        var framework = Framework(
            [GeneratedTarget(path: path, region: region)],
            [new FrameworkGeneratedRegion { Path = path, Region = region }]);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Valid, result.State);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Framework lifecycle validation accepts a source-backed managed block without generated-region identity")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateAcceptsSourceBackedManagedBlock()
    {
        const string path = ".agents/loader.md";
        const string region = "managed";
        var framework = Framework(
            [
                SourceBackedTarget(
                    path: path,
                    sourceAssetPath: path,
                    region: region),
            ],
            generatedRegions: []);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Valid, result.State);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Framework lifecycle validation accepts normalized historical source provenance absent from the current payload")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateAcceptsHistoricalSourceProvenance()
    {
        var framework = Framework(
            [
                SourceBackedTarget(
                    path: "AGENTS.md",
                    sourceAssetPath: ".agents/retired/legacy-loader.md"),
            ],
            generatedRegions: []);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Valid, result.State);
        Assert.Null(result.Cause);
    }

    [Fact(DisplayName = "Framework lifecycle validation accepts repeated exact source provenance")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateAcceptsRepeatedSourceProvenance()
    {
        var framework = Framework(
            [
                SourceBackedTarget(path: "AGENTS.md", sourceAssetPath: ".agents/loader.md"),
                SourceBackedTarget(path: "CLAUDE.md", sourceAssetPath: ".agents/loader.md"),
                SourceBackedTarget(path: "GEMINI.md", sourceAssetPath: ".agents/loader.md"),
            ],
            generatedRegions: []);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Valid, result.State);
        Assert.Null(result.Cause);
    }

    [Theory(DisplayName = "Framework lifecycle validation blocks malformed source provenance")]
    [InlineData("/absolute/source.md")]
    [InlineData(".agents\\source.md")]
    [InlineData("../source.md")]
    [InlineData(".agents//source.md")]
    [InlineData(".agents/e\u0301.md")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateBlocksMalformedSourceProvenance(string sourceAssetPath)
    {
        var framework = Framework(
            [SourceBackedTarget(path: "AGENTS.md", sourceAssetPath: sourceAssetPath)],
            generatedRegions: []);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Blocked, result.State);
        Assert.Contains("source-asset", result.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Framework lifecycle validation blocks null source provenance on a non-generated target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateBlocksNullSourceProvenanceOnNonGeneratedTarget()
    {
        var framework = Framework(
            [UnprovenancedTarget("AGENTS.md")],
            generatedRegions: []);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Blocked, result.State);
        Assert.Contains("source provenance", result.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Framework lifecycle validation blocks source provenance on a generated target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateBlocksSourceProvenanceOnGeneratedTarget()
    {
        const string path = ".agents/loader.md";
        const string region = "entries";
        var framework = Framework(
            [
                SourceBackedTarget(
                    path: path,
                    sourceAssetPath: ".agents/loader.md",
                    region: region),
            ],
            [new FrameworkGeneratedRegion { Path = path, Region = region }]);

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Blocked, result.State);
        Assert.Contains("source provenance", result.Cause, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Framework lifecycle validation blocks contradictory identity")]
    [InlineData("unordered")]
    [InlineData("path-case-alias")]
    [InlineData("orphan-region")]
    [InlineData("invalid-hash")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateBlocksContradictoryIdentity(string scenario)
    {
        var framework = Framework();
        framework = scenario switch
        {
            "unordered" => Framework(
                [SourceBackedTarget("z.md"), SourceBackedTarget("a.md")],
                generatedRegions: []),
            "path-case-alias" => Framework(
                [SourceBackedTarget("AGENTS.md"), SourceBackedTarget("agents.md")],
                generatedRegions: []),
            "orphan-region" => Framework(
                [SourceBackedTarget("AGENTS.md")],
                [new FrameworkGeneratedRegion { Path = "AGENTS.md", Region = "managed" }]),
            "invalid-hash" => new FrameworkLifecycleState
            {
                Coverage = framework.Coverage,
                Source = new FrameworkLifecycleSource
                {
                    Id = framework.Source.Id,
                    Version = framework.Source.Version,
                    InventoryFingerprint = "invalid",
                },
                Targets = framework.Targets,
                GeneratedRegions = framework.GeneratedRegions,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The lifecycle scenario is not defined."),
        };

        var result = LifecycleFrameworkValidator.Validate(framework);

        Assert.Equal(LifecycleSectionValidationState.Blocked, result.State);
        Assert.NotNull(result.Cause);
    }

    [Fact(DisplayName = "Framework lifecycle validation blocks Extension path collision")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void ValidateBlocksCrossSectionPathCollision()
    {
        var extensions = new ExtensionLifecycleState
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Packages =
            [
                new LifecycleExtensionPackageV1
                {
                    Id = "toolkit",
                    Version = null,
                    Source = null,
                    Dependencies = [],
                    Paths = [".agents/loader.md"],
                },
            ],
            Paths =
            [
                new LifecycleExtensionPathV1
                {
                    Path = ".agents/loader.md",
                    Owners = ["toolkit"],
                    BaselineFingerprint = Fingerprint,
                    FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
                },
            ],
        };

        var result = LifecycleFrameworkValidator.ValidateNoCrossSectionCollisions(
            Framework(),
            extensions);

        Assert.Equal(LifecycleSectionValidationState.Blocked, result.State);
    }

    private static FrameworkLifecycleState Framework()
        => Framework(
            [SourceBackedTarget(".agents/loader.md")],
            generatedRegions: []);

    private static FrameworkLifecycleState Framework(
        FrameworkLifecycleTarget[] targets,
        FrameworkGeneratedRegion[] generatedRegions)
        => new()
        {
            Coverage = LifecycleSchema.CompleteCoverage,
            Source = new FrameworkLifecycleSource
            {
                Id = "open-forge",
                Version = "1.0.0",
                InventoryFingerprint = Fingerprint,
            },
            Targets = targets,
            GeneratedRegions = generatedRegions,
        };

    private static FrameworkLifecycleTarget SourceBackedTarget(string path)
        => SourceBackedTarget(
            path: path,
            sourceAssetPath: ".agents/loader.md");

    private static FrameworkLifecycleTarget SourceBackedTarget(
        string path,
        string sourceAssetPath,
        string? region = null)
        => new()
        {
            Path = path,
            SourceAssetPath = sourceAssetPath,
            Region = region,
            BaselineFingerprint = Fingerprint,
            FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
        };

    private static FrameworkLifecycleTarget GeneratedTarget(
        string path,
        string region)
        => new()
        {
            Path = path,
            SourceAssetPath = null,
            Region = region,
            BaselineFingerprint = Fingerprint,
            FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
        };

    private static FrameworkLifecycleTarget UnprovenancedTarget(string path)
        => new()
        {
            Path = path,
            SourceAssetPath = null,
            Region = null,
            BaselineFingerprint = Fingerprint,
            FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
        };
}
