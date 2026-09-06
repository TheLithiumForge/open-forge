using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Update;

internal static class UpdateOperationFactory
{
    internal static UpdateOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var mutationRevalidator = new MutationRevalidator(validator);
        var planBuilder = UpdatePlanBuilder.Create();
        var recoveryReader = new RecoveryBundleReader();
        var recoveryCatalogue = new RecoveryBundleCatalogue(recoveryReader);
        return new UpdateOperation(
            interactiveSession,
            planBuilder,
            new UpdateApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                new UpdateApplicationPreflight(
                    new UpdatePlanRevalidator(planBuilder, mutationRevalidator)),
                new UpdateRecoveryOperation(
                    new RecoveryBundleStore(recoveryReader),
                    recoveryCatalogue,
                    new RecoveryBundleDeletionGuard(
                        recoveryCatalogue,
                        recoveryReader)),
                new UpdateEffectApplication(
                    new FileChangeApplier(mutationRevalidator, validator)),
                new UpdateAppliedVerifier(planBuilder)));
    }
}
