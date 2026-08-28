using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

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
                [Target("z.md"), Target("a.md")],
                generatedRegions: []),
            "path-case-alias" => Framework(
                [Target("AGENTS.md"), Target("agents.md")],
                generatedRegions: []),
            "orphan-region" => Framework(
                [Target("AGENTS.md", region: null)],
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
            [Target(".agents/loader.md")],
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

    private static FrameworkLifecycleTarget Target(
        string path,
        string? region = null)
        => new()
        {
            Path = path,
            Region = region,
            BaselineFingerprint = Fingerprint,
            FingerprintKind = LifecycleSchema.SemanticFingerprintKind,
        };
}
