using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
namespace OpenForge.Cli.Core.Commands.Index;

internal static class IndexOperationFactory
{
    internal static IndexOperation Create(WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var projectionReader = new IndexProjectionReader(physicalPathResolver);
        var applicationOperation = new IndexApplicationOperation(
            lockManager: lockStoreRoot is null
                ? WorkspaceLockManager.CreateForCurrentUser()
                : new WorkspaceLockManager(lockStoreRoot),
            revalidator: revalidator,
            fileChangeApplier: new FileChangeApplier(revalidator, validator),
            projectionReader: projectionReader);
        return new IndexOperation(
            projectionReader: projectionReader,
            planBuilder: new IndexPlanBuilder(),
            applicationOperation: applicationOperation,
            resultBuilder: new IndexResultBuilder());
    }
}
