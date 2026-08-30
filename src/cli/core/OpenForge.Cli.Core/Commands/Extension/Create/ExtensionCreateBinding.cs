using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal static class ExtensionCreateBinding
{
    internal static ExtensionCreateSymbols CreateSymbols(System.CommandLine.Command extensionGroup)
        => ExtensionCreateSymbols.Create(extensionGroup);

    internal static CliCommandBinding<ExtensionCreateRequest, ExtensionCreateResult> Close(
        ExtensionCreateSymbols symbols,
        ExtensionCreateBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        var binder = new ExtensionCreateRequestBinder(symbols);
        var invalidFactory = new ExtensionCreateInvalidResultFactory(symbols);
        return new CliCommandBinding<ExtensionCreateRequest, ExtensionCreateResult>(
            symbols.CreateCommand,
            new CliCommandBindingComponents<ExtensionCreateRequest, ExtensionCreateResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Absent,
                Binder = binder.Bind,
                InvalidResultFactory = invalidFactory.Create,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
