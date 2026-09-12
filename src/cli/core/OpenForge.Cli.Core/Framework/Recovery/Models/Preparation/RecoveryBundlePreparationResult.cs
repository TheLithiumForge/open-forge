using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

internal enum RecoveryBundlePreparationState
{
    NotNeeded,
    Prepared,
    Incomplete,
    Blocked,
    Cancelled,
}

internal sealed record RecoveryBundlePreparationResult
{
    private RecoveryBundlePreparationResult(
        RecoveryBundlePreparationState state,
        RecoveryBundlePreparation? preparation,
        string? residualPath,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Preparation = preparation;
        ResidualPath = residualPath;
        Failure = failure;
        Cause = cause;
    }

    internal RecoveryBundlePreparationState State { get; }

    internal RecoveryBundlePreparation? Preparation { get; }

    internal string? ResidualPath { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundlePreparationResult NotNeeded()
        => new(
            state: RecoveryBundlePreparationState.NotNeeded,
            preparation: null,
            residualPath: null,
            failure: null,
            cause: null);

    internal static RecoveryBundlePreparationResult Prepared(RecoveryBundlePreparation preparation)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        return new RecoveryBundlePreparationResult(
            state: RecoveryBundlePreparationState.Prepared,
            preparation: preparation,
            residualPath: null,
            failure: null,
            cause: null);
    }

    internal static RecoveryBundlePreparationResult Incomplete(
        string cause,
        string? residualPath = null,
        FilesystemFailure? failure = null)
        => Classified(
            state: RecoveryBundlePreparationState.Incomplete,
            cause: cause,
            residualPath: residualPath,
            failure: failure);

    internal static RecoveryBundlePreparationResult Blocked(
        string cause,
        string? residualPath = null,
        FilesystemFailure? failure = null)
        => Classified(
            state: RecoveryBundlePreparationState.Blocked,
            cause: cause,
            residualPath: residualPath,
            failure: failure);

    internal static RecoveryBundlePreparationResult Cancelled(string? residualPath = null)
        => new(
            state: RecoveryBundlePreparationState.Cancelled,
            preparation: null,
            residualPath: residualPath,
            failure: null,
            cause: null);

    private static RecoveryBundlePreparationResult Classified(
        RecoveryBundlePreparationState state,
        string cause,
        string? residualPath,
        FilesystemFailure? failure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new RecoveryBundlePreparationResult(
            state: state,
            preparation: null,
            residualPath: residualPath,
            failure: failure,
            cause: cause);
    }
}
