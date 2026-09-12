using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal sealed record LibraryMappingSetRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required WorkspaceRelativeDirectory SourceRoot { get; init; }
    public required LibraryDestinationRoot DestinationRoot { get; init; }
    public required ImmutableArray<SourceRelativeEligiblePath> Paths { get; init; }
}
