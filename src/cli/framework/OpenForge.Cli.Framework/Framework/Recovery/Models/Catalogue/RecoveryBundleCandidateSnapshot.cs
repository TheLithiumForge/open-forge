using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

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
        if (integrity is not (RecoveryBundleIntegrity.Malformed or RecoveryBundleIntegrity.Unsupported or RecoveryBundleIntegrity.Unavailable))
        {
            throw new ArgumentOutOfRangeException(nameof(integrity));
        }

        if (kind is not (RecoveryBundleCandidateKind.Final or RecoveryBundleCandidateKind.Draft))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
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
