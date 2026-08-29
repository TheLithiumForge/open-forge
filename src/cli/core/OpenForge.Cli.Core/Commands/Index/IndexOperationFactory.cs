using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.Core.Commands.Index;

internal static class IndexOperationFactory
{
    internal static IndexOperation Create()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator, physicalPathResolver);
        var recoveryReader = new RecoveryBundleReader();
        var recoveryCatalogue = new RecoveryBundleCatalogue(recoveryReader);
        var projectionReader = new IndexProjectionReader(physicalPathResolver);
        var recoveryLifecycle = new IndexRecoveryLifecycle(
            store: new RecoveryBundleStore(recoveryReader),
            catalogue: recoveryCatalogue,
            deletionGuard: new RecoveryBundleDeletionGuard(
                recoveryCatalogue,
                recoveryReader));
        var applicationOperation = new IndexApplicationOperation(
            lockManager: new WorkspaceLockManager(physicalPathResolver),
            revalidator: revalidator,
            fileChangeApplier: new FileChangeApplier(revalidator, validator),
            projectionReader: projectionReader,
            recoveryLifecycle: recoveryLifecycle);
        return new IndexOperation(
            projectionReader: projectionReader,
            planBuilder: new IndexPlanBuilder(),
            applicationOperation: applicationOperation,
            resultBuilder: new IndexResultBuilder());
    }
}
