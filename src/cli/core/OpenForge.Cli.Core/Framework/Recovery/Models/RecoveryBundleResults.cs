using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

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

internal enum RecoveryBundleReadState
{
    Valid,
    Malformed,
    Unsupported,
    Unavailable,
    Cancelled,
}

internal sealed record RecoveryBundleVerifiedRead
{
    public required string BundlePath { get; init; }

    public required string WorkspacePhysicalPath { get; init; }

    public required string WorkspaceKey { get; init; }

    public required string Command { get; init; }

    public required RecoveryBundleAttribution Attribution { get; init; }

    public required Guid OperationId { get; init; }

    public required ImmutableArray<RecoveryBundleEntry> Entries { get; init; }
}

internal sealed record RecoveryBundleReadResult
{
    private RecoveryBundleReadResult(
        RecoveryBundleReadState state,
        RecoveryBundleVerifiedRead? verified,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Verified = verified;
        Failure = failure;
        Cause = cause;
    }

    internal RecoveryBundleReadState State { get; }

    internal RecoveryBundleVerifiedRead? Verified { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleReadResult Valid(RecoveryBundleVerifiedRead verified)
    {
        ArgumentNullException.ThrowIfNull(verified);
        return new RecoveryBundleReadResult(
            state: RecoveryBundleReadState.Valid,
            verified: verified,
            failure: null,
            cause: null);
    }

    internal static RecoveryBundleReadResult Classified(
        RecoveryBundleReadState state,
        string cause,
        FilesystemFailure? failure = null)
    {
        if (state is RecoveryBundleReadState.Valid or RecoveryBundleReadState.Cancelled)
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new RecoveryBundleReadResult(
            state: state,
            verified: null,
            failure: failure,
            cause: cause);
    }

    internal static RecoveryBundleReadResult Cancelled()
        => new(
            state: RecoveryBundleReadState.Cancelled,
            verified: null,
            failure: null,
            cause: null);
}

internal sealed record RecoveryBundleFinalReadResult
{
    public required RecoveryBundleReadResult Read { get; init; }

    public RecoveryBundlePreparation? Preparation { get; init; }
}

internal enum RecoveryBundleCandidateKind
{
    Final,
    Draft,
}

internal enum RecoveryBundleIntegrity
{
    Verified,
    Malformed,
    Unsupported,
    Unavailable,
    Incomplete,
}

internal sealed record RecoveryBundleCandidateSnapshot
{
    private RecoveryBundleCandidateSnapshot(
        string path,
        RecoveryBundleCandidateKind kind,
        RecoveryBundleIntegrity integrity,
        RecoveryBundleVerifiedRead? verified,
        FilesystemFailure? failure,
        string? cause)
    {
        Path = path;
        Kind = kind;
        Integrity = integrity;
        Verified = verified;
        Failure = failure;
        Cause = cause;
    }

    internal string Path { get; }

    internal RecoveryBundleCandidateKind Kind { get; }

    internal RecoveryBundleIntegrity Integrity { get; }

    internal RecoveryBundleVerifiedRead? Verified { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleCandidateSnapshot VerifiedFinal(RecoveryBundleVerifiedRead verified)
    {
        ArgumentNullException.ThrowIfNull(verified);
        return new(
            path: verified.BundlePath,
            kind: RecoveryBundleCandidateKind.Final,
            integrity: RecoveryBundleIntegrity.Verified,
            verified: verified,
            failure: null,
            cause: null);
    }

    internal static RecoveryBundleCandidateSnapshot IncompleteDraft(string path)
        => new(
            path: System.IO.Path.GetFullPath(path),
            kind: RecoveryBundleCandidateKind.Draft,
            integrity: RecoveryBundleIntegrity.Incomplete,
            verified: null,
            failure: null,
            cause: "The exact-name recovery draft is incomplete and its contents are not trusted.");

    internal static RecoveryBundleCandidateSnapshot Classified(
        string path,
        RecoveryBundleCandidateKind kind,
        RecoveryBundleIntegrity integrity,
        string cause,
        FilesystemFailure? failure = null)
    {
        if (integrity is RecoveryBundleIntegrity.Verified or RecoveryBundleIntegrity.Incomplete)
        {
            throw new ArgumentOutOfRangeException(nameof(integrity));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new RecoveryBundleCandidateSnapshot(
            path: System.IO.Path.GetFullPath(path),
            kind: kind,
            integrity: integrity,
            verified: null,
            failure: failure,
            cause: cause);
    }
}

internal enum RecoveryBundleCatalogueState
{
    Available,
    Unavailable,
    Cancelled,
}

internal sealed record RecoveryBundleCatalogueResult
{
    private RecoveryBundleCatalogueResult(
        RecoveryBundleCatalogueState state,
        ImmutableArray<RecoveryBundleCandidateSnapshot> candidates,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Candidates = candidates;
        Failure = failure;
        Cause = cause;
    }

    internal RecoveryBundleCatalogueState State { get; }

    internal ImmutableArray<RecoveryBundleCandidateSnapshot> Candidates { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleCatalogueResult Available(
        ImmutableArray<RecoveryBundleCandidateSnapshot> candidates)
        => new(
            state: RecoveryBundleCatalogueState.Available,
            candidates: candidates,
            failure: null,
            cause: null);

    internal static RecoveryBundleCatalogueResult Unavailable(
        string cause,
        FilesystemFailure? failure = null)
        => new(
            state: RecoveryBundleCatalogueState.Unavailable,
            candidates: [],
            failure: failure,
            cause: cause);

    internal static RecoveryBundleCatalogueResult Cancelled()
        => new(
            state: RecoveryBundleCatalogueState.Cancelled,
            candidates: [],
            failure: null,
            cause: null);
}

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
