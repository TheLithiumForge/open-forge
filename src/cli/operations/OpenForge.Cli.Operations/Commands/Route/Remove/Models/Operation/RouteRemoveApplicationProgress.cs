using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal sealed record RouteRemoveApplicationProgress
{
    public ImmutableArray<RouteRemoveApplicationReceipt> Receipts { get; init; } = [];

    public required RouteRemoveRecovery Recovery { get; init; }

    public required RouteRemoveVerificationState Verification { get; init; }

    public ImmutableArray<RouteRemoveFinding> Findings { get; init; } = [];
}

internal abstract record RouteRemoveApplicationReceipt;

internal sealed record RouteRemoveFileChangeReceipt(
    FileChangeReceipt Receipt) : RouteRemoveApplicationReceipt;

internal sealed record RouteRemoveDirectoryDeletionReceipt(
    DirectoryDeletionReceipt Receipt) : RouteRemoveApplicationReceipt;
