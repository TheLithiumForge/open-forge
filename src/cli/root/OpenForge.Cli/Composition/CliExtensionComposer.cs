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
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Composition;

internal static class CliExtensionComposer
{
    internal static CliExtensionComposition Compose(
        CliInteractiveSession interactiveSession)
    {
        var group = ExtensionBinding.CreateGroup();
        var listSymbols = ExtensionListBinding.CreateSymbols(group);
        var inspectSymbols = ExtensionInspectBinding.CreateSymbols(group);
        var createSymbols = ExtensionCreateBinding.CreateSymbols(group);
        return new CliExtensionComposition
        {
            Branch = new CliRootBranch(group, ExtensionHelpSections.CreateGroup(), []),
            ListBinding = BuildList(listSymbols),
            InspectBinding = BuildInspect(inspectSymbols),
            CreateBinding = BuildCreate(createSymbols, interactiveSession),
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
}
