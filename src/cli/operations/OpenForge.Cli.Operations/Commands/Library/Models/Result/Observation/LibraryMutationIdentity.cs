using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationIdentity
{
    public required string? LibraryId { get; init; }

    public required string? SourceRoot { get; init; }
    public required string? DestinationRoot { get; init; }

    public required LibraryMode Mode { get; init; }

    public required bool SourceIndependent { get; init; }
}
