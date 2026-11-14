using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Tool.Status;


internal static class ToolStatusBinding
{
    internal static ToolStatusSymbols CreateSymbols(Command parent)
    {
        var command = new Command(ToolStatusDefinitions.CommandName, "Check tool versions, project additions and PATH readiness.");
        var id = new Argument<string?>("id") { Description = "The exact catalogue tool ID.", Arity = ArgumentArity.ZeroOrOne };
        command.Arguments.Add(id);
        parent.Subcommands.Add(command);
        return new(command, id);
    }

    internal static CliRequestBinding<ToolStatusRequest, ToolStatusResult> CreateRequestBinding(ToolStatusSymbols symbols, Func<ToolStatusOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols, catalogue),
            InvalidResultFactory = input => new ToolStatusResult
            {
                Workspace = null,
                Status = InvalidStatus(input),
                Data = new([]),
                Findings = [new(ToolStatusDefinitions.InvalidInputCode, null, string.Join(" ", input.InvalidInput.Diagnostics), null)],
            },
            Operation = (request, token) => createOperation().ExecuteAsync(request, token),
        };

    internal static CliBindResult<ToolStatusRequest, ToolStatusResult> Bind(CliBindingParse parse, CliInvocation invocation, ToolStatusSymbols symbols, ToolCatalogue catalogue)
    {
        var id = parse.Result.GetValue(symbols.Id);
        if (id is not null && !catalogue.Entries.ContainsKey(id))
            return CliBindResult<ToolStatusRequest, ToolStatusResult>.Invalid(new ToolStatusResult
            {
                Workspace = invocation.Workspace,
                Status = CliSemanticStatus.Invalid,
                Data = new([]),
                Findings = [new(ToolStatusDefinitions.InvalidInputCode, id, "Use one exact catalogue ID.", null)],
            });
        return CliBindResult<ToolStatusRequest, ToolStatusResult>.Bound(new(invocation.Workspace ?? throw new InvalidOperationException("Tool Status requires a selected directory."), id));
    }

    private static CliSemanticStatus InvalidStatus(CliInvalidBindingInput input)
    {
        if (input.InvalidInput.Source != CliInvalidInputSource.Workspace) return CliSemanticStatus.Invalid;
        if (input.WorkspaceSelectionState == CliWorkspaceSelectionState.Unsafe) return CliSemanticStatus.Blocked;
        return CliSemanticStatus.Incomplete;
    }
}
