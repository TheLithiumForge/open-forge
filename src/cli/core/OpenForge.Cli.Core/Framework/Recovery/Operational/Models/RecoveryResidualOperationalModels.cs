using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

internal sealed record RecoveryCandidateObservation(
    string Path,
    RecoveryBundleCandidateKind Kind,
    RecoveryBundleIntegrity Integrity);

internal sealed class RecoveryDoctorCandidateObservation
{
    private RecoveryDoctorCandidateObservation(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundleComparison? comparison)
    {
        var attribution = candidate.Verified?.Attribution;
        if (candidate.Verified is null && comparison is not null
            || attribution?.Producer == RecoveryBundleProducer.Framework && comparison is null
            || attribution?.Producer != RecoveryBundleProducer.Framework && comparison is not null)
        {
            throw new ArgumentException(
                "Recovery attribution and comparison must match one verified Framework final.",
                nameof(comparison));
        }

        if (comparison is not null
            && (!Equals(comparison.Attribution, attribution)
                || !string.Equals(comparison.BundlePath, candidate.Path, StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "A recovery comparison must match its exact verified candidate.",
                nameof(comparison));
        }

        Path = candidate.Path;
        Kind = candidate.Kind;
        Integrity = candidate.Integrity;
        Attribution = attribution;
        Comparison = comparison;
        Cause = candidate.Cause;
    }

    internal string Path { get; }

    internal RecoveryBundleCandidateKind Kind { get; }

    internal RecoveryBundleIntegrity Integrity { get; }

    internal RecoveryBundleAttribution? Attribution { get; }

    internal RecoveryBundleComparison? Comparison { get; }

    internal string? Cause { get; }

    internal static RecoveryDoctorCandidateObservation Create(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundleComparison? comparison)
        => new(candidate, comparison);
}

internal sealed record RecoveryResidualStatusView(
    OperationalViewState State,
    IReadOnlyList<RecoveryCandidateObservation> Candidates);

internal sealed record RecoveryResidualDoctorView(
    OperationalViewState State,
    IReadOnlyList<RecoveryDoctorCandidateObservation> Candidates,
    string? Cause);
