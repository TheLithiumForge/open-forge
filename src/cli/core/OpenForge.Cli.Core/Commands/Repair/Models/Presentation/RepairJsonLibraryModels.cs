using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Presentation;

internal sealed record RepairJsonLibraryProposal
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

internal sealed record RepairJsonSelectedLibrary
{
    public required RepairJsonLibraryProposal Proposal { get; init; }
    public required string[] Origins { get; init; }
}

internal sealed record RepairJsonLibraryStep
{
    public required int Ordinal { get; init; }
    public required RepairJsonSelectedLibrary Selection { get; init; }
    public required string[] Dependencies { get; init; }
    public required string[] Verification { get; init; }
    public required bool HasEffect { get; init; }
    public required string Outcome { get; init; }
}

internal sealed record RepairJsonLibraryExecution
{
    public required RepairJsonLibraryReceipt[] Receipts { get; init; }
    public required string? ForwardBundlePath { get; init; }
    public required string? ForwardCleanupState { get; init; }
    public required string[]? ForwardResidualPaths { get; init; }
    public required RepairJsonLibraryInterruption? Cancellation { get; init; }
    public required RepairJsonLibraryInterruption? UnexpectedFailure { get; init; }
}

internal sealed record RepairJsonLibraryReceipt
{
    public required RepairJsonLibraryProposal Proposal { get; init; }
    public required string OriginalBundlePath { get; init; }
    public required string Effect { get; init; }
    public required string Verification { get; init; }
    public required RecoveryBundleManifestStateV1? After { get; init; }
    public required string? AfterComparison { get; init; }
    public required string? Cause { get; init; }
}

internal sealed record RepairJsonLibraryInterruption(string Stage, int? EffectOrdinal, string? Cause);
