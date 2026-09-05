using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusRecoveryFindingAggregator
{
    internal static void Add(
        StatusFindingCollector findings,
        RecoveryResidualStatusView observation,
        StatusRecovery recovery)
    {
        switch (observation.State)
        {
            case OperationalViewState.Complete:
                break;
            case OperationalViewState.Incomplete:
                findings.Add(StatusFindingCode.RecoveryCatalogueUnavailable, null,
                    "The recovery-candidate catalogue is incomplete.");
                break;
            case OperationalViewState.Blocked:
                findings.Add(StatusFindingCode.WorkspaceUnsafe, null,
                    "The recovery-candidate boundary is blocked.");
                break;
            case OperationalViewState.Interrupted:
                findings.Add(StatusFindingCode.Interrupted, null,
                    "The recovery-candidate observation was interrupted.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(observation), observation.State,
                    "The recovery-residual view state is not defined.");
        }

        foreach (var candidate in recovery.Candidates)
        {
            AddCandidate(findings, candidate);
        }
    }

    private static void AddCandidate(StatusFindingCollector findings, StatusRecoveryCandidate candidate)
    {
        StatusFindingCode? code = (candidate.Kind, candidate.Integrity) switch
        {
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Verified)
                => StatusFindingCode.RecoveryCandidateVerified,
            (RecoveryBundleCandidateKind.Draft, RecoveryBundleIntegrity.Incomplete)
                => StatusFindingCode.RecoveryDraftIncomplete,
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Malformed)
                => StatusFindingCode.RecoveryFinalMalformed,
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Unsupported)
                => StatusFindingCode.RecoveryFinalUnsupported,
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Unavailable)
                => StatusFindingCode.RecoveryFinalUnavailable,
            (RecoveryBundleCandidateKind.Draft, _)
                => null,
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Incomplete)
                => StatusFindingCode.RecoveryFinalUnavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(candidate), candidate.Integrity,
                "The recovery candidate state is not defined."),
        };
        if (code.HasValue)
        {
            findings.Add(code.Value, candidate.Path, "The exact-name recovery candidate requires attention.");
        }
    }
}
