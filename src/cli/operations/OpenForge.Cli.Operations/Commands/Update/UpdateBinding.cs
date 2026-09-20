using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Update;

internal static class UpdateBinding
{
    internal static UpdateSymbols CreateSymbols()
        => UpdateSymbols.Create();

    internal static CliRequestBinding<UpdateRequest, UpdateResult> CreateRequestBinding(
        UpdateSymbols symbols,
        CliHelpContent help,
        CliOperation<UpdateRequest, UpdateResult> operation)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        var binder = new UpdateRequestBinder(symbols);
        var invalidResultFactory = new UpdateWorkspaceResultFactory(symbols);
        return new CliRequestBinding<UpdateRequest, UpdateResult>
        {
            Command = symbols.UpdateCommand,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = invalidResultFactory.Create,
            Operation = operation,
        };
    }
}
