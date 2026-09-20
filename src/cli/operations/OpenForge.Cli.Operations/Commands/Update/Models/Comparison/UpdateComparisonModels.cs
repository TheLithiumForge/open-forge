using System.Collections.Immutable;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Update.Shared.Validation;

namespace OpenForge.Cli.Core.Commands.Update.Models.Comparison;

internal enum UpdateComparisonTargetKind
{
    File,
    ManagedRegion,
    GeneratedRegion,
}

internal enum UpdateComparisonFingerprintKind
{
    OpenForgeMarkdownV1,
    ExactBytes,
}

internal enum UpdateComparisonCurrentState
{
    Missing,
    Same,
    FormatOnly,
    Changed,
    Unavailable,
    Blocked,
}

internal enum UpdateComparisonIntendedState
{
    Same,
    Changed,
    New,
    Retired,
    Unavailable,
    Blocked,
}

internal enum UpdateRetirementEligibility
{
    NotApplicable,
    Eligible,
    Ineligible,
    Unavailable,
    Blocked,
}

internal sealed record UpdateComparisonByteFacts
{
    public required ImmutableArray<byte>? ExactBytes { get; init; }

    public required string? Sha256 { get; init; }

    internal bool IsAvailable => ExactBytes.HasValue;

    internal void Validate(string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        if (ExactBytes is { IsDefault: true })
        {
            throw new ArgumentException(
                "Comparison byte facts cannot contain a default immutable array.",
                parameterName);
        }

        if (Sha256 is not null && !UpdateValueSyntax.IsSha256(Sha256))
        {
            throw new ArgumentException(
                "Comparison byte facts require a lowercase SHA-256 fingerprint.",
                parameterName);
        }

        if (ExactBytes.HasValue && Sha256 is null)
        {
            throw new ArgumentException(
                "Available comparison bytes require an exact fingerprint.",
                parameterName);
        }

        if (ExactBytes.HasValue
            && Sha256 is not null
            && !string.Equals(
                Sha256,
                Convert.ToHexString(SHA256.HashData(ExactBytes.Value.AsSpan())).ToLowerInvariant(),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Comparison byte facts require a fingerprint matching the exact bytes.",
                parameterName);
        }
    }
}

internal sealed record UpdateComparison
{
    public required string RelativePath { get; init; }

    public required UpdateComparisonTargetKind Kind { get; init; }

    public required string? RegionIdentity { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required bool? SourceAssetPresentInCurrentInventory { get; init; }

    public required UpdateComparisonFingerprintKind FingerprintKind { get; init; }

    public required string? CurrentFingerprint { get; init; }

    public required string? IntendedFingerprint { get; init; }

    public required UpdateComparisonCurrentState CurrentState { get; init; }

    public required UpdateComparisonIntendedState IntendedState { get; init; }

    public required UpdateRetirementEligibility RetirementEligibility { get; init; }

    public required UpdateComparisonByteFacts CurrentBytes { get; init; }

    public required UpdateComparisonByteFacts IntendedBytes { get; init; }

    internal void Validate()
    {
        ValidateRelativePath(RelativePath, nameof(RelativePath));
        ValidateKindAndRegion();
        ValidateSourceProvenance();
        ValidateFingerprints();
        ValidateStates();
        ArgumentNullException.ThrowIfNull(CurrentBytes);
        ArgumentNullException.ThrowIfNull(IntendedBytes);
        CurrentBytes.Validate(nameof(CurrentBytes));
        IntendedBytes.Validate(nameof(IntendedBytes));
    }

    private void ValidateKindAndRegion()
    {
        if (!Enum.IsDefined(Kind))
        {
            throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "The comparison target kind is not defined.");
        }

        if (Kind == UpdateComparisonTargetKind.File)
        {
            if (RegionIdentity is not null)
            {
                throw new ArgumentException("A file comparison cannot carry a region identity.", nameof(RegionIdentity));
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(RegionIdentity) || !UpdateValueSyntax.IsCanonicalRelative(RegionIdentity))
        {
            throw new ArgumentException("A region comparison requires one canonical region identity.", nameof(RegionIdentity));
        }
    }

    private void ValidateSourceProvenance()
    {
        if (SourceAssetPath is not null && !UpdateValueSyntax.IsCanonicalRelative(SourceAssetPath))
        {
            throw new ArgumentException("Source asset provenance must be canonical and relative.", nameof(SourceAssetPath));
        }

        if (Kind == UpdateComparisonTargetKind.GeneratedRegion)
        {
            if (SourceAssetPath is not null || SourceAssetPresentInCurrentInventory is not null)
            {
                throw new ArgumentException("Generated comparisons require null source provenance.", nameof(SourceAssetPath));
            }

            if (FingerprintKind != UpdateComparisonFingerprintKind.ExactBytes)
            {
                throw new ArgumentException("Generated comparisons require exact-byte identity.", nameof(FingerprintKind));
            }
            return;
        }

        if ((string.IsNullOrWhiteSpace(SourceAssetPath) && IntendedState != UpdateComparisonIntendedState.Retired)
            || SourceAssetPresentInCurrentInventory is null)
        {
            throw new ArgumentException("Authored comparisons require source provenance.", nameof(SourceAssetPath));
        }
    }

    private void ValidateFingerprints()
    {
        if (!Enum.IsDefined(FingerprintKind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(FingerprintKind),
                FingerprintKind,
                "The comparison fingerprint kind is not defined.");
        }

        ValidateFingerprint(CurrentFingerprint, nameof(CurrentFingerprint));
        ValidateFingerprint(IntendedFingerprint, nameof(IntendedFingerprint));
    }

    private void ValidateStates()
    {
        if (!Enum.IsDefined(CurrentState))
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentState), CurrentState, "The comparison current state is not defined.");
        }
        if (!Enum.IsDefined(IntendedState))
        {
            throw new ArgumentOutOfRangeException(nameof(IntendedState), IntendedState, "The comparison intended state is not defined.");
        }
        if (!Enum.IsDefined(RetirementEligibility))
        {
            throw new ArgumentOutOfRangeException(nameof(RetirementEligibility), RetirementEligibility, "The retirement state is not defined.");
        }

        if ((CurrentState == UpdateComparisonCurrentState.Missing) != (CurrentFingerprint is null))
        {
            throw new ArgumentException("Missing current comparisons require a null current fingerprint.");
        }

        var intendedFingerprintMissing = IntendedState is
            UpdateComparisonIntendedState.Retired
            or UpdateComparisonIntendedState.Unavailable
            or UpdateComparisonIntendedState.Blocked;
        if (intendedFingerprintMissing != (IntendedFingerprint is null))
        {
            throw new ArgumentException("Intended fingerprint nullability does not match intended state.");
        }

    }

    private static void ValidateFingerprint(string? value, string parameterName)
    {
        if (value is not null && !UpdateValueSyntax.IsSha256(value))
        {
            throw new ArgumentException("Comparison fingerprints must be lowercase SHA-256 values.", parameterName);
        }
    }

    private static void ValidateRelativePath(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || !UpdateValueSyntax.IsCanonicalRelative(value))
        {
            throw new ArgumentException("Comparison paths must be canonical workspace-relative paths.", parameterName);
        }
    }
}
