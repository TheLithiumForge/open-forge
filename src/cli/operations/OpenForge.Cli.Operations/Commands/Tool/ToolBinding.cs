using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Application;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool;

internal sealed record ToolWriteSymbols(ToolCommandKind Kind, Command Command, Argument<string?> Id, Option<bool> Yes, Option<bool> DryRun);

internal static class ToolBinding
{
    internal static Command CreateGroup()
    {
        var group = new Command(ToolCommandDefinitions.Group.Name, ToolCommandDefinitions.Group.Description);
        group.SetAction(static _ => 0);
        return group;
    }

    internal static Command CreateListSymbols(Command parent)
    {
        ArgumentNullException.ThrowIfNull(parent);
        var command = new Command(ToolCommandDefinitions.ListName, ToolCommandDefinitions.ListPurpose);
        parent.Subcommands.Add(command);
        return command;
    }

    internal static ToolWriteSymbols CreateWriteSymbols(Command parent, ToolCommandKind kind)
    {
        ArgumentNullException.ThrowIfNull(parent);
        var command = new Command(ToolCommandDefinitions.Name(kind),
            kind == ToolCommandKind.Remove ? ToolCommandDefinitions.RemovePurpose : ToolCommandDefinitions.InstallPurpose);
        var id = new Argument<string?>(ToolCommandDefinitions.IdArgument)
        {
            Description = kind == ToolCommandKind.Remove
                ? "The tool ID from open-forge tool list."
                : "The tool ID from open-forge tool list. Leave it out to install this project's tools that are missing.",
            // Remove needs an ID, but the parser keeps it optional so help still works. The binder rejects a missing one.
            Arity = ArgumentArity.ZeroOrOne,
        };
        var yes = new Option<bool>(ToolCommandDefinitions.YesOption)
        {
            Description = kind == ToolCommandKind.Remove
                ? "Don't ask. Keep the tool installed and show its uninstall commands."
                : "Accept the shown plan without asking.",
            Arity = ArgumentArity.Zero,
        };
        var dryRun = new Option<bool>(ToolCommandDefinitions.DryRunOption) { Description = "Show the plan without running or changing anything.", Arity = ArgumentArity.Zero };
        command.Arguments.Add(id);
        command.Options.Add(yes);
        command.Options.Add(dryRun);
        parent.Subcommands.Add(command);
        return new(kind, command, id, yes, dryRun);
    }

    internal static CliRequestBinding<ToolListRequest, ToolListResult> CreateListBinding(
        Command command, CliHelpContent help, CliOperation<ToolListRequest, ToolListResult> operation)
        => new()
        {
            Command = command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => invocation.Workspace is { } workspace
                ? CliBindResult<ToolListRequest, ToolListResult>.Bound(new(workspace))
                : CliBindResult<ToolListRequest, ToolListResult>.Invalid(ListInvalid(null, CliSemanticStatus.Blocked)),
            InvalidResultFactory = input => ListInvalid(input, Status(input)),
            Operation = operation,
        };

    internal static CliRequestBinding<ToolWriteRequest, ToolWriteResult> CreateWriteBinding(
        ToolWriteSymbols symbols, ToolCatalogue catalogue, CliHelpContent help, CliOperation<ToolWriteRequest, ToolWriteResult> operation)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) =>
            {
                var id = parse.Result.GetValue(symbols.Id);
                if (parse.Result.Errors.Count != 0 || id is not null && string.IsNullOrWhiteSpace(id) || id is null && symbols.Kind == ToolCommandKind.Remove)
                    return Invalid(new(ToolFindingCode.InvalidInput, CliSemanticStatus.Invalid));
                if (id is not null && catalogue.Find(id) is null)
                    return Invalid(new(ToolFindingCode.UnknownTool, CliSemanticStatus.Invalid, id));
                if (invocation.Workspace is not { } workspace)
                    return Invalid(new(ToolFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked));
                return CliBindResult<ToolWriteRequest, ToolWriteResult>.Bound(new()
                {
                    Kind = symbols.Kind,
                    Workspace = workspace,
                    Id = id,
                    Yes = parse.Result.GetValue(symbols.Yes),
                    Preview = parse.Result.GetValue(symbols.DryRun),
                    Format = invocation.Presentation.Format,
                });

                CliBindResult<ToolWriteRequest, ToolWriteResult> Invalid(ToolFinding finding)
                    => CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolResults.Invalid(symbols.Kind, invocation.Workspace, finding));
            },
            InvalidResultFactory = input => ToolResults.Invalid(symbols.Kind, null,
                new(ToolFindingCode.InvalidInput, Status(input), Detail: string.Join("\n", input.InvalidInput.Diagnostics))),
            Operation = operation,
        };

    private static ToolListResult ListInvalid(CliInvalidBindingInput? input, CliSemanticStatus status) => new()
    {
        Workspace = null,
        Status = status,
        Data = new([]),
        Findings = [new(ToolFindingCode.InvalidInput, status, Detail: input is null ? null : string.Join("\n", input.InvalidInput.Diagnostics))],
    };

    private static CliSemanticStatus Status(CliInvalidBindingInput input)
    {
        if (input.InvalidInput.Source != CliInvalidInputSource.Workspace) return CliSemanticStatus.Invalid;
        return input.WorkspaceSelectionState == CliWorkspaceSelectionState.Unsafe ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete;
    }
}
