using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateComparisonContractTests
{
    [Fact(DisplayName = "Update validates file comparison provenance and fingerprints"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ValidatesFileComparisonPathProvenanceAndFingerprints()
    {
        var comparison = Comparison("docs/framework.md") with
        {
            Kind = UpdateComparisonTargetKind.File,
            RegionIdentity = null,
        };

        comparison.Validate();

        Assert.Equal("docs/framework.md", comparison.RelativePath);
        Assert.Equal("assets/framework.md", comparison.SourceAssetPath);
        Assert.Equal(64, comparison.CurrentFingerprint?.Length);
        Assert.Equal(64, comparison.IntendedFingerprint?.Length);
    }

    [Fact(DisplayName = "Update validates managed and generated region identity and null provenance"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ValidatesManagedAndGeneratedRegionIdentityAndNullProvenance()
    {
        var managed = Comparison("docs/index.md") with
        {
            Kind = UpdateComparisonTargetKind.ManagedRegion,
            RegionIdentity = "entries",
        };
        var generated = Comparison("docs/index.md") with
        {
            Kind = UpdateComparisonTargetKind.GeneratedRegion,
            RegionIdentity = "entries",
            SourceAssetPath = null,
            SourceAssetPresentInCurrentInventory = null,
            FingerprintKind = UpdateComparisonFingerprintKind.ExactBytes,
        };

        managed.Validate();
        generated.Validate();

        Assert.Equal("entries", managed.RegionIdentity);
        Assert.Null(generated.SourceAssetPath);
        Assert.Null(generated.SourceAssetPresentInCurrentInventory);
    }

    [Fact(DisplayName = "Update represents a historical retired source absent from current inventory"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RepresentsHistoricalRetiredSourceAbsentFromCurrentInventory()
    {
        var comparison = Comparison("docs/retired.md") with
        {
            SourceAssetPresentInCurrentInventory = false,
            CurrentState = UpdateComparisonCurrentState.BaselineEquivalent,
            IntendedState = UpdateComparisonIntendedState.Retired,
            IntendedFingerprint = null,
            IntendedBytes = new UpdateComparisonByteFacts
            {
                ExactBytes = null,
                Sha256 = null,
            },
            RetirementEligibility = UpdateRetirementEligibility.Eligible,
        };

        comparison.Validate();

        Assert.False(comparison.SourceAssetPresentInCurrentInventory);
        Assert.Equal(UpdateComparisonIntendedState.Retired, comparison.IntendedState);
        Assert.Equal(UpdateRetirementEligibility.Eligible, comparison.RetirementEligibility);
    }

    [Fact(DisplayName = "Update rejects invalid fingerprints, nullability, and finite states"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsInvalidFingerprintNullabilityAndFiniteStates()
    {
        Assert.Throws<ArgumentException>(() => (Comparison("docs/invalid.md") with
        {
            CurrentFingerprint = "not-a-fingerprint",
        }).Validate());
        Assert.Throws<ArgumentException>(() => (Comparison("docs/missing.md") with
        {
            CurrentState = UpdateComparisonCurrentState.Missing,
        }).Validate());
        Assert.Throws<ArgumentException>(() => (Comparison("docs/non-new.md") with
        {
            BaselineFingerprint = null,
        }).Validate());
        Assert.Throws<ArgumentException>(() => (Comparison("docs/bytes.md") with
        {
            CurrentBytes = new UpdateComparisonByteFacts
            {
                ExactBytes = ImmutableArray.Create<byte>(1, 2, 3),
                Sha256 = Hash,
            },
        }).Validate());
        var genuinelyNew = Comparison("docs/new.md") with
        {
            BaselineFingerprint = null,
            IntendedState = UpdateComparisonIntendedState.New,
        };
        genuinelyNew.Validate();
        Assert.Throws<ArgumentOutOfRangeException>(() => (Comparison("docs/unknown.md") with
        {
            IntendedState = (UpdateComparisonIntendedState)int.MaxValue,
        }).Validate());
    }

    [Fact(DisplayName = "Update orders comparisons by path, kind, and region"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void OrdersComparisonsByPathAndKindAndRegion()
    {
        var comparisons = new[]
        {
            Comparison("docs/z.md") with { Kind = UpdateComparisonTargetKind.File, RegionIdentity = null },
            Comparison("docs/a.md") with
            {
                Kind = UpdateComparisonTargetKind.GeneratedRegion,
                RegionIdentity = "z",
                SourceAssetPath = null,
                SourceAssetPresentInCurrentInventory = null,
                FingerprintKind = UpdateComparisonFingerprintKind.ExactBytes,
            },
            Comparison("docs/a.md") with { Kind = UpdateComparisonTargetKind.ManagedRegion, RegionIdentity = "a" },
        };

        var result = new UpdateResult(new UpdateResultFormation
        {
            Workspace = null,
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = false,
            Source = null,
            Comparisons = comparisons,
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [],
        });
        var ordered = result.Comparisons;

        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, ordered[0].Kind);
        Assert.Equal("a", ordered[0].RegionIdentity);
        Assert.Equal(UpdateComparisonTargetKind.GeneratedRegion, ordered[1].Kind);
        Assert.Equal("z", ordered[1].RegionIdentity);
        Assert.Equal("docs/z.md", ordered[2].RelativePath);
    }

    private static UpdateComparison Comparison(string path)
        => new()
        {
            RelativePath = path,
            Kind = UpdateComparisonTargetKind.File,
            RegionIdentity = null,
            SourceAssetPath = "assets/framework.md",
            SourceAssetPresentInCurrentInventory = true,
            FingerprintKind = UpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
            BaselineFingerprint = Hash,
            CurrentFingerprint = Hash,
            IntendedFingerprint = Hash,
            CurrentState = UpdateComparisonCurrentState.BaselineEquivalent,
            IntendedState = UpdateComparisonIntendedState.Same,
            RetirementEligibility = UpdateRetirementEligibility.NotApplicable,
            CurrentBytes = Bytes(),
            IntendedBytes = Bytes(),
        };

    private static UpdateComparisonByteFacts Bytes()
        => new()
        {
            ExactBytes = ImmutableArray.Create<byte>(1, 2, 3),
            Sha256 = ByteHash,
        };

    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private const string ByteHash = "039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81";
}
