using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.References.Shared.Binding;

internal static class ReferencesBindingInputReader
{
    internal static ReferencesBindingInput Read(
        ParseResult result,
        ReferencesSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);

        var source = ReadSource(result, symbols.Source);
        var directionFacts = CliOptionResultFactsReader.Read(result, symbols.Direction);
        var directionSpelling = ReadScalar(result, symbols.Direction, directionFacts);
        var direction = ReadDirection(directionFacts, directionSpelling, out var directionCause);
        var includeFacts = CliOptionResultFactsReader.Read(result, symbols.Include);
        var excludeFacts = CliOptionResultFactsReader.Read(result, symbols.Exclude);
        var selectors = ReferencesSelectorOccurrenceReader.Read(
            result,
            ReadValues(result, symbols.Include),
            ReadValues(result, symbols.Exclude),
            includeFacts,
            excludeFacts,
            out var filterCause);

        return new ReferencesBindingInput(
            source,
            directionSpelling,
            direction,
            selectors,
            source is not null,
            directionFacts.IsExplicit,
            includeFacts.IsExplicit || excludeFacts.IsExplicit,
            directionCause,
            filterCause);
    }

    private static string? ReadSource(ParseResult result, Argument<string?> source)
    {
        try
        {
            return result.GetValue(source);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string? ReadScalar(
        ParseResult result,
        Option<string[]> option,
        CliOptionResultFacts facts)
    {
        if (!facts.IsExplicit || facts.IsExplicitWithoutValue)
        {
            return null;
        }

        try
        {
            return result.GetValue(option)?.LastOrDefault()
                ?? ReadLastTokenValue(result, option);
        }
        catch (InvalidOperationException)
        {
            return ReadLastTokenValue(result, option);
        }
    }

    private static string? ReadLastTokenValue<T>(ParseResult result, Option<T> option)
    {
        if (result.GetResult(option) is not OptionResult optionResult)
        {
            return null;
        }

        return optionResult.Tokens
            .LastOrDefault(token => token.Type == TokenType.Argument)
            ?.Value;
    }

    private static string[] ReadValues(ParseResult result, Option<string[]> option)
    {
        try
        {
            return result.GetValue(option) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }

    private static ReferencesDirection? ReadDirection(
        CliOptionResultFacts facts,
        string? spelling,
        out string? cause)
    {
        cause = null;
        if (!facts.IsExplicit)
        {
            return ReferencesDirection.Both;
        }

        if (facts.IdentifierCount != 1 || facts.ValueCount != 1)
        {
            cause = "Direction must occur exactly once with one scalar value.";
            return null;
        }

        return spelling switch
        {
            ReferencesDefinitions.In => ReferencesDirection.In,
            ReferencesDefinitions.Out => ReferencesDirection.Out,
            ReferencesDefinitions.Both => ReferencesDirection.Both,
            _ => InvalidDirection(spelling, out cause),
        };
    }

    private static ReferencesDirection? InvalidDirection(
        string? spelling,
        out string? cause)
    {
        cause = $"Direction '{spelling ?? ""}' is not one of in, out, or both.";
        return null;
    }
}
