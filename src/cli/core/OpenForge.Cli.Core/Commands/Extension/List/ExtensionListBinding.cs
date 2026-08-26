using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal static class ExtensionListBinding
{
    internal static ExtensionListSymbols CreateSymbols(System.CommandLine.Command extensionGroup)
        => ExtensionListSymbols.Create(extensionGroup);

    internal static CliCommandBinding<ExtensionListRequest, ExtensionListResult> Close(
        ExtensionListSymbols symbols,
        ExtensionListBindingComponents components)
    {
        var binder = new ExtensionListRequestBinder(symbols);
        var invalidFactory = new ExtensionListWorkspaceResultFactory(symbols);
        return new CliCommandBinding<ExtensionListRequest, ExtensionListResult>(
            symbols.ListCommand,
            new CliCommandBindingComponents<ExtensionListRequest, ExtensionListResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = invalidFactory.Create,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
