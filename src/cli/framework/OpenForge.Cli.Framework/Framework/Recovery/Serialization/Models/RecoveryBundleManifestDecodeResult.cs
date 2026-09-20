namespace OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

internal enum RecoveryBundleManifestState
{
    Valid,
    Malformed,
    Unsupported,
}

internal sealed record RecoveryBundleManifestDecodeResult
{
    public required RecoveryBundleManifestState State { get; init; }

    public RecoveryBundleManifestV1? Document { get; init; }

    public OpenForge.Cli.Core.Framework.Recovery.Models.Identity.RecoveryBundleAttribution? Attribution { get; init; }

    public required System.Collections.Immutable.ImmutableArray<OpenForge.Cli.Core.Framework.Recovery.Models.Entries.RecoveryEntry> Entries { get; init; }

    public string? Cause { get; init; }
}
