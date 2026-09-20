namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

internal sealed record FilesystemReceiptState
{
    private FilesystemReceiptState(
        FilesystemEffectState effectState,
        FilesystemVerificationState verificationState,
        FilesystemNotStartedReason? notStartedReason)
    {
        EffectState = effectState;
        VerificationState = verificationState;
        NotStartedReason = notStartedReason;
    }

    internal FilesystemEffectState EffectState { get; }

    internal FilesystemVerificationState VerificationState { get; }

    internal FilesystemNotStartedReason? NotStartedReason { get; }

    internal static FilesystemReceiptState VerificationFailed { get; } = new(
        FilesystemEffectState.Applied,
        FilesystemVerificationState.Failed,
        notStartedReason: null);

    internal static FilesystemReceiptState CompletionUnknown { get; } = new(
        FilesystemEffectState.Unknown,
        FilesystemVerificationState.NotStarted,
        notStartedReason: null);

    internal static FilesystemReceiptState NotStarted(FilesystemNotStartedReason reason)
        => new(
            FilesystemEffectState.NotStarted,
            FilesystemVerificationState.NotStarted,
            reason);
}
