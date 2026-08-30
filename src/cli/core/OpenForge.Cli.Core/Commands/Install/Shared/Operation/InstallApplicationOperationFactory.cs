using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal static class InstallApplicationOperationFactory
{
    internal static InstallApplicationComposition Create(
        PhysicalPathResolver physicalPathResolver,
        FileExpectationValidator validator,
        LifecycleStore lifecycleStore,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var revalidator = new MutationRevalidator(validator);
        var recoveryReader = new RecoveryBundleReader();
        var recoveryCatalogue = new RecoveryBundleCatalogue(recoveryReader);
        var operation = new InstallApplicationOperation(
            lockManager: lockStoreRoot is null
                ? WorkspaceLockManager.CreateForCurrentUser()
                : new WorkspaceLockManager(lockStoreRoot),
            preconditionValidator: new InstallApplicationPreconditionValidator(
                mutationRevalidator: revalidator,
                intendedStateBuilder: new InstallIntendedStateBuilder(physicalPathResolver),
                recoveryCatalogue: recoveryCatalogue),
            effectApplier: new InstallApplicationEffectApplier(
                new DirectoryCreationApplier(revalidator, validator),
                new FileChangeApplier(revalidator, validator)),
            recoveryOperation: new InstallRecoveryOperation(
                new RecoveryBundleStore(recoveryReader),
                recoveryCatalogue,
                new RecoveryBundleDeletionGuard(
                    recoveryCatalogue,
                    recoveryReader)),
            verifier: new InstallAppliedVerifier(
                physicalPathResolver,
                lifecycleStore));
        return new InstallApplicationComposition(
            operation,
            recoveryCatalogue);
    }
}

internal sealed record InstallApplicationComposition(
    InstallApplicationOperation Operation,
    RecoveryBundleCatalogue RecoveryCatalogue);
