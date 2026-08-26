using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal static class ExtensionInspectBinding
{
    internal static ExtensionInspectSymbols CreateSymbols(System.CommandLine.Command extensionGroup)
        => ExtensionInspectSymbols.Create(extensionGroup);

    internal static CliCommandBinding<ExtensionInspectRequest, ExtensionInspectResult> Close(
        ExtensionInspectSymbols symbols,
        ExtensionInspectBindingComponents components)
    {
        var binder = new ExtensionInspectRequestBinder(symbols);
        var invalidFactory = new ExtensionInspectWorkspaceResultFactory(symbols);
        return new CliCommandBinding<ExtensionInspectRequest, ExtensionInspectResult>(
            symbols.InspectCommand,
            new CliCommandBindingComponents<ExtensionInspectRequest, ExtensionInspectResult>
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
