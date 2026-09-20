using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal enum LibraryRegistrationReadState
{
    Missing,
    Complete,
    Malformed,
    Unavailable,
    Blocked,
}

internal sealed record LibraryRegistrationRead
{
    public string? OwnershipObservation { get; init; }

    public required LibraryRegistrationReadState State { get; init; }
    public required LibraryRegistrationSet? Record { get; init; }
    public required FileStateSnapshot? Snapshot { get; init; }
    public required string? Cause { get; init; }
}
