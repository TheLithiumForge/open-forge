using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Uninstall;

internal static class UninstallBinding
{
    internal static UninstallSymbols CreateSymbols()
    {
        var command = new Command(UninstallDefinitions.Command.Name, UninstallDefinitions.Command.Description);
        var selection = StripSelectionBinding.AddSymbols(command);
        var automatic = new Option<bool>(UninstallDefinitions.Automatic.Name) { Description = UninstallDefinitions.Automatic.Description, Arity = ArgumentArity.Zero };
        command.Options.Add(automatic);
        return new UninstallSymbols { Command = command, Selection = selection, Automatic = automatic };
    }

    internal static CliRequestBinding<StripRequest, StripResult> CreateRequestBinding(UninstallSymbols symbols, UninstallBindingComponents components)
        => new()
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols),
            InvalidResultFactory = input => StripResultFactory.Invalid(StripCommandKind.Uninstall, null,
                StripResultFactory.Cannot(StripCommandKind.Uninstall, string.Join(" ", input.InvalidInput.Diagnostics))),
            Operation = components.Operation.ExecuteAsync,
        };

    private static CliBindResult<StripRequest, StripResult> Bind(CliBindingParse parse, CliInvocation invocation, UninstallSymbols symbols)
    {
        var result = parse.Result;
        CliBindResult<StripRequest, StripResult> Invalid(string cause)
            => CliBindResult<StripRequest, StripResult>.Invalid(StripResultFactory.Invalid(StripCommandKind.Uninstall, invocation.Workspace, cause));
        if (result.Errors.Count > 0)
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Uninstall, string.Join(" ", result.Errors.Select(error => error.Message))));
        }

        var selection = StripSelectionBinding.Read(result, symbols.Selection, StripCommandKind.Uninstall);
        if (selection.Problem is { } problem)
        {
            return Invalid(problem);
        }

        var automatic = result.GetValue(symbols.Automatic);
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A bound uninstall invocation requires a selected workspace.");
        return CliBindResult<StripRequest, StripResult>.Bound(new StripRequest
        {
            Workspace = workspace,
            Command = StripCommandKind.Uninstall,
            Parts = selection.Parts,
            Flatten = selection.Flatten,
            RemoveEdited = selection.RemoveEdited,
            DryRun = selection.DryRun,
            Automatic = automatic,
            AllowInteractiveConfirmation = invocation.Presentation.Format == CliFormat.Text && !automatic,
            RetryOptions = CliRetryOptions.From(invocation),
        });
    }
}
