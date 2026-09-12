using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;

internal sealed record LibraryDetachApplicationInput
{
    public required WorkspaceLockLease Lease { get; init; }

    public required LibraryDetachPlan Plan { get; init; }

    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
}
