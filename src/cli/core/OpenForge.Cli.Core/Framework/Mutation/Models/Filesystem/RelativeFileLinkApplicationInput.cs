using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

internal sealed record RelativeFileLinkApplicationInput
{
    public required RelativeFileLinkEffect Effect { get; init; }
    public required NoFollowLeafObservation Expected { get; init; }
    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
}
