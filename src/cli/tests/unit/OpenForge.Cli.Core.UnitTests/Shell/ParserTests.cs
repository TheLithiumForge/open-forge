using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Shell;

public sealed class ParserTests
{
    [Fact(DisplayName = "CLI root exposes canonical options and typed defaults")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void RootExposesOnlyCanonicalGlobalOptionsAndTypedDefaults()
    {
        var tree = CreateTree();

        Assert.Equal(
            ["--workspace", "--json", "--view", "--verbose", "--help", "--version"],
            tree.Root.Options.Select(option => option.Name));
        Assert.All(tree.Root.Options, option => Assert.Empty(option.Aliases));

        var input = CliGlobalInputReader.Read(new CliParser(tree).Parse([]));
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
        var parser = new CliParser(CreateTree());
        var scalar = parser.Parse(["--workspace", "one", "--workspace", "two"]);
        var booleans = parser.Parse(["--json", "--json", "--verbose", "--verbose"]);

        Assert.NotEmpty(scalar.Result.Errors);
        Assert.Equal(CliInvalidInputSource.Parser, CliTerminalValidator.Validate(scalar).InvalidInput?.Source);
        Assert.Empty(booleans.Result.Errors);
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
        var parser = new CliParser(tree);
        var help = parser.Parse(["--help", "--help"]);
        var version = parser.Parse(["--version", "--version"]);

        Assert.Empty(help.Result.Errors);
        Assert.Empty(version.Result.Errors);
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
        var resolution = CliTerminalValidator.Validate(new CliParser(CreateTree()).Parse(arguments));

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
        var resolution = CliTerminalValidator.Validate(new CliParser(CreateTree()).Parse([option]));

        Assert.NotNull(resolution.InvalidInput);
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
        var parse = new CliParser(tree).Parse(arguments);

        Assert.Empty(parse.Result.Errors);
        var invalid = CliTerminalValidator.Validate(parse).InvalidInput;
        Assert.NotNull(invalid);
        Assert.Equal(CliInvalidInputSource.Delimiter, invalid.Source);

        var equals = CliTerminalValidator.Validate(
            new CliParser(tree).Parse(["route", "list", "--depth=1"]));
        Assert.Null(equals.InvalidInput);
    }

    [Fact(DisplayName = "Aggregated Route List delimiter policies stop at the option terminator")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void AggregatedDelimiterPoliciesDoNotRejectOptionLikeSiblingSourceAfterTerminator()
    {
        var tree = CreateRouteListPolicyTree(out var inspectSourceReference);
        var parse = new CliParser(tree).Parse(["route", "inspect", "--", "--depth"]);

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
        var parse = new CliParser(CreateTree()).Parse(["--view", "compact", "--unknown"]);

        var invalid = Assert.IsType<CliInvalidInput>(CliTerminalValidator.Validate(parse).InvalidInput);
        Assert.Equal(CliInvalidInputSource.Parser, invalid.Source);
        Assert.Contains(invalid.Diagnostics, diagnostic =>
            diagnostic.Contains("unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "CLI terminal validation precedes terminal short circuiting")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void TerminalValidationOccursBeforeShortCircuiting()
    {
        var parser = new CliParser(CreateTree());
        var version = CliTerminalValidator.Validate(parser.Parse(["--json", "--version"]));
        var conflict = CliTerminalValidator.Validate(parser.Parse(["--help", "--version"]));

        Assert.Equal(CliTerminalMode.Version, version.TerminalMode);
        Assert.Equal(CliOutputFormat.Json, version.Input?.OutputFormat);
        Assert.Equal(CliInvalidInputSource.Semantic, conflict.InvalidInput?.Source);
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
        var parser = new CliParser(tree);

        Assert.Equal(CliBindingSelectionState.Root, CliBindingSelector.Select(parser.Parse([])).State);
        Assert.Equal(CliBindingSelectionState.Group, CliBindingSelector.Select(parser.Parse(["group"])).State);
        var selected = CliBindingSelector.Select(parser.Parse(["group", "leaf"]));
        Assert.Equal(CliBindingSelectionState.Leaf, selected.State);
        Assert.Same(binding, selected.Binding);
        Assert.Equal(
            CliBindingSelectionState.NoLeaf,
            CliBindingSelector.Select(parser.Parse(["group", "unbound"])).State);
        Assert.Null(tree.FindBinding(new Command("leaf")));
    }

    [Fact(DisplayName = "CLI unknown input remains a parser fact")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    public void UnknownInputRemainsAParserFact()
    {
        var parse = new CliParser(CreateTree()).Parse(["unknown"]);

        Assert.NotEmpty(parse.Result.Errors);
        Assert.Equal(CliInvalidInputSource.Parser, CliTerminalValidator.Validate(parse).InvalidInput?.Source);
    }

    private static CliCommandTree CreateTree()
    {
        return CliCommandTree.Create(CliHelpContent.Empty, [], []);
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
