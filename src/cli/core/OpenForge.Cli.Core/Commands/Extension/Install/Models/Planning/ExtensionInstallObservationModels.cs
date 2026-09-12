using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;

internal sealed record ExtensionInstallTargetObservation(
    FileStateSnapshot? Snapshot,
    ExtensionInstallFinding? Finding)
{
    internal static ExtensionInstallTargetObservation Complete(FileStateSnapshot snapshot)
        => new(snapshot, null);

    internal static ExtensionInstallTargetObservation Stop(ExtensionInstallFinding finding)
        => new(null, finding);
}
