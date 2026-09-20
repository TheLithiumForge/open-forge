using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Presentation.Cleanup;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Presentation.Context.Shared.Help;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Binding;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Presentation.Find;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index.Shared.Help;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Help;
using OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Binding;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Status;
using OpenForge.Cli.Core.Presentation.Status.Shared.Help;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Presentation.Update.Shared.Prompts;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliStandaloneComposer
{
    internal static CliStandaloneComposition Compose(
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors,
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var findSymbols = FindBinding.CreateSymbols();
        var indexSymbols = IndexBinding.CreateSymbols();
        var statusSymbols = StatusBinding.CreateSymbols();
        var doctorSymbols = DoctorBinding.CreateSymbols();
        var repairSymbols = RepairBinding.CreateSymbols();
        var cleanupSymbols = CleanupBinding.CreateSymbols();
        var contextSymbols = ContextBinding.CreateSymbols();
        var referencesSymbols = ReferencesBinding.CreateSymbols();
        var installSymbols = InstallBinding.CreateSymbols();
        var updateSymbols = UpdateBinding.CreateSymbols();
        return new CliStandaloneComposition
        {
            FindBinding = BuildFind(findSymbols),
            IndexBinding = BuildIndex(indexSymbols, lockStoreRoot),
            StatusBinding = BuildStatus(
                statusSymbols,
                operationalContributors,
                physicalPathResolver),
            DoctorBinding = BuildDoctor(doctorSymbols, operationalContributors),
            RepairBinding = BuildRepair(repairSymbols, interaction, lockStoreRoot, operationalContributors),
            CleanupBinding = BuildCleanup(cleanupSymbols, lockStoreRoot),
            ContextBinding = BuildContext(contextSymbols),
            ReferencesBinding = BuildReferences(referencesSymbols),
            InstallBinding = BuildInstall(installSymbols, interaction, lockStoreRoot),
            UpdateBinding = BuildUpdate(updateSymbols, interaction, lockStoreRoot),
            RootLeaves =
            [
                new CliRootLeaf(findSymbols.FindCommand),
                new CliRootLeaf(indexSymbols.IndexCommand),
                new CliRootLeaf(statusSymbols.StatusCommand),
                new CliRootLeaf(doctorSymbols.DoctorCommand),
                new CliRootLeaf(repairSymbols.RepairCommand),
                new CliRootLeaf(cleanupSymbols.CleanupCommand),
                new CliRootLeaf(contextSymbols.ContextCommand),
                new CliRootLeaf(referencesSymbols.ReferencesCommand),
                new CliRootLeaf(installSymbols.InstallCommand),
                new CliRootLeaf(updateSymbols.UpdateCommand),
            ],
        };
    }

    private static ICliCommandBinding BuildCleanup(CleanupSymbols symbols, WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(CleanupBinding.CreateRequestBinding(symbols, new CleanupBindingComponents
        {
            Help = CleanupPresentation.CreateHelp(),
            Operation = new CleanupOperation(lockStoreRoot),
            InvalidResultFactory = CleanupInvalidResultFactory.Create,
        }), CleanupPresentation.Rendering);

    private static ICliCommandBinding BuildRepair(
        RepairSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors)
        => CliReportBinding.Close(RepairBinding.CreateRequestBinding(
            symbols,
            RepairPresentation.CreateHelp(),
            RepairOperationFactory.Create(
                RepairPromptAdapters.Create(interaction.Prompts, RepairPresentation.Rendering),
                lockStoreRoot,
                operationalContributors).ExecuteAsync), RepairPresentation.Rendering);

    private static ICliCommandBinding BuildFind(FindSymbols symbols)
        => CliReportBinding.Close(FindBinding.CreateRequestBinding(
            symbols,
            new FindBindingComponents
            {
                Help = FindPresentation.CreateHelp(),
                Operation = FindOperationFactory.Create(),
            }), FindPresentation.Rendering);

    private static ICliCommandBinding BuildIndex(
        IndexSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(IndexBinding.CreateRequestBinding(
            symbols: symbols,
            help: IndexHelpSections.Create(),
            operation: IndexOperationFactory.Create(lockStoreRoot).ExecuteAsync), OpenForge.Cli.Core.Presentation.Index.IndexPresentation.Rendering);

    private static ICliCommandBinding BuildStatus(
        StatusSymbols symbols,
        OperationalContributorCatalogue operationalContributors,
        PhysicalPathResolver physicalPathResolver)
        => CliReportBinding.Close(StatusBinding.CreateRequestBinding(
            symbols,
            new StatusBindingComponents
            {
                Help = StatusHelpSections.Create(),
                Operation = new StatusOperation(
                    operationalContributors,
                    physicalPathResolver),
            }), StatusPresentation.Rendering);

    private static ICliCommandBinding BuildDoctor(
        DoctorSymbols symbols,
        OperationalContributorCatalogue operationalContributors)
        => CliReportBinding.Close(DoctorBinding.CreateRequestBinding(
            symbols,
            new DoctorBindingComponents
            {
                Help = DoctorPresentation.CreateHelp(),
                Operation = new DoctorOperation(operationalContributors),
            }), DoctorPresentation.Rendering);

    private static ICliCommandBinding BuildInstall(
        InstallSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(InstallBinding.CreateRequestBinding(
            symbols: symbols,
            help: InstallHelpSections.Create(),
            operation: InstallOperationFactory.Create(
                interaction.Prompts.PlanConfirmation<InstallResult, InstallData, InstallConfirmationFacts>(
                    InstallPresentation.Rendering,
                    static (InstallConfirmationFacts facts) => InstallPlanConfirmationQuestion.Create(facts)),
                lockStoreRoot).ExecuteAsync), InstallPresentation.Rendering);

    private static ICliCommandBinding BuildUpdate(
        UpdateSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => CliReportBinding.Close(UpdateBinding.CreateRequestBinding(
            symbols: symbols,
            help: UpdatePresentation.CreateHelp(),
            operation: UpdateOperationFactory.Create(
                interaction.Prompts.PlanConfirmation<UpdateResult, UpdateData, UpdateConfirmationFacts>(
                    UpdatePresentation.Rendering,
                    static (UpdateConfirmationFacts facts) => UpdatePlanConfirmationQuestion.Create(facts)),
                lockStoreRoot).ExecuteAsync), UpdatePresentation.Rendering);

    private static ICliCommandBinding BuildReferences(ReferencesSymbols symbols)
        => CliReportBinding.Close(ReferencesBinding.CreateRequestBinding(
            symbols,
            new ReferencesBindingComponents
            {
                Help = OpenForge.Cli.Core.Presentation.References.ReferencesPresentation.CreateHelp(),
                Operation = ReferencesOperationFactory.Create(),
            }), OpenForge.Cli.Core.Presentation.References.ReferencesPresentation.Rendering);

    private static ICliCommandBinding BuildContext(ContextSymbols symbols)
        => CliReportBinding.Close(ContextBinding.CreateRequestBinding(
            symbols,
            new ContextBindingComponents
            {
                Help = ContextHelpSections.Create(),
                Operation = ContextOperationFactory.Create(),
            }), OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering);
}
