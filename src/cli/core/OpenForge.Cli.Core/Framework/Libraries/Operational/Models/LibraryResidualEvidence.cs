using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

namespace OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

internal sealed record LibraryResidualEvidence
{
    internal LibraryResidualEvidence(
        LibraryId libraryId,
        LibrariesRecordRead currentRecord,
        LibraryRecoveryPriorRecord? verifiedPriorRecord,
        RecoveryEntrySetObservation residual,
        RecoveryEntryComparison entry)
    {
        ArgumentNullException.ThrowIfNull(libraryId);
        ArgumentNullException.ThrowIfNull(currentRecord);
        ArgumentNullException.ThrowIfNull(residual);
        ArgumentNullException.ThrowIfNull(entry);
        var verified = residual.Candidate.Verified
            ?? throw new ArgumentException("Library residual evidence requires a verified final.", nameof(residual));
        if (verified.Attribution.Producer != RecoveryBundleProducer.Library
            || !residual.Entries.Contains(entry))
        {
            throw new ArgumentException("A Library residual requires its exact Library-attributed candidate and observed entry.", nameof(residual));
        }

        if (verifiedPriorRecord is { } priorRecord
            && !verified.Entries.Contains(priorRecord.Entry))
        {
            throw new ArgumentException("Prior record evidence must retain an entry from the exact verified residual.", nameof(verifiedPriorRecord));
        }

        if (!(currentRecord.State == LibrariesRecordReadState.Complete
                && currentRecord.Record?.Libraries.Any(record => record.Id == libraryId) == true)
            && verifiedPriorRecord?.Record.Libraries.Any(record => record.Id == libraryId) != true)
        {
            throw new ArgumentException("Library identity requires current or verified prior current-v1 record membership.", nameof(libraryId));
        }

        LibraryId = libraryId;
        CurrentRecord = currentRecord;
        VerifiedPriorRecord = verifiedPriorRecord;
        Residual = residual;
        Entry = entry;
    }

    internal LibraryId LibraryId { get; }
    internal LibrariesRecordRead CurrentRecord { get; }
    internal LibraryRecoveryPriorRecord? VerifiedPriorRecord { get; }
    internal RecoveryEntrySetObservation Residual { get; }
    internal RecoveryEntryComparison Entry { get; }
}

internal sealed record LibraryRecoveryPriorRecord
{
    internal LibraryRecoveryPriorRecord(LibrariesRecord record, RecoveryEntry entry, RecoveryContentIdentity payloadIdentity)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(payloadIdentity);
        if (!string.Equals(entry.TargetPath, LibraryPathIdentity.RecordRelativePath, StringComparison.Ordinal)
            || entry.Prior.OrdinaryFile != payloadIdentity || entry.PriorPayload is null)
        {
            throw new ArgumentException("Prior record evidence requires its exact verified ordinary payload identity.", nameof(payloadIdentity));
        }

        Record = record;
        Entry = entry;
        PayloadIdentity = payloadIdentity;
    }

    internal LibrariesRecord Record { get; }
    internal RecoveryEntry Entry { get; }
    internal RecoveryContentIdentity PayloadIdentity { get; }
}
