using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Update;

internal sealed class UpdateRequestBinder(UpdateSymbols symbols)
{
    private readonly UpdateSymbols _symbols = symbols;

    internal CliBindResult<UpdateRequest, UpdateResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var input = UpdateBindingInputReader.Read(parse.Result, _symbols);
        if (invocation.Workspace is not { } workspace)
        {
            return CliBindResult<UpdateRequest, UpdateResult>.Invalid(
                UpdateInvalidResultFactory.WorkspaceUnavailable(input));
        }

        var allowsInteractiveConfirmation = invocation.Presentation.Format == CliOutputFormat.Human
            && !input.Automatic
            && input.Mode == UpdateMode.Apply;
        return CliBindResult<UpdateRequest, UpdateResult>.Bound(
            new UpdateRequest(
                workspace,
                input.Mode,
                input.Force,
                input.Prune,
                input.Automatic,
                allowsInteractiveConfirmation));
    }
}
