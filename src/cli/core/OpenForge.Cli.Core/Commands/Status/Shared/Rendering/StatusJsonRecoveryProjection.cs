using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonRecoveryProjection
{
    internal static StatusJsonRecovery Create(StatusRecovery recovery)
        => new()
        {
            VerifiedFinals = StatusJsonContextProjection.Value(recovery.VerifiedFinals),
            IncompleteDrafts = StatusJsonContextProjection.Value(recovery.IncompleteDrafts),
            Candidates = recovery.Candidates.Select(candidate => new StatusJsonRecoveryCandidate
            {
                Path = candidate.Path,
                Kind = StatusWireVocabulary.RecoveryKind(candidate.Kind),
                Integrity = StatusWireVocabulary.RecoveryIntegrity(candidate.Integrity),
            }).ToArray(),
        };
}
