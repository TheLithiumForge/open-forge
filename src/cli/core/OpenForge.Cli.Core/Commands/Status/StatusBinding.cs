using OpenForge.Cli.Core.Commands.Status.Models.Binding;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Status;

internal static class StatusBinding
{
    internal static StatusSymbols CreateSymbols() => StatusSymbols.Create();

    internal static CliCommandBinding<StatusRequest, StatusResult> Close(
        StatusSymbols symbols,
        StatusBindingComponents components)
    {
        var binder = new StatusRequestBinder();
        return new CliCommandBinding<StatusRequest, StatusResult>(
            symbols.StatusCommand,
            new CliCommandBindingComponents<StatusRequest, StatusResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = StatusWorkspaceResultFactory.Create,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
