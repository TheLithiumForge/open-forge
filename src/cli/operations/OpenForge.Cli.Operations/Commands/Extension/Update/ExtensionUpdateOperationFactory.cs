using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal static class ExtensionUpdateOperationFactory
{
    internal static ExtensionUpdateOperation Create(
        ExtensionUpdateInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var sourceReader = new ExtensionSourceReader(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var planner = new ExtensionUpdatePlanner(
            sourceReader,
            validator,
            physicalPathResolver,
            interaction.SelectPackages,
            interaction.SelectionQuestion);
        var permissions = new ExtensionPermissionOperation(interaction.Permission);
        return new ExtensionUpdateOperation(
            planner,
            new MutationPreflight(validator),
            permissions,
            new ExtensionUpdateApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                planner,
                revalidator,
                permissions,
                new ExtensionUpdateEffectApplier(
                    new DirectoryCreationApplier(revalidator, validator),
                    new FileChangeApplier(revalidator, validator))),
            interaction);
    }
}
