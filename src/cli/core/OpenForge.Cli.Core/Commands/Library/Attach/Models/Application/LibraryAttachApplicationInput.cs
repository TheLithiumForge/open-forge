using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Models.Application;

internal sealed record LibraryAttachApplicationInput
{
    public required WorkspaceLockLease Lease { get; init; }

    public required LibraryAttachPlan Plan { get; init; }

    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
}
