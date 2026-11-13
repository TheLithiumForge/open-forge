using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Models.Provider;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Inspect;

internal static class ToolInspectBinding
{
    internal static ToolInspectSymbols CreateSymbols(Command group)
    {
        var command = new Command("inspect", "Explain one exact tool definition and its requirements.");
        var id = new Argument<string?>("id") { Description = "One exact tool ID.", Arity = ArgumentArity.ZeroOrOne };
        command.Arguments.Add(id);
        var source = new Option<string?>("--source") { Description = "Read one exact local manifest or catalogue.", HelpName = "path", Arity = ArgumentArity.ExactlyOne };
        command.Options.Add(source);
        var version = new Option<string?>("--tool-version") { Description = "Select one exact SemVer pin.", HelpName = "exact", Arity = ArgumentArity.ExactlyOne };
        command.Options.Add(version);
        group.Add(command);
        return new(command, id, source, version);
    }

    internal static CliRequestBinding<ToolInspectRequest, ToolInspectResult> Create(
        ToolInspectSymbols symbols, CliHelpContent help, ToolInspectOperation operation)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(symbols, parse, invocation),
            InvalidResultFactory = input => new ToolInspectResult
            {
                Status = CliSemanticStatus.Blocked,
                Issues = [new(ToolReadIssueCode.WorkspaceUnavailable, CliSemanticStatus.Blocked, input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory,
                    string.Join(" ", input.InvalidInput.Diagnostics))],
            },
            Operation = operation.ExecuteAsync,
        };

    private static CliBindResult<ToolInspectRequest, ToolInspectResult> Bind(ToolInspectSymbols symbols, CliBindingParse parse, CliInvocation invocation)
    {
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("The tool command requires a selected workspace.");
        var source = parse.Result.GetValue(symbols.Source);
        var version = parse.Result.GetValue(symbols.Version);
        var id = parse.Result.GetValue(symbols.Id);
        if (id is not null && !ToolIdentity.IsToolId(id)) return Invalid(invocation, "The tool ID is invalid.");
        if (id is null) return Invalid(invocation, "One tool ID is required.");
        var sourceFacts = CliOptionResultFactsReader.Read(parse.Result, symbols.Source);
        if (sourceFacts.IdentifierCount > 1 || sourceFacts.IsExplicitWithoutValue || source is not null && string.IsNullOrWhiteSpace(source))
            return Invalid(invocation, "--source requires one non-empty value.");
        var versionFacts = CliOptionResultFactsReader.Read(parse.Result, symbols.Version);
        if (versionFacts.IdentifierCount > 1 || versionFacts.IsExplicitWithoutValue || version is not null && string.IsNullOrWhiteSpace(version))
            return Invalid(invocation, "--tool-version requires one non-empty value.");
        if (version is not null && !ToolIdentity.IsVersion(version)) return Invalid(invocation, "The exact tool version must be full SemVer.");
        if (source is not null)
        {
            try { source = Path.GetFullPath(source, invocation.WorkspaceRequest.CurrentDirectory); }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
            { return Invalid(invocation, "The source path is invalid."); }
        }
        return CliBindResult<ToolInspectRequest, ToolInspectResult>.Bound(new(workspace, id, source, version));
    }

    private static CliBindResult<ToolInspectRequest, ToolInspectResult> Invalid(CliInvocation invocation, string cause)
        => CliBindResult<ToolInspectRequest, ToolInspectResult>.Invalid(new()
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = invocation.Workspace,
            Issues = [new(ToolReadIssueCode.InvalidInput, CliSemanticStatus.Invalid, "tool inspect", cause)],
        });
}
