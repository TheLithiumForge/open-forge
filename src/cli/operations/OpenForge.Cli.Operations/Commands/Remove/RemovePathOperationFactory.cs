using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Commands.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Remove;

internal static class RemovePathOperationFactory
{
    internal static RemovePathOperation Create(
        WorkspaceLockStoreRoot? lockStoreRoot = null,
        CliPlanConfirmation<Models.Result.RemoveResult, Models.Interaction.RemoveConfirmationQuestion>? confirmation = null)
    {
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var ownershipStore = new WorkspaceOwnershipStore();
        var application = new RemovePathApplication(
            resolver,
            lockStoreRoot is null ? WorkspaceLockManager.CreateForCurrentUser() : new WorkspaceLockManager(lockStoreRoot),
            validator,
            new FileChangeApplier(revalidator, validator),
            new DirectoryCreationApplier(revalidator, validator),
            new DirectoryDeletionApplier(revalidator, validator));
        return new RemovePathOperation(
            new RemovePathPlanner(resolver, ownershipStore),
            application,
            confirmation);
    }
}
