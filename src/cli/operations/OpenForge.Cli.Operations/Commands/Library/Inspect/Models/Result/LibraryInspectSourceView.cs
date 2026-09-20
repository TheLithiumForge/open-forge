using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;

internal sealed record LibraryInspectSourceView
{
    public required LibrarySourceRootViewState RootState { get; init; }

    public required LibraryInventoryViewState State { get; init; }

    public required LibraryEligiblePath[] EligiblePaths { get; init; }

    public int ExcludedCount { get; init; }
}
