using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Find;

internal static class FindBinding
{
    internal static FindSymbols CreateSymbols()
    {
        return FindSymbols.Create();
    }

    internal static CliCommandBinding<FindRequest, FindResult> Close(
        FindSymbols symbols,
        FindBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);

        var resultBuilder = new FindResultBuilder();
        var binder = new FindRequestBinder(symbols, resultBuilder);
        var workspaceResultFactory = new FindWorkspaceResultFactory(symbols, resultBuilder);
        return new CliCommandBinding<FindRequest, FindResult>(
            symbols.FindCommand,
            new CliCommandBindingComponents<FindRequest, FindResult>
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
