using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.List.Models.Result;

internal sealed record LibraryListFinding
{
    public required LibraryListFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }
    public required string? LibraryId { get; init; }
    public required string? Path { get; init; }
    public required string Cause { get; init; }
}
