using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

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

        var delimiterViolation = CliDelimiterGuard.Validate(parse.OriginalArguments, parse.DelimiterPolicies);
        if (delimiterViolation is not null)
        {
            return Invalid(
                "cli.delimiter.invalid",
                CliInvalidInputSource.Delimiter,
                [delimiterViolation.Describe()]);
        }

        var input = CliGlobalInputReader.Read(parse);
        try
        {
            var terminalMode = CliTerminalPolicy.Resolve(input.Help, input.Version);
            return new CliGlobalInputResolution(input, terminalMode, null);
        }
        catch (ArgumentException exception)
        {
            return Invalid(
                "cli.terminal.conflict",
                CliInvalidInputSource.Semantic,
                [exception.Message]);
        }
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
}
