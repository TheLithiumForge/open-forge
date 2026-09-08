using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;

internal sealed record LibraryMutationIdentity
{
    public required string? LibraryId { get; init; }

    public required string? SourceRoot { get; init; }

    public required LibraryMode Mode { get; init; }

    public required bool SourceIndependent { get; init; }
}
