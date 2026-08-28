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
    Attention,
    Blocked,
    Cancelled,
}

internal sealed record RecoveryBundleDeletionResult
{
    private RecoveryBundleDeletionResult(
        RecoveryBundleDeletionState state,
        string? residualPath,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        ResidualPath = residualPath;
        Failure = failure;
        Cause = cause;
    }

    internal RecoveryBundleDeletionState State { get; }

    internal string? ResidualPath { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RecoveryBundleDeletionResult Deleted()
        => new(
            state: RecoveryBundleDeletionState.Deleted,
            residualPath: null,
            failure: null,
            cause: null);

    internal static RecoveryBundleDeletionResult Attention(
        string residualPath,
        string cause,
        FilesystemFailure? failure = null)
        => new(
            state: RecoveryBundleDeletionState.Attention,
            residualPath: residualPath,
            failure: failure,
            cause: cause);

    internal static RecoveryBundleDeletionResult Blocked(string cause)
        => new(
            state: RecoveryBundleDeletionState.Blocked,
            residualPath: null,
            failure: null,
            cause: cause);

    internal static RecoveryBundleDeletionResult Cancelled()
        => new(
            state: RecoveryBundleDeletionState.Cancelled,
            residualPath: null,
            failure: null,
            cause: null);
}
