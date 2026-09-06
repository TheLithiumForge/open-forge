using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal static class ExtensionUpdateBinding
{
    internal static ExtensionUpdateSymbols CreateSymbols(Command extensionGroup)
    {
        ArgumentNullException.ThrowIfNull(extensionGroup);

        var command = new Command(
            ExtensionUpdateDefinitions.UpdateCommand.Name,
            ExtensionUpdateDefinitions.UpdateCommand.Description);
        var stableIds = new Argument<string[]>("stable-id")
        {
            Description = "Exact managed stable IDs selected from the reviewed source.",
            Arity = ArgumentArity.ZeroOrMore,
        };
        var source = new Option<string?>(ExtensionUpdateDefinitions.Source.Name)
        {
            Description = ExtensionUpdateDefinitions.Source.Description,
            HelpName = ExtensionUpdateDefinitions.Source.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var all = Boolean(ExtensionUpdateDefinitions.All);
        var force = Boolean(ExtensionUpdateDefinitions.Force);
        var prune = Boolean(ExtensionUpdateDefinitions.Prune);
        var automatic = Boolean(ExtensionUpdateDefinitions.Automatic);
        var dryRun = Boolean(ExtensionUpdateDefinitions.DryRun);

        command.Arguments.Add(stableIds);
        command.Options.Add(source);
        command.Options.Add(all);
        command.Options.Add(force);
        command.Options.Add(prune);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        extensionGroup.Subcommands.Add(command);

        return new ExtensionUpdateSymbols(
            command,
            stableIds,
            source,
            all,
            force,
            prune,
            automatic,
            dryRun);
    }

    internal static CliCommandBinding<ExtensionUpdateRequest, ExtensionUpdateResult> Close(
        ExtensionUpdateSymbols symbols,
        CliHelpContent help,
        ExtensionUpdateOperation operation,
        CliRendererSet<ExtensionUpdateResult> renderers,
        CliDiagnosticRenderer<ExtensionUpdateResult>? diagnosticRenderer)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(help);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(renderers);

        var binder = new ExtensionUpdateRequestBinder(symbols);
        return new CliCommandBinding<ExtensionUpdateRequest, ExtensionUpdateResult>(
            symbols.Command,
            new CliCommandBindingComponents<ExtensionUpdateRequest, ExtensionUpdateResult>
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

    internal static ExtensionUpdateRequest BindRequest(
        ExtensionUpdateSymbols symbols,
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        if (invocation.Workspace is not { } workspace)
        {
            throw new InvalidOperationException(
                "The Extension Update request requires a selected workspace.");
        }

        return ReadRequest(parse.Result, symbols, workspace, invocation.Presentation);
    }

    private static Option<bool> Boolean(CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };

    private sealed class ExtensionUpdateRequestBinder(ExtensionUpdateSymbols symbols)
    {
        private readonly ExtensionUpdateSymbols _symbols = symbols;

        internal CliBindResult<ExtensionUpdateRequest, ExtensionUpdateResult> Bind(
            CliBindingParse parse,
            CliInvocation invocation)
        {
            var input = ReadInput(parse.Result);
            if (invocation.Workspace is not { } workspace)
            {
                return CliBindResult<ExtensionUpdateRequest, ExtensionUpdateResult>.Invalid(
                    Invalid(null, input, "The selected workspace is unavailable."));
            }

            var request = ReadRequest(parse.Result, _symbols, workspace, invocation.Presentation);
            var cause = Validate(parse.Result, request);
            return cause is null
                ? CliBindResult<ExtensionUpdateRequest, ExtensionUpdateResult>.Bound(request)
                : CliBindResult<ExtensionUpdateRequest, ExtensionUpdateResult>.Invalid(
                    Invalid(request.Workspace, input, cause));
        }

        internal ExtensionUpdateResult CreateInvalid(CliInvalidBindingInput input)
        {
            ArgumentNullException.ThrowIfNull(input);
            var bindingInput = ReadInput(input.BindingParse.Result);
            var cause = input.InvalidInput.Diagnostics.Count == 1
                ? input.InvalidInput.Diagnostics[0]
                : string.Join(" ", input.InvalidInput.Diagnostics);
            return Invalid(null, bindingInput, cause);
        }

        private ExtensionUpdateBindingInput ReadInput(ParseResult result)
            => new(
                result.GetValue(_symbols.DryRun)
                    ? ExtensionUpdateMode.DryRun
                    : ExtensionUpdateMode.Apply,
                result.GetValue(_symbols.Force),
                result.GetValue(_symbols.Prune),
                result.GetValue(_symbols.Automatic));

        private string? Validate(ParseResult result, ExtensionUpdateRequest request)
        {
            var sourceFacts = CliOptionResultFactsReader.Read(result, _symbols.Source);
            if (sourceFacts.IdentifierCount > 1
                || sourceFacts.IsExplicitWithoutValue
                || request.SourcePath is not null && string.IsNullOrWhiteSpace(request.SourcePath))
            {
                return "--source accepts exactly one non-empty package or catalogue path.";
            }

            if (request.All && request.RequestedIds.Count > 0)
            {
                return "Explicit Extension IDs and --all cannot be combined.";
            }

            if (request.RequestedIds.Any(string.IsNullOrWhiteSpace)
                || request.RequestedIds.Distinct(StringComparer.Ordinal).Count()
                    != request.RequestedIds.Count)
            {
                return "Extension Update requires unique non-empty stable-ID operands.";
            }

            return result.Errors.Count == 0
                ? null
                : string.Join(" ", result.Errors.Select(error => error.Message));
        }
    }

    private static ExtensionUpdateRequest ReadRequest(
        ParseResult result,
        ExtensionUpdateSymbols symbols,
        Framework.Workspace.CliWorkspace workspace,
        CliPresentation presentation)
    {
        var automatic = result.GetValue(symbols.Automatic);
        return new ExtensionUpdateRequest(
            workspace,
            result.GetValue(symbols.DryRun)
                ? ExtensionUpdateMode.DryRun
                : ExtensionUpdateMode.Apply,
            ReadIds(result, symbols.StableIds),
            result.GetValue(symbols.All),
            ReadSource(result, symbols.Source),
            result.GetValue(symbols.Force),
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

    private static string? ReadSource(ParseResult result, Option<string?> option)
    {
        try
        {
            return result.GetValue(option);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ExtensionUpdateResult Invalid(
        Framework.Workspace.CliWorkspace? workspace,
        ExtensionUpdateBindingInput input,
        string cause)
        => ExtensionUpdateResult.Empty(
            workspace,
            input.Mode,
            input.Force,
            input.Prune,
            input.Automatic,
            new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.InvalidInput,
                cause));
}
