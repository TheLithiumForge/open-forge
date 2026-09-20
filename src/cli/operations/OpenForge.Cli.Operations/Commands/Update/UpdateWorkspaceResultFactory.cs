using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Update;

internal sealed class UpdateWorkspaceResultFactory(UpdateSymbols symbols)
{
    private readonly UpdateSymbols _symbols = symbols;

    internal UpdateResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var parsed = UpdateBindingInputReader.Read(
            input.BindingParse.Result,
            _symbols);
        return UpdateInvalidResultFactory.WorkspaceUnavailable(parsed);
    }
}
