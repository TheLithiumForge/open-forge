
namespace OpenForge.Cli.Core.Commands.Library.Models.Presentation.Envelope;

internal sealed record LibraryJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}
