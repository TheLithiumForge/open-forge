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
    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI option result facts reader reports an omitted option as implicit")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionResultFactsReaderReportsOmittedOptionAsImplicit()
    {
        var tree = CreateTree();
        var parse = tree.Parse([]);

        AssertFacts(
            CliOptionResultFactsReader.Read(parse.Result, tree.Options.Detail),
            false,
            0,
            0);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI root exposes canonical options and typed defaults")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RootExposesOnlyCanonicalGlobalOptionsAndTypedDefaults()
    {
        var tree = CreateTree();

        Assert.Equal(
            ["--workspace", "--format", "--detail", "--detail-filter", "--help", "--version"],
            tree.Root.Options.Select(option => option.Name));
        Assert.All(tree.Root.Options, option => Assert.Empty(option.Aliases));

        var input = CliGlobalInputReader.Read(tree.Parse([]));
        Assert.Null(input.WorkspaceValue);
        Assert.Equal(0, input.WorkspaceOccurrences);
        Assert.Equal(CliFormat.Text, input.OutputFormat);
        Assert.Equal(0, input.FormatOccurrences);
        Assert.Equal(CliDetail.Minimal, input.Detail);
        Assert.Equal(0, input.DetailOccurrences);
        Assert.Null(input.Filter);
        Assert.Equal(0, input.FilterOccurrences);
        Assert.False(input.Help);
        Assert.Equal(0, input.HelpOccurrences);
        Assert.False(input.Version);
        Assert.Equal(0, input.VersionOccurrences);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI parser rejects repeated singleton options")]
    [InlineData("--workspace", "one", "two")]
    [InlineData("--format", "text", "json")]
    [InlineData("--detail", "minimal", "debug")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RepeatedSingletonOptionsAreInvalid(string option, string first, string second)
    {
        var tree = CreateTree();
        var parse = tree.Parse([option, first, option, second]);
        Assert.NotEmpty(parse.Result.Errors);
        Assert.Equal(CliInvalidInputSource.Parser, CliTerminalValidator.Validate(parse).InvalidInput?.Source);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI severity filters union repeated values and all wins")]
    [InlineData("warning", "error", false)]
    [InlineData("all", "warning", true)]
    [InlineData("warning", "all", true)]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RepeatedFiltersUnion(string first, string second, bool all)
    {
        var tree = CreateTree();
        var parse = tree.Parse(["--detail-filter", first, "--detail-filter", second]);
        Assert.Empty(parse.Result.Errors);
        AssertFacts(CliOptionResultFactsReader.Read(parse.Result, tree.Options.DetailFilter), true, 2, 2);
        var input = Assert.IsType<CliGlobalInput>(CliTerminalValidator.Validate(parse).Input);
        Assert.Equal(2, input.FilterOccurrences);
        var expected = all ? Enum.GetValues<CliSeverity>() : [CliSeverity.Error, CliSeverity.Warning];
        Assert.NotNull(input.Filter);
        Assert.True(input.Filter.SetEquals(expected));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI rejects retired presentation flags")]
    [InlineData("--json")]
    [InlineData("--verbose")]
    [InlineData("--view=compact")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RetiredPresentationFlagsAreInvalid(string option)
    {
        var invalid = CliTerminalValidator.Validate(CreateTree().Parse([option])).InvalidInput;
        Assert.Equal(CliInvalidInputSource.Parser, invalid?.Source);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI accepts every finite detail and format value")]
    [InlineData("minimal", (int)CliDetail.Minimal, "text", (int)CliFormat.Text)]
    [InlineData("standard", (int)CliDetail.Standard, "json", (int)CliFormat.Json)]
    [InlineData("full", (int)CliDetail.Full, "text", (int)CliFormat.Text)]
    [InlineData("debug", (int)CliDetail.Debug, "json", (int)CliFormat.Json)]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void FinitePresentationValuesAreAccepted(string detail, int detailValue, string format, int formatValue)
    {
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse(["--detail", detail, "--format", format]));
        Assert.Null(resolution.InvalidInput);
        var input = Assert.IsType<CliGlobalInput>(resolution.Input);
        Assert.Equal((CliDetail)detailValue, input.Detail);
        Assert.Equal((CliFormat)formatValue, input.OutputFormat);
        Assert.Equal(1, input.DetailOccurrences);
        Assert.Equal(1, input.FormatOccurrences);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI rejects unknown presentation values")]
    [InlineData("--detail", "compact")]
    [InlineData("--detail", "expanded")]
    [InlineData("--format", "yaml")]
    [InlineData("--detail-filter", "fatal")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void UnknownPresentationValuesAreInvalid(string option, string value)
    {
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse([option, value]));
        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI native value forms resolve through typed global input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--workspace", "path", "path", false, true)]
    [InlineData("--workspace=path", null, "path", false, true)]
    [InlineData("--workspace:path", null, "path", false, true)]
    [InlineData("--detail", "standard", null, true, false)]
    [InlineData("--detail=standard", null, null, true, false)]
    [InlineData("--detail:standard", null, null, true, false)]
    public void NativeValueFormsResolveThroughTypedGlobalInput(
        string option,
        string? separateValue,
        string? expectedWorkspace,
        bool standardDetail,
        bool usesWorkspace)
    {
        var arguments = separateValue is null
            ? new[] { option }
            : new[] { option, separateValue };
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse(arguments));

        Assert.Null(resolution.InvalidInput);
        var input = Assert.IsType<CliGlobalInput>(resolution.Input);
        Assert.Equal(expectedWorkspace, input.WorkspaceValue);
        Assert.Equal(standardDetail ? CliDetail.Standard : CliDetail.Minimal, input.Detail);

        Assert.Equal(usesWorkspace ? 1 : 0, input.WorkspaceOccurrences);
        Assert.Equal(usesWorkspace ? 0 : 1, input.DetailOccurrences);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "CLI attached-empty value forms resolve as invalid input")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--workspace=")]
    [InlineData("--workspace:")]
    [InlineData("--detail=")]
    [InlineData("--detail:")]
    [InlineData("--format=")]
    [InlineData("--format:")]
    [InlineData("--detail-filter=")]
    [InlineData("--detail-filter:")]
    public void AttachedEmptyValueFormsAreInvalidWithoutGlobalFallback(string option)
    {
        var resolution = CliTerminalValidator.Validate(CreateTree().Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
        Assert.Null(resolution.Input);
        Assert.Equal(CliTerminalMode.None, resolution.TerminalMode);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Route List depth accepts native delimiters before option termination")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--depth", "1")]
    [InlineData("--depth=1", null)]
    [InlineData("--depth:1", null)]
    public void RouteListDepthAcceptsNativeFormsBeforeTerminator(
        string option,
        string? separateValue)
    {
        var tree = CreateRouteListTree(out _);
        string[] arguments = separateValue is null
            ? ["route", "list", option]
            : ["route", "list", option, separateValue];
        var parse = tree.Parse(arguments);

        Assert.Empty(parse.Result.Errors);
        Assert.Null(CliTerminalValidator.Validate(parse).InvalidInput);

        var equals = CliTerminalValidator.Validate(
            tree.Parse(["route", "list", "--depth=1"]));
        Assert.Null(equals.InvalidInput);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI preserves an option-like sibling source after the terminator")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void OptionLikeSiblingSourceAfterTerminatorIsPreserved()
    {
        var tree = CreateRouteListTree(out var inspectSourceReference);
        var parse = tree.Parse(["route", "inspect", "--", "--depth"]);

        Assert.Empty(parse.Result.Errors);
        Assert.Equal("--depth", parse.Result.GetValue(inspectSourceReference));

        var resolution = CliTerminalValidator.Validate(parse);
        Assert.Null(resolution.InvalidInput);
        Assert.NotNull(resolution.Input);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI parser diagnostics preserve unknown-option errors")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void ParserDiagnosticsPreserveUnknownOptionErrors()
    {
        var parse = CreateTree().Parse(["--detail", "minimal", "--unknown"]);

        var invalid = Assert.IsType<CliInvalidInput>(CliTerminalValidator.Validate(parse).InvalidInput);
        Assert.Equal(CliInvalidInputSource.Parser, invalid.Source);
        Assert.Contains(invalid.Diagnostics, diagnostic =>
            diagnostic.Contains("unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI terminal validation precedes terminal short circuiting")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalValidationOccursBeforeShortCircuiting()
    {
        var tree = CreateTree();
        var version = CliTerminalValidator.Validate(tree.Parse(["--format=json", "--version"]));
        var conflict = CliTerminalValidator.Validate(tree.Parse(["--help", "--version"]));

        Assert.Equal(CliTerminalMode.Version, version.TerminalMode);
        Assert.Equal(CliFormat.Json, version.Input?.OutputFormat);
        Assert.Equal(CliInvalidInputSource.Semantic, conflict.InvalidInput?.Source);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "CLI terminal globals remain no-op at root, group, and custom leaf")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalGlobalsRemainNoOpAtRootGroupAndCustomLeaf()
    {
        var tree = CreateTerminalTree();

        var root = AssertTerminalNoOp(
            tree,
            ["--format=json", "--help"],
            CliTerminalMode.Help);
        Assert.Equal(CliFormat.Json, root.OutputFormat);

        var group = AssertTerminalNoOp(
            tree,
            ["group", "--detail=debug", "--version"],
            CliTerminalMode.Version);
        Assert.Equal(CliDetail.Debug, group.Detail);

        var leaf = AssertTerminalNoOp(
            tree,
            ["group", "leaf", "--format=json", "--help"],
            CliTerminalMode.Help);
        Assert.Equal(CliFormat.Json, leaf.OutputFormat);
    }

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
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

    [Trait("Boundary", "Input")]
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
            [new CliRootBranch(group, CliHelpContent.Empty)],
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

    [Trait("Boundary", "Input")]
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
            [new CliRootBranch(group, CliHelpContent.Empty)],
            []);
        tree.Root.TreatUnmatchedTokensAsErrors = false;
        return tree;
    }

    private static CliCommandTree CreateRouteListTree(
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
                CliHelpContent.Empty)],
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
