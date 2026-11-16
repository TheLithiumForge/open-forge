using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Uninstall;

internal static class UninstallBinding
{
    internal static UninstallSymbols CreateSymbols()
    {
        var command = new Command(UninstallDefinitions.Command.Name, UninstallDefinitions.Command.Description);
        var strip = new Option<string[]>(UninstallDefinitions.Strip.Name)
        {
            Description = UninstallDefinitions.Strip.Description,
            HelpName = UninstallDefinitions.Strip.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var flatten = new Option<string?>(UninstallDefinitions.Flatten.Name) { Description = UninstallDefinitions.Flatten.Description, HelpName = UninstallDefinitions.Flatten.ValueName, Arity = ArgumentArity.ExactlyOne };
        var removeEdited = new Option<string[]>(UninstallDefinitions.RemoveEdited.Name)
        {
            Description = UninstallDefinitions.RemoveEdited.Description,
            HelpName = UninstallDefinitions.RemoveEdited.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var dryRun = new Option<bool>(UninstallDefinitions.DryRun.Name) { Description = UninstallDefinitions.DryRun.Description, Arity = ArgumentArity.Zero };
        var automatic = new Option<bool>(UninstallDefinitions.Automatic.Name) { Description = UninstallDefinitions.Automatic.Description, Arity = ArgumentArity.Zero };
        command.Options.Add(strip);
        command.Options.Add(flatten);
        command.Options.Add(removeEdited);
        command.Options.Add(dryRun);
        command.Options.Add(automatic);
        return new UninstallSymbols { Command = command, Strip = strip, Flatten = flatten, RemoveEdited = removeEdited, DryRun = dryRun, Automatic = automatic };
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

        var stripFacts = CliOptionResultFactsReader.Read(result, symbols.Strip);
        var stripValues = CliOptionResultFactsReader.ReadValues(result, symbols.Strip);
        if (stripFacts.IsExplicitWithoutValue)
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Uninstall, $"supply a part after each {StrippingDefinitions.StripOption}."));
        }

        if (!StripPartGrammar.TryRead(stripValues, out var parts, out var rejected))
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Uninstall,
                $"'{rejected}' is not a {StrippingDefinitions.StripOption} part. Use {StrippingDefinitions.PartList()}."));
        }

        var editedFacts = CliOptionResultFactsReader.Read(result, symbols.RemoveEdited);
        var edited = CliOptionResultFactsReader.ReadValues(result, symbols.RemoveEdited);
        if (editedFacts.IsExplicitWithoutValue || edited.Any(string.IsNullOrWhiteSpace))
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Uninstall, $"supply a workspace-relative path after each {StrippingDefinitions.RemoveEditedOption}."));
        }

        var automatic = result.GetValue(symbols.Automatic);
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A bound uninstall invocation requires a selected workspace.");
        return CliBindResult<StripRequest, StripResult>.Bound(new StripRequest
        {
            Workspace = workspace,
            Command = StripCommandKind.Uninstall,
            Parts = parts,
            Flatten = result.GetValue(symbols.Flatten),
            RemoveEdited = [.. edited.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            DryRun = result.GetValue(symbols.DryRun),
            Automatic = automatic,
            AllowInteractiveConfirmation = invocation.Presentation.Format == CliFormat.Text && !automatic,
            RetryOptions = CliRetryOptions.From(invocation),
        });
    }
}
