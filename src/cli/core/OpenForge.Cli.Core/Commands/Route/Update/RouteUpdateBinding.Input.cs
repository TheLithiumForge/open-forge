using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal sealed partial class RouteUpdateBinding
{
    private static RouteUpdateBindingInput ReadInput(
        ParseResult parseResult,
        IReadOnlyList<string> originalArguments,
        RouteUpdateSymbols symbols)
    {
        var responsibilityFacts = CliOptionResultFactsReader.Read(
            parseResult,
            symbols.Responsibility);
        return new RouteUpdateBindingInput
        {
            Target = parseResult.GetValue(symbols.SourceReference),
            Description = ReadSingleton(parseResult, symbols.Description),
            Tags = ReadMany(parseResult, symbols.Tag),
            Responsibility = ReadResponsibility(
                parseResult,
                symbols.Responsibility,
                responsibilityFacts,
                originalArguments),
            Template = ReadSingleton(parseResult, symbols.Template),
            DescriptionFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Description),
            TagFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Tag),
            ResponsibilityFacts = responsibilityFacts,
            TemplateFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Template),
            ParserErrors = parseResult.Errors.Select(error => error.Message).ToArray(),
        };
    }

    private static string? ReadResponsibility(
        ParseResult parseResult,
        Option<string?> option,
        CliOptionResultFacts facts,
        IReadOnlyList<string> originalArguments)
    {
        var value = ReadSingleton(parseResult, option);
        if (value is not null)
        {
            return value;
        }

        if (facts is not
            {
                IsExplicit: true,
                IdentifierCount: 1,
                ValueCount: 0,
            })
        {
            return null;
        }

        var equalsEmpty = $"{option.Name}=";
        var colonEmpty = $"{option.Name}:";
        foreach (var argument in originalArguments)
        {
            if (string.Equals(argument, "--", StringComparison.Ordinal))
            {
                return null;
            }

            if (string.Equals(argument, equalsEmpty, StringComparison.Ordinal)
                || string.Equals(argument, colonEmpty, StringComparison.Ordinal))
            {
                return string.Empty;
            }
        }

        return null;
    }

    private static string? ReadSingleton(
        ParseResult parseResult,
        Option<string?> option)
    {
        try
        {
            return parseResult.GetValue(option);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string[] ReadMany(
        ParseResult parseResult,
        Option<string[]> option)
    {
        try
        {
            return parseResult.GetValue(option) ?? [];
        }
        catch (InvalidOperationException)
        {
            return [];
        }
    }
}
