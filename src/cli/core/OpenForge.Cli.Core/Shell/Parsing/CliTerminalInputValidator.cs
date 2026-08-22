using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliTerminalInputValidator
{
    internal static CliInvalidInput? Validate(CliParseOutcome parse, CliGlobalInput input)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Help && !input.Version)
        {
            return null;
        }

        if (parse.Result.UnmatchedTokens.Count > 0)
        {
            return Invalid();
        }

        return ValidateCommandResult(parse.Result.RootCommandResult, parse.Options);
    }

    private static CliInvalidInput? ValidateCommandResult(
        CommandResult commandResult,
        CliGlobalOptionSymbols options)
    {
        foreach (var child in commandResult.Children)
        {
            switch (child)
            {
                case CommandResult nestedCommand:
                    var nestedInvalid = ValidateCommandResult(nestedCommand, options);
                    if (nestedInvalid is not null)
                    {
                        return nestedInvalid;
                    }

                    break;
                case ArgumentResult:
                    return Invalid();
                case OptionResult optionResult
                    when !optionResult.Implicit && !IsGlobalOption(optionResult.Option, options):
                    return Invalid();
            }
        }

        return null;
    }

    private static bool IsGlobalOption(
        Option option,
        CliGlobalOptionSymbols options)
    {
        return ReferenceEquals(option, options.Workspace)
            || ReferenceEquals(option, options.Json)
            || ReferenceEquals(option, options.View)
            || ReferenceEquals(option, options.Verbose)
            || ReferenceEquals(option, options.Help)
            || ReferenceEquals(option, options.Version);
    }

    private static CliInvalidInput Invalid()
    {
        return new CliInvalidInput(
            "cli.terminal.input",
            CliInvalidInputSource.Semantic,
            ["Terminal options cannot be combined with command input."]);
    }
}
