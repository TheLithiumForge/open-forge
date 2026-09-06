using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Composition;

internal static class CliExtensionComposer
{
    internal static CliExtensionComposition Compose(
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var group = ExtensionBinding.CreateGroup();
        var listSymbols = ExtensionListBinding.CreateSymbols(group);
        var inspectSymbols = ExtensionInspectBinding.CreateSymbols(group);
        var createSymbols = ExtensionCreateBinding.CreateSymbols(group);
        var installSymbols = ExtensionInstallBinding.CreateSymbols(group);
        var updateSymbols = ExtensionUpdateBinding.CreateSymbols(group);
        var removeSymbols = ExtensionRemoveBinding.CreateSymbols(group);
        return new CliExtensionComposition
        {
            Branch = new CliRootBranch(group, ExtensionHelpSections.CreateGroup(), []),
            ListBinding = BuildList(listSymbols),
            InspectBinding = BuildInspect(inspectSymbols),
            CreateBinding = BuildCreate(createSymbols, interactiveSession),
            InstallBinding = BuildInstall(
                installSymbols,
                interactiveSession,
                lockStoreRoot),
            UpdateBinding = BuildUpdate(
                updateSymbols,
                interactiveSession,
                lockStoreRoot),
            RemoveBinding = BuildRemove(
                removeSymbols,
                interactiveSession,
                lockStoreRoot),
        };
    }

    private static ICliCommandBinding BuildList(ExtensionListSymbols symbols)
        => ExtensionListBinding.Close(
            symbols,
            new ExtensionListBindingComponents
            {
                Help = ExtensionListHelpSections.Create(),
                Operation = ExtensionListOperationFactory.Create(),
                Renderers = new CliRendererSet<ExtensionListResult>(
                    ExtensionListHumanRenderer.Render,
                    ExtensionListJsonRenderer.Render),
                DiagnosticRenderer = ExtensionListDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildInspect(ExtensionInspectSymbols symbols)
        => ExtensionInspectBinding.Close(
            symbols,
            new ExtensionInspectBindingComponents
            {
                Help = ExtensionInspectHelpSections.Create(),
                Operation = ExtensionInspectOperationFactory.Create(),
                Renderers = new CliRendererSet<ExtensionInspectResult>(
                    ExtensionInspectHumanRenderer.Render,
                    ExtensionInspectJsonRenderer.Render),
                DiagnosticRenderer = ExtensionInspectDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildCreate(
        ExtensionCreateSymbols symbols,
        CliInteractiveSession interactiveSession)
        => ExtensionCreateBinding.Close(
            symbols,
            new ExtensionCreateBindingComponents
            {
                Help = ExtensionCreateHelpSections.Create(),
                Operation = ExtensionCreateOperationFactory.Create(interactiveSession),
                Renderers = new CliRendererSet<ExtensionCreateResult>(
                    ExtensionCreateHumanRenderer.Render,
                    ExtensionCreateJsonRenderer.Render),
                DiagnosticRenderer = ExtensionCreateDiagnosticRenderer.Render,
            });

    private static ICliCommandBinding BuildInstall(
        ExtensionInstallSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => ExtensionInstallBinding.Close(
            symbols,
            ExtensionInstallPresentation.CreateHelp(),
            ExtensionInstallOperationFactory.Create(interactiveSession, lockStoreRoot),
            new CliRendererSet<ExtensionInstallResult>(
                ExtensionInstallPresentation.RenderHuman,
                ExtensionInstallJsonProjection.RenderJson),
            ExtensionInstallPresentation.RenderDiagnostic);

    private static ICliCommandBinding BuildUpdate(
        ExtensionUpdateSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => ExtensionUpdateBinding.Close(
            symbols,
            ExtensionUpdatePresentation.CreateHelp(),
            ExtensionUpdateOperationFactory.Create(interactiveSession, lockStoreRoot),
            new CliRendererSet<ExtensionUpdateResult>(
                ExtensionUpdatePresentation.RenderHuman,
                ExtensionUpdateJsonProjection.RenderJson),
            ExtensionUpdatePresentation.RenderDiagnostic);

    private static ICliCommandBinding BuildRemove(
        ExtensionRemoveSymbols symbols,
        CliInteractiveSession interactiveSession,
        WorkspaceLockStoreRoot? lockStoreRoot)
        => ExtensionRemoveBinding.Close(
            symbols,
            ExtensionRemovePresentation.CreateHelp(),
            ExtensionRemoveOperationFactory.Create(interactiveSession, lockStoreRoot),
            new CliRendererSet<ExtensionRemoveResult>(
                ExtensionRemovePresentation.RenderHuman,
                ExtensionRemoveJsonProjection.RenderJson),
            ExtensionRemovePresentation.RenderDiagnostic);
}
