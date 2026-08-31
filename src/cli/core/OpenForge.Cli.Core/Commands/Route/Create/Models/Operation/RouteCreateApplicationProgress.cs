using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;

internal sealed record RouteCreateApplicationProgress
{
    public required ImmutableArray<FileChangeReceipt> Receipts { get; init; }

    public PlannedFileChange? UncertainAttempt { get; init; }

    public required RouteCreateRecovery Recovery { get; init; }

    public required RouteCreateVerificationState Verification { get; init; }

    public required ImmutableArray<RouteCreateFinding> Findings { get; init; }
}
