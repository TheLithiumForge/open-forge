using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class ParserTests
{
    [Fact(DisplayName = "CLI option result facts enforce explicit occurrence and token count invariants")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsEnforceExplicitOccurrenceAndTokenCountInvariants()
    {
        var omitted = new CliOptionResultFacts(false, 0, 0);
        var explicitNoValue = new CliOptionResultFacts(true, 1, 0);
        var explicitValue = new CliOptionResultFacts(true, 1, 1);
        var repeated = new CliOptionResultFacts(true, 2, 3);

        Assert.False(omitted.IsExplicit);
        Assert.Equal(0, omitted.IdentifierCount);
        Assert.Equal(0, omitted.ValueCount);
        Assert.True(explicitNoValue.IsExplicit);
        Assert.Equal(1, explicitNoValue.IdentifierCount);
        Assert.Equal(0, explicitNoValue.ValueCount);
        Assert.True(explicitValue.IsExplicit);
        Assert.Equal(1, explicitValue.IdentifierCount);
        Assert.Equal(1, explicitValue.ValueCount);
        Assert.True(repeated.IsExplicit);
        Assert.Equal(2, repeated.IdentifierCount);
        Assert.Equal(3, repeated.ValueCount);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CliOptionResultFacts(true, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CliOptionResultFacts(true, 1, -1));
        Assert.Throws<ArgumentException>(
            () => new CliOptionResultFacts(false, 1, 0));
        Assert.Throws<ArgumentException>(
            () => new CliOptionResultFacts(false, 0, 1));
        Assert.Throws<ArgumentException>(
            () => new CliOptionResultFacts(true, 0, 0));
    }

    [Fact(DisplayName = "CLI option result facts reader reports an omitted option as implicit")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsReaderReportsOmittedOptionAsImplicit()
    {
        var tree = CreateTree();
        var parse = tree.Parse([]);

        AssertFacts(
            CliOptionResultFactsReader.Read(parse.Result, tree.Options.View),
            false,
            0,
            0);
    }

    [Fact(DisplayName = "CLI option result facts reader reports one explicit value")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsReaderReportsOneExplicitValue()
    {
        var option = new Option<string?>("--value")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        var parse = ParseStandalone(option, ["--value", "one"]);

        Assert.Empty(parse.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(parse, option),
            true,
            1,
            1);
    }

    [Fact(DisplayName = "CLI option result facts reader reports an explicit no-value occurrence")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsReaderReportsExplicitNoValueOccurrence()
    {
        var option = new Option<bool>("--flag")
        {
            Arity = ArgumentArity.Zero,
        };
        var parse = ParseStandalone(option, ["--flag"]);

        Assert.Empty(parse.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(parse, option),
            true,
            1,
            0);
    }

    [Fact(DisplayName = "CLI option result facts reader aggregates repeated scalar occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsReaderAggregatesRepeatedScalarOccurrences()
    {
        var option = new Option<string?>("--value")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        var parse = ParseStandalone(
            option,
            ["--value", "one", "--value", "two"]);

        Assert.NotEmpty(parse.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(parse, option),
            true,
            2,
            2);
    }

    [Fact(DisplayName = "CLI root exposes canonical options and typed defaults")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RootExposesOnlyCanonicalGlobalOptionsAndTypedDefaults()
    {
        var tree = CreateTree();

        Assert.Equal(
            ["--workspace", "--json", "--view", "--verbose", "--help", "--version"],
            tree.Root.Options.Select(option => option.Name));
        Assert.All(tree.Root.Options, option => Assert.Empty(option.Aliases));

        var input = CliGlobalInputReader.Read(tree.Parse([]));
        Assert.Null(input.WorkspaceValue);
        Assert.Equal(0, input.WorkspaceOccurrences);
        Assert.Equal(CliOutputFormat.Human, input.OutputFormat);
        Assert.Equal(0, input.JsonOccurrences);
        Assert.Equal(CliView.Expanded, input.View);
        Assert.Equal(0, input.ViewOccurrences);
        Assert.Equal(CliVerbosity.Normal, input.Verbosity);
        Assert.Equal(0, input.VerboseOccurrences);
        Assert.False(input.Help);
        Assert.Equal(0, input.HelpOccurrences);
        Assert.False(input.Version);
        Assert.Equal(0, input.VersionOccurrences);
    }

    [Fact(DisplayName = "CLI parser rejects scalar repetition and accepts idempotent Booleans")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RepeatedScalarOptionsAreParserErrorsAndBooleansAreIdempotent()
    {
        var tree = CreateTree();
        var scalar = tree.Parse(["--workspace", "one", "--workspace", "two"]);
        var booleans = tree.Parse(["--json", "--json", "--verbose", "--verbose"]);

        Assert.NotEmpty(scalar.Result.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(scalar.Result, tree.Options.Workspace),
            true,
            2,
            2);
        Assert.Equal(CliInvalidInputSource.Parser, CliTerminalValidator.Validate(scalar).InvalidInput?.Source);
        Assert.Empty(booleans.Result.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(booleans.Result, booleans.Options.Json),
            true,
            2,
            0);
        AssertFacts(
            CliOptionResultFactsReader.Read(booleans.Result, booleans.Options.Verbose),
            true,
            2,
            0);
        var input = CliGlobalInputReader.Read(booleans);
        Assert.Equal(2, input.JsonOccurrences);
        Assert.Equal(2, input.VerboseOccurrences);
        Assert.Equal(CliOutputFormat.Json, input.OutputFormat);
        Assert.Equal(CliVerbosity.Verbose, input.Verbosity);
    }

    [Fact(DisplayName = "CLI parser owns repeated terminal flag occurrences")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RepeatedTerminalFlagsUseParserOwnedOccurrences()
    {
        var tree = CreateTree();
        var help = tree.Parse(["--help", "--help"]);
        var version = tree.Parse(["--version", "--version"]);

        Assert.Empty(help.Result.Errors);
        Assert.Empty(version.Result.Errors);
        AssertFacts(
            CliOptionResultFactsReader.Read(help.Result, tree.Options.Help),
            true,
            2,
            0);
        AssertFacts(
            CliOptionResultFactsReader.Read(version.Result, tree.Options.Version),
            true,
            2,
            0);
        Assert.Equal(
            2,
            Assert.IsType<OptionResult>(help.Result.GetResult(tree.Options.Help)).IdentifierTokenCount);
        Assert.Equal(
            2,
            Assert.IsType<OptionResult>(version.Result.GetResult(tree.Options.Version)).IdentifierTokenCount);

        var helpInput = CliGlobalInputReader.Read(help);
        Assert.True(helpInput.Help);
        Assert.Equal(2, helpInput.HelpOccurrences);
        var versionInput = CliGlobalInputReader.Read(version);
        Assert.True(versionInput.Version);
        Assert.Equal(2, versionInput.VersionOccurrences);
    }

    [Theory(DisplayName = "CLI native value forms resolve through typed global input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--workspace", "path", "path", false, true)]
    [InlineData("--workspace=path", null, "path", false, true)]
    [InlineData("--workspace:path", null, "path", false, true)]
    [InlineData("--view", "compact", null, true, false)]
    [InlineData("--view=compact", null, null, true, false)]
    [InlineData("--view:compact", null, null, true, false)]
    public void NativeValueFormsResolveThroughTypedGlobalInput(
        string option,
        string? separateValue,
        string? expectedWorkspace,
        bool compactView,
        bool usesWorkspace)
    {
        var arguments = separateValue is null
            ? new[] { option }
            : new[] { option, separateValue };
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse(arguments));

        Assert.Null(resolution.InvalidInput);
        var input = Assert.IsType<CliGlobalInput>(resolution.Input);
        Assert.Equal(expectedWorkspace, input.WorkspaceValue);
        Assert.Equal(compactView ? CliView.Compact : CliView.Expanded, input.View);

        Assert.Equal(usesWorkspace ? 1 : 0, input.WorkspaceOccurrences);
        Assert.Equal(usesWorkspace ? 0 : 1, input.ViewOccurrences);
    }

    [Theory(DisplayName = "CLI attached-empty value forms resolve as invalid input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--workspace=")]
    [InlineData("--workspace:")]
    [InlineData("--view=")]
    [InlineData("--view:")]
    public void AttachedEmptyValueFormsAreInvalidWithoutGlobalFallback(string option)
    {
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
        Assert.Equal(CliTerminalMode.None, resolution.TerminalMode);
    }

    [Fact(DisplayName = "CLI explicit empty workspace value is semantic invalid input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ExplicitEmptyWorkspaceValueIsSemanticInvalidInput()
    {
        var resolution = CliTerminalValidator.Validate(
            CreateTree().Parse(["--workspace", string.Empty]));

        Assert.Equal(CliInvalidInputSource.Semantic, resolution.InvalidInput?.Source);
        Assert.Null(resolution.Input);
        Assert.Equal(CliTerminalMode.None, resolution.TerminalMode);
    }

    [Theory(DisplayName = "Route List depth keeps its explicit delimiter policy before option termination")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--depth", "1")]
    [InlineData("--depth:1", null)]
    public void RouteListDepthPolicyRejectsNonEqualsFormsBeforeTerminator(
        string option,
        string? separateValue)
    {
        var tree = CreateRouteListPolicyTree(out _);
        string[] arguments = separateValue is null
            ? ["route", "list", option]
            : ["route", "list", option, separateValue];
        var parse = tree.Parse(arguments);

        Assert.Empty(parse.Result.Errors);
        var invalid = CliTerminalValidator.Validate(parse).InvalidInput;
        Assert.NotNull(invalid);
        Assert.Equal(CliInvalidInputSource.Delimiter, invalid.Source);

        var equals = CliTerminalValidator.Validate(
            tree.Parse(["route", "list", "--depth=1"]));
        Assert.Null(equals.InvalidInput);
    }

    [Fact(DisplayName = "Aggregated Route List delimiter policies stop at the option terminator")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void AggregatedDelimiterPoliciesDoNotRejectOptionLikeSiblingSourceAfterTerminator()
    {
        var tree = CreateRouteListPolicyTree(out var inspectSourceReference);
        var parse = tree.Parse(["route", "inspect", "--", "--depth"]);

        Assert.Empty(parse.Result.Errors);
        Assert.Equal("--depth", parse.Result.GetValue(inspectSourceReference));

        var resolution = CliTerminalValidator.Validate(parse);
        Assert.Null(resolution.InvalidInput);
        Assert.NotNull(resolution.Input);
    }

    [Fact(DisplayName = "CLI parser diagnostics precede delimiter diagnostics")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserDiagnosticsTakePrecedenceOverDelimiterViolations()
    {
        var parse = CreateTree().Parse(["--view", "compact", "--unknown"]);

        var invalid = Assert.IsType<CliInvalidInput>(CliTerminalValidator.Validate(parse).InvalidInput);
        Assert.Equal(CliInvalidInputSource.Parser, invalid.Source);
        Assert.Contains(invalid.Diagnostics, diagnostic =>
            diagnostic.Contains("unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "CLI terminal validation precedes terminal short circuiting")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalValidationOccursBeforeShortCircuiting()
    {
        var tree = CreateTree();
        var version = CliTerminalValidator.Validate(tree.Parse(["--json", "--version"]));
        var conflict = CliTerminalValidator.Validate(tree.Parse(["--help", "--version"]));

        Assert.Equal(CliTerminalMode.Version, version.TerminalMode);
        Assert.Equal(CliOutputFormat.Json, version.Input?.OutputFormat);
        Assert.Equal(CliInvalidInputSource.Semantic, conflict.InvalidInput?.Source);
    }

    [Fact(DisplayName = "CLI terminal globals remain no-op at root, group, and custom leaf")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalGlobalsRemainNoOpAtRootGroupAndCustomLeaf()
    {
        var tree = CreateTerminalTree();

        var root = AssertTerminalNoOp(
            tree,
            ["--json", "--help"],
            CliTerminalMode.Help);
        Assert.Equal(CliOutputFormat.Json, root.OutputFormat);

        var group = AssertTerminalNoOp(
            tree,
            ["group", "--verbose", "--version"],
            CliTerminalMode.Version);
        Assert.Equal(CliVerbosity.Verbose, group.Verbosity);

        var leaf = AssertTerminalNoOp(
            tree,
            ["group", "leaf", "--json", "--help"],
            CliTerminalMode.Help);
        Assert.Equal(CliOutputFormat.Json, leaf.OutputFormat);
    }

    [Fact(DisplayName = "CLI terminal modes reject domain operands local options and unmatched input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalModesRejectTypedDomainInputAtRootGroupAndCustomLeaf()
    {
        var tree = CreateTerminalTree();

        AssertTerminalDomainInput(tree, ["--help", "root-input"]);
        AssertTerminalDomainInput(tree, ["group", "--version", "group-input"]);
        AssertTerminalDomainInput(tree, ["group", "leaf", "--help", "source"]);
        AssertTerminalDomainInput(tree, ["group", "leaf", "--version", "--local"]);
        AssertTerminalDomainInput(tree, ["group", "leaf", "--help", "--unmatched"]);
        AssertTerminalDomainInput(
            tree,
            ["group", "leaf", "--help", "--", "--looks-like-option"]);
    }

    [Fact(DisplayName = "CLI typed terminal input validator reports domain input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TypedTerminalInputValidatorReportsDomainInput()
    {
        var tree = CreateTerminalTree();
        var parse = tree.Parse(["group", "leaf", "--help", "source"]) with
        {
            OriginalArguments = Array.AsReadOnly<string>(["group", "leaf", "--help"]),
        };
        var input = CliGlobalInputReader.Read(parse);

        var invalid = Assert.IsType<CliInvalidInput>(
            CliTerminalInputValidator.Validate(parse, input));
        Assert.Equal(CliInvalidInputSource.Semantic, invalid.Source);
        var diagnostic = Assert.Single(invalid.Diagnostics);
        Assert.NotEmpty(diagnostic);
        Assert.InRange(diagnostic.Length, 1, 4096);
    }

    [Fact(DisplayName = "CLI binding selection uses exact command identity")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void BindingSelectionUsesExactCommandIdentity()
    {
        var group = new Command("group");
        var leaf = new Command("leaf");
        var unbound = new Command("unbound");
        group.Add(leaf);
        group.Add(unbound);
        var binding = new StubBinding(leaf);
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(group, CliHelpContent.Empty, [])],
            [binding]);

        Assert.Equal(CliBindingSelectionState.Root, CliBindingSelector.Select(tree.Parse([])).State);
        Assert.Equal(CliBindingSelectionState.Group, CliBindingSelector.Select(tree.Parse(["group"])).State);
        var selected = CliBindingSelector.Select(tree.Parse(["group", "leaf"]));
        Assert.Equal(CliBindingSelectionState.Leaf, selected.State);
        Assert.Same(binding, selected.Binding);
        Assert.Equal(
            CliBindingSelectionState.NoLeaf,
            CliBindingSelector.Select(tree.Parse(["group", "unbound"])).State);
        Assert.Null(tree.FindBinding(new Command("leaf")));
    }

    [Fact(DisplayName = "CLI unknown input remains a parser fact")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void UnknownInputRemainsAParserFact()
    {
        var parse = CreateTree().Parse(["unknown"]);

        Assert.NotEmpty(parse.Result.Errors);
        Assert.Equal(CliInvalidInputSource.Parser, CliTerminalValidator.Validate(parse).InvalidInput?.Source);
    }

    private static CliCommandTree CreateTree()
    {
        return CliCommandTree.Create(CliHelpContent.Empty, [], []);
    }

    private static ParseResult ParseStandalone<T>(Option<T> option, string[] arguments)
    {
        var root = new RootCommand();
        root.Options.Add(option);
        root.SetAction(static _ => 0);
        return root.Parse(arguments);
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

    private static CliGlobalInput AssertTerminalNoOp(
        CliCommandTree tree,
        string[] arguments,
        CliTerminalMode expectedMode)
    {
        var resolution = CliTerminalValidator.Validate(tree.Parse(arguments));

        Assert.Null(resolution.InvalidInput);
        Assert.Equal(expectedMode, resolution.TerminalMode);
        return Assert.IsType<CliGlobalInput>(resolution.Input);
    }

    private static void AssertTerminalDomainInput(
        CliCommandTree tree,
        string[] arguments)
    {
        var resolution = CliTerminalValidator.Validate(tree.Parse(arguments));
        var invalid = Assert.IsType<CliInvalidInput>(resolution.InvalidInput);

        Assert.Equal(CliInvalidInputSource.Semantic, invalid.Source);
        Assert.Single(invalid.Diagnostics);
        Assert.Equal(CliTerminalMode.None, resolution.TerminalMode);
        Assert.Null(resolution.Input);
    }

    private static CliCommandTree CreateTerminalTree()
    {
        var group = new Command("group")
        {
            TreatUnmatchedTokensAsErrors = false,
        };
        group.SetAction(static _ => 0);

        var leaf = new Command("leaf")
        {
            TreatUnmatchedTokensAsErrors = false,
        };
        leaf.SetAction(static _ => 0);
        leaf.Arguments.Add(
            new Argument<string?>("source")
            {
                Arity = ArgumentArity.ZeroOrOne,
            });
        leaf.Options.Add(
            new Option<bool>("--local")
            {
                Arity = ArgumentArity.Zero,
            });
        group.Subcommands.Add(leaf);

        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(group, CliHelpContent.Empty, [])],
            []);
        tree.Root.TreatUnmatchedTokensAsErrors = false;
        return tree;
    }

    private static CliCommandTree CreateRouteListPolicyTree(
        out Argument<string?> inspectSourceReference)
    {
        var route = new Command("route");
        route.SetAction(static _ => 0);

        var list = new Command("list");
        list.Options.Add(
            new Option<string?>("--depth")
            {
                Arity = ArgumentArity.ExactlyOne,
            });
        route.Subcommands.Add(list);

        var inspect = new Command("inspect");
        inspectSourceReference = new Argument<string?>("source-reference")
        {
            Arity = ArgumentArity.ExactlyOne,
        };
        inspect.Arguments.Add(inspectSourceReference);
        route.Subcommands.Add(inspect);

        return CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(
                route,
                CliHelpContent.Empty,
                [new CliDelimiterPolicy("--depth", CliDelimiterShape.Equals)])],
            []);
    }

    private sealed class StubBinding(Command command) : ICliCommandBinding
    {
        public Command Command { get; } = command;

        public CliHelpContent Help { get; } = CliHelpContent.Empty;

        public CliWorkspaceRequirement WorkspaceRequirement => CliWorkspaceRequirement.Absent;

        public ValueTask<CliProcessCompletion> InvokeAsync(
            CliBindingParse parse,
            CliInvocation invocation,
            CliOutputWriters writers,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public ValueTask<CliProcessCompletion> PresentInvalidAsync(
            CliInvalidBindingInput input,
            CliOutputWriters writers,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
