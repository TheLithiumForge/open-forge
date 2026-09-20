using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Comparison.Models;

internal sealed record LibraryInspectComparisonRead
{
    public required LibraryRegisteredPath[] RegisteredPaths { get; init; }
    public required LibraryEligiblePath[] EligiblePaths { get; init; }
    public required LibraryPathComparison[] Comparisons { get; init; }
    public required LibraryInspectFinding[] Findings { get; init; }
}
