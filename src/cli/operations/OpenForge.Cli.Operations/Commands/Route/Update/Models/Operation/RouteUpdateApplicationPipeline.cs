using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;

internal sealed record RouteUpdateApplicationPipelineInput
{
    public required RouteUpdatePlan Plan { get; init; }
    public required WorkspaceLockLease Lease { get; init; }
    public required Guid OperationId { get; init; }
}
