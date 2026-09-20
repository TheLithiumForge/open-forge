using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

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

    public required ImmutableArray<RecoveryEntry> Entries { get; init; }
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
        if (state is not (RecoveryBundleReadState.Malformed or RecoveryBundleReadState.Unsupported or RecoveryBundleReadState.Unavailable))
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
