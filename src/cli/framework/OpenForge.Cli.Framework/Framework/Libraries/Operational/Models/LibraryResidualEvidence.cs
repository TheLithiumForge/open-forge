using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

internal sealed record LibraryResidualEvidence
{
    internal LibraryResidualEvidence(
        LibraryId libraryId,
        LibraryRegistrationRead currentRecord,
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

        if (!(currentRecord.State == LibraryRegistrationReadState.Complete
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
    internal LibraryRegistrationRead CurrentRecord { get; }
    internal LibraryRecoveryPriorRecord? VerifiedPriorRecord { get; }
    internal RecoveryEntrySetObservation Residual { get; }
    internal RecoveryEntryComparison Entry { get; }
}

internal sealed record LibraryRecoveryPriorRecord
{
    internal LibraryRecoveryPriorRecord(LibraryRegistrationSet record, RecoveryEntry entry, RecoveryContentIdentity payloadIdentity)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(payloadIdentity);
        if (!string.Equals(entry.TargetPath, WorkspaceOwnershipDefinitions.RelativePath, StringComparison.Ordinal)
            || entry.Prior.OrdinaryFile != payloadIdentity || entry.PriorPayload is null)
        {
            throw new ArgumentException("Prior record evidence requires its exact verified ordinary payload identity.", nameof(payloadIdentity));
        }

        Record = record;
        Entry = entry;
        PayloadIdentity = payloadIdentity;
    }

    internal LibraryRegistrationSet Record { get; }
    internal RecoveryEntry Entry { get; }
    internal RecoveryContentIdentity PayloadIdentity { get; }
}
