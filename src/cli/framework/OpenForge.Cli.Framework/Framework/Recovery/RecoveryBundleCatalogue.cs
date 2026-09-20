using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal static class RecoveryBundleCatalogue
{
    internal static async ValueTask<RecoveryBundleCatalogueResult> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }

        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return RecoveryBundleCatalogueResult.Available([]);
        }

        if (!RecoveryBundleStorage.TryGetObserverDirectory(
            storeRoot,
            workspace.PhysicalRoot,
            out var workspaceDirectory,
            out var exists,
            out var directoryFailure))
        {
            return RecoveryBundleCatalogueResult.Unavailable(
                directoryFailure?.DirectCause
                    ?? "The external recovery storage path is unavailable.",
                directoryFailure);
        }

        if (!exists)
        {
            return RecoveryBundleCatalogueResult.Available([]);
        }

        try
        {
            var candidates = ImmutableArray.CreateBuilder<RecoveryBundleCandidateSnapshot>();
            var paths = Directory
                .EnumerateFileSystemEntries(workspaceDirectory, "*", SearchOption.TopDirectoryOnly)
                .Order(StringComparer.Ordinal);
            foreach (var path in paths)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return RecoveryBundleCatalogueResult.Cancelled();
                }

                if (!RecoveryBundleFormatV1.TryParseCandidateFileName(
                    Path.GetFileName(path),
                    out _,
                    out var kind))
                {
                    continue;
                }

                if (!TryValidateCandidateFile(path, kind, out var unavailable))
                {
                    candidates.Add(unavailable
                        ?? throw new InvalidOperationException(
                            "An unavailable recovery candidate requires its exact snapshot."));
                    continue;
                }

                if (kind == RecoveryBundleCandidateKind.Draft)
                {
                    candidates.Add(RecoveryBundleCandidateSnapshot.IncompleteDraft(path));
                    continue;
                }

                var read = await RecoveryBundleReader.ReadFinalAsync(
                    workspace,
                    path,
                    cancellationToken).ConfigureAwait(false);
                if (read.State == RecoveryBundleReadState.Cancelled)
                {
                    return RecoveryBundleCatalogueResult.Cancelled();
                }

                candidates.Add(FromRead(path, read));
            }

            return RecoveryBundleCatalogueResult.Available(candidates.ToImmutable());
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
            return Unavailable(FilesystemFailureKind.Unsupported, exception);
        }
        catch (IOException exception)
        {
            return Unavailable(FilesystemFailureKind.InputOutput, exception);
        }
    }

    private static RecoveryBundleCandidateSnapshot FromRead(
        string path,
        RecoveryBundleReadResult read)
    {
        if (read.State == RecoveryBundleReadState.Valid && read.Verified is { } verified)
        {
            return RecoveryBundleCandidateSnapshot.VerifiedFinal(verified);
        }

        var integrity = read.State switch
        {
            RecoveryBundleReadState.Malformed => RecoveryBundleIntegrity.Malformed,
            RecoveryBundleReadState.Unsupported => RecoveryBundleIntegrity.Unsupported,
            _ => RecoveryBundleIntegrity.Unavailable,
        };
        return RecoveryBundleCandidateSnapshot.Classified(
            path,
            RecoveryBundleCandidateKind.Final,
            integrity,
            read.Cause ?? "The exact-name recovery final could not be verified.",
            read.Failure);
    }

    private static bool TryValidateCandidateFile(
        string path,
        RecoveryBundleCandidateKind kind,
        out RecoveryBundleCandidateSnapshot? unavailable)
    {
        try
        {
            RecoveryBundleStorage.ValidateOrdinaryFile(path);
            unavailable = null;
            return true;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException
            or ArgumentException
            or PathTooLongException)
        {
            var failure = FilesystemFailure.FromException(FailureKind(exception), exception);
            unavailable = RecoveryBundleCandidateSnapshot.Classified(
                path,
                kind,
                RecoveryBundleIntegrity.Unavailable,
                failure.DirectCause,
                failure);
            return false;
        }
    }

    private static RecoveryBundleCatalogueResult Unavailable(
        FilesystemFailureKind kind,
        Exception exception)
    {
        var failure = FilesystemFailure.FromException(kind, exception);
        return RecoveryBundleCatalogueResult.Unavailable(failure.DirectCause, failure);
    }

    private static FilesystemFailureKind FailureKind(Exception exception)
        => exception switch
        {
            UnauthorizedAccessException => FilesystemFailureKind.AccessDenied,
            NotSupportedException or PlatformNotSupportedException => FilesystemFailureKind.Unsupported,
            ArgumentException or PathTooLongException => FilesystemFailureKind.InvalidPath,
            _ => FilesystemFailureKind.InputOutput,
        };
}
