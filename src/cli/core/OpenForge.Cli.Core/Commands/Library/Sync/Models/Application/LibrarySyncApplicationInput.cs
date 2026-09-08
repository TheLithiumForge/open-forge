using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Application;

internal sealed record LibrarySyncApplicationInput
{
    public required WorkspaceLockLease Lease { get; init; }

    public required LibrarySyncPlan Plan { get; init; }

    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
}
