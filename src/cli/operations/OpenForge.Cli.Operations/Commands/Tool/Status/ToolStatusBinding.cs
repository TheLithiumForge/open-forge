using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Status;

internal static class ToolStatusBinding
{
    internal static ToolStatusSymbols CreateSymbols(Command group)
    {
        var command = new Command("status", "Passively check recorded tool installs and configuration.");
        var id = new Argument<string?>("id") { Description = "One exact tool ID.", Arity = ArgumentArity.ZeroOrOne };
        command.Arguments.Add(id);

        group.Add(command);
        return new(command, id);
    }

    internal static CliRequestBinding<ToolStatusRequest, ToolStatusResult> Create(
        ToolStatusSymbols symbols, CliHelpContent help, ToolStatusOperation operation)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(symbols, parse, invocation),
            InvalidResultFactory = input => new ToolStatusResult
            {
                Status = CliSemanticStatus.Blocked,
                Issues = [new(ToolReadIssueCode.WorkspaceUnavailable, CliSemanticStatus.Blocked, input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory,
                    string.Join(" ", input.InvalidInput.Diagnostics))],
            },
            Operation = operation.ExecuteAsync,
        };

    private static CliBindResult<ToolStatusRequest, ToolStatusResult> Bind(ToolStatusSymbols symbols, CliBindingParse parse, CliInvocation invocation)
    {
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("The tool command requires a selected workspace.");

        var id = parse.Result.GetValue(symbols.Id);
        if (id is not null && !ToolIdentity.IsToolId(id)) return Invalid(invocation, "The tool ID is invalid.");


        return CliBindResult<ToolStatusRequest, ToolStatusResult>.Bound(new(workspace, id));
    }

    private static CliBindResult<ToolStatusRequest, ToolStatusResult> Invalid(CliInvocation invocation, string cause)
        => CliBindResult<ToolStatusRequest, ToolStatusResult>.Invalid(new()
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = invocation.Workspace,
            Issues = [new(ToolReadIssueCode.InvalidInput, CliSemanticStatus.Invalid, "tool status", cause)],
        });
}
