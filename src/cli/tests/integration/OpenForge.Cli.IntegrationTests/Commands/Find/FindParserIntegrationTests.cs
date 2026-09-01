using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindParserIntegrationTests
{
    [Theory(DisplayName = "Pinned Find parser preserves native forms, cross-option occurrence order, and explicit missing values")]
    [InlineData("spaced-equals-colon")]
    [InlineData("missing-include")]
    [InlineData("missing-exclude")]
    [InlineData("missing-tag")]
    [InlineData("missing-heading")]
    [InlineData("missing-require")]
    [InlineData("missing-within")]
    [InlineData("missing-content")]
    [Trait("Feature", "find-query")]
    [Trait("Evidence", "Integration")]
    public void PinnedParserPreservesNativeFormsOccurrencesCrossOptionOrderAndMissingValues(string scenario)
    {
        var symbols = FindSymbols.Create();
        var parse = symbols.FindCommand.Parse(ArgumentsFor(scenario));

        Assert.Empty(parse.Errors);
        if (scenario.StartsWith("missing-", StringComparison.Ordinal))
        {
            AssertExplicitWithoutValue(parse, symbols, scenario["missing-".Length..]);
            return;
        }

        Assert.Equal(
            ["Architecture", "CurrentTruth"],
            Assert.IsType<string[]>(parse.GetValue(symbols.Tag)));
        Assert.Equal(
            ["Instructions", "Axioms"],
            Assert.IsType<string[]>(parse.GetValue(symbols.Heading)));
        AssertFacts(parse, symbols.Tag, 2, 2);
        AssertFacts(parse, symbols.Heading, 2, 2);
        Assert.Equal(
            new[]
            {
                (TokenType.Option, "--tag"),
                (TokenType.Argument, "Architecture"),
                (TokenType.Option, "--heading"),
                (TokenType.Argument, "Instructions"),
                (TokenType.Option, "--tag"),
                (TokenType.Argument, "CurrentTruth"),
                (TokenType.Option, "--heading"),
                (TokenType.Argument, "Axioms"),
            },
            parse.Tokens.Select(token => (token.Type, token.Value)));
    }

    private static string[] ArgumentsFor(string scenario)
    {
        return scenario switch
        {
            "spaced-equals-colon" =>
            ["--tag", "Architecture", "--heading=Instructions", "--tag:CurrentTruth", "--heading", "Axioms"],
            "missing-include" => ["--include"],
            "missing-exclude" => ["--exclude"],
            "missing-tag" => ["--tag"],
            "missing-heading" => ["--heading"],
            "missing-require" => ["--require"],
            "missing-within" => ["--within"],
            "missing-content" => ["--content"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown Find parser case."),
        };
    }

    private static void AssertExplicitWithoutValue(
        ParseResult parse,
        FindSymbols symbols,
        string optionName)
    {
        switch (optionName)
        {
            case "include":
                AssertFacts(parse, symbols.Include, 1, 0);
                break;
            case "exclude":
                AssertFacts(parse, symbols.Exclude, 1, 0);
                break;
            case "tag":
                AssertFacts(parse, symbols.Tag, 1, 0);
                break;
            case "heading":
                AssertFacts(parse, symbols.Heading, 1, 0);
                break;
            case "require":
                AssertFacts(parse, symbols.Require, 1, 0);
                break;
            case "within":
                AssertFacts(parse, symbols.Within, 1, 0);
                break;
            case "content":
                AssertFacts(parse, symbols.Content, 1, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(optionName), optionName, "Unknown Find option.");
        }

        Assert.Equal(
            [(TokenType.Option, $"--{optionName}")],
            parse.Tokens.Select(token => (token.Type, token.Value)));
    }

    private static void AssertFacts<T>(
        ParseResult parse,
        Option<T> option,
        int identifierCount,
        int valueCount)
    {
        var result = Assert.IsType<OptionResult>(parse.GetResult(option));
        Assert.Equal(identifierCount, result.IdentifierTokenCount);
        Assert.Equal(valueCount, result.Tokens.Count(token => token.Type == TokenType.Argument));

        var facts = CliOptionResultFactsReader.Read(parse, option);
        Assert.True(facts.IsExplicit);
        Assert.Equal(identifierCount, facts.IdentifierCount);
        Assert.Equal(valueCount, facts.ValueCount);
        Assert.Equal(valueCount == 0, facts.IsExplicitWithoutValue);
    }
}
