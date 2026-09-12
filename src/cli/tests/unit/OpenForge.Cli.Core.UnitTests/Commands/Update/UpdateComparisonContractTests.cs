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

    [Theory(DisplayName = "Update comparison coordinates retain admitted relative-path syntax"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    [InlineData("docs/CON.md")]
    [InlineData("docs/a?.md")]
    [InlineData("docs/name.")]
    [InlineData("docs/name ")]
    [InlineData("docs/cafe\u0301.md")]
    [InlineData("docs/a:b.md")]
    [InlineData("é:x")]
    public void RetainsAdmittedComparisonCoordinateSyntax(string path)
    {
        var comparison = Comparison(path) with
        {
            Kind = UpdateComparisonTargetKind.ManagedRegion,
            RegionIdentity = path,
            SourceAssetPath = path,
        };

        comparison.Validate();

        Assert.Equal(path, comparison.RelativePath);
        Assert.Equal(path, comparison.RegionIdentity);
        Assert.Equal(path, comparison.SourceAssetPath);
    }

    [Theory(DisplayName = "Update comparison paths reject non-relative and segmented syntax"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    [InlineData("C:x")]
    [InlineData("/x")]
    [InlineData("docs\\x")]
    [InlineData("docs//x")]
    [InlineData("docs/x/")]
    [InlineData("docs/./x")]
    [InlineData("docs/../x")]
    [InlineData("docs/\u0000x")]
    [InlineData("docs/\u0085x")]
    public void RejectsNonCanonicalComparisonPathSyntax(string path)
    {
        var exception = Assert.Throws<ArgumentException>(() => Comparison(path).Validate());

        Assert.Equal("RelativePath", exception.ParamName);
        Assert.Equal(
            "Comparison paths must be canonical workspace-relative paths. (Parameter 'RelativePath')",
            exception.Message);
    }

    [Fact(DisplayName = "Update comparison whitespace keeps path region and provenance precedence"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void KeepsComparisonWhitespaceAdmissionAndGuardPrecedence()
    {
        var path = Assert.Throws<ArgumentException>(() => (Comparison(" ") with
        {
            Kind = (UpdateComparisonTargetKind)int.MaxValue,
        }).Validate());
        var region = Assert.Throws<ArgumentException>(() => (Comparison("docs/index.md") with
        {
            Kind = UpdateComparisonTargetKind.ManagedRegion,
            RegionIdentity = " ",
            SourceAssetPath = "docs//source.md",
        }).Validate());
        var provenance = Assert.Throws<ArgumentException>(() => (Comparison("docs/index.md") with
        {
            SourceAssetPath = " ",
            CurrentFingerprint = "invalid",
        }).Validate());

        Assert.Equal("RelativePath", path.ParamName);
        Assert.Equal(
            "Comparison paths must be canonical workspace-relative paths. (Parameter 'RelativePath')",
            path.Message);
        Assert.Equal("RegionIdentity", region.ParamName);
        Assert.Equal(
            "A region comparison requires one canonical region identity. (Parameter 'RegionIdentity')",
            region.Message);
        Assert.Equal("SourceAssetPath", provenance.ParamName);
        Assert.Equal(
            "Authored comparisons require source provenance. (Parameter 'SourceAssetPath')",
            provenance.Message);
    }

    [Theory(DisplayName = "Update comparison fingerprints require exactly lowercase SHA-256 syntax"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaag")]
    public void RejectsComparisonFingerprintSyntax(string fingerprint)
    {
        var exception = Assert.Throws<ArgumentException>(() => (Comparison("docs/index.md") with
        {
            CurrentFingerprint = fingerprint,
        }).Validate());

        Assert.Equal("CurrentFingerprint", exception.ParamName);
        Assert.Equal(
            "Comparison fingerprints must be lowercase SHA-256 values. (Parameter 'CurrentFingerprint')",
            exception.Message);
    }

    [Fact(DisplayName = "Update unavailable byte facts retain an independently available fingerprint"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RetainsHashOnlyUnavailableComparisonBytes()
    {
        var facts = new UpdateComparisonByteFacts
        {
            ExactBytes = null,
            Sha256 = Hash,
        };

        facts.Validate("bytes");

        Assert.False(facts.IsAvailable);
        Assert.Null(facts.ExactBytes);
        Assert.Equal(Hash, facts.Sha256);
    }

    [Fact(DisplayName = "Update unavailable byte facts still reject uppercase fingerprints"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsInvalidFingerprintWithoutAvailableComparisonBytes()
    {
        var facts = new UpdateComparisonByteFacts
        {
            ExactBytes = null,
            Sha256 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        };

        var exception = Assert.Throws<ArgumentException>(() => facts.Validate("bytes"));

        Assert.Equal("bytes", exception.ParamName);
        Assert.Equal(
            "Comparison byte facts require a lowercase SHA-256 fingerprint. (Parameter 'bytes')",
            exception.Message);
    }

    [Fact(DisplayName = "Update available byte facts require their exact fingerprint"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RejectsAvailableComparisonBytesWithoutFingerprint()
    {
        var facts = Bytes() with { Sha256 = null };

        var exception = Assert.Throws<ArgumentException>(() => facts.Validate("bytes"));

        Assert.Equal("bytes", exception.ParamName);
        Assert.Equal(
            "Available comparison bytes require an exact fingerprint. (Parameter 'bytes')",
            exception.Message);
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
