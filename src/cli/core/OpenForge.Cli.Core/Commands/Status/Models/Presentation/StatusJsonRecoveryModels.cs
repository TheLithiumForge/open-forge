namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusJsonRecovery
{
    public required StatusJsonIntegerValue VerifiedFinals { get; init; }

    public required StatusJsonIntegerValue IncompleteDrafts { get; init; }

    public required StatusJsonRecoveryCandidate[] Candidates { get; init; }
}
internal sealed class StatusJsonRecoveryCandidate
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Integrity { get; init; }
}
