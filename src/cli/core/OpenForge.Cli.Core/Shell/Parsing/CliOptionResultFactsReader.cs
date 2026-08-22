using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliOptionResultFactsReader
{
    private static readonly CliOptionResultFacts OmittedFacts = new(false, 0, 0);

    internal static CliOptionResultFacts Read<T>(ParseResult result, Option<T> option)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(option);

        if (result.GetResult(option) is not OptionResult optionResult
            || optionResult.Implicit)
        {
            return OmittedFacts;
        }

        return new CliOptionResultFacts(
            true,
            optionResult.IdentifierTokenCount,
            optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
    }
}
