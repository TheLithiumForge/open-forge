using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.Framework.Recovery.Comparison;

internal static class RecoveryEntryComparer
{
    // Pure identity classification receives independent IO facts. Missing is an
    // observed identity; a raw link is never replaced by its target's identity.
    internal static RecoveryEntryComparison Compare(RecoveryEntryComparisonInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Leaf.State is NoFollowLeafState.Inaccessible
            or NoFollowLeafState.Unknown)
        {
            return Classified(
                input,
                RecoveryBundleTargetComparisonState.Unavailable,
                input.Leaf.Failure?.DirectCause
                    ?? "The recovery target could not be safely observed.");
        }

        if (input.Leaf.State is NoFollowLeafState.Directory
            or NoFollowLeafState.Link
            or NoFollowLeafState.ReparsePoint
            or NoFollowLeafState.Special)
        {
            return Classified(
                input,
                RecoveryBundleTargetComparisonState.Blocked,
                "The recovery target is an unsupported or unsafe filesystem object.");
        }

        RecoveryEntryState observed;
        switch (input.Leaf.State)
        {
            case NoFollowLeafState.Missing:
                observed = RecoveryEntryState.Missing;
                break;
            case NoFollowLeafState.RelativeFileLink:
                observed = RecoveryEntryState.RelativeLink(
                    input.Leaf.RelativeFileLink
                        ?? throw new InvalidOperationException(
                            "A relative link observation requires its raw identity."));
                break;
            case NoFollowLeafState.OrdinaryFile:
                if (input.OrdinaryContent?.Failure is { } failure)
                {
                    return Classified(
                        input,
                        RecoveryBundleTargetComparisonState.Unavailable,
                        failure.DirectCause);
                }

                observed = RecoveryEntryState.Ordinary(
                    input.OrdinaryContent?.Identity
                        ?? throw new InvalidOperationException(
                            "An ordinary recovery observation requires content identity."));
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    input.Leaf.State,
                    "The no-follow leaf state is not defined.");
        }

        return new RecoveryEntryComparison
        {
            Input = input,
            State = ReadComparisonState(observed, input.Context.Entry),
            Observed = observed,
            Cause = null,
        };
    }

    private static RecoveryBundleTargetComparisonState ReadComparisonState(
        RecoveryEntryState observed,
        RecoveryEntry entry)
    {
        if (observed == entry.Prior)
        {
            return RecoveryBundleTargetComparisonState.Prior;
        }

        if (observed == entry.Intended)
        {
            return RecoveryBundleTargetComparisonState.Intended;
        }

        return RecoveryBundleTargetComparisonState.Third;
    }

    private static RecoveryEntryComparison Classified(
        RecoveryEntryComparisonInput input,
        RecoveryBundleTargetComparisonState state,
        string cause)
        => new()
        {
            Input = input,
            State = state,
            Observed = null,
            Cause = cause,
        };
}
