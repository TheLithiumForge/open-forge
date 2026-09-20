using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliTerminalValidator
{
    internal static CliGlobalInputResolution Validate(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        if (parse.Result.Errors.Count > 0)
        {
            return Invalid(
                "cli.parser.invalid",
                CliInvalidInputSource.Parser,
                parse.Result.Errors.Select(error => error.Message));
        }

        CliGlobalInput input;
        try
        {
            input = CliGlobalInputReader.Read(parse);
        }
        catch (ArgumentException exception)
        {
            return Invalid(
                "cli.semantic.invalid",
                CliInvalidInputSource.Semantic,
                [exception.Message]);
        }

        CliTerminalMode terminalMode;
        try
        {
            terminalMode = CliTerminalPolicy.Resolve(input.Help, input.Version);
        }
        catch (ArgumentException exception)
        {
            return Invalid(
                "cli.terminal.conflict",
                CliInvalidInputSource.Semantic,
                [exception.Message]);
        }

        var terminalInvalid = CliTerminalInputValidator.Validate(parse, input);
        return terminalInvalid is null
            ? new CliGlobalInputResolution(input, terminalMode, null)
            : Invalid(terminalInvalid);
    }

    private static CliGlobalInputResolution Invalid(
        string code,
        CliInvalidInputSource source,
        IEnumerable<string> diagnostics)
    {
        return new CliGlobalInputResolution(
            null,
            CliTerminalMode.None,
            new CliInvalidInput(code, source, diagnostics));
    }

    private static CliGlobalInputResolution Invalid(CliInvalidInput invalidInput)
    {
        return new CliGlobalInputResolution(
            null,
            CliTerminalMode.None,
            invalidInput);
    }
}
