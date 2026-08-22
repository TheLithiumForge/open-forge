using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.IntegrationTests.Parsing;

public sealed class SystemCommandLineBehaviorTests
{
    [Fact(DisplayName = "Pinned parser reports scalar repetition and Boolean occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void PinnedParserReportsScalarRepetitionAndCountsBooleanOccurrences()
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var parser = new CliParser(tree);
        var scalar = parser.Parse(["--view=compact", "--view=expanded"]);
        var booleans = parser.Parse(["--json", "--json"]);

        Assert.NotEmpty(scalar.Result.Errors);
        var scalarResult = Assert.IsType<OptionResult>(scalar.Result.GetResult(tree.Options.View));
        Assert.Equal(2, scalarResult.IdentifierTokenCount);
        Assert.Equal(2, scalarResult.Tokens.Count(token => token.Type == TokenType.Argument));
        Assert.Empty(booleans.Result.Errors);
        var booleanResult = Assert.IsType<OptionResult>(booleans.Result.GetResult(tree.Options.Json));
        Assert.Equal(2, booleanResult.IdentifierTokenCount);
        Assert.Empty(booleanResult.Tokens);
    }

    [Theory(DisplayName = "Pinned parser accepts native global value forms and owns occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace", "example", "example", true)]
    [InlineData("--workspace=example", null, "example", true)]
    [InlineData("--workspace:example", null, "example", true)]
    [InlineData("--view", "compact", "compact", false)]
    [InlineData("--view=compact", null, "compact", false)]
    [InlineData("--view:compact", null, "compact", false)]
    public void ParserAcceptsNativeGlobalValueFormsAndOwnsOccurrences(
        string option,
        string? separateValue,
        string expectedValue,
        bool workspace)
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        string[] arguments = separateValue is null
            ? [option]
            : [option, separateValue];
        var parse = new CliParser(tree).Parse(arguments);

        Assert.Empty(parse.Result.Errors);
        Assert.Equal(arguments, parse.OriginalArguments);
        var optionResult = Assert.IsType<OptionResult>(
            parse.Result.GetResult(workspace ? tree.Options.Workspace : tree.Options.View));
        Assert.Equal(1, optionResult.IdentifierTokenCount);
        Assert.Single(optionResult.Tokens, token => token.Type == TokenType.Argument);

        var input = Assert.IsType<CliGlobalInput>(CliGlobalInputReader.Read(parse));
        if (workspace)
        {
            Assert.Equal(expectedValue, parse.Result.GetValue(tree.Options.Workspace));
            Assert.Equal(expectedValue, input.WorkspaceValue);
            Assert.Equal(1, input.WorkspaceOccurrences);
            Assert.Equal(0, input.ViewOccurrences);
        }
        else
        {
            Assert.Equal(CliView.Compact, parse.Result.GetValue(tree.Options.View));
            Assert.Equal(CliView.Compact, input.View);
            Assert.Equal(0, input.WorkspaceOccurrences);
            Assert.Equal(1, input.ViewOccurrences);
        }
    }

    [Theory(DisplayName = "Pinned parser attached-empty global values remain invalid")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace=")]
    [InlineData("--workspace:")]
    [InlineData("--view=")]
    [InlineData("--view:")]
    public void ParserAttachedEmptyGlobalValuesRemainInvalid(string option)
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var resolution = CliTerminalValidator.Validate(new CliParser(tree).Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
    }

    [Fact(DisplayName = "Pinned parser keeps a global option after attached-empty route-list depth")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void ParserKeepsGlobalOptionAfterAttachedEmptyRouteListDepth()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RouteGroup, CliHelpContent.Empty, symbols.DelimiterPolicies)],
            []);

        var parse = new CliParser(tree).Parse(["route", "list", "--depth=", "--json"]);

        Assert.Empty(parse.Result.Errors);
        var depth = Assert.IsType<OptionResult>(parse.Result.GetResult(symbols.Depth));
        Assert.Equal(1, depth.IdentifierTokenCount);
        Assert.Empty(depth.Tokens);
        var json = Assert.IsType<OptionResult>(parse.Result.GetResult(tree.Options.Json));
        Assert.Equal(1, json.IdentifierTokenCount);
        Assert.True(parse.Result.GetValue(tree.Options.Json));
    }
}
