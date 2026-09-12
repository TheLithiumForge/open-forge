using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
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
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairOperationFactory
{
    internal static RepairOperation Create(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        ArgumentNullException.ThrowIfNull(operationalContributors);
        return new RepairOperation(CreateComponents(
            interactiveSession,
            lockStoreRoot,
            operationalContributors,
            new PhysicalPathResolver()));
    }

    internal static RepairOperationComponents CreateDefaultComponents()
    {
        var physicalPathResolver = new PhysicalPathResolver();
        return CreateComponents(
            new CliInteractiveSession(TextReader.Null, TextWriter.Null, canPrompt: false),
            lockStoreRoot: null,
            RepairOperationalContributorFactory.Create(physicalPathResolver),
            physicalPathResolver);
    }

    private static RepairOperationComponents CreateComponents(
        CliInteractiveSession interactiveSession,
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
            interactiveSession);
    }
}
