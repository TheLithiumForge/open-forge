using System.CommandLine;
using OpenForge.Cli.Core.Commands.Export.Models.Binding;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Export;

internal static class ExportBinding
{
    internal static ExportSymbols CreateSymbols()
    {
        var command = new Command(ExportDefinitions.Command.Name, ExportDefinitions.Command.Description);
        // The binder reports a missing destination, so --help still works without one.
        var destination = new Argument<string?>(ExportDefinitions.Destination.Name) { Description = ExportDefinitions.Destination.Description, Arity = ArgumentArity.ZeroOrOne };
        command.Arguments.Add(destination);
        var selection = StripSelectionBinding.AddSymbols(command);
        return new ExportSymbols { Command = command, Destination = destination, Selection = selection };
    }

    internal static CliRequestBinding<StripRequest, StripResult> CreateRequestBinding(ExportSymbols symbols, ExportBindingComponents components)
        => new()
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols),
            InvalidResultFactory = input => StripResultFactory.Invalid(StripCommandKind.Export, null,
                StripResultFactory.Cannot(StripCommandKind.Export, string.Join(" ", input.InvalidInput.Diagnostics))),
            Operation = components.Operation.ExecuteAsync,
        };

    private static CliBindResult<StripRequest, StripResult> Bind(CliBindingParse parse, CliInvocation invocation, ExportSymbols symbols)
    {
        var result = parse.Result;
        CliBindResult<StripRequest, StripResult> Invalid(string cause)
            => CliBindResult<StripRequest, StripResult>.Invalid(StripResultFactory.Invalid(StripCommandKind.Export, invocation.Workspace, cause));
        if (result.Errors.Count > 0)
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Export, string.Join(" ", result.Errors.Select(error => error.Message))));
        }

        var destination = result.GetValue(symbols.Destination);
        if (string.IsNullOrWhiteSpace(destination))
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Export, "supply a destination folder."));
        }

        var selection = StripSelectionBinding.Read(result, symbols.Selection, StripCommandKind.Export);
        if (selection.Problem is { } problem)
        {
            return Invalid(problem);
        }

        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A bound export invocation requires a selected workspace.");
        return CliBindResult<StripRequest, StripResult>.Bound(new StripRequest
        {
            Workspace = workspace,
            Command = StripCommandKind.Export,
            Destination = destination,
            Parts = selection.Parts,
            Flatten = selection.Flatten,
            RemoveEdited = selection.RemoveEdited,
            DryRun = selection.DryRun,
            RetryOptions = CliRetryOptions.From(invocation),
        });
    }
}
