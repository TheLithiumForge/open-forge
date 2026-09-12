using System.CommandLine;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindBindingTests
{
    [Fact(DisplayName = "Find symbols expose the exact detached seven-option grammar")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void SymbolsExposeTheExactDetachedGrammar()
    {
        var symbols = FindSymbols.Create();

        Assert.Equal("find", symbols.FindCommand.Name);
        Assert.Empty(symbols.FindCommand.Aliases);
        Assert.Empty(symbols.FindCommand.Arguments);
        Assert.Empty(symbols.FindCommand.Subcommands);
        Assert.Equal(
            ["--include", "--exclude", "--tag", "--heading", "--require", "--within", "--content"],
            symbols.FindCommand.Options.Select(option => option.Name));

        AssertRepeatable(symbols.Include);
        AssertRepeatable(symbols.Exclude);
        AssertRepeatable(symbols.Tag);
        AssertRepeatable(symbols.Heading);
        AssertSingleton(symbols.Require);
        AssertSingleton(symbols.Within);
        AssertSingleton(symbols.Content);
        Assert.Equal("source-reference", FindDefinitions.Include.ValueName);
        Assert.Equal("source-reference", FindDefinitions.Exclude.ValueName);
        Assert.Equal("tag", FindDefinitions.Tag.ValueName);
        Assert.Equal("heading", FindDefinitions.Heading.ValueName);
        Assert.Equal("all|any", FindDefinitions.Require.ValueName);
        Assert.Equal("part[,part...]", FindDefinitions.Within.ValueName);
        Assert.Equal("part[,part...]", FindDefinitions.Content.ValueName);
    }

    [Theory(DisplayName = "Find binding preserves omission defaults and distinguishes a supplied view")]
    [InlineData("omitted", "expanded")]
    [InlineData("compact", "compact")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void BinderPreservesDefaultsAndSuppliedView(
        string viewInput,
        string expectedEffectiveView)
    {
        var symbols = FindSymbols.Create();
        var parse = ParseFullCommand(
            symbols,
            viewInput == "omitted"
                ? ["find"]
                : ["find", "--view=compact"]);
        Assert.Empty(parse.Result.Errors);

        var workspace = Workspace();
        var invocation = Invocation(
            workspace,
            viewInput == "omitted" ? CliView.Expanded : CliView.Compact);
        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            invocation);

        var request = Assert.IsType<FindRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Empty(request.UniverseFilter.Include);
        Assert.Empty(request.UniverseFilter.Exclude);
        Assert.Empty(request.Query.Predicates);
        Assert.Empty(request.Query.EffectivePredicates);
        Assert.Equal(FindRequirement.All, request.Query.Requirement);
        Assert.Equal(
            ["frontmatter"],
            request.Query.Within.Tag.Select(region => region.CanonicalValue));
        Assert.Equal(
            ["body"],
            request.Query.Within.Heading.Select(region => region.CanonicalValue));
        Assert.Empty(request.Query.Within.Supplied);
        Assert.Empty(request.Presentation.Content.Supplied);
        Assert.Empty(request.Presentation.Content.Effective);
        Assert.Equal(
            viewInput == "omitted" ? (CliView?)null : CliView.Compact,
            request.Presentation.SuppliedView);
        Assert.Equal(
            expectedEffectiveView == "expanded" ? CliView.Expanded : CliView.Compact,
            request.Presentation.EffectiveView);
    }

    [Theory(DisplayName = "Find binding preserves predicate order across spaced, equals, and colon native forms")]
    [InlineData("spaced-equals-colon")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void BinderPreservesCrossOptionPredicateOrderAcrossNativeForms(string nativeFormSet)
    {
        var symbols = FindSymbols.Create();
        var parse = ParseFullCommand(symbols, NativePredicateArguments(nativeFormSet));
        Assert.Empty(parse.Result.Errors);

        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            Invocation(Workspace()));
        var request = Assert.IsType<FindRequest>(bound.Request);

        Assert.Equal(
            [
                (FindPredicateKind.Tag, "Architecture", "Architecture"),
                (FindPredicateKind.Heading, "Instructions", "Instructions"),
                (FindPredicateKind.Tag, "CurrentTruth", "CurrentTruth"),
                (FindPredicateKind.Heading, "Axioms", "Axioms"),
            ],
            request.Query.Predicates.Select(predicate =>
                (predicate.Kind, predicate.SuppliedValue, predicate.ComparisonValue)));
        Assert.Equal(
            request.Query.Predicates,
            request.Query.EffectivePredicates);
    }

    [Theory(DisplayName = "Find binding rejects missing and repeated singleton option values")]
    [InlineData("missing-require")]
    [InlineData("repeated-require")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void BinderRejectsMissingAndRepeatedSingletonValues(string scenario)
    {
        var symbols = FindSymbols.Create();
        var arguments = SingletonArguments(scenario);
        var parse = symbols.FindCommand.Parse(arguments);
        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(Workspace()));

        var result = Assert.IsType<FindResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(FindCoverageState.NotStarted, result.Coverage.State);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(FindFindingCode.InvalidInput, finding.Code);
        Assert.Equal(CliSemanticStatus.Invalid, finding.Status);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal("open-forge find --help", next.Command);
    }

    [Theory(DisplayName = "Find workspace failure preserves recoverable values, local invalid precedence, and unavailable finding")]
    [InlineData("local-invalid")]
    [InlineData("workspace-unavailable")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void WorkspaceFailurePreservesValuesLocalInvalidPrecedenceAndUnavailableFinding(string scenario)
    {
        var symbols = FindSymbols.Create();
        var arguments = WorkspaceFailureArguments(scenario);
        var parse = symbols.FindCommand.Parse(arguments);
        var input = new CliInvalidBindingInput(
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                ["The selected workspace is missing."]),
            GlobalInput(),
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliBindingParse(parse, arguments));

        var result = new FindWorkspaceResultFactory(symbols, new FindResultBuilder()).Create(input);

        Assert.Null(result.Workspace);
        Assert.Empty(result.Matches);
        if (scenario == "local-invalid")
        {
            Assert.Equal(CliSemanticStatus.Invalid, result.Status);
            Assert.Equal(FindCoverageState.NotStarted, result.Coverage.State);
            Assert.Equal(
                ["docs"],
                result.Universe.Include.Select(selector => selector.Value));
            var selector = Assert.Single(result.Universe.Include);
            Assert.Equal(SourceReferenceKind.SourceId, selector.Form);
            Assert.Equal(FindSelectorResolution.Invalid, selector.Resolution);
            Assert.Null(selector.Identity);
            Assert.Equal(
                ["Architecture"],
                result.Query.Predicates.Select(predicate => predicate.SuppliedValue));
            Assert.Equal(
                ["metadata"],
                result.Presentation.Content.Supplied.Select(part => part.CanonicalValue));
            Assert.Equal(FindFindingCode.InvalidInput, Assert.Single(result.Findings).Code);
            return;
        }

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(FindCoverageState.Blocked, result.Coverage.State);
        Assert.Equal(FindFindingCode.WorkspaceUnavailable, Assert.Single(result.Findings).Code);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal("open-forge doctor", next.Command);
        Assert.Equal(
            "Inspect the blocked workspace or source boundary before rerunning Find.",
            next.Reason);
    }

    private static void AssertRepeatable<T>(Option<T[]> option)
    {
        Assert.Equal(ArgumentArity.ZeroOrMore, option.Arity);
        Assert.False(option.AllowMultipleArgumentsPerToken);
        Assert.Equal(typeof(T[]), option.ValueType);
    }

    private static void AssertSingleton<T>(Option<T> option)
    {
        Assert.Equal(ArgumentArity.ZeroOrOne, option.Arity);
        Assert.Equal(typeof(T), option.ValueType);
    }

    private static CliParseOutcome ParseFullCommand(
        FindSymbols symbols,
        string[] arguments)
    {
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.FindCommand, CliHelpContent.Empty, [])],
            []);
        return tree.Parse(arguments);
    }

    private static string[] NativePredicateArguments(string nativeFormSet)
    {
        return nativeFormSet switch
        {
            "spaced-equals-colon" =>
            ["find", "--tag", "Architecture", "--heading=Instructions", "--tag:CurrentTruth", "--heading", "Axioms"],
            _ => throw new ArgumentOutOfRangeException(
                nameof(nativeFormSet),
                nativeFormSet,
                "The Find native-form case is not defined."),
        };
    }

    private static string[] SingletonArguments(string scenario)
    {
        return scenario switch
        {
            "missing-require" => ["--require"],
            "repeated-require" => ["--tag=Architecture", "--require=all", "--require=all"],
            _ => throw new ArgumentOutOfRangeException(
                nameof(scenario),
                scenario,
                "The Find singleton case is not defined."),
        };
    }

    private static string[] WorkspaceFailureArguments(string scenario)
    {
        return scenario switch
        {
            "local-invalid" =>
            [
                "--include=docs",
                "--tag=Architecture",
                "--content=metadata",
                "--require=all",
                "--require=any",
            ],
            "workspace-unavailable" => ["--include=docs"],
            _ => throw new ArgumentOutOfRangeException(
                nameof(scenario),
                scenario,
                "The Find workspace-failure case is not defined."),
        };
    }

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliView view = CliView.Expanded)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliOutputFormat.Json, view, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    private static CliGlobalInput GlobalInput()
    {
        return new CliGlobalInput(
            null,
            0,
            CliOutputFormat.Json,
            0,
            CliView.Expanded,
            0,
            CliVerbosity.Normal,
            0,
            false,
            0,
            false,
            0);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-binding-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
