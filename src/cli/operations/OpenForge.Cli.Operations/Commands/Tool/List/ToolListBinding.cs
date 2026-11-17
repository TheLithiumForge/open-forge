using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Tool.List;


internal static class ToolListBinding
{
    internal static Command CreateSymbols(Command parent)
    {
        var command = new Command(ToolListDefinitions.CommandName, ToolListDefinitions.Purpose);
        parent.Subcommands.Add(command);
        return command;
    }

    internal static CliRequestBinding<ToolListRequest, ToolListResult> CreateRequestBinding(Command symbols, Func<ToolListOperation> createOperation, CliHelpContent help, ToolCatalogue catalogue)
        => new()
        {
            Command = symbols,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => CliBindResult<ToolListRequest, ToolListResult>.Bound(new(invocation.Workspace ?? throw new InvalidOperationException("Tool List requires a selected directory."))),
            InvalidResultFactory = input => new ToolListResult
            {
                Workspace = null,
                Status = InvalidStatus(input),
                Data = ToolListOperation.Catalogue(catalogue),
                Findings = [new(ToolListDefinitions.InvalidInputCode, null, string.Join(" ", input.InvalidInput.Diagnostics), null)],
            },
            Operation = (request, token) => createOperation().ExecuteAsync(request, token),
        };

    private static CliSemanticStatus InvalidStatus(CliInvalidBindingInput input)
    {
        if (input.InvalidInput.Source != CliInvalidInputSource.Workspace) return CliSemanticStatus.Invalid;
        if (input.WorkspaceSelectionState == CliWorkspaceSelectionState.Unsafe) return CliSemanticStatus.Blocked;
        return CliSemanticStatus.Incomplete;
    }
}
