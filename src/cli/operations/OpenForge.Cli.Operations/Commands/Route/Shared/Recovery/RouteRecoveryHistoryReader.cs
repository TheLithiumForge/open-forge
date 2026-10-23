using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Recovery;

internal static class RouteRecoveryHistoryReader
{
    internal static async ValueTask<RouteRecoveryHistory> ReadAsync(CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var stores = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, cancellationToken).ConfigureAwait(false);
        return new RouteRecoveryHistory(
            stores.SelectMany(store => store.Catalogue.Candidates).Select(candidate => candidate.Path).ToImmutableArray(),
            stores.Where(store => store.Catalogue.State == RecoveryBundleCatalogueState.Unavailable)
                .Select(store => new RouteRecoveryCoverageFailure(store.Location?.WorkspaceDirectory,
                    store.Catalogue.Cause ?? throw new InvalidOperationException("Unavailable recovery coverage requires a cause.")))
                .ToImmutableArray());
    }
}
