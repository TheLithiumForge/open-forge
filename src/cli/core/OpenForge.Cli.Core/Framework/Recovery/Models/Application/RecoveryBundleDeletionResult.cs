using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Application;

internal enum RecoveryBundleDeletionState
{
    Deleted,
    Failed,
    Blocked,
    Cancelled,
}

internal enum RecoveryBundleDisposition
{
    Removed,
    Retained,
    Unknown,
}

internal sealed record RecoveryBundleDeletionResult
{
    internal RecoveryBundleDeletionResult(
        RecoveryBundleDeletionState state,
        RecoveryBundleDisposition disposition,
        string? residualPath,
        FilesystemFailure? failure,
        string? cause)
    {
        _ = disposition switch
        {
            RecoveryBundleDisposition.Removed
                or RecoveryBundleDisposition.Retained
                or RecoveryBundleDisposition.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(disposition),
                disposition,
                "The recovery bundle disposition is not defined."),
        };

        var normalizedResidualPath = ValidateResidualPath(residualPath);
        ValidateCoherence(state, disposition, normalizedResidualPath, failure, cause);
        State = state;
        Disposition = disposition;
        ResidualPath = normalizedResidualPath;
        Failure = failure;
        Cause = cause;
    }

    internal RecoveryBundleDeletionState State { get; }

    internal RecoveryBundleDisposition Disposition { get; }

    internal string? ResidualPath { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleDeletionResult Deleted()
        => new(
            state: RecoveryBundleDeletionState.Deleted,
            disposition: RecoveryBundleDisposition.Removed,
            residualPath: null,
            failure: null,
            cause: null);

    internal static RecoveryBundleDeletionResult FailedRetained(
        string residualPath,
        string cause,
        FilesystemFailure? failure = null)
        => new(
            state: RecoveryBundleDeletionState.Failed,
            disposition: RecoveryBundleDisposition.Retained,
            residualPath: residualPath,
            failure: failure,
            cause: cause);

    internal static RecoveryBundleDeletionResult FailedUnknown(
        string residualPath,
        string cause,
        FilesystemFailure? failure = null)
        => new(
            state: RecoveryBundleDeletionState.Failed,
            disposition: RecoveryBundleDisposition.Unknown,
            residualPath: residualPath,
            failure: failure,
            cause: cause);

    internal static RecoveryBundleDeletionResult BlockedUnknown(
        string cause,
        string? residualPath = null)
        => new(
            state: RecoveryBundleDeletionState.Blocked,
            disposition: RecoveryBundleDisposition.Unknown,
            residualPath: residualPath,
            failure: null,
            cause: cause);

    internal static RecoveryBundleDeletionResult BlockedRetained(
        string residualPath,
        string cause)
        => new(
            state: RecoveryBundleDeletionState.Blocked,
            disposition: RecoveryBundleDisposition.Retained,
            residualPath: residualPath,
            failure: null,
            cause: cause);

    internal static RecoveryBundleDeletionResult CancelledUnknown()
        => new(
            state: RecoveryBundleDeletionState.Cancelled,
            disposition: RecoveryBundleDisposition.Unknown,
            residualPath: null,
            failure: null,
            cause: null);

    internal static RecoveryBundleDeletionResult CancelledRetained(string residualPath)
        => new(
            state: RecoveryBundleDeletionState.Cancelled,
            disposition: RecoveryBundleDisposition.Retained,
            residualPath: residualPath,
            failure: null,
            cause: null);

    private static void ValidateCoherence(
        RecoveryBundleDeletionState state,
        RecoveryBundleDisposition disposition,
        string? residualPath,
        FilesystemFailure? failure,
        string? cause)
    {
        var valid = state switch
        {
            RecoveryBundleDeletionState.Deleted => disposition == RecoveryBundleDisposition.Removed
                && residualPath is null
                && failure is null
                && cause is null,
            RecoveryBundleDeletionState.Failed => disposition is RecoveryBundleDisposition.Retained
                    or RecoveryBundleDisposition.Unknown
                && !string.IsNullOrWhiteSpace(cause),
            RecoveryBundleDeletionState.Blocked => disposition is RecoveryBundleDisposition.Retained
                    or RecoveryBundleDisposition.Unknown
                && failure is null
                && !string.IsNullOrWhiteSpace(cause),
            RecoveryBundleDeletionState.Cancelled => disposition is RecoveryBundleDisposition.Retained
                    or RecoveryBundleDisposition.Unknown
                && failure is null
                && cause is null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The recovery deletion state is not defined."),
        };
        if (!valid || disposition == RecoveryBundleDisposition.Retained && residualPath is null)
        {
            throw new ArgumentException(
                "Recovery bundle deletion facts do not match their state and disposition.");
        }
    }

    private static string? ValidateResidualPath(string? residualPath)
    {
        if (residualPath is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(residualPath);
        if (!Path.IsPathFullyQualified(residualPath))
        {
            throw new ArgumentException(
                "A recovery bundle deletion residual path must be fully qualified.",
                nameof(residualPath));
        }

        return Path.GetFullPath(residualPath);
    }
}
