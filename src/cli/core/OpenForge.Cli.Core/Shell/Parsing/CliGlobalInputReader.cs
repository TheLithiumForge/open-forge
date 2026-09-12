using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliGlobalInputReader
{
    internal static CliGlobalInput Read(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var result = parse.Result;
        var options = parse.Options;
        var workspaceFacts = CliOptionResultFactsReader.Read(result, options.Workspace);
        var viewFacts = CliOptionResultFactsReader.Read(result, options.View);
        var workspaceValue = result.GetValue(options.Workspace);
        EnsureWorkspaceValue(options.Workspace, workspaceFacts, workspaceValue);
        EnsureValue(options.View, viewFacts);
        var json = result.GetValue(options.Json);
        var verbose = result.GetValue(options.Verbose);
        return new CliGlobalInput(
            workspaceValue,
            workspaceFacts.IdentifierCount,
            json ? CliOutputFormat.Json : CliOutputFormat.Human,
            ReadOccurrences(result, options.Json),
            result.GetValue(options.View),
            viewFacts.IdentifierCount,
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
        return CliOptionResultFactsReader.Read(result, option).IdentifierCount;
    }

    private static void EnsureValue<T>(
        Option<T> option,
        CliOptionResultFacts facts)
    {
        if (facts.IsExplicitWithoutValue)
        {
            throw new ArgumentException($"{option.Name} requires a value.");
        }
    }

    private static void EnsureWorkspaceValue(
        Option<string?> option,
        CliOptionResultFacts facts,
        string? value)
    {
        EnsureValue(option, facts);
        if (facts.IsExplicit && string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{option.Name} requires a non-empty value.");
        }
    }
}
