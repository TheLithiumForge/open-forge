using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal sealed record RouteMoveApplicationProgress
{
    public ImmutableArray<RouteMoveApplicationReceipt> Receipts { get; init; } = [];

    public required RouteMoveRecovery Recovery { get; init; }

    public required RouteMoveVerificationState Verification { get; init; }

    public ImmutableArray<RouteMoveFinding> Findings { get; init; } = [];
}

internal abstract record RouteMoveApplicationReceipt;

internal sealed record RouteMoveDirectoryCreationReceipt(
    DirectoryCreationReceipt Receipt) : RouteMoveApplicationReceipt;

internal sealed record RouteMoveFileChangeReceipt(
    FileChangeReceipt Receipt) : RouteMoveApplicationReceipt;

internal sealed record RouteMoveDirectoryDeletionReceipt(
    DirectoryDeletionReceipt Receipt) : RouteMoveApplicationReceipt;
