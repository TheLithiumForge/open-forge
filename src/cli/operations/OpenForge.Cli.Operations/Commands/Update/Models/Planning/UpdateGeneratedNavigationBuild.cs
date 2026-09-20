using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdateGeneratedNavigationBuild(
    IReadOnlyDictionary<string, byte[]> TargetBytes,
    IReadOnlyList<FileStateSnapshot> ProjectionInputs,
    UpdateFinding? Finding);

internal sealed record UpdateProjectionInputRead(
    FileStateSnapshot? Snapshot,
    UpdateGeneratedNavigationBuild? Build);
