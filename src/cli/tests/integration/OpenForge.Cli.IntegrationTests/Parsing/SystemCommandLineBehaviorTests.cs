using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.IntegrationTests.Parsing;

public sealed class SystemCommandLineBehaviorTests
{
    [Fact(DisplayName = "Binding parse retains raw attached-empty spelling that the pinned parser erases")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void BindingParseRetainsRawAttachedEmptySpelling()
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteUpdateBinding.CreateSymbols(route);
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(route, CliHelpContent.Empty)],
            []);
        string[] attachedArguments =
        [
            "route", "update", "memory/project-alpha/overview", "--responsibility=",
        ];
        string[] bareArguments =
        [
            "route", "update", "memory/project-alpha/overview", "--responsibility",
        ];
        var attached = tree.Parse(attachedArguments);
        var bare = tree.Parse(bareArguments);
        var attachedResult = Assert.IsType<OptionResult>(
            attached.Result.GetResult(symbols.Responsibility));
        var bareResult = Assert.IsType<OptionResult>(
            bare.Result.GetResult(symbols.Responsibility));

        Assert.Equal(attachedResult.IdentifierToken?.Value, bareResult.IdentifierToken?.Value);
        Assert.Empty(attachedResult.Tokens);
        Assert.Empty(bareResult.Tokens);
        Assert.Equal(
            attached.Result.Tokens.Select(token => (token.Type, token.Value)),
            bare.Result.Tokens.Select(token => (token.Type, token.Value)));

        var attachedCarrier = new CliBindingParse(
            attached.Result,
            attached.OriginalArguments);
        var bareCarrier = new CliBindingParse(
            bare.Result,
            bare.OriginalArguments);
        Assert.Equal(attachedArguments, attachedCarrier.OriginalArguments);
        Assert.Equal(bareArguments, bareCarrier.OriginalArguments);
        Assert.NotEqual(
            attachedCarrier.OriginalArguments[^1],
            bareCarrier.OriginalArguments[^1]);
    }

    [Fact(DisplayName = "Pinned parser reports scalar repetition and Boolean occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void PinnedParserReportsScalarRepetitionAndCountsBooleanOccurrences()
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var scalar = tree.Parse(["--view=compact", "--view=expanded"]);
        var booleans = tree.Parse(["--json", "--json"]);

        Assert.NotEmpty(scalar.Result.Errors);
        var scalarResult = Assert.IsType<OptionResult>(scalar.Result.GetResult(tree.Options.View));
        Assert.Equal(2, scalarResult.IdentifierTokenCount);
        Assert.Equal(2, scalarResult.Tokens.Count(token => token.Type == TokenType.Argument));
        AssertFacts(
            CliOptionResultFactsReader.Read(scalar.Result, tree.Options.View),
            true,
            2,
            2);
        Assert.Empty(booleans.Result.Errors);
        var booleanResult = Assert.IsType<OptionResult>(booleans.Result.GetResult(tree.Options.Json));
        Assert.Equal(2, booleanResult.IdentifierTokenCount);
        Assert.Empty(booleanResult.Tokens);
        AssertFacts(
            CliOptionResultFactsReader.Read(booleans.Result, tree.Options.Json),
            true,
            2,
            0);
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
        var parse = tree.Parse(arguments);

        Assert.Empty(parse.Result.Errors);
        Assert.Equal(arguments, parse.OriginalArguments);
        var optionResult = Assert.IsType<OptionResult>(
            parse.Result.GetResult(workspace ? tree.Options.Workspace : tree.Options.View));
        Assert.Equal(1, optionResult.IdentifierTokenCount);
        Assert.Equal(1, optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
        AssertFacts(
            workspace
                ? CliOptionResultFactsReader.Read(parse.Result, tree.Options.Workspace)
                : CliOptionResultFactsReader.Read(parse.Result, tree.Options.View),
            true,
            1,
            1);

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
        var resolution = CliTerminalValidator.Validate(tree.Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
    }

    [Theory(DisplayName = "Pinned parser preserves following globals after attached-empty scalar values")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace=", "workspace", "--json", "json")]
    [InlineData("--workspace=", "workspace", "--verbose", "verbose")]
    [InlineData("--workspace=", "workspace", "--help", "help")]
    [InlineData("--workspace=", "workspace", "--version", "version")]
    [InlineData("--workspace:", "workspace", "--json", "json")]
    [InlineData("--workspace:", "workspace", "--verbose", "verbose")]
    [InlineData("--workspace:", "workspace", "--help", "help")]
    [InlineData("--workspace:", "workspace", "--version", "version")]
    [InlineData("--view=", "view", "--json", "json")]
    [InlineData("--view=", "view", "--verbose", "verbose")]
    [InlineData("--view=", "view", "--help", "help")]
    [InlineData("--view=", "view", "--version", "version")]
    [InlineData("--view:", "view", "--json", "json")]
    [InlineData("--view:", "view", "--verbose", "verbose")]
    [InlineData("--view:", "view", "--help", "help")]
    [InlineData("--view:", "view", "--version", "version")]
    public void ParserPreservesFollowingGlobalsAfterAttachedEmptyScalarValues(
        string emptyOption,
        string emptyOptionName,
        string followingOption,
        string followingOptionName)
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var parse = tree.Parse([emptyOption, followingOption]);

        Assert.Empty(parse.Result.Errors);
        AssertFacts(
            ReadGlobalFacts(parse.Result, tree.Options, emptyOptionName),
            true,
            1,
            0);
        AssertFacts(
            ReadGlobalFacts(parse.Result, tree.Options, followingOptionName),
            true,
            1,
            0);
        Assert.True(parse.Result.GetValue(ReadBooleanOption(tree.Options, followingOptionName)));

        var followingResult = Assert.IsType<OptionResult>(
            parse.Result.GetResult(ReadBooleanOption(tree.Options, followingOptionName)));
        Assert.Equal(1, followingResult.IdentifierTokenCount);
        Assert.Equal(0, followingResult.Tokens.Count(token => token.Type == TokenType.Argument));

        var resolution = CliTerminalValidator.Validate(parse);
        Assert.Equal(CliInvalidInputSource.Semantic, resolution.InvalidInput?.Source);
        Assert.Null(resolution.Input);
    }

    [Theory(DisplayName = "Pinned parser normalizes native forms for policy-free typed depth")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--depth", "7")]
    [InlineData("--depth=7", null)]
    [InlineData("--depth:7", null)]
    public void ParserNormalizesNativeFormsForPolicyFreeTypedDepth(
        string option,
        string? separateValue)
    {
        var depth = new Option<int>("--depth")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        var root = new RootCommand();
        root.Options.Add(depth);
        root.SetAction(static _ => 0);
        string[] arguments = separateValue is null
            ? [option]
            : [option, separateValue];

        var parse = root.Parse(arguments);

        Assert.Empty(parse.Errors);
        Assert.Equal(7, parse.GetValue(depth));
        AssertFacts(
            CliOptionResultFactsReader.Read(parse, depth),
            true,
            1,
            1);
    }

    [Fact(DisplayName = "Pinned parser aggregates repeated multi-value options from OptionResult facts")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void PinnedParserAggregatesRepeatedMultiValueOptions()
    {
        var items = new Option<string[]>("--item")
        {
            Arity = ArgumentArity.ZeroOrMore,
        };
        var root = new RootCommand();
        root.Options.Add(items);
        root.SetAction(static _ => 0);

        var parse = root.Parse(["--item", "one", "--item", "two", "--item", "three"]);

        Assert.Empty(parse.Errors);
        var values = Assert.IsType<string[]>(parse.GetValue(items));
        Assert.Equal(["one", "two", "three"], values);
        AssertFacts(
            CliOptionResultFactsReader.Read(parse, items),
            true,
            3,
            3);
    }

    [Theory(DisplayName = "Pinned parser preserves following globals after attached-empty route-list depth")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--depth=", "--json", "json")]
    [InlineData("--depth=", "--verbose", "verbose")]
    [InlineData("--depth=", "--help", "help")]
    [InlineData("--depth=", "--version", "version")]
    [InlineData("--depth:", "--json", "json")]
    [InlineData("--depth:", "--verbose", "verbose")]
    [InlineData("--depth:", "--help", "help")]
    [InlineData("--depth:", "--version", "version")]
    public void ParserPreservesFollowingGlobalsAfterAttachedEmptyRouteListDepth(
        string emptyDepth,
        string followingOption,
        string followingOptionName)
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RouteGroup, CliHelpContent.Empty)],
            []);

        var parse = tree.Parse(["route", "list", emptyDepth, followingOption]);

        Assert.Empty(parse.Result.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(parse.Result, symbols.Depth),
            true,
            1,
            0);
        AssertFacts(
            ReadGlobalFacts(parse.Result, tree.Options, followingOptionName),
            true,
            1,
            0);
        var following = ReadBooleanOption(tree.Options, followingOptionName);
        Assert.True(parse.Result.GetValue(following));

        var followingResult = Assert.IsType<OptionResult>(parse.Result.GetResult(following));
        Assert.Equal(1, followingResult.IdentifierTokenCount);
        Assert.Equal(0, followingResult.Tokens.Count(token => token.Type == TokenType.Argument));
    }

    private static CliOptionResultFacts ReadGlobalFacts(
        ParseResult result,
        CliGlobalOptionSymbols options,
        string optionName)
    {
        return optionName switch
        {
            "workspace" => CliOptionResultFactsReader.Read(result, options.Workspace),
            "view" => CliOptionResultFactsReader.Read(result, options.View),
            "json" => CliOptionResultFactsReader.Read(result, options.Json),
            "verbose" => CliOptionResultFactsReader.Read(result, options.Verbose),
            "help" => CliOptionResultFactsReader.Read(result, options.Help),
            "version" => CliOptionResultFactsReader.Read(result, options.Version),
            _ => throw new ArgumentOutOfRangeException(nameof(optionName), optionName, "Unknown test option."),
        };
    }

    private static Option<bool> ReadBooleanOption(
        CliGlobalOptionSymbols options,
        string optionName)
    {
        return optionName switch
        {
            "json" => options.Json,
            "verbose" => options.Verbose,
            "help" => options.Help,
            "version" => options.Version,
            _ => throw new ArgumentOutOfRangeException(nameof(optionName), optionName, "Unknown Boolean test option."),
        };
    }

    private static void AssertFacts(
        CliOptionResultFacts facts,
        bool isExplicit,
        int identifierCount,
        int valueCount)
    {
        Assert.Equal(isExplicit, facts.IsExplicit);
        Assert.Equal(identifierCount, facts.IdentifierCount);
        Assert.Equal(valueCount, facts.ValueCount);
    }
}
