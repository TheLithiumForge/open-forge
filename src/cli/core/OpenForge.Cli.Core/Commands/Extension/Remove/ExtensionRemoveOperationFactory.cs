using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal static class ExtensionRemoveOperationFactory
{
    internal static ExtensionRemoveOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        var physicalPathResolver = new PhysicalPathResolver();
        var lifecycleStore = new LifecycleStore(physicalPathResolver);
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var planner = new ExtensionRemovePlanner(
            interactiveSession,
            lifecycleStore,
            new LifecycleOwnershipReader(physicalPathResolver),
            physicalPathResolver);
        return new ExtensionRemoveOperation(
            planner,
            new MutationPreflight(validator),
            new ExtensionRemoveApplicationOperation(
                lockStoreRoot is null
                    ? WorkspaceLockManager.CreateForCurrentUser()
                    : new WorkspaceLockManager(lockStoreRoot),
                planner,
                revalidator,
                new FileChangeApplier(
                    revalidator,
                    validator)));
    }
}
