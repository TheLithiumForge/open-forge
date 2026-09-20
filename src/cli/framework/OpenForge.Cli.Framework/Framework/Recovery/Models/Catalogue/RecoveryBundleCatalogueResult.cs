using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

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
