using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;

internal sealed record RelativeFileLinkReceipt
{
    private RelativeFileLinkReceipt(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        NoFollowLeafObservation? after,
        FilesystemEffectState effectState,
        FilesystemVerificationState verificationState,
        FilesystemNotStartedReason? notStartedReason,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(before);
        if (after is not null
            && !string.Equals(before.LogicalPath, after.LogicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A link receipt after-state must describe the observed logical target.",
                nameof(after));
        }

        if (notStartedReason is not null
            && effectState != FilesystemEffectState.NotStarted)
        {
            throw new ArgumentException(
                "Only a not-started link receipt may carry a not-started reason.",
                nameof(notStartedReason));
        }

        Effect = effect;
        Before = before;
        After = after;
        EffectState = effectState;
        VerificationState = verificationState;
        NotStartedReason = notStartedReason;
        Cause = cause;
    }

    internal RelativeFileLinkEffect Effect { get; }

    internal NoFollowLeafObservation Before { get; }

    internal NoFollowLeafObservation? After { get; }

    internal FilesystemEffectState EffectState { get; }

    internal FilesystemVerificationState VerificationState { get; }

    internal FilesystemNotStartedReason? NotStartedReason { get; }

    internal string? Cause { get; }

    internal static RelativeFileLinkReceipt Verified(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        NoFollowLeafObservation after)
    {
        ArgumentNullException.ThrowIfNull(after);
        ValidateIntended(effect, after);
        return new RelativeFileLinkReceipt(
            effect,
            before,
            after,
            FilesystemEffectState.Applied,
            FilesystemVerificationState.Verified,
            notStartedReason: null,
            cause: null);
    }

    internal static RelativeFileLinkReceipt VerificationFailed(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        NoFollowLeafObservation after,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(after);
        if (effect.Intended.MatchesObservation(after))
        {
            throw new ArgumentException(
                "A verification-failed link receipt cannot carry the intended after-state.",
                nameof(after));
        }

        return new RelativeFileLinkReceipt(
            effect,
            before,
            after,
            FilesystemEffectState.Applied,
            FilesystemVerificationState.Failed,
            notStartedReason: null,
            ValidateCause(cause));
    }

    internal static RelativeFileLinkReceipt NotStarted(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        FilesystemNotStartedReason reason,
        string cause)
    {
        _ = reason switch
        {
            FilesystemNotStartedReason.Cancelled
                or FilesystemNotStartedReason.TargetChanged
                or FilesystemNotStartedReason.ApplicationFailed
                or FilesystemNotStartedReason.ContractRejected => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The not-started reason is not defined."),
        };

        return new RelativeFileLinkReceipt(
            effect,
            before,
            after: null,
            FilesystemEffectState.NotStarted,
            FilesystemVerificationState.NotStarted,
            reason,
            ValidateCause(cause));
    }

    internal static RelativeFileLinkReceipt CompletionUnknown(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        NoFollowLeafObservation? after,
        string cause)
    {
        if (after is not null && effect.Intended.MatchesObservation(after))
        {
            throw new ArgumentException(
                "An unknown-completion link receipt cannot carry the intended after-state.",
                nameof(after));
        }

        return new RelativeFileLinkReceipt(
            effect,
            before,
            after,
            FilesystemEffectState.Unknown,
            FilesystemVerificationState.Failed,
            notStartedReason: null,
            ValidateCause(cause));
    }

    private static void ValidateIntended(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation after)
    {
        if (!effect.Intended.MatchesObservation(after))
        {
            throw new ArgumentException(
                "A verified link receipt requires the exact intended after-state.",
                nameof(after));
        }
    }

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause;
    }
}
