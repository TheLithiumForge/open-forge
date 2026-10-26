using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal static class RecoveryBundleCatalogue
{
    internal static async ValueTask<RecoveryBundleCatalogueResult> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => await ReadAsync(RecoveryStoreLocator.Local(workspace), cancellationToken).ConfigureAwait(false);

    internal static async ValueTask<ImmutableArray<RecoveryStoreObservation>> ReadStoresAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var local = RecoveryStoreLocator.Local(workspace);
        var localCatalogue = await ReadAsync(local, cancellationToken).ConfigureAwait(false);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        var legacyCatalogue = legacy is null
            ? RecoveryBundleCatalogueResult.Unavailable("The legacy recovery store location is unavailable.")
            : await ReadAsync(legacy, cancellationToken).ConfigureAwait(false);
        return
        [
            new(RecoveryStoreKind.Workspace, local, localCatalogue),
            new(RecoveryStoreKind.Legacy, legacy, legacyCatalogue),
        ];
    }

    internal static async ValueTask<RecoveryBundleCatalogueResult> ReadAsync(
        RecoveryStoreLocation location,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }

        if (!RecoveryBundleStorage.TryGetObserverDirectory(
            location.RootPath,
            location.Workspace.PhysicalRoot,
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

                if (!TryValidateCandidateFile(location, path, kind, out var unavailable))
                {
                    candidates.Add(unavailable
                        ?? throw new InvalidOperationException(
                            "An unavailable recovery candidate requires its exact snapshot."));
                    continue;
                }

                if (kind == RecoveryBundleCandidateKind.Draft)
                {
                    candidates.Add(RecoveryBundleCandidateSnapshot.IncompleteDraft(location, path));
                    continue;
                }

                var read = await RecoveryBundleReader.ReadFinalAsync(
                    location,
                    path,
                    cancellationToken).ConfigureAwait(false);
                if (read.State == RecoveryBundleReadState.Cancelled)
                {
                    return RecoveryBundleCatalogueResult.Cancelled();
                }

                candidates.Add(FromRead(location, path, read));
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
        RecoveryStoreLocation location,
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
            location,
            path,
            RecoveryBundleCandidateKind.Final,
            integrity,
            read.Cause ?? "The exact-name recovery final could not be verified.",
            read.Failure);
    }

    private static bool TryValidateCandidateFile(
        RecoveryStoreLocation location,
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
                location,
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
