using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairBinding
{
    internal static RepairSymbols CreateSymbols() => RepairSymbols.Create();

    internal static CliCommandBinding<RepairRequest, RepairResult> Close(
        RepairSymbols symbols,
        CliHelpContent help,
        CliOperation<RepairRequest, RepairResult> operation,
        CliRendererSet<RepairResult> renderers,
        CliDiagnosticRenderer<RepairResult>? diagnosticRenderer)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(help);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(renderers);
        return new CliCommandBinding<RepairRequest, RepairResult>(
            symbols.RepairCommand,
            new CliCommandBindingComponents<RepairRequest, RepairResult>
            {
                Help = help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = new RepairRequestBinder(symbols).Bind,
                InvalidResultFactory = new RepairWorkspaceResultFactory(symbols).Create,
                Operation = operation,
                Renderers = renderers,
                DiagnosticRenderer = diagnosticRenderer,
            });
    }
}
