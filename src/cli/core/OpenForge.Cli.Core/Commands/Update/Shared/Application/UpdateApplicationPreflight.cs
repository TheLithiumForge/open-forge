using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal sealed class UpdateApplicationPreflight(UpdatePlanRevalidator revalidator)
{
    private readonly UpdatePlanRevalidator _revalidator = revalidator;

    internal ValueTask<UpdatePlanRevalidation> ValidateAsync(
        UpdatePlanExecution execution,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
        => _revalidator.RevalidateAsync(execution, lease, cancellationToken);
}
