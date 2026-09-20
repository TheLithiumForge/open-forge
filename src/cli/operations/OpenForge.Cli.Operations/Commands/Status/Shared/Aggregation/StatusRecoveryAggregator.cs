using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusRecoveryAggregator
{
    internal static StatusRecovery Build(RecoveryResidualStatusView recovery)
    {
        var candidates = recovery.Candidates
            .OrderBy(candidate => candidate.Path, StringComparer.Ordinal)
            .Select(candidate => new StatusRecoveryCandidate(
                candidate.Path,
                StatusStateMap.RecoveryCandidate(candidate.Kind),
                StatusStateMap.RecoveryIntegrity(candidate.Integrity)))
            .ToArray();
        var state = recovery.State == OperationalViewState.Complete
            ? StatusValueState.Available
            : StatusValueState.Unavailable;
        return new StatusRecovery
        {
            VerifiedFinals = StatusMeasurementCalculator.Value(
                state,
                candidates.LongCount(candidate => candidate.Kind == StatusRecoveryCandidateKind.Final
                    && candidate.Integrity == StatusRecoveryIntegrity.Verified)),
            IncompleteDrafts = StatusMeasurementCalculator.Value(
                state,
                candidates.LongCount(candidate => candidate.Kind == StatusRecoveryCandidateKind.Draft
                    && candidate.Integrity == StatusRecoveryIntegrity.Incomplete)),
            Candidates = candidates,
        };
    }

    internal static StatusRecovery Unavailable()
    {
        var unavailable = new StatusIntegerValue(StatusValueState.Unavailable, null);
        return new StatusRecovery
        {
            VerifiedFinals = unavailable,
            IncompleteDrafts = unavailable,
            Candidates = [],
        };
    }
}
