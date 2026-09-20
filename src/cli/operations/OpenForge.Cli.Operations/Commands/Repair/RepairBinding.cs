using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Repair;

internal static class RepairBinding
{
    internal static RepairSymbols CreateSymbols() => RepairSymbols.Create();

    internal static CliRequestBinding<RepairRequest, RepairResult> CreateRequestBinding(
        RepairSymbols symbols,
        CliHelpContent help,
        CliOperation<RepairRequest, RepairResult> operation)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(help);
        ArgumentNullException.ThrowIfNull(operation);
        return new CliRequestBinding<RepairRequest, RepairResult>
        {
            Command = symbols.RepairCommand,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = new RepairRequestBinder(symbols).Bind,
            InvalidResultFactory = new RepairWorkspaceResultFactory(symbols).Create,
            Operation = operation,
        };
    }
}
