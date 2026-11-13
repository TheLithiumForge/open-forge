using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
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

namespace OpenForge.Cli.Core.Commands.Tool.List;

internal static class ToolListBinding
{
    internal static ToolListSymbols CreateSymbols(Command group)
    {
        var command = new Command("list", "List available definitions and installed tools.");

        var source = new Option<string?>("--source") { Description = "Read one exact local manifest or catalogue.", HelpName = "path", Arity = ArgumentArity.ExactlyOne };
        command.Options.Add(source);
        var role = new Option<string?>("--role") { Description = "Filter by a role capability.", HelpName = "role", Arity = ArgumentArity.ExactlyOne };
        command.Options.Add(role);
        group.Add(command);
        return new(command, source, role);
    }

    internal static CliRequestBinding<ToolListRequest, ToolListResult> Create(
        ToolListSymbols symbols, CliHelpContent help, ToolListOperation operation)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(symbols, parse, invocation),
            InvalidResultFactory = input => new ToolListResult
            {
                Status = CliSemanticStatus.Blocked,
                Issues = [new(ToolReadIssueCode.WorkspaceUnavailable, CliSemanticStatus.Blocked, input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory,
                    string.Join(" ", input.InvalidInput.Diagnostics))],
            },
            Operation = operation.ExecuteAsync,
        };

    private static CliBindResult<ToolListRequest, ToolListResult> Bind(ToolListSymbols symbols, CliBindingParse parse, CliInvocation invocation)
    {
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("The tool command requires a selected workspace.");
        var source = parse.Result.GetValue(symbols.Source);
        var role = parse.Result.GetValue(symbols.Role);

        var sourceFacts = CliOptionResultFactsReader.Read(parse.Result, symbols.Source);
        if (sourceFacts.IdentifierCount > 1 || sourceFacts.IsExplicitWithoutValue || source is not null && string.IsNullOrWhiteSpace(source))
            return Invalid(invocation, "--source requires one non-empty value.");
        var roleFacts = CliOptionResultFactsReader.Read(parse.Result, symbols.Role);
        if (roleFacts.IdentifierCount > 1 || roleFacts.IsExplicitWithoutValue || role is not null && string.IsNullOrWhiteSpace(role))
            return Invalid(invocation, "--role requires one non-empty value.");
        if (role is not null && (!ToolIdentity.IsId(role) || !ToolVocabulary.Roles.Contains(role))) return Invalid(invocation, "The role is unknown.");
        if (source is not null)
        {
            try { source = Path.GetFullPath(source, invocation.WorkspaceRequest.CurrentDirectory); }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
            { return Invalid(invocation, "The source path is invalid."); }
        }
        return CliBindResult<ToolListRequest, ToolListResult>.Bound(new(workspace, source, role));
    }

    private static CliBindResult<ToolListRequest, ToolListResult> Invalid(CliInvocation invocation, string cause)
        => CliBindResult<ToolListRequest, ToolListResult>.Invalid(new()
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = invocation.Workspace,
            Issues = [new(ToolReadIssueCode.InvalidInput, CliSemanticStatus.Invalid, "tool list", cause)],
        });
}
