using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Binding;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Binding;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Binding;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliStandaloneComposer
{
    internal static CliStandaloneComposition Compose(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors,
        LifecycleDocumentSnapshotReader lifecycleSnapshotReader)
    {
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
                lifecycleSnapshotReader),
            DoctorBinding = BuildDoctor(doctorSymbols, operationalContributors),
            RepairBinding = BuildRepair(repairSymbols, interactiveSession, lockStoreRoot, operationalContributors),
            CleanupBinding = BuildCleanup(cleanupSymbols, lockStoreRoot),
            ContextBinding = BuildContext(contextSymbols),
            ReferencesBinding = BuildReferences(referencesSymbols),
            InstallBinding = BuildInstall(installSymbols, interactiveSession, lockStoreRoot),
            UpdateBinding = BuildUpdate(updateSymbols, interactiveSession, lockStoreRoot),
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

    private static CliCommandBinding<CleanupRequest, CleanupResult> BuildCleanup(CleanupSymbols symbols, WorkspaceLockStoreRoot? lockStoreRoot)
        => CleanupBinding.Close(symbols, new CleanupBindingComponents
        {
            Help = CleanupHelpSections.Create(),
            Operation = new CleanupOperation(lockStoreRoot),
            InvalidResultFactory = CleanupInvalidResultFactory.Create,
            Renderers = new CliRendererSet<CleanupResult>(CleanupPresentation.Human, CleanupPresentation.Json),
            DiagnosticRenderer = CleanupPresentation.Diagnostic,
        });

    private static CliCommandBinding<RepairRequest, RepairResult> BuildRepair(
        RepairSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot,
        OperationalContributorCatalogue operationalContributors)
        => RepairBinding.Close(
            symbols,
            RepairPresentation.CreateHelp(),
            RepairOperationFactory.Create(interactiveSession, lockStoreRoot, operationalContributors).ExecuteAsync,
            RepairPresentation.CreateRenderers(),
            RepairPresentation.RenderDiagnostic);

    private static CliCommandBinding<FindRequest, FindResult> BuildFind(FindSymbols symbols)
        => FindBinding.Close(
            symbols,
            new FindBindingComponents
            {
                Help = FindHelpSections.Create(),
                Operation = FindOperationFactory.Create(),
                Renderers = new CliRendererSet<FindResult>(
                    FindHumanRenderer.Render,
                    FindJsonRenderer.Render),
                DiagnosticRenderer = FindDiagnosticRenderer.Render,
            });

    private static CliCommandBinding<IndexRequest, IndexResult> BuildIndex(
        IndexSymbols symbols,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => IndexBinding.Close(
            symbols: symbols,
            help: IndexHelpSections.Create(),
            operation: IndexOperationFactory.Create(lockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<IndexResult>(
                IndexHumanRenderer.Render,
                IndexJsonRenderer.Render),
            diagnosticRenderer: IndexDiagnosticRenderer.Render);

    private static CliCommandBinding<StatusRequest, StatusResult> BuildStatus(
        StatusSymbols symbols,
        OperationalContributorCatalogue operationalContributors,
        LifecycleDocumentSnapshotReader lifecycleSnapshotReader)
        => StatusBinding.Close(
            symbols,
            new StatusBindingComponents
            {
                Help = StatusHelpSections.Create(),
                Operation = new StatusOperation(
                    operationalContributors,
                    lifecycleSnapshotReader),
                Renderers = new CliRendererSet<StatusResult>(
                    StatusHumanRenderer.Render,
                    StatusJsonRenderer.Render),
                DiagnosticRenderer = StatusDiagnosticRenderer.Render,
            });

    private static CliCommandBinding<DoctorRequest, DoctorResult> BuildDoctor(
        DoctorSymbols symbols,
        OperationalContributorCatalogue operationalContributors)
        => DoctorBinding.Close(
            symbols,
            new DoctorBindingComponents
            {
                Help = DoctorHelpSections.Create(),
                Operation = new DoctorOperation(operationalContributors),
                Renderers = new CliRendererSet<DoctorResult>(
                    DoctorHumanRenderer.Render,
                    DoctorJsonRenderer.Render),
                DiagnosticRenderer = DoctorDiagnosticRenderer.Render,
            });

    private static CliCommandBinding<InstallRequest, InstallResult> BuildInstall(
        InstallSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => InstallBinding.Close(
            symbols: symbols,
            help: InstallHelpSections.Create(),
            operation: InstallOperationFactory.Create(
                interactiveSession,
                lockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<InstallResult>(
                InstallHumanRenderer.Render,
                InstallJsonRenderer.Render),
            diagnosticRenderer: InstallDiagnosticRenderer.Render);

    private static CliCommandBinding<UpdateRequest, UpdateResult> BuildUpdate(
        UpdateSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => UpdateBinding.Close(
            symbols: symbols,
            help: UpdateHelpSections.Create(),
            operation: UpdateOperationFactory.Create(
                interactiveSession,
                lockStoreRoot).ExecuteAsync,
            renderers: new CliRendererSet<UpdateResult>(
                UpdateHumanRenderer.Render,
                UpdateJsonRenderer.Render),
            diagnosticRenderer: UpdateDiagnosticRenderer.Render);

    private static CliCommandBinding<ReferencesRequest, ReferencesResult> BuildReferences(ReferencesSymbols symbols)
        => ReferencesBinding.Close(
            symbols,
            new ReferencesBindingComponents
            {
                Help = ReferencesHelpSections.Create(),
                Operation = ReferencesOperationFactory.Create(),
                Renderers = new CliRendererSet<ReferencesResult>(
                    ReferencesHumanRenderer.Render,
                    ReferencesJsonRenderer.Render),
                DiagnosticRenderer = ReferencesDiagnosticRenderer.Render,
            });

    private static CliCommandBinding<ContextRequest, ContextResult> BuildContext(ContextSymbols symbols)
        => ContextBinding.Close(
            symbols,
            new ContextBindingComponents
            {
                Help = ContextHelpSections.Create(),
                Operation = ContextOperationFactory.Create(),
                Renderers = new CliRendererSet<ContextResult>(
                    ContextHumanRenderer.Render,
                    ContextJsonRenderer.Render),
                DiagnosticRenderer = ContextDiagnosticRenderer.Render,
            });
}
