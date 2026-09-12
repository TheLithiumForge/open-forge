using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal static class ExtensionRemoveBinding
{
    internal static ExtensionRemoveSymbols CreateSymbols(Command extensionGroup)
    {
        ArgumentNullException.ThrowIfNull(extensionGroup);

        var command = new Command(
            ExtensionRemoveDefinitions.RemoveCommand.Name,
            ExtensionRemoveDefinitions.RemoveCommand.Description);
        var stableIds = new Argument<string[]>(ExtensionRemoveDefinitions.StableId.Name)
        {
            Description = ExtensionRemoveDefinitions.StableId.Description,
            Arity = ArgumentArity.ZeroOrMore,
        };
        var prune = Boolean(ExtensionRemoveDefinitions.Prune);
        var automatic = Boolean(ExtensionRemoveDefinitions.Automatic);
        var dryRun = Boolean(ExtensionRemoveDefinitions.DryRun);

        command.Arguments.Add(stableIds);
        command.Options.Add(prune);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        extensionGroup.Subcommands.Add(command);

        return new ExtensionRemoveSymbols(command, stableIds, prune, automatic, dryRun);
    }

    internal static CliCommandBinding<ExtensionRemoveRequest, ExtensionRemoveResult> Close(
        ExtensionRemoveSymbols symbols,
        CliHelpContent help,
        ExtensionRemoveOperation operation,
        CliRendererSet<ExtensionRemoveResult> renderers,
        CliDiagnosticRenderer<ExtensionRemoveResult>? diagnosticRenderer)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(help);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(renderers);

        var binder = new ExtensionRemoveRequestBinder(symbols);
        return new CliCommandBinding<ExtensionRemoveRequest, ExtensionRemoveResult>(
            symbols.Command,
            new CliCommandBindingComponents<ExtensionRemoveRequest, ExtensionRemoveResult>
            {
                Help = help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = binder.CreateInvalid,
                Operation = operation.ExecuteAsync,
                Renderers = renderers,
                DiagnosticRenderer = diagnosticRenderer,
            });
    }

    internal static ExtensionRemoveRequest BindRequest(
        ExtensionRemoveSymbols symbols,
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        if (invocation.Workspace is not { } workspace)
        {
            throw new InvalidOperationException(
                "The Extension Remove request requires a selected workspace.");
        }

        return ReadRequest(parse.Result, symbols, workspace, invocation.Presentation);
    }

    private static Option<bool> Boolean(CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };

    private sealed class ExtensionRemoveRequestBinder(ExtensionRemoveSymbols symbols)
    {
        private readonly ExtensionRemoveSymbols _symbols = symbols;

        internal CliBindResult<ExtensionRemoveRequest, ExtensionRemoveResult> Bind(
            CliBindingParse parse,
            CliInvocation invocation)
        {
            ArgumentNullException.ThrowIfNull(parse);
            ArgumentNullException.ThrowIfNull(invocation);
            var input = ReadInput(parse.Result);
            if (invocation.Workspace is not { } workspace)
            {
                return CliBindResult<ExtensionRemoveRequest, ExtensionRemoveResult>.Invalid(
                    Invalid(
                        workspace: null,
                        input,
                        ExtensionRemoveFindingCode.InvalidInput,
                        "The selected workspace is unavailable."));
            }

            var request = ReadRequest(parse.Result, _symbols, workspace, invocation.Presentation);
            var validation = Validate(parse.Result, request);
            return validation is null
                ? CliBindResult<ExtensionRemoveRequest, ExtensionRemoveResult>.Bound(request)
                : CliBindResult<ExtensionRemoveRequest, ExtensionRemoveResult>.Invalid(
                    Invalid(
                        request.Workspace,
                        input,
                        request.RequestedIds.Count == 0 && !request.AllowInteraction
                            ? ExtensionRemoveFindingCode.SelectionRequired
                            : ExtensionRemoveFindingCode.InvalidInput,
                        validation));
        }

        internal ExtensionRemoveResult CreateInvalid(CliInvalidBindingInput input)
        {
            ArgumentNullException.ThrowIfNull(input);
            var bindingInput = ReadInput(input.BindingParse.Result);
            var cause = input.InvalidInput.Diagnostics.Count == 1
                ? input.InvalidInput.Diagnostics[0]
                : string.Join(" ", input.InvalidInput.Diagnostics);
            return Invalid(
                workspace: null,
                bindingInput,
                ExtensionRemoveFindingCode.InvalidInput,
                cause);
        }

        private ExtensionRemoveBindingInput ReadInput(ParseResult result)
            => new(
                result.GetValue(_symbols.DryRun)
                    ? ExtensionRemoveMode.DryRun
                    : ExtensionRemoveMode.Apply,
                result.GetValue(_symbols.Prune),
                result.GetValue(_symbols.Automatic));

        private string? Validate(ParseResult result, ExtensionRemoveRequest request)
        {
            if (request.RequestedIds.Any(string.IsNullOrWhiteSpace)
                || request.RequestedIds.Distinct(StringComparer.Ordinal).Count()
                    != request.RequestedIds.Count)
            {
                return "Extension Remove requires unique non-empty stable-ID operands.";
            }

            if (request.RequestedIds.Count == 0 && !request.AllowInteraction)
            {
                return "Extension Remove requires explicit stable IDs outside a prompt-capable human request.";
            }

            return result.Errors.Count == 0
                ? null
                : string.Join(" ", result.Errors.Select(error => error.Message));
        }
    }

    private static ExtensionRemoveRequest ReadRequest(
        ParseResult result,
        ExtensionRemoveSymbols symbols,
        Framework.Workspace.Models.CliWorkspace workspace,
        CliPresentation presentation)
    {
        var automatic = result.GetValue(symbols.Automatic);
        return new ExtensionRemoveRequest(
            workspace,
            result.GetValue(symbols.DryRun)
                ? ExtensionRemoveMode.DryRun
                : ExtensionRemoveMode.Apply,
            ReadIds(result, symbols.StableIds),
            result.GetValue(symbols.Prune),
            automatic,
            allowInteraction: presentation.Format == CliOutputFormat.Human && !automatic);
    }

    private static string[] ReadIds(ParseResult result, Argument<string[]> argument)
    {
        try
        {
            return result.GetValue(argument) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    private static ExtensionRemoveResult Invalid(
        Framework.Workspace.Models.CliWorkspace? workspace,
        ExtensionRemoveBindingInput input,
        ExtensionRemoveFindingCode code,
        string cause)
        => ExtensionRemoveResult.Empty(
            workspace,
            input.Mode,
            input.Prune,
            input.Automatic,
            new ExtensionRemoveFinding(code, cause));
}
