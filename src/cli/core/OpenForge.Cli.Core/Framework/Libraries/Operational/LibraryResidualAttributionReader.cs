using System.Collections.Immutable;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Libraries.Operational;

internal static class LibraryResidualAttributionReader
{
    internal static async ValueTask<ImmutableArray<LibraryResidualEvidence>> ReadAsync(
        CliWorkspace workspace,
        LibraryDoctorView libraries,
        RecoveryResidualDoctorView recovery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(libraries);
        ArgumentNullException.ThrowIfNull(recovery);
        cancellationToken.ThrowIfCancellationRequested();
        if (libraries.Record.State is not (LibrariesRecordReadState.Complete or LibrariesRecordReadState.Missing))
        {
            return [];
        }

        var attributed = ImmutableArray.CreateBuilder<LibraryResidualEvidence>();
        foreach (var candidate in recovery.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var residual = candidate.EntryComparisons;
            if (candidate.Kind != RecoveryBundleCandidateKind.Final
                || candidate.Integrity != RecoveryBundleIntegrity.Verified
                || candidate.Attribution?.Producer != RecoveryBundleProducer.Library
                || residual is null
                || !IsLibraryCommand(candidate.Attribution.Operation, residual.Candidate.Verified?.Command))
            {
                continue;
            }

            var selected = await RecoveryBundleReader.ReadSelectedFinalAsync(
                workspace,
                residual.Candidate,
                cancellationToken).ConfigureAwait(false);
            if (selected.Read.State == RecoveryBundleReadState.Cancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
                continue;
            }

            if (selected.Read.Verified is not { } verified
                || verified.Attribution.Producer != RecoveryBundleProducer.Library
                || !IsLibraryCommand(verified.Attribution.Operation, verified.Command)
                || !string.Equals(
                    verified.Attribution.Subject.Identity,
                    WorkspaceIdentity.Key(workspace.PhysicalRoot),
                    StringComparison.Ordinal))
            {
                continue;
            }

            var prior = libraries.Record.State == LibrariesRecordReadState.Missing
                ? await ReadPriorRecordAsync(
                    residual,
                    verified,
                    cancellationToken).ConfigureAwait(false)
                : null;
            var record = libraries.Record.Record ?? prior?.Record;
            if (record is null
                || FindLibrary(record, verified.Entries) is not { } library)
            {
                continue;
            }

            foreach (var entry in residual.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.State != RecoveryBundleTargetComparisonState.Intended
                    || !IsAttributedEntry(library, entry.Input.Context.Entry))
                {
                    continue;
                }

                attributed.Add(new LibraryResidualEvidence(
                    library.Id,
                    libraries.Record,
                    prior,
                    residual,
                    entry));
            }
        }

        return [.. attributed
            .OrderBy(value => value.Residual.Candidate.Path, StringComparer.Ordinal)
            .ThenBy(value => value.Entry.Input.Context.Entry.Ordinal)];
    }

    private static bool IsLibraryCommand(RecoveryBundleOperation operation, string? command)
        => operation switch
        {
            RecoveryBundleOperation.Attach => string.Equals(command, "library attach", StringComparison.Ordinal),
            RecoveryBundleOperation.Sync => string.Equals(command, "library sync", StringComparison.Ordinal),
            RecoveryBundleOperation.Detach => string.Equals(command, "library detach", StringComparison.Ordinal),
            _ => false,
        };

    private static LibraryRecord? FindLibrary(
        LibrariesRecord record,
        IReadOnlyList<RecoveryEntry> entries)
    {
        LibraryRecord? selected = null;
        foreach (var entry in entries.Where(entry => entry.Kind is RecoveryEntryKind.RelativeFileLinkCreate
                     or RecoveryEntryKind.RelativeFileLinkDelete))
        {
            var matches = record.Libraries.Where(library => IsAttributedLink(library, entry)).ToArray();
            if (matches.Length != 1
                || selected is not null && selected.Id != matches[0].Id)
            {
                return null;
            }

            selected = matches[0];
        }

        return selected ?? (record.Libraries.Length == 1 ? record.Libraries[0] : null);
    }

    private static bool IsAttributedEntry(LibraryRecord library, RecoveryEntry entry)
        => entry.Kind switch
        {
            RecoveryEntryKind.RelativeFileLinkCreate or RecoveryEntryKind.RelativeFileLinkDelete =>
                IsAttributedLink(library, entry),
            RecoveryEntryKind.OrdinaryCreate or RecoveryEntryKind.OrdinaryReplace
                or RecoveryEntryKind.OrdinaryReplaceGeneratedRegion or RecoveryEntryKind.OrdinaryDelete => true,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "The recovery entry kind is not defined."),
        };

    private static bool IsAttributedLink(LibraryRecord library, RecoveryEntry entry)
    {
        var path = library.Paths.FirstOrDefault(value =>
            string.Equals(value.Value, entry.TargetPath, StringComparison.Ordinal));
        if (path is null)
        {
            return false;
        }

        var expected = LibraryPathIdentity.Map(library.SourceRoot, path).ExpectedRelativeLink.Value;
        return string.Equals(entry.Prior.RelativeFileLink?.RawRelativeTarget, expected, StringComparison.Ordinal)
            || string.Equals(entry.Intended.RelativeFileLink?.RawRelativeTarget, expected, StringComparison.Ordinal);
    }

    private static async ValueTask<LibraryRecoveryPriorRecord?> ReadPriorRecordAsync(
        RecoveryEntrySetObservation residual,
        RecoveryBundleVerifiedRead verified,
        CancellationToken cancellationToken)
    {
        var candidates = verified.Entries.Where(entry =>
                string.Equals(entry.TargetPath, LibraryPathIdentity.RecordRelativePath, StringComparison.Ordinal)
                && entry.Prior.OrdinaryFile is not null
                && entry.PriorPayload is not null)
            .ToArray();
        if (candidates.Length != 1)
        {
            return null;
        }

        var entry = candidates[0];
        var payload = await ReadPayloadAsync(
            verified.BundlePath,
            entry.PriorPayload!,
            entry.Prior.OrdinaryFile!,
            cancellationToken).ConfigureAwait(false);
        if (payload is null)
        {
            return null;
        }

        var confirmed = await RecoveryBundleReader.ReadSelectedFinalAsync(
            residual.Workspace,
            residual.Candidate,
            cancellationToken).ConfigureAwait(false);
        if (confirmed.Read.State == RecoveryBundleReadState.Cancelled)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }

        if (confirmed.Read.Verified is null)
        {
            return null;
        }

        var decoded = LibrariesRecordCodec.Read(payload);
        if (decoded.State != LibrariesRecordReadState.Complete
            || decoded.Issue != LibrariesRecordDecodeIssue.None
            || decoded.Record is not { } record)
        {
            return null;
        }

        var originalEntry = residual.Candidate.Verified!.Entries[entry.Ordinal];
        return new LibraryRecoveryPriorRecord(record, originalEntry, entry.Prior.OrdinaryFile!);
    }

    private static async ValueTask<byte[]?> ReadPayloadAsync(
        string bundlePath,
        string payloadName,
        RecoveryContentIdentity expected,
        CancellationToken cancellationToken)
    {
        try
        {
            RecoveryBundleStorage.ValidateOrdinaryFile(bundlePath);
            await using var file = RecoveryBundleStorage.OpenReadFile(bundlePath);
            using var archive = new ZipArchive(file, ZipArchiveMode.Read, leaveOpen: false);
            var matches = archive.Entries.Where(entry =>
                string.Equals(entry.FullName, payloadName, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1
                || matches[0].Length != expected.Length
                || expected.Length > int.MaxValue)
            {
                return null;
            }

            await using var stream = matches[0].Open();
            var bytes = new byte[(int)expected.Length];
            await stream.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
            return expected.Matches(bytes) ? bytes : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException
            or InvalidDataException
            or UnauthorizedAccessException
            or ArgumentException
            or NotSupportedException)
        {
            return null;
        }
    }
}
