using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Recovery;

internal static class ExtensionRecoveryHistory
{
    internal static bool RequiresWarning(RecoveryBundleCandidateSnapshot candidate)
        => candidate is not
        {
            Kind: RecoveryBundleCandidateKind.Final,
            Integrity: RecoveryBundleIntegrity.Verified,
            Verified: not null,
        };
}
