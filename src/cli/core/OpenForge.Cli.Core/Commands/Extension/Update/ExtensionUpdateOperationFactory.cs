using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal static class ExtensionUpdateOperationFactory
{
    internal static ExtensionUpdateOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        var physicalPathResolver = new PhysicalPathResolver();
        var lifecycleStore = new LifecycleStore(physicalPathResolver);
        var validator = new FileExpectationValidator(physicalPathResolver);
        var sourceReader = new ExtensionSourceReader(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var planner = new ExtensionUpdatePlanner(
            sourceReader,
            lifecycleStore,
            validator,
            new FrameworkLifecycleCurrentnessReader(physicalPathResolver),
            physicalPathResolver);
        var permissions = new ExtensionPermissionOperation(interactiveSession);
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
                    new FileChangeApplier(revalidator, validator))));
    }
}
