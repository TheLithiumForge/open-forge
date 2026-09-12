using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Application;

internal sealed record OrdinaryFileRecoveryResult
{
    public required RecoveryEntryComparison Before { get; init; }
    public required RecoveryEntryComparison? After { get; init; }
    public required FilesystemEffectState Effect { get; init; }
    public required FilesystemVerificationState Verification { get; init; }
    public required FilesystemFailure? Failure { get; init; }
    public required string? Cause { get; init; }
}
