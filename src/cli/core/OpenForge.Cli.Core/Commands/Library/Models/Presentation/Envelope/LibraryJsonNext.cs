
namespace OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;

internal sealed record LibraryJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
