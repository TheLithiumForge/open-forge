using OpenForge.Cli.Core.Commands.Help.Models.Binding;
using OpenForge.Cli.Core.Commands.Help.Models.Request;
using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Help;

internal static class HelpBinding
{
    internal static HelpSymbols CreateSymbols() => HelpSymbols.Create();

    internal static CliRequestBinding<HelpRequest, HelpResult> CreateRequestBinding(
        HelpSymbols symbols, CliHelpContent help, HelpOperation operation)
    {
        var binder = new HelpRequestBinder(symbols);
        return new CliRequestBinding<HelpRequest, HelpResult>
        {
            Command = symbols.HelpCommand,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Absent,
            Binder = binder.Bind,
            InvalidResultFactory = HelpInvalidResultFactory.Create,
            Operation = operation.ExecuteAsync,
        };
    }
}
