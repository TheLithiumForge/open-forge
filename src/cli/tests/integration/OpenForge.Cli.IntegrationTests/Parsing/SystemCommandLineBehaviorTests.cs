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
    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Pinned parser reports scalar repetition and Boolean occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public void PinnedParserReportsScalarRepetitionAndCountsBooleanOccurrences()
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var scalar = tree.Parse(["--detail=minimal", "--detail=standard"]);
        var booleans = tree.Parse(["--help", "--help"]);

        Assert.NotEmpty(scalar.Result.Errors);
        var scalarResult = Assert.IsType<OptionResult>(scalar.Result.GetResult(tree.Options.Detail));
        Assert.Equal(2, scalarResult.IdentifierTokenCount);
        Assert.Equal(2, scalarResult.Tokens.Count(token => token.Type == TokenType.Argument));
        AssertFacts(
            CliOptionResultFactsReader.Read(scalar.Result, tree.Options.Detail),
            true,
            2,
            2);
        Assert.Empty(booleans.Result.Errors);
        var booleanResult = Assert.IsType<OptionResult>(booleans.Result.GetResult(tree.Options.Help));
        Assert.Equal(2, booleanResult.IdentifierTokenCount);
        Assert.Empty(booleanResult.Tokens);
        AssertFacts(
            CliOptionResultFactsReader.Read(booleans.Result, tree.Options.Help),
            true,
            2,
            0);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Pinned parser accepts native global value forms and owns occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace", "example", "example", true)]
    [InlineData("--workspace=example", null, "example", true)]
    [InlineData("--workspace:example", null, "example", true)]
    [InlineData("--detail", "minimal", "minimal", false)]
    [InlineData("--detail=minimal", null, "minimal", false)]
    [InlineData("--detail:minimal", null, "minimal", false)]
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
            parse.Result.GetResult(workspace ? tree.Options.Workspace : tree.Options.Detail));
        Assert.Equal(1, optionResult.IdentifierTokenCount);
        Assert.Equal(1, optionResult.Tokens.Count(token => token.Type == TokenType.Argument));
        AssertFacts(
            workspace
                ? CliOptionResultFactsReader.Read(parse.Result, tree.Options.Workspace)
                : CliOptionResultFactsReader.Read(parse.Result, tree.Options.Detail),
            true,
            1,
            1);

        var input = Assert.IsType<CliGlobalInput>(CliGlobalInputReader.Read(parse));
        if (workspace)
        {
            Assert.Equal(expectedValue, parse.Result.GetValue(tree.Options.Workspace));
            Assert.Equal(expectedValue, input.WorkspaceValue);
            Assert.Equal(1, input.WorkspaceOccurrences);
            Assert.Equal(0, input.DetailOccurrences);
        }
        else
        {
            Assert.Equal(CliDetail.Minimal, parse.Result.GetValue(tree.Options.Detail));
            Assert.Equal(CliDetail.Minimal, input.Detail);
            Assert.Equal(0, input.WorkspaceOccurrences);
            Assert.Equal(1, input.DetailOccurrences);
        }
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Pinned parser attached-empty global values remain invalid")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace=")]
    [InlineData("--workspace:")]
    [InlineData("--detail=")]
    [InlineData("--detail:")]
    public void ParserAttachedEmptyGlobalValuesRemainInvalid(string option)
    {
        var tree = CliCommandTree.Create(CliHelpContent.Empty, [], []);
        var resolution = CliTerminalValidator.Validate(tree.Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Pinned parser preserves following globals after attached-empty scalar values")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--workspace=", "workspace", "--format=json", "format")]
    [InlineData("--workspace=", "workspace", "--detail-filter=warning", "filter")]
    [InlineData("--workspace=", "workspace", "--help", "help")]
    [InlineData("--workspace=", "workspace", "--version", "version")]
    [InlineData("--workspace:", "workspace", "--format=json", "format")]
    [InlineData("--workspace:", "workspace", "--detail-filter=warning", "filter")]
    [InlineData("--workspace:", "workspace", "--help", "help")]
    [InlineData("--workspace:", "workspace", "--version", "version")]
    [InlineData("--detail=", "detail", "--format=json", "format")]
    [InlineData("--detail=", "detail", "--detail-filter=warning", "filter")]
    [InlineData("--detail=", "detail", "--help", "help")]
    [InlineData("--detail=", "detail", "--version", "version")]
    [InlineData("--detail:", "detail", "--format=json", "format")]
    [InlineData("--detail:", "detail", "--detail-filter=warning", "filter")]
    [InlineData("--detail:", "detail", "--help", "help")]
    [InlineData("--detail:", "detail", "--version", "version")]
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
            followingOptionName is "format" or "filter" ? 1 : 0);
        AssertFollowingGlobal(parse.Result, tree.Options, followingOptionName);

        var resolution = CliTerminalValidator.Validate(parse);
        Assert.Equal(CliInvalidInputSource.Semantic, resolution.InvalidInput?.Source);
        Assert.Null(resolution.Input);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Pinned parser preserves following globals after attached-empty route-list depth")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--depth=", "--format=json", "format")]
    [InlineData("--depth=", "--detail-filter=warning", "filter")]
    [InlineData("--depth=", "--help", "help")]
    [InlineData("--depth=", "--version", "version")]
    [InlineData("--depth:", "--format=json", "format")]
    [InlineData("--depth:", "--detail-filter=warning", "filter")]
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
            followingOptionName is "format" or "filter" ? 1 : 0);
        AssertFollowingGlobal(parse.Result, tree.Options, followingOptionName);
    }

    private static CliOptionResultFacts ReadGlobalFacts(
        ParseResult result,
        CliGlobalOptionSymbols options,
        string optionName)
    {
        return optionName switch
        {
            "workspace" => CliOptionResultFactsReader.Read(result, options.Workspace),
            "detail" => CliOptionResultFactsReader.Read(result, options.Detail),
            "format" => CliOptionResultFactsReader.Read(result, options.Format),
            "filter" => CliOptionResultFactsReader.Read(result, options.DetailFilter),
            "help" => CliOptionResultFactsReader.Read(result, options.Help),
            "version" => CliOptionResultFactsReader.Read(result, options.Version),
            _ => throw new ArgumentOutOfRangeException(nameof(optionName), optionName, "Unknown test option."),
        };
    }

    private static void AssertFollowingGlobal(ParseResult result, CliGlobalOptionSymbols options, string optionName)
    {
        switch (optionName)
        {
            case "format":
                Assert.Equal(CliFormat.Json, result.GetValue(options.Format));
                break;
            case "filter":
                Assert.Equal([CliSeverityFilter.Warning], result.GetValue(options.DetailFilter));
                break;
            case "help":
                Assert.True(result.GetValue(options.Help));
                break;
            case "version":
                Assert.True(result.GetValue(options.Version));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(optionName));
        }
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
