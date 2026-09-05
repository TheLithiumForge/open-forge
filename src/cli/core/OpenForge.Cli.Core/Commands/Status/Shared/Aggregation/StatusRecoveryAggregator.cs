using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusRecoveryAggregator
{
    internal static StatusRecovery Build(RecoveryResidualStatusView recovery)
    {
        var candidates = recovery.Candidates
            .OrderBy(candidate => candidate.Path, StringComparer.Ordinal)
            .Select(candidate => new StatusRecoveryCandidate(candidate.Path, candidate.Kind, candidate.Integrity))
            .ToArray();
        var state = recovery.State == OperationalViewState.Complete
            ? OperationalValueState.Available
            : OperationalValueState.Unavailable;
        return new StatusRecovery
        {
            VerifiedFinals = StatusMeasurementCalculator.Value(
                state,
                candidates.LongCount(candidate => candidate.Kind == RecoveryBundleCandidateKind.Final
                    && candidate.Integrity == RecoveryBundleIntegrity.Verified)),
            IncompleteDrafts = StatusMeasurementCalculator.Value(
                state,
                candidates.LongCount(candidate => candidate.Kind == RecoveryBundleCandidateKind.Draft
                    && candidate.Integrity == RecoveryBundleIntegrity.Incomplete)),
            Candidates = candidates,
        };
    }

    internal static StatusRecovery Unavailable()
    {
        var unavailable = new StatusIntegerValue(OperationalValueState.Unavailable, null);
        return new StatusRecovery
        {
            VerifiedFinals = unavailable,
            IncompleteDrafts = unavailable,
            Candidates = [],
        };
    }
}
