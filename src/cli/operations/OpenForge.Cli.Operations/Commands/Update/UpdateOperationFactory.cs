using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Update;

internal static class UpdateOperationFactory
{
    internal static UpdateOperation Create(
        CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> planConfirmation,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(planConfirmation);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var mutationRevalidator = new MutationRevalidator(validator);
        var planBuilder = UpdatePlanBuilder.Create();
        return new UpdateOperation(
            planConfirmation,
            planBuilder,
            new UpdateApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                new UpdatePlanRevalidator(planBuilder, mutationRevalidator),
                new UpdateEffectApplication(
                    new DirectoryCreationApplier(mutationRevalidator, validator),
                    new FileChangeApplier(mutationRevalidator, validator)),
                new UpdateAppliedVerifier(planBuilder)));
    }
}
