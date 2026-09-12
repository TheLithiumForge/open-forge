using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

internal sealed record RecoveryEntryComparison
{
    public required RecoveryEntryComparisonInput Input { get; init; }
    public required RecoveryBundleTargetComparisonState State { get; init; }
    public required RecoveryEntryState? Observed { get; init; }
    public required string? Cause { get; init; }
}
