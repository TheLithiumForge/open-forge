using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Context;

internal static class ContextBinding
{
    internal static ContextSymbols CreateSymbols() => ContextSymbols.Create();

    internal static CliCommandBinding<ContextRequest, ContextResult> Close(
        ContextSymbols symbols,
        ContextBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);
        var binder = new ContextRequestBinder(symbols);
        var workspaceResultFactory = new ContextWorkspaceResultFactory(symbols);
        return new CliCommandBinding<ContextRequest, ContextResult>(
            symbols.ContextCommand,
            new CliCommandBindingComponents<ContextRequest, ContextResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = workspaceResultFactory.Create,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
