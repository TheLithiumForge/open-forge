namespace OpenForge.Cli.Core.Commands.Shared.LibraryLinks.Models;

internal sealed record LibraryLinkRecreation
{
    public required string Workspace { get; init; }
    public required string Id { get; init; }
    public required string SourceRoot { get; init; }
    public required string DestinationRoot { get; init; }
    public required bool GitIgnore { get; init; }
}
