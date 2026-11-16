using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Strip.Models.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Strip;

internal static class StripBinding
{
    internal static StripSymbols CreateSymbols()
    {
        var command = new Command(StripDefinitions.Command.Name, StripDefinitions.Command.Description);
        var parts = new Argument<string[]>(StripDefinitions.Parts.Name) { Description = StripDefinitions.Parts.Description, Arity = ArgumentArity.ZeroOrMore };
        var flatten = new Option<string?>(StripDefinitions.Flatten.Name) { Description = StripDefinitions.Flatten.Description, HelpName = StripDefinitions.Flatten.ValueName, Arity = ArgumentArity.ExactlyOne };
        var dryRun = new Option<bool>(StripDefinitions.DryRun.Name) { Description = StripDefinitions.DryRun.Description, Arity = ArgumentArity.Zero };
        var automatic = new Option<bool>(StripDefinitions.Automatic.Name) { Description = StripDefinitions.Automatic.Description, Arity = ArgumentArity.Zero };
        command.Arguments.Add(parts);
        command.Options.Add(flatten);
        command.Options.Add(dryRun);
        command.Options.Add(automatic);
        return new StripSymbols { Command = command, Parts = parts, Flatten = flatten, DryRun = dryRun, Automatic = automatic };
    }

    internal static CliRequestBinding<StripRequest, StripResult> CreateRequestBinding(StripSymbols symbols, StripBindingComponents components)
        => new()
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => Bind(parse, invocation, symbols),
            InvalidResultFactory = input => StripResultFactory.Invalid(StripCommandKind.Strip, null,
                StripResultFactory.Cannot(StripCommandKind.Strip, string.Join(" ", input.InvalidInput.Diagnostics))),
            Operation = (request, token) => components.Runner.RunAsync(request, null, token),
        };

    private static CliBindResult<StripRequest, StripResult> Bind(CliBindingParse parse, CliInvocation invocation, StripSymbols symbols)
    {
        var result = parse.Result;
        CliBindResult<StripRequest, StripResult> Invalid(string cause)
            => CliBindResult<StripRequest, StripResult>.Invalid(StripResultFactory.Invalid(StripCommandKind.Strip, invocation.Workspace, cause));
        if (result.Errors.Count > 0)
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Strip, string.Join(" ", result.Errors.Select(error => error.Message))));
        }

        if (!StripPartGrammar.TryRead(result.GetValue(symbols.Parts) ?? [], out var parts, out var rejected))
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Strip, $"'{rejected}' is not a part. Use {StrippingDefinitions.PartList()}."));
        }

        var flatten = result.GetValue(symbols.Flatten);
        if (parts.IsEmpty && flatten is null)
        {
            return Invalid(StripResultFactory.Cannot(StripCommandKind.Strip, $"select at least one part or {StrippingDefinitions.FlattenOption}."));
        }

        var automatic = result.GetValue(symbols.Automatic);
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A bound strip invocation requires a selected workspace.");
        return CliBindResult<StripRequest, StripResult>.Bound(new StripRequest
        {
            Workspace = workspace,
            Command = StripCommandKind.Strip,
            Parts = parts,
            Flatten = flatten,
            DryRun = result.GetValue(symbols.DryRun),
            Automatic = automatic,
            AllowInteractiveConfirmation = invocation.Presentation.Format == CliFormat.Text && !automatic,
            RetryOptions = CliRetryOptions.From(invocation),
        });
    }
}
