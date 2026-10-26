using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Recovery;

internal static class InstallRecoveryCatalogueReader
{
    internal static async ValueTask<RecoveryBundleCatalogueResult> ReadBlockingAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var stores = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (stores.Any(store => store.Catalogue.State == RecoveryBundleCatalogueState.Cancelled))
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }

        var local = stores.Single(store => store.Kind == RecoveryStoreKind.Workspace).Catalogue;
        if (local.State != RecoveryBundleCatalogueState.Available)
        {
            return local;
        }

        var resolver = new PhysicalPathResolver();
        var blocking = ImmutableArray.CreateBuilder<RecoveryBundleCandidateSnapshot>();
        try
        {
            foreach (var candidate in stores.Where(store => store.Catalogue.State == RecoveryBundleCatalogueState.Available)
                .SelectMany(store => store.Catalogue.Candidates))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (candidate.Verified is null)
                {
                    blocking.Add(candidate);
                    continue;
                }

                var observation = await RecoveryEntrySetObserver.ReadAsync(
                    resolver, workspace, candidate, cancellationToken).ConfigureAwait(false);
                if (RequiresRecoveryBoundary(observation))
                {
                    blocking.Add(candidate);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }

        return RecoveryBundleCatalogueResult.Available(blocking.ToImmutable());
    }

    private static bool RequiresRecoveryBoundary(RecoveryEntrySetObservation observation)
    {
        var entries = observation.Entries;
        if (!entries.All(entry => entry.State is RecoveryBundleTargetComparisonState.Prior
            or RecoveryBundleTargetComparisonState.Intended or RecoveryBundleTargetComparisonState.Third))
        {
            return true;
        }

        // A changed identity invalidates the old operation's recovery snapshot.
        // Keep the bundle as history rather than replaying or deleting its bytes.
        if (entries.Any(entry => entry.State == RecoveryBundleTargetComparisonState.Third))
        {
            return false;
        }

        return entries.Any(entry => entry.State == RecoveryBundleTargetComparisonState.Prior)
            && entries.Any(entry => entry.State == RecoveryBundleTargetComparisonState.Intended);
    }
}
