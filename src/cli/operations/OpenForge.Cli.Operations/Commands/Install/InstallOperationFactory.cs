using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Install;

internal static class InstallOperationFactory
{
    internal static InstallOperation Create(
        CliPlanConfirmation<InstallResult, InstallConfirmationFacts> planConfirmation,
        WorkspaceLockStoreRoot? lockStoreRoot = null)
    {
        ArgumentNullException.ThrowIfNull(planConfirmation);
        var physicalPathResolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(physicalPathResolver);
        var preflight = new MutationPreflight(validator);
        var application = InstallApplicationOperationFactory.Create(
            physicalPathResolver: physicalPathResolver,
            validator: validator,
            lockStoreRoot: lockStoreRoot);
        return new InstallOperation(
            planConfirmation: planConfirmation,
            planBuilder: new InstallPlanBuilder(
                physicalPathResolver),
            preflight: preflight,
            applicationOperation: application);
    }
}
