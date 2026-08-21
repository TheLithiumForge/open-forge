using System.CommandLine;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
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
        Assert.Equal(CliView.Expanded, input.View);
        Assert.Equal(CliVerbosity.Normal, input.Verbosity);
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

    [Theory(DisplayName = "CLI delimiter rules run after parser success")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Unit")]
    [InlineData("--workspace=path", (int)CliInvalidInputSource.Delimiter)]
    [InlineData("--view", (int)CliInvalidInputSource.Delimiter, "compact")]
    [InlineData("--view:compact", (int)CliInvalidInputSource.Delimiter)]
    public void DelimiterRulesAreAppliedAfterParserSuccess(
        string option,
        int expected,
        string? value = null)
    {
        var arguments = value is null ? new[] { option } : new[] { option, value };
        var resolution = CliTerminalValidator.Validate(new CliParser(CreateTree()).Parse(arguments));

        Assert.Equal((CliInvalidInputSource)expected, resolution.InvalidInput?.Source);
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
            CliInvalidInput invalidInput,
            CliGlobalInput input,
            CliProcessEnvironment environment,
            CliOutputWriters writers,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
