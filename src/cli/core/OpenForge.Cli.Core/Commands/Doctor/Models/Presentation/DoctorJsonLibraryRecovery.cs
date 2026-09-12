using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed record DoctorJsonLibraryRecovery
{
    public required string LibraryId { get; init; }
    public required string BundlePath { get; init; }
    public required RecoveryBundleAttributionV1 Attribution { get; init; }
    public required int EntryOrdinal { get; init; }
    public required string LogicalPath { get; init; }
    public required string EntryKind { get; init; }
    public required string CurrentRecordState { get; init; }
    public required bool VerifiedPriorRecord { get; init; }
    public required RecoveryBundleManifestStateV1 Prior { get; init; }
    public required RecoveryBundleManifestStateV1 Intended { get; init; }
    public required RecoveryBundleManifestStateV1? Observed { get; init; }
    public required string Comparison { get; init; }
}
