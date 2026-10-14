using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairOperationFactory
{
    internal static RepairOperation Create(
        RepairInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors,
        RepairRenameServices? renames = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(operationalContributors);
        return new RepairOperation(CreateComponents(
            interaction,
            lockStoreRoot,
            operationalContributors,
            new PhysicalPathResolver(), renames));
    }

    internal static RepairOperationComponents CreateDefaultComponents(RepairRenameServices? renames = null)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return CreateComponents(
            DisabledInteraction(),
            lockStoreRoot: null,
            RepairOperationalContributorFactory.Create(physicalPathResolver),
            physicalPathResolver, renames);
    }

    private static RepairOperationComponents CreateComponents(
        RepairInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors,
        PhysicalPathResolver physicalPathResolver,
        RepairRenameServices? renames)
    {
        var diagnosisReader = new DoctorDiagnosisReader(operationalContributors);
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var catalogueReader = new RepairCatalogueReader(physicalPathResolver, renames?.Read);
        var maintenance = new RepairMaintenancePlanner(physicalPathResolver, renames?.Scaffolding);
        var application = new RepairApplicationOperation(
            new RepairMutationServices(
                new MutationPreflight(validator),
            lockStoreRoot is null
                ? WorkspaceLockManager.CreateForCurrentUser()
                : new WorkspaceLockManager(lockStoreRoot),
            revalidator,
                new FileChangeApplier(revalidator, validator)),
            new RepairPostVerifier(diagnosisReader, catalogueReader),
            new RepairPlanRevalidator(diagnosisReader, catalogueReader, maintenance));
        return new RepairOperationComponents(
            diagnosisReader,
            catalogueReader,
            application,
            interaction)
        { MaintenancePlanner = maintenance };
    }

    private static RepairInteraction DisabledInteraction()
        => new(
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairReferencePromptAnswer>.Unavailable()),
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairLibraryPromptAnswer>.Unavailable()),
            static (_, _, _, _) => ValueTask.FromResult(
                CliPromptReply<bool>.Unavailable()));
}
