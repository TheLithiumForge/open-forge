using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Parsing;

namespace OpenForge.Cli.UnitTests.Parsing;

public sealed class CliParserTests
{
    [Fact(DisplayName = "CLI root exposes only canonical global options without aliases and uses typed defaults"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RootExposesCanonicalOptionsAndDefaults()
    {
        var tree = CliRootTree.Create();

        Assert.Equal(
            ["--workspace", "--json", "--view", "--verbose", "--help", "--version"],
            tree.Root.Options.Select(option => option.Name));
        Assert.All(tree.Root.Options, option => Assert.Empty(option.Aliases));
        Assert.Equal(ArgumentArity.ExactlyOne, tree.Options.Workspace.Arity);
        Assert.Equal(ArgumentArity.ExactlyOne, tree.Options.View.Arity);

        var resolution = CliGlobalInputResolver.Resolve(tree.Parse([]));

        var input = Assert.IsType<CliGlobalInput>(resolution.Input);
        Assert.Null(input.WorkspaceValue);
        Assert.Equal(0, input.WorkspaceOccurrences);
        Assert.Equal(CliOutputFormat.Human, input.OutputFormat);
        Assert.Equal(CliView.Expanded, input.View);
        Assert.Equal(CliVerbosity.Normal, input.Verbosity);
        Assert.Equal(CliTerminalMode.None, resolution.TerminalMode);
    }

    [Theory(DisplayName = "CLI parser rejects repeated workspace values in equals and separate forms"),
     InlineData("--workspace=same", null, "--workspace=same", null),
     InlineData("--workspace=first", null, "--workspace=second", null),
     InlineData("--workspace", "same", "--workspace", "same"),
     InlineData("--workspace", "first", "--workspace", "second"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserRejectsRepeatedWorkspaceValues(
        string firstOption,
        string? firstValue,
        string secondOption,
        string? secondValue)
    {
        var parse = CliRootTree.Create().Parse(BuildArguments(firstOption, firstValue, secondOption, secondValue));

        Assert.NotEmpty(parse.Result.Errors);
        var optionResult = Assert.IsType<OptionResult>(parse.Result.GetResult(parse.Options.Workspace));
        Assert.Equal(2, optionResult.IdentifierTokenCount);
        Assert.Equal(2, optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
        Assert.Equal(
            CliInvalidInputSource.Parser,
            Assert.IsType<CliInvalidInput>(CliGlobalInputResolver.Resolve(parse).InvalidInput).Source);
    }

    [Theory(DisplayName = "CLI parser rejects repeated view values in equals and separate forms"),
     InlineData("--view=compact", null, "--view=compact", null),
     InlineData("--view=compact", null, "--view=expanded", null),
     InlineData("--view", "compact", "--view", "compact"),
     InlineData("--view", "compact", "--view", "expanded"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserRejectsRepeatedViewValues(
        string firstOption,
        string? firstValue,
        string secondOption,
        string? secondValue)
    {
        var parse = CliRootTree.Create().Parse(BuildArguments(firstOption, firstValue, secondOption, secondValue));

        Assert.NotEmpty(parse.Result.Errors);
        var optionResult = Assert.IsType<OptionResult>(parse.Result.GetResult(parse.Options.View));
        Assert.Equal(2, optionResult.IdentifierTokenCount);
        Assert.Equal(2, optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
        Assert.Equal(
            CliInvalidInputSource.Parser,
            Assert.IsType<CliInvalidInput>(CliGlobalInputResolver.Resolve(parse).InvalidInput).Source);
    }

    [Fact(DisplayName = "CLI parser aggregates repeated Boolean occurrences while values remain idempotent"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserAggregatesIdempotentBooleanOccurrences()
    {
        var parse = CliRootTree.Create().Parse(["--json", "--json", "--verbose", "--verbose"]);

        Assert.Empty(parse.Result.Errors);
        var input = CliGlobalInput.Read(parse);
        Assert.Equal(CliOutputFormat.Json, input.OutputFormat);
        Assert.Equal(2, input.JsonOccurrences);
        Assert.Equal(CliVerbosity.Verbose, input.Verbosity);
        Assert.Equal(2, input.VerboseOccurrences);
        Assert.Equal(2, parse.Result.GetResult(parse.Options.Json)?.IdentifierTokenCount);
        Assert.Equal(2, parse.Result.GetResult(parse.Options.Verbose)?.IdentifierTokenCount);
    }

    [Fact(DisplayName = "CLI parser tokens normalize equals and whitespace while original arguments retain delimiter shape"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserTokensDoNotRetainDelimiterProvenance()
    {
        var tree = CliRootTree.Create();
        var workspaceEquals = tree.Parse(["--workspace=example"]);
        var workspaceSeparate = tree.Parse(["--workspace", "example"]);
        var viewEquals = CliRootTree.Create().Parse(["--view=compact"]);
        var viewSeparate = CliRootTree.Create().Parse(["--view", "compact"]);

        Assert.Equal(NormalizedTokens(workspaceEquals), NormalizedTokens(workspaceSeparate));
        Assert.Equal(NormalizedTokens(viewEquals), NormalizedTokens(viewSeparate));
        Assert.Equal(["--workspace=example"], workspaceEquals.OriginalArguments);
        Assert.Equal(["--workspace", "example"], workspaceSeparate.OriginalArguments);
    }

    [Theory(DisplayName = "CLI delimiter guard enforces exact workspace and view attachment shapes"),
     InlineData("--workspace", false),
     InlineData("--workspace=path", true),
     InlineData("--workspace:path", true),
     InlineData("--view=compact", false),
     InlineData("--view", true),
     InlineData("--view:compact", true),
     Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void DelimiterGuardEnforcesExactShapes(string argument, bool expectsViolation)
    {
        var tree = CliRootTree.Create();

        var violation = CliDelimiterGuard.Validate([argument], tree.DelimiterPolicies);

        Assert.Equal(expectsViolation, violation is not null);
    }

    [Fact(DisplayName = "CLI delimiter guard ignores unknown option prefixes"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void DelimiterGuardDoesNotPrefixMatch()
    {
        var policies = CliRootTree.Create().DelimiterPolicies;

        Assert.Null(CliDelimiterGuard.Validate(["--view-extra=compact"], policies));
        Assert.Null(CliDelimiterGuard.Validate(["--workspace-extra:path"], policies));
    }

    [Fact(DisplayName = "CLI delimiter guard does not inspect attached values or following tokens"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void DelimiterGuardDoesNotInspectValuesOrFollowingTokens()
    {
        var policies = CliRootTree.Create().DelimiterPolicies;

        Assert.Null(CliDelimiterGuard.Validate(["--view=not-a-view"], policies));
        Assert.Null(CliDelimiterGuard.Validate(["--workspace", "--view=compact"], policies));
        Assert.Null(CliDelimiterGuard.Validate(["--workspace", "anything-at-all"], policies));
        Assert.NotNull(CliDelimiterGuard.Validate(["--workspace="], policies));
    }

    [Fact(DisplayName = "CLI parser diagnostics take precedence over delimiter violations"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserDiagnosticsTakePrecedence()
    {
        var parse = CliRootTree.Create().Parse(["--view", "compact", "--unknown"]);

        var resolution = CliGlobalInputResolver.Resolve(parse);

        var invalid = Assert.IsType<CliInvalidInput>(resolution.InvalidInput);
        Assert.Equal(CliInvalidInputSource.Parser, invalid.Source);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Contains("unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "CLI parser-valid input can produce one typed delimiter violation"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserValidInputProducesTypedDelimiterViolation()
    {
        var parse = CliRootTree.Create().Parse(["--view", "compact"]);
        Assert.Empty(parse.Result.Errors);

        var resolution = CliGlobalInputResolver.Resolve(parse);

        var invalid = Assert.IsType<CliInvalidInput>(resolution.InvalidInput);
        Assert.Equal(CliInvalidInputSource.Delimiter, invalid.Source);
        Assert.Equal(CliDefinitions.DelimiterError, invalid.Error);
        Assert.Single(invalid.Diagnostics);
    }

    [Fact(DisplayName = "CLI delimiter guard fails closed for an unknown delimiter style"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void DelimiterGuardRejectsUnknownStyle()
    {
        var option = CliDefinitions.Options.Workspace with
        {
            DelimiterStyle = (CliDelimiterStyle)int.MaxValue,
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliDelimiterGuard.Validate(
                [option.Name],
                [new CliDelimiterPolicy(option)]));
    }

    [Fact(DisplayName = "CLI delimiter guard explicitly accepts an option with no delimiter policy"), Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void DelimiterGuardAcceptsNoDelimiterStyle()
    {
        var option = CliDefinitions.Options.Json;

        var violation = CliDelimiterGuard.Validate(
            [option.Name],
            [new CliDelimiterPolicy(option)]);

        Assert.Null(violation);
    }

    private static string[] BuildArguments(
        string firstOption,
        string? firstValue,
        string secondOption,
        string? secondValue)
    {
        var arguments = new List<string> { firstOption };
        if (firstValue is not null)
        {
            arguments.Add(firstValue);
        }

        arguments.Add(secondOption);
        if (secondValue is not null)
        {
            arguments.Add(secondValue);
        }

        return arguments.ToArray();
    }

    private static (TokenType Type, string Value)[] NormalizedTokens(CliRootParse parse)
    {
        return parse.Result.Tokens.Select(token => (token.Type, token.Value)).ToArray();
    }
}
