using System.Buffers;
using System.IO.Compression;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal sealed class RecoveryBundleReader
{
    private static readonly object VerifiedFinalAuthority = new();

    internal sealed class VerifiedFinalToken
    {
        internal VerifiedFinalToken(
            RecoveryBundleVerifiedRead verified,
            object authority)
        {
            ArgumentNullException.ThrowIfNull(verified);
            if (!ReferenceEquals(authority, VerifiedFinalAuthority))
            {
                throw new ArgumentException(
                    "A recovery preparation token requires real final semantic readback authority.",
                    nameof(authority));
            }

            Verified = verified;
        }

        internal RecoveryBundleVerifiedRead Verified { get; }
    }

    internal ValueTask<RecoveryBundleReadResult> VerifyExpectedAsync(
        RecoveryBundleInput input,
        string bundlePath,
        RecoveryBundleCandidateKind kind,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return ReadCoreAsync(
            input.Workspace,
            bundlePath,
            input,
            kind,
            cancellationToken);
    }

    internal ValueTask<RecoveryBundleReadResult> ReadFinalAsync(
        CliWorkspace workspace,
        string bundlePath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        return ReadCoreAsync(
            workspace,
            bundlePath,
            expectedInput: null,
            RecoveryBundleCandidateKind.Final,
            cancellationToken);
    }

    internal async ValueTask<RecoveryBundleFinalReadResult> ReadExpectedFinalAsync(
        RecoveryBundleInput input,
        string bundlePath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var read = await ReadCoreAsync(
            input.Workspace,
            bundlePath,
            input,
            RecoveryBundleCandidateKind.Final,
            cancellationToken).ConfigureAwait(false);
        return new RecoveryBundleFinalReadResult
        {
            Read = read,
            Preparation = read.Verified is { } verified
                ? new RecoveryBundlePreparation(
                    new VerifiedFinalToken(verified, VerifiedFinalAuthority))
                : null,
        };
    }

    private static async ValueTask<RecoveryBundleReadResult> ReadCoreAsync(
        CliWorkspace workspace,
        string bundlePath,
        RecoveryBundleInput? expectedInput,
        RecoveryBundleCandidateKind kind,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleReadResult.Cancelled();
        }

        try
        {
            var normalizedPath = Path.GetFullPath(bundlePath);
            RecoveryBundleStorage.ValidateOrdinaryFile(normalizedPath);
            await using var stream = RecoveryBundleStorage.OpenReadFile(normalizedPath);
            using var archive = new ZipArchive(
                stream,
                ZipArchiveMode.Read,
                leaveOpen: false,
                entryNameEncoding: null);
            if (archive.Entries.Count == 0
                || !string.Equals(
                    archive.Entries[0].FullName,
                    RecoveryBundleFormatV1.ManifestEntryName,
                    StringComparison.Ordinal))
            {
                return Malformed("The recovery archive does not begin with manifest.json.");
            }

            RecoveryBundleManifestDecodeResult manifest;
            await using (var manifestStream = archive.Entries[0].Open())
            {
                manifest = await RecoveryBundleManifestCodec.DecodeAsync(
                    manifestStream,
                    cancellationToken).ConfigureAwait(false);
            }

            if (manifest.State == RecoveryBundleManifestState.Unsupported)
            {
                return RecoveryBundleReadResult.Classified(
                    RecoveryBundleReadState.Unsupported,
                    manifest.Cause ?? "The recovery manifest schema is unsupported.");
            }

            if (manifest.State != RecoveryBundleManifestState.Valid
                || manifest.Document is not { } document)
            {
                return Malformed(manifest.Cause ?? "The recovery manifest is malformed.");
            }

            var identityFailure = ValidateIdentity(
                workspace,
                normalizedPath,
                document,
                expectedInput,
                kind);
            if (identityFailure is not null)
            {
                return Malformed(identityFailure);
            }

            if (expectedInput is not null
                && !MatchesExpectedEntries(expectedInput, manifest.Entries))
            {
                return Malformed("The recovery bundle entries do not match the expected operation targets.");
            }

            if (archive.Entries.Count != manifest.Entries.Length + 1)
            {
                return Malformed("The recovery archive entry count does not match its manifest.");
            }

            for (var index = 0; index < manifest.Entries.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var expected = manifest.Entries[index];
                var payload = archive.Entries[index + 1];
                if (!string.Equals(payload.FullName, expected.PayloadName, StringComparison.Ordinal)
                    || payload.Length != expected.Prior.Length)
                {
                    return Malformed("A recovery payload name or length does not match its manifest.");
                }

                var payloadHash = await HashEntryAsync(
                    payload,
                    expected.Prior.Length,
                    cancellationToken).ConfigureAwait(false);
                if (!string.Equals(payloadHash, expected.Prior.Sha256, StringComparison.Ordinal))
                {
                    return Malformed("A recovery payload does not match its declared SHA-256.");
                }
            }

            _ = RecoveryBundleManifestCodec.TryParseOperationId(document, out var operationId);
            return RecoveryBundleReadResult.Valid(new RecoveryBundleVerifiedRead
            {
                BundlePath = normalizedPath,
                WorkspacePhysicalPath = WorkspaceIdentity.NormalizePhysicalPath(
                    document.WorkspacePath),
                WorkspaceKey = document.WorkspaceKey,
                Command = document.Command,
                OperationId = operationId,
                Entries = manifest.Entries,
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleReadResult.Cancelled();
        }
        catch (InvalidDataException exception)
        {
            return Malformed(exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(FilesystemFailureKind.AccessDenied, exception);
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return Unavailable(FilesystemFailureKind.InvalidPath, exception);
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            return RecoveryBundleReadResult.Classified(
                RecoveryBundleReadState.Unsupported,
                exception.Message);
        }
        catch (IOException exception)
        {
            return Unavailable(FilesystemFailureKind.InputOutput, exception);
        }
    }

    private static string? ValidateIdentity(
        CliWorkspace workspace,
        string bundlePath,
        RecoveryBundleManifestV1 document,
        RecoveryBundleInput? expectedInput,
        RecoveryBundleCandidateKind kind)
    {
        var workspacePath = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot);
        var workspaceKey = WorkspaceIdentity.Key(workspacePath);
        if (!string.Equals(document.WorkspacePath, workspacePath, PathComparison())
            || !string.Equals(document.WorkspaceKey, workspaceKey, StringComparison.Ordinal)
            || !RecoveryBundleManifestCodec.TryParseOperationId(document, out var operationId))
        {
            return "The recovery bundle workspace or operation identity is invalid.";
        }

        if (expectedInput is not null
            && (operationId != expectedInput.OperationId
                || !string.Equals(document.Command, expectedInput.Command, StringComparison.Ordinal)))
        {
            return "The recovery bundle does not match the expected operation.";
        }

        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return "The recovery store cannot be observed after the bundle was opened.";
        }

        var expectedPath = kind == RecoveryBundleCandidateKind.Final
            ? RecoveryBundlePathIdentity.FinalPath(storeRoot, workspacePath, operationId)
            : RecoveryBundlePathIdentity.DraftPath(storeRoot, workspacePath, operationId);
        if (!string.Equals(bundlePath, expectedPath, PathComparison()))
        {
            return "The recovery bundle path does not match its manifest identity.";
        }

        return null;
    }

    private static bool MatchesExpectedEntries(
        RecoveryBundleInput input,
        IReadOnlyList<RecoveryBundleEntry> entries)
    {
        if (entries.Count != input.RecoveryTargets.Length)
        {
            return false;
        }

        for (var index = 0; index < entries.Count; index++)
        {
            var expected = RecoveryBundleEntry.FromTarget(
                input,
                input.RecoveryTargets[index],
                index);
            if (entries[index] != expected)
            {
                return false;
            }
        }

        return true;
    }

    private static async ValueTask<string> HashEntryAsync(
        ZipArchiveEntry entry,
        long expectedLength,
        CancellationToken cancellationToken)
    {
        await using var stream = entry.Open();
        var buffer = ArrayPool<byte>.Shared.Rent(RecoveryBundleFormatV1.StreamBufferSize);
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long length = 0;
            while (true)
            {
                var read = await stream.ReadAsync(
                    buffer.AsMemory(0, RecoveryBundleFormatV1.StreamBufferSize),
                    cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                length += read;
                if (length > expectedLength)
                {
                    throw new InvalidDataException("A recovery payload exceeded its declared length.");
                }

                hash.AppendData(buffer, 0, read);
            }

            if (length != expectedLength)
            {
                throw new InvalidDataException("A recovery payload ended before its declared length.");
            }

            return Convert.ToHexStringLower(hash.GetHashAndReset());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static RecoveryBundleReadResult Malformed(string cause)
        => RecoveryBundleReadResult.Classified(RecoveryBundleReadState.Malformed, cause);

    private static RecoveryBundleReadResult Unavailable(
        FilesystemFailureKind kind,
        Exception exception)
    {
        var failure = FilesystemFailure.FromException(kind, exception);
        return RecoveryBundleReadResult.Classified(
            RecoveryBundleReadState.Unavailable,
            failure.DirectCause,
            failure);
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
