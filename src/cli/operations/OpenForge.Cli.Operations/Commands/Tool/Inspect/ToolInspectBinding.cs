using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect;


internal static class ToolInspectBinding
{
    internal static ToolInspectSymbols CreateSymbols(Command parent)
    {
        var command = new Command(ToolInspectDefinitions.CommandName, ToolInspectDefinitions.Purpose);
        var id = new Argument<string?>("id") { Arity = ArgumentArity.ExactlyOne };
        command.Arguments.Add(id);
        parent.Subcommands.Add(command);
        return new(command, id);
    }

    internal static CliRequestBinding<ToolInspectRequest, ToolInspectResult> CreateRequestBinding(ToolInspectSymbols symbols, Func<ToolInspectOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols, catalogue),
            InvalidResultFactory = input => new ToolInspectResult
            {
                Workspace = null,
                Status = InvalidStatus(input),
                Data = new([]),
                Findings = [new(ToolInspectDefinitions.InvalidInputCode, null, string.Join(" ", input.InvalidInput.Diagnostics), null)],
            },
            Operation = (request, token) => createOperation().ExecuteAsync(request, token),
        };

    internal static CliBindResult<ToolInspectRequest, ToolInspectResult> Bind(CliBindingParse parse, CliInvocation invocation, ToolInspectSymbols symbols, ToolCatalogue catalogue)
    {
        var id = parse.Result.GetValue(symbols.Id);
        if (id is null || id is not null && !catalogue.Entries.ContainsKey(id))
            return CliBindResult<ToolInspectRequest, ToolInspectResult>.Invalid(new ToolInspectResult
            {
                Workspace = invocation.Workspace,
                Status = CliSemanticStatus.Invalid,
                Data = new([]),
                Findings = [new(ToolInspectDefinitions.InvalidInputCode, id, "Use one exact catalogue ID.", null)],
            });
        return CliBindResult<ToolInspectRequest, ToolInspectResult>.Bound(new(invocation.Workspace ?? throw new InvalidOperationException("Tool Inspect requires a selected directory."), id));
    }

    private static CliSemanticStatus InvalidStatus(CliInvalidBindingInput input)
    {
        if (input.InvalidInput.Source != CliInvalidInputSource.Workspace) return CliSemanticStatus.Invalid;
        if (input.WorkspaceSelectionState == CliWorkspaceSelectionState.Unsafe) return CliSemanticStatus.Blocked;
        return CliSemanticStatus.Incomplete;
    }
}
