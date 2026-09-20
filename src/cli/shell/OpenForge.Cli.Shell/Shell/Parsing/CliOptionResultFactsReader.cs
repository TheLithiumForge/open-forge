using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

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

    /// <summary>
    /// The values of a repeatable option, empty when it was omitted or when the
    /// parser cannot produce a value. A binder reports a malformed option through
    /// its own findings, so reading one never throws.
    /// </summary>
    internal static string[] ReadValues(ParseResult result, Option<string[]> option)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(option);

        try
        {
            return result.GetValue(option) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    /// <summary>
    /// The value of a single-valued option, null on the same terms as
    /// <see cref="ReadValues"/>.
    /// </summary>
    internal static string? ReadValue(ParseResult result, Option<string?> option)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(option);

        try
        {
            return result.GetValue(option);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
