namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusRecoveryCandidate(
    string Path,
    StatusRecoveryCandidateKind Kind,
    StatusRecoveryIntegrity Integrity);

internal sealed record StatusRecovery
{
    public required StatusIntegerValue VerifiedFinals { get; init; }

    public required StatusIntegerValue IncompleteDrafts { get; init; }

    public required IReadOnlyList<StatusRecoveryCandidate> Candidates { get; init; }
}
