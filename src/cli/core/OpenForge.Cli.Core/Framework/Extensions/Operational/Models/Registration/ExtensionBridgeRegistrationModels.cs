using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

internal enum ExtensionBridgeRegistrationState
{
    Current,
    Missing,
    Unreadable,
    Inconsistent,
}

internal enum ExtensionBridgeRegistrationCoverage
{
    Complete,
    Incomplete,
    Blocked,
}

internal sealed record ExtensionBridgeRegistrationObservation
{
    internal ExtensionBridgeRegistrationObservation(BridgeCandidate candidate, BridgeObservationComparison comparison)
    {
        var targetPath = candidate.TargetPath;
        var owners = candidate.Owners;
        var packageId = candidate.PackageId;
        var reviewedSourceIdentity = candidate.SourceIdentity;
        var sourceAssetPath = candidate.SourceAssetPath;
        var parentPath = comparison.ParentPath;
        var expectedEntry = comparison.ExpectedEntry;
        var actualEntry = comparison.ActualEntry;
        var state = comparison.State;
        var cause = comparison.Cause;
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewedSourceIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAssetPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedEntry);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension bridge-registration state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(owners);
        var ownerValues = owners
            .Select(value => value ?? throw new ArgumentException(
                "Extension bridge-registration owners cannot contain null members.",
                nameof(owners)))
            .ToArray();
        if (ownerValues.Length == 0
            || ownerValues.Any(string.IsNullOrWhiteSpace)
            || ownerValues.Distinct(StringComparer.Ordinal).Count() != ownerValues.Length)
        {
            throw new ArgumentException(
                "Extension bridge-registration observations require unique non-empty owners.",
                nameof(owners));
        }

        switch (state)
        {
            case ExtensionBridgeRegistrationState.Current
                when actualEntry is null
                    || !string.Equals(expectedEntry, actualEntry, StringComparison.Ordinal)
                    || cause is not null:
                throw new ArgumentException(
                    "A current bridge-registration observation requires an equal actual entry and no cause.",
                    nameof(actualEntry));
            case ExtensionBridgeRegistrationState.Missing
                when actualEntry is not null || cause is not null:
                throw new ArgumentException(
                    "A missing bridge-registration observation requires no actual entry or cause.",
                    nameof(actualEntry));
            case ExtensionBridgeRegistrationState.Unreadable
                when actualEntry is not null || string.IsNullOrWhiteSpace(cause):
                throw new ArgumentException(
                    "An unreadable bridge-registration observation requires a cause and no actual entry.",
                    nameof(cause));
            case ExtensionBridgeRegistrationState.Inconsistent
                when actualEntry is null
                    || string.Equals(expectedEntry, actualEntry, StringComparison.Ordinal)
                    || cause is not null:
                throw new ArgumentException(
                    "An inconsistent bridge-registration observation requires unequal entries and no cause.",
                    nameof(actualEntry));
        }

        TargetPath = targetPath;
        Owners = new ReadOnlyCollection<string>(ownerValues);
        PackageId = packageId;
        ReviewedSourceIdentity = reviewedSourceIdentity;
        SourceAssetPath = sourceAssetPath;
        ParentPath = parentPath;
        ExpectedEntry = expectedEntry;
        ActualEntry = actualEntry;
        State = state;
        Cause = cause;
    }

    internal string TargetPath { get; }

    internal IReadOnlyList<string> Owners { get; }

    internal string PackageId { get; }

    internal string ReviewedSourceIdentity { get; }

    internal string SourceAssetPath { get; }

    internal string ParentPath { get; }

    internal string ExpectedEntry { get; }

    internal string? ActualEntry { get; }

    internal ExtensionBridgeRegistrationState State { get; }

    internal string? Cause { get; }
}

internal sealed class ExtensionBridgeRegistrationFacts
{
    private ExtensionBridgeRegistrationFacts(
        ExtensionBridgeRegistrationCoverage coverage,
        IReadOnlyList<ExtensionBridgeRegistrationObservation> observations,
        string? cause)
    {
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coverage),
                coverage,
                "The Extension bridge-registration coverage is not defined.");
        }

        ArgumentNullException.ThrowIfNull(observations);
        if (observations.Any(observation => observation is null))
        {
            throw new ArgumentException(
                "Extension bridge-registration observations cannot contain null members.",
                nameof(observations));
        }

        if (coverage == ExtensionBridgeRegistrationCoverage.Complete && cause is not null
            || coverage is ExtensionBridgeRegistrationCoverage.Incomplete
                or ExtensionBridgeRegistrationCoverage.Blocked
                && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "The Extension bridge-registration coverage and cause do not match.",
                nameof(cause));
        }

        Coverage = coverage;
        Observations = new ReadOnlyCollection<ExtensionBridgeRegistrationObservation>(
            observations.ToArray());
        Cause = cause;
    }

    internal ExtensionBridgeRegistrationCoverage Coverage { get; }

    internal IReadOnlyList<ExtensionBridgeRegistrationObservation> Observations { get; }

    internal string? Cause { get; }

    internal static ExtensionBridgeRegistrationFacts Complete(
        IEnumerable<ExtensionBridgeRegistrationObservation> observations)
        => new(ExtensionBridgeRegistrationCoverage.Complete, Snapshot(observations), null);

    internal static ExtensionBridgeRegistrationFacts Incomplete(string cause)
        => new(ExtensionBridgeRegistrationCoverage.Incomplete, [], cause);

    internal static ExtensionBridgeRegistrationFacts Blocked(string cause)
        => new(ExtensionBridgeRegistrationCoverage.Blocked, [], cause);

    private static IReadOnlyList<ExtensionBridgeRegistrationObservation> Snapshot(
        IEnumerable<ExtensionBridgeRegistrationObservation> observations)
    {
        ArgumentNullException.ThrowIfNull(observations);
        return observations
            .Select(observation => observation ?? throw new ArgumentException(
                "Extension bridge-registration observations cannot contain null members.",
                nameof(observations)))
            .ToArray();
    }
}
