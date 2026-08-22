using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliGlobalInputReader
{
    internal static CliGlobalInput Read(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var result = parse.Result;
        var options = parse.Options;
        var json = result.GetValue(options.Json);
        var verbose = result.GetValue(options.Verbose);
        return new CliGlobalInput(
            result.GetValue(options.Workspace),
            ReadOccurrences(result, options.Workspace),
            json ? CliOutputFormat.Json : CliOutputFormat.Human,
            ReadOccurrences(result, options.Json),
            result.GetValue(options.View),
            ReadOccurrences(result, options.View),
            verbose ? CliVerbosity.Verbose : CliVerbosity.Normal,
            ReadOccurrences(result, options.Verbose),
            result.GetValue(options.Help),
            ReadOccurrences(result, options.Help),
            result.GetValue(options.Version),
            ReadOccurrences(result, options.Version));
    }

    private static int ReadOccurrences<T>(
        ParseResult result,
        Option<T> option)
    {
        return result.GetResult(option) is OptionResult optionResult
            ? optionResult.IdentifierTokenCount
            : 0;
    }
}
