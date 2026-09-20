using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectProjectionView
{
    public required LibraryCoverage State { get; init; }

    public required LibraryPathComparison[] Comparisons { get; init; }
}
