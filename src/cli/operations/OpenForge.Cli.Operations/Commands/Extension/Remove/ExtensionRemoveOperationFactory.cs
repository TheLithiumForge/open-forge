using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal static class ExtensionRemoveOperationFactory
{
    internal static ExtensionRemoveOperation Create(
        ExtensionRemoveInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var planner = new ExtensionRemovePlanner(
            interaction.SelectPackages,
            interaction.SelectionQuestion,
            physicalPathResolver);
        var permissions = new ExtensionPermissionOperation(interaction.Permission);
        return new ExtensionRemoveOperation(
            planner,
            new MutationPreflight(validator),
            permissions,
            new ExtensionRemoveApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                planner,
                revalidator,
                permissions,
                new FileChangeApplier(revalidator, validator)),
            interaction);
    }
}
