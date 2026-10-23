using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Recovery;

internal static class ExtensionRecoveryHistory
{
    internal static ImmutableArray<RecoveryStoreObservation> Unavailable(CliWorkspace workspace, string cause)
        =>
        [
            new(RecoveryStoreKind.Workspace, RecoveryStoreLocator.Local(workspace), RecoveryBundleCatalogueResult.Unavailable(cause)),
            new(RecoveryStoreKind.Legacy, RecoveryStoreLocator.Legacy(workspace), RecoveryBundleCatalogueResult.Unavailable(cause)),
        ];

    internal static bool RequiresWarning(RecoveryBundleCandidateSnapshot candidate)
        => candidate is not
        {
            Kind: RecoveryBundleCandidateKind.Final,
            Integrity: RecoveryBundleIntegrity.Verified,
            Verified: not null,
        };
}
