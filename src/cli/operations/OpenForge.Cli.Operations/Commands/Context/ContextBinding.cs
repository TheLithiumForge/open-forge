using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Context;

internal static class ContextBinding
{
    internal static ContextSymbols CreateSymbols() => ContextSymbols.Create();

    internal static CliRequestBinding<ContextRequest, ContextResult> CreateRequestBinding(
        ContextSymbols symbols,
        ContextBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);
        var binder = new ContextRequestBinder(symbols);
        var workspaceResultFactory = new ContextWorkspaceResultFactory(symbols);
        return new CliRequestBinding<ContextRequest, ContextResult>
        {
            Command = symbols.ContextCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = workspaceResultFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
