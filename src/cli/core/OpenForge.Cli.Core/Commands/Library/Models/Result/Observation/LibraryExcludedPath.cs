using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryExcludedPath
{
    public required string Path { get; init; }

    public required LibraryExclusionKind Reason { get; init; }
}
