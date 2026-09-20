using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemoveTopologyBuild(
    ExtensionRemoveTopology Topology,
    IReadOnlyList<ExtensionRemoveGeneratedRegion> Regions,
    IReadOnlyList<ExtensionRemoveGeneratedChange> Changes);

internal sealed record ExtensionRemoveGeneratedChange(
    string Path,
    PlannedFileChange Change,
    FileStateSnapshot Before);
