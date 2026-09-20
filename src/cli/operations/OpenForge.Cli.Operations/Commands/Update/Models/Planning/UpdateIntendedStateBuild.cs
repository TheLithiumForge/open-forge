using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdateIntendedStateBuild(
    IReadOnlyList<UpdateComparisonObservation> Observations,
    IReadOnlyList<FileStateSnapshot> ProjectionInputs,
    UpdateFinding? Finding,
    bool Cancelled);
