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

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairOperationFactory
{
    internal static RepairOperation Create(
        RepairInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(operationalContributors);
        return new RepairOperation(CreateComponents(
            interaction,
            lockStoreRoot,
            operationalContributors,
            new PhysicalPathResolver()));
    }

    internal static RepairOperationComponents CreateDefaultComponents()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return CreateComponents(
            DisabledInteraction(),
            lockStoreRoot: null,
            RepairOperationalContributorFactory.Create(physicalPathResolver),
            physicalPathResolver);
    }

    private static RepairOperationComponents CreateComponents(
        RepairInteraction interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors,
        PhysicalPathResolver physicalPathResolver)
    {
        var diagnosisReader = new DoctorDiagnosisReader(operationalContributors);
        var validator = new FileExpectationValidator(physicalPathResolver);
        var revalidator = new MutationRevalidator(validator);
        var catalogueReader = new RepairCatalogueReader(physicalPathResolver);
        var application = new RepairApplicationOperation(
            new RepairMutationServices(
                new MutationPreflight(validator),
            lockStoreRoot is null
                ? WorkspaceLockManager.CreateForCurrentUser()
                : new WorkspaceLockManager(lockStoreRoot),
            revalidator,
                new FileChangeApplier(revalidator, validator)),
            new RepairPostVerifier(diagnosisReader),
            new RepairPlanRevalidator(diagnosisReader, catalogueReader));
        return new RepairOperationComponents(
            diagnosisReader,
            catalogueReader,
            application,
            interaction);
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
