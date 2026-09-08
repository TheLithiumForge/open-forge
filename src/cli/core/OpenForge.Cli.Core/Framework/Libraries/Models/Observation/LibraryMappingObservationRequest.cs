using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal sealed record LibraryMappingObservationRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required LibraryMapping Mapping { get; init; }
}
