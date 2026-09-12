using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

internal static class RecoveryBundleIdentity
{
    internal static bool Matches(
        RecoveryBundleVerifiedRead selected,
        RecoveryBundleVerifiedRead observed)
        => string.Equals(selected.BundlePath, observed.BundlePath, PathComparison())
            && string.Equals(
                selected.WorkspacePhysicalPath,
                observed.WorkspacePhysicalPath,
                PathComparison())
            && string.Equals(selected.WorkspaceKey, observed.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(selected.Command, observed.Command, StringComparison.Ordinal)
            && selected.Attribution == observed.Attribution
            && selected.OperationId == observed.OperationId
            && selected.Entries.SequenceEqual(observed.Entries);

    internal static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
    {
        if (candidate.Kind != RecoveryBundleCandidateKind.Final
            || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified)
        {
            return false;
        }

        return Matches(verified, preparation.Verified);
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
