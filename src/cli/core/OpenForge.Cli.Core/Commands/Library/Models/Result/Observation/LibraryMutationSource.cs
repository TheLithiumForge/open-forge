using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationSource
{
    public required LibrarySourceRootViewState RootState { get; init; }

    public required LibraryMutationInventoryState InventoryState { get; init; }

    public required LibraryEligiblePath[] EligiblePaths { get; init; }

    public required LibraryExcludedPath[] ExcludedPaths { get; init; }

    public required LibraryUnavailablePath[] UnavailablePaths { get; init; }
    public required string? LexicalRoot { get; init; }

    public required string? PhysicalRoot { get; init; }

    public required bool? LexicallyContained { get; init; }

    public required bool? PhysicallyContained { get; init; }

}
