using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Install;

internal static class InstallOperationFactory
{
    internal static InstallOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var preflight = new MutationPreflight(validator);
        var lifecycleStore = new LifecycleStore(physicalPathResolver);
        var application = InstallApplicationOperationFactory.Create(
            physicalPathResolver: physicalPathResolver,
            validator: validator,
            lifecycleStore: lifecycleStore,
            lockStoreRoot: lockStoreRoot);
        return new InstallOperation(
            interactiveSession: interactiveSession,
            planBuilder: new InstallPlanBuilder(
                physicalPathResolver,
                lifecycleStore),
            preflight: preflight,
            applicationOperation: application);
    }
}
