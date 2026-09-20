using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Validation;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairMutationServices(
    MutationPreflight Preflight,
    WorkspaceLockManager LockManager,
    MutationRevalidator Revalidator,
    FileChangeApplier Applier);
