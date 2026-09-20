using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Models.Operation;

internal sealed record UpdateRecoveryDeletionOutcome
{
    public required UpdateRecoveryState State { get; init; }

    public required string? ResidualPath { get; init; }

    public required UpdateFinding? Finding { get; init; }
}
