using System.Collections.Immutable;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal sealed class RecoveryBundleStore(RecoveryBundleReader reader)
{
    private readonly RecoveryBundleReader _reader = reader;

    internal async ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        RecoveryBundleInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.RecoveryTargets.IsEmpty)
        {
            return RecoveryBundlePreparationResult.NotNeeded();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundlePreparationResult.Cancelled();
        }

        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.Create);
        if (storeRoot is null)
        {
            return RecoveryBundlePreparationResult.Incomplete(
                "Current-user LocalApplicationData is unavailable for recovery storage.");
        }

        if (!RecoveryBundleStorage.TryEnsureWriterDirectory(
            storeRoot,
            input.Workspace.PhysicalRoot,
            out _,
            out var directoryFailure))
        {
            return RecoveryBundlePreparationResult.Incomplete(
                directoryFailure?.DirectCause
                    ?? "The external recovery storage path is unavailable.",
                failure: directoryFailure);
        }

        var draftPath = RecoveryBundlePathIdentity.DraftPath(
            storeRoot,
            input.Workspace.PhysicalRoot,
            input.OperationId);
        var finalPath = RecoveryBundlePathIdentity.FinalPath(
            storeRoot,
            input.Workspace.PhysicalRoot,
            input.OperationId);
        var candidateConflict = ObserveCandidateConflict(draftPath, finalPath);
        if (candidateConflict is not null)
        {
            return candidateConflict;
        }

        var entries = CreateEntries(input);
        string? knownResidualPath = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var file = RecoveryBundleStorage.OpenCreateNewFile(draftPath);
            knownResidualPath = draftPath;
            await WriteDraftAsync(
                file,
                input,
                entries,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundlePreparationResult.Cancelled(ObserveResidualPath(
                draftPath,
                finalPath,
                knownResidualPath));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.AccessDenied,
                    exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.InvalidPath,
                    exception));
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.Unsupported,
                    exception));
        }
        catch (IOException exception)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.InputOutput,
                    exception));
        }

        RecoveryBundleReadResult draftRead;
        try
        {
            draftRead = await _reader.VerifyExpectedAsync(
                input,
                draftPath,
                RecoveryBundleCandidateKind.Draft,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundlePreparationResult.Cancelled(ObserveResidualPath(
                draftPath,
                finalPath,
                knownResidualPath));
        }
        catch (Exception)
        {
            return RecoveryBundlePreparationResult.Incomplete(
                "The recovery bundle draft could not be read back after it was written.",
                ObserveResidualPath(draftPath, finalPath, knownResidualPath));
        }

        if (draftRead.State != RecoveryBundleReadState.Valid)
        {
            return FromReadFailure(draftRead, draftPath);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundlePreparationResult.Cancelled(draftPath);
        }

        try
        {
            File.Move(draftPath, finalPath, overwrite: false);
            knownResidualPath = finalPath;
        }
        catch (UnauthorizedAccessException exception)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.AccessDenied,
                    exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.InvalidPath,
                    exception));
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            return Incomplete(
                draftPath: draftPath,
                finalPath: finalPath,
                knownResidualPath: knownResidualPath,
                failure: FilesystemFailure.FromException(
                    FilesystemFailureKind.Unsupported,
                    exception));
        }
        catch (IOException exception)
        {
            return RecoveryBundlePreparationResult.Blocked(
                "The deterministic recovery bundle could not be published without collision.",
                ObserveResidualPath(draftPath, finalPath, knownResidualPath),
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }

        RecoveryBundleFinalReadResult finalRead;
        try
        {
            finalRead = await _reader.ReadExpectedFinalAsync(
                input,
                finalPath,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundlePreparationResult.Cancelled(ObserveResidualPath(
                draftPath,
                finalPath,
                knownResidualPath));
        }
        catch (Exception)
        {
            return RecoveryBundlePreparationResult.Incomplete(
                "The published recovery bundle could not be read back after it was written.",
                ObserveResidualPath(draftPath, finalPath, knownResidualPath));
        }

        if (finalRead.Read.State != RecoveryBundleReadState.Valid
            || finalRead.Preparation is not { } preparation)
        {
            return FromReadFailure(finalRead.Read, finalPath);
        }

        return RecoveryBundlePreparationResult.Prepared(preparation);
    }

    private static ImmutableArray<RecoveryBundleEntry> CreateEntries(RecoveryBundleInput input)
    {
        var entries = ImmutableArray.CreateBuilder<RecoveryBundleEntry>(
            input.RecoveryTargets.Length);
        for (var index = 0; index < input.RecoveryTargets.Length; index++)
        {
            entries.Add(RecoveryBundleEntry.FromTarget(
                input,
                input.RecoveryTargets[index],
                index));
        }

        return entries.MoveToImmutable();
    }

    private static async ValueTask WriteDraftAsync(
        FileStream file,
        RecoveryBundleInput input,
        ImmutableArray<RecoveryBundleEntry> entries,
        CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(
            file,
            ZipArchiveMode.Create,
            leaveOpen: true,
            entryNameEncoding: null);
        var manifest = archive.CreateEntry(
            RecoveryBundleFormatV1.ManifestEntryName,
            CompressionLevel.NoCompression);
        await using (var manifestStream = manifest.Open())
        {
            await manifestStream.WriteAsync(
                RecoveryBundleManifestCodec.Serialize(input, entries),
                cancellationToken).ConfigureAwait(false);
        }

        for (var index = 0; index < entries.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = archive.CreateEntry(
                entries[index].PayloadName,
                CompressionLevel.NoCompression);
            await using var payloadStream = payload.Open();
            await payloadStream.WriteAsync(
                input.RecoveryTargets[index].Before.Bytes.AsMemory(),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static RecoveryBundlePreparationResult FromReadFailure(
        RecoveryBundleReadResult result,
        string residualPath)
        => result.State switch
        {
            RecoveryBundleReadState.Cancelled => RecoveryBundlePreparationResult.Cancelled(residualPath),
            RecoveryBundleReadState.Unavailable => RecoveryBundlePreparationResult.Incomplete(
                result.Cause ?? "The recovery bundle could not be read.",
                residualPath,
                result.Failure),
            _ => RecoveryBundlePreparationResult.Blocked(
                result.Cause ?? "The recovery bundle failed semantic verification.",
                residualPath,
                result.Failure),
        };

    private static RecoveryBundlePreparationResult Incomplete(
        string draftPath,
        string finalPath,
        string? knownResidualPath,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return RecoveryBundlePreparationResult.Incomplete(
            failure.DirectCause,
            ObserveResidualPath(draftPath, finalPath, knownResidualPath),
            failure);
    }

    private static RecoveryBundlePreparationResult? ObserveCandidateConflict(
        string draftPath,
        string finalPath)
    {
        if (!RecoveryBundleStorage.TryObservePath(
            draftPath,
            out var draftAttributes,
            out var draftFailure))
        {
            var failure = draftFailure
                ?? throw new InvalidOperationException(
                    "An unavailable recovery draft observation requires a typed failure.");
            return RecoveryBundlePreparationResult.Incomplete(
                failure.DirectCause,
                draftPath,
                failure);
        }

        if (draftAttributes is not null)
        {
            return RecoveryBundlePreparationResult.Blocked(
                "The deterministic recovery bundle identity already exists.",
                draftPath);
        }

        if (!RecoveryBundleStorage.TryObservePath(
            finalPath,
            out var finalAttributes,
            out var finalFailure))
        {
            var failure = finalFailure
                ?? throw new InvalidOperationException(
                    "An unavailable recovery final observation requires a typed failure.");
            return RecoveryBundlePreparationResult.Incomplete(
                failure.DirectCause,
                finalPath,
                failure);
        }

        return finalAttributes is null
            ? null
            : RecoveryBundlePreparationResult.Blocked(
                "The deterministic recovery bundle identity already exists.",
                finalPath);
    }

    private static string? ObserveResidualPath(
        string draftPath,
        string finalPath,
        string? knownResidualPath)
    {
        var expectedPath = string.Equals(knownResidualPath, finalPath, StringComparison.Ordinal)
            ? finalPath
            : draftPath;
        var alternatePath = string.Equals(expectedPath, finalPath, StringComparison.Ordinal)
            ? draftPath
            : finalPath;
        if (!RecoveryBundleStorage.TryObservePath(expectedPath, out var expectedAttributes, out _))
        {
            return expectedPath;
        }

        if (expectedAttributes is not null)
        {
            return expectedPath;
        }

        if (!RecoveryBundleStorage.TryObservePath(alternatePath, out var alternateAttributes, out _))
        {
            return alternatePath;
        }

        return alternateAttributes is null ? null : alternatePath;
    }
}
