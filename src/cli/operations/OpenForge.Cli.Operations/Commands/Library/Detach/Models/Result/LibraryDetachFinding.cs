using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;

internal sealed record LibraryDetachFinding
{
    public required LibraryDetachFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }
    public required string? LibraryId { get; init; }
    public required string? Path { get; init; }
    public required string Cause { get; init; }
    public required LibraryDetachOccupantKind? OccupantKind { get; init; }
}
