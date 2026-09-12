using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install;

internal static class ExtensionInstallBinding
{
    internal static ExtensionInstallSymbols CreateSymbols(Command extensionGroup)
    {
        var command = new Command(
            ExtensionInstallDefinitions.InstallCommand.Name,
            ExtensionInstallDefinitions.InstallCommand.Description);
        var ids = new Argument<string[]>("stable-id")
        {
            Description = "Exact stable IDs selected from the reviewed source.",
            Arity = ArgumentArity.ZeroOrMore,
        };
        var source = new Option<string?>(ExtensionInstallDefinitions.Source.Name)
        {
            Description = ExtensionInstallDefinitions.Source.Description,
            HelpName = ExtensionInstallDefinitions.Source.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var all = Boolean(ExtensionInstallDefinitions.All);
        var force = Boolean(ExtensionInstallDefinitions.Force);
        var automatic = Boolean(ExtensionInstallDefinitions.Automatic);
        var dryRun = Boolean(ExtensionInstallDefinitions.DryRun);
        command.Arguments.Add(ids);
        command.Options.Add(source);
        command.Options.Add(all);
        command.Options.Add(force);
        command.Options.Add(automatic);
        command.Options.Add(dryRun);
        extensionGroup.Add(command);
        return new ExtensionInstallSymbols(command, ids, source, all, force, automatic, dryRun);
    }

    internal static CliCommandBinding<ExtensionInstallRequest, ExtensionInstallResult> Close(
        ExtensionInstallSymbols symbols,
        CliHelpContent help,
        ExtensionInstallOperation operation,
        CliRendererSet<ExtensionInstallResult> renderers,
        CliDiagnosticRenderer<ExtensionInstallResult>? diagnosticRenderer)
    {
        var binder = new ExtensionInstallRequestBinder(symbols);
        return new CliCommandBinding<ExtensionInstallRequest, ExtensionInstallResult>(
            symbols.Command,
            new CliCommandBindingComponents<ExtensionInstallRequest, ExtensionInstallResult>
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

    private static Option<bool> Boolean(CliOptionDefinition<bool> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            Arity = ArgumentArity.Zero,
        };
}

internal sealed class ExtensionInstallRequestBinder(ExtensionInstallSymbols symbols)
{
    private readonly ExtensionInstallSymbols _symbols = symbols;

    internal CliBindResult<ExtensionInstallRequest, ExtensionInstallResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        if (invocation.Workspace is not { } workspace)
        {
            var input = ReadInput(parse.Result);
            return CliBindResult<ExtensionInstallRequest, ExtensionInstallResult>.Invalid(
                Invalid(
                    workspace: null,
                    input,
                    "The selected workspace is unavailable."));
        }

        var request = ReadRequest(parse.Result, workspace, invocation.Presentation);
        var cause = Validate(parse.Result, request);
        return cause is null
            ? CliBindResult<ExtensionInstallRequest, ExtensionInstallResult>.Bound(request)
            : CliBindResult<ExtensionInstallRequest, ExtensionInstallResult>.Invalid(Invalid(request, cause));
    }

    internal ExtensionInstallResult CreateInvalid(CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return Invalid(
            workspace: null,
            ReadInput(input.BindingParse.Result),
            cause);
    }

    private ExtensionInstallRequest ReadRequest(
        ParseResult result,
        Framework.Workspace.Models.CliWorkspace workspace,
        CliPresentation presentation)
    {
        var input = ReadInput(result);
        var ids = ReadIds(result);
        return new ExtensionInstallRequest(
            workspace,
            input.Mode,
            ids,
            result.GetValue(_symbols.All),
            ReadSource(result),
            input.Force,
            input.Automatic,
            allowInteraction: presentation.Format == CliOutputFormat.Human
                && !input.Automatic);
    }

    private ExtensionInstallBindingInput ReadInput(ParseResult result)
        => new(
            result.GetValue(_symbols.DryRun)
                ? ExtensionInstallMode.DryRun
                : ExtensionInstallMode.Apply,
            result.GetValue(_symbols.Force),
            result.GetValue(_symbols.Automatic));

    private string? Validate(ParseResult result, ExtensionInstallRequest request)
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
            || request.RequestedIds.Distinct(StringComparer.Ordinal).Count() != request.RequestedIds.Count)
        {
            return "Extension Install requires unique non-empty stable-ID operands.";
        }

        return result.Errors.Count == 0
            ? null
            : string.Join(" ", result.Errors.Select(error => error.Message));
    }

    private string[] ReadIds(ParseResult result)
    {
        try
        {
            return result.GetValue(_symbols.StableIds) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    private string? ReadSource(ParseResult result)
    {
        try
        {
            return result.GetValue(_symbols.Source);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ExtensionInstallResult Invalid(
        Framework.Workspace.Models.CliWorkspace? workspace,
        ExtensionInstallBindingInput input,
        string cause)
        => ExtensionInstallResult.Empty(
            workspace,
            input.Mode,
            input.Force,
            input.Automatic,
            new ExtensionInstallFinding(ExtensionInstallFindingCode.InvalidInput, cause));

    private static ExtensionInstallResult Invalid(
        ExtensionInstallRequest request,
        string cause)
        => Invalid(
            request.Workspace,
            new ExtensionInstallBindingInput(
                request.Mode,
                request.Force,
                request.Automatic),
            cause);
}
