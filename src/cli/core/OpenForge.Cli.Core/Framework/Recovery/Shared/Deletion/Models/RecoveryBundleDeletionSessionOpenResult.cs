using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;

internal enum RecoveryBundleDeletionSessionOpenState
{
    Opened,
    Changed,
    Unavailable,
    Blocked,
    Cancelled,
}

internal sealed record RecoveryBundleDeletionSessionOpenResult
{
    public required RecoveryBundleDeletionSessionOpenState State { get; init; }

    public required RecoveryBundleCatalogueResult? ObservedCatalogue { get; init; }

    public required RecoveryBundleDeletionSession? Session { get; init; }

    public required FilesystemFailure? Failure { get; init; }

    public required string? Cause { get; init; }
}
