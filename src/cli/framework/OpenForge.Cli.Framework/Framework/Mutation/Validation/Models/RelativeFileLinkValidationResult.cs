using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

internal enum RelativeFileLinkValidationState
{
    Matched,
    Mismatched,
    Blocked,
    Failed,
    Cancelled,
}

internal sealed record RelativeFileLinkValidationResult
{
    private RelativeFileLinkValidationResult(
        RelativeFileLinkValidationState state,
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation? expected,
        NoFollowLeafObservation? actual,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Effect = effect;
        Expected = expected;
        Actual = actual;
        Failure = failure;
        Cause = cause;
    }

    internal RelativeFileLinkValidationState State { get; }

    internal RelativeFileLinkEffect Effect { get; }

    internal NoFollowLeafObservation? Expected { get; }

    internal NoFollowLeafObservation? Actual { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RelativeFileLinkValidationResult Matched(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation expected,
        NoFollowLeafObservation actual)
        => Create(
            RelativeFileLinkValidationState.Matched,
            effect,
            expected,
            actual,
            failure: null,
            cause: null);

    internal static RelativeFileLinkValidationResult Mismatched(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation expected,
        NoFollowLeafObservation actual,
        string cause)
        => Create(
            RelativeFileLinkValidationState.Mismatched,
            effect,
            expected,
            actual,
            failure: null,
            ValidateCause(cause));

    internal static RelativeFileLinkValidationResult Blocked(
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation? actual,
        string cause)
        => Create(
            RelativeFileLinkValidationState.Blocked,
            effect,
            expected: null,
            actual,
            failure: null,
            ValidateCause(cause));

    internal static RelativeFileLinkValidationResult Failed(
        RelativeFileLinkEffect effect,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return Create(
            RelativeFileLinkValidationState.Failed,
            effect,
            expected: null,
            actual: null,
            failure,
            failure.DirectCause);
    }

    internal static RelativeFileLinkValidationResult Cancelled(
        RelativeFileLinkEffect effect)
        => Create(
            RelativeFileLinkValidationState.Cancelled,
            effect,
            expected: null,
            actual: null,
            failure: null,
            cause: null);

    private static RelativeFileLinkValidationResult Create(
        RelativeFileLinkValidationState state,
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation? expected,
        NoFollowLeafObservation? actual,
        FilesystemFailure? failure,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(effect);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The relative file-link validation state is not defined.");
        }

        return new RelativeFileLinkValidationResult(
            state,
            effect,
            expected,
            actual,
            failure,
            cause);
    }

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause;
    }
}
