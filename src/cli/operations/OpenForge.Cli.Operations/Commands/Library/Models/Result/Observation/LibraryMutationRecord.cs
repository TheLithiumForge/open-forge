using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationRecord
{
    public required string Path { get; init; }

    public required LibraryMutationRecordState State { get; init; }

    public required string[] RegisteredPaths { get; init; }

    public required LibraryRegistrationsDocument? Intended { get; init; }
}
