using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;

internal sealed record LibraryAttachFinding
{
    internal WorkspaceLockFailureInfo? LockFailure { get; init; }

    public required LibraryAttachFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }
    public required string? LibraryId { get; init; }
    public required string? Path { get; init; }
    public required string Cause { get; init; }
}
