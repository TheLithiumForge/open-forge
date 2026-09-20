using OpenForge.Cli.Core.Framework.Workspace.Models;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References;

public sealed class ReferencesBindingTests
{
    [Fact(DisplayName = "References symbols expose one direct source operand and the exact direction and filter options"), Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void SymbolsExposeExactDirectGrammar()
    {
        var symbols = ReferencesBinding.CreateSymbols();

        Assert.Equal("references", symbols.ReferencesCommand.Name);
        Assert.Empty(symbols.ReferencesCommand.Aliases);
        Assert.Single(symbols.ReferencesCommand.Arguments);
        Assert.Empty(symbols.ReferencesCommand.Subcommands);
        Assert.Equal(
            ["--direction", "--include", "--exclude"],
            symbols.ReferencesCommand.Options.Select(option => option.Name));
        Assert.Equal("source-reference", symbols.Source.Name);
        Assert.Equal(ArgumentArity.ZeroOrOne, symbols.Source.Arity);
        Assert.Equal("in|out|both", symbols.Direction.HelpName);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Direction.Arity);
        Assert.Equal(typeof(string[]), symbols.Direction.ValueType);
        Assert.False(symbols.Direction.AllowMultipleArgumentsPerToken);
        Assert.Equal(typeof(string[]), symbols.Include.ValueType);
        Assert.Equal(typeof(string[]), symbols.Exclude.ValueType);
        AssertRepeatable(symbols.Include);
        AssertRepeatable(symbols.Exclude);
    }

    [Theory(DisplayName = "References binding applies omission and exact direction values without aliases"), Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    [InlineData("omitted", (int)ReferencesDirection.Both)]
    [InlineData("in", (int)ReferencesDirection.In)]
    [InlineData("out", (int)ReferencesDirection.Out)]
    [InlineData("both", (int)ReferencesDirection.Both)]
    public void BinderPreservesDirectionDefaultAndExactValues(
        string direction,
        int expectedValue)
    {
        var expected = (ReferencesDirection)expectedValue;
        var symbols = ReferencesBinding.CreateSymbols();
        string[] arguments = direction == "omitted"
            ? ["references", "docs"]
            : ["references", "docs", $"--direction={direction}"];
        var parse = Parse(symbols, arguments);
        var bound = new ReferencesRequestBinder(symbols).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            Invocation());

        var request = Assert.IsType<ReferencesRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(expected, request.Direction);
        Assert.Equal("docs", request.SourceReference);
        Assert.Empty(request.SelectorOccurrences);
    }

    [Fact(DisplayName = "References binding preserves repeated include and exclude occurrences in global command-line order across native value forms"), Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BinderPreservesOrderedDuplicateFilterOccurrences()
    {
        var symbols = ReferencesBinding.CreateSymbols();
        var parse = Parse(
            symbols,
            [
                "references", "docs", "--include", "alpha", "--exclude=beta",
                "--include:alpha", "--exclude", "beta",
            ]);
        var bound = new ReferencesRequestBinder(symbols).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            Invocation());

        var request = Assert.IsType<ReferencesRequest>(bound.Request);
        Assert.Equal(
            [
                (SourceUniverseSelectorRole.Include, "alpha", 1),
                (SourceUniverseSelectorRole.Exclude, "beta", 2),
                (SourceUniverseSelectorRole.Include, "alpha", 3),
                (SourceUniverseSelectorRole.Exclude, "beta", 4),
            ],
            request.SelectorOccurrences.Select(value => (value.Role, value.Value, value.Position)));
    }

    [Theory(DisplayName = "References binder returns typed invalid results before workspace or domain work for semantic input errors"), Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    [InlineData("missing-source", "references.invalid-source", (int)ReferencesDirection.Both, true, true)]
    [InlineData("invalid-direction", "references.invalid-direction", null, false, false)]
    [InlineData("repeated-direction", "references.invalid-direction", null, false, false)]
    [InlineData("filter-with-out", "references.invalid-filter", (int)ReferencesDirection.Out, false, true)]
    [InlineData("missing-filter-value", "references.invalid-filter", (int)ReferencesDirection.Both, true, true)]
    public void BinderFormsTypedInvalidResultForSemanticInputErrors(
        string scenario,
        string expectedCode,
        int? expectedDirectionValue,
        bool expectsIncoming,
        bool expectsOutgoing)
    {
        var symbols = ReferencesBinding.CreateSymbols();
        var parse = Parse(symbols, InvalidArguments(scenario));
        var bound = new ReferencesRequestBinder(symbols).Bind(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            Invocation());

        var result = Assert.IsType<ReferencesResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => ReferencesDefinitions.ReadMachineName(finding.Code) == expectedCode);
        Assert.Null(result.Source);
        Assert.Equal(
            expectedDirectionValue is null ? null : (ReferencesDirection)expectedDirectionValue.Value,
            result.RequestedDirection);
        Assert.Equal(expectsIncoming, result.Incoming is not null);
        Assert.Equal(expectsOutgoing, result.Outgoing is not null);
        Assert.Equal(expectsIncoming, result.IncomingSelection is not null);
    }

    [Fact(DisplayName = "References binder gives semantic invalid filter precedence over an unavailable workspace"), Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BinderPreservesInvalidFilterPrecedenceOverUnavailableWorkspace()
    {
        var symbols = ReferencesBinding.CreateSymbols();
        var parse = Parse(symbols, ["references", "docs", "--direction=out", "--include=alpha"]);
        var invalidInput = new CliInvalidBindingInput(
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                ["The selected workspace is missing."]),
            GlobalInput(),
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliBindingParse(parse.Result, parse.OriginalArguments));

        var result = new ReferencesWorkspaceResultFactory(symbols).Create(invalidInput);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ReferencesFindingCode.InvalidFilter);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ReferencesFindingCode.WorkspaceUnavailable);
        Assert.Null(result.Source);

        var validParse = Parse(symbols, ["references", "docs", "--direction=in"]);
        var unavailableInput = new CliInvalidBindingInput(
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                ["The selected workspace is missing."]),
            GlobalInput(),
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliBindingParse(validParse.Result, validParse.OriginalArguments));

        var unavailable = new ReferencesWorkspaceResultFactory(symbols).Create(unavailableInput);

        Assert.Equal(CliSemanticStatus.Blocked, unavailable.Status);
        Assert.Null(unavailable.Workspace);
        Assert.Null(unavailable.Source);
        Assert.Equal(ReferencesCoverage.Blocked, unavailable.Incoming!.Coverage);
        Assert.Null(unavailable.Outgoing);
        Assert.Contains(unavailable.Findings, finding => finding.Code == ReferencesFindingCode.WorkspaceUnavailable);
    }


    private static void AssertRepeatable(Option<string[]> option)
    {
        Assert.Equal(ArgumentArity.ZeroOrMore, option.Arity);
        Assert.False(option.AllowMultipleArgumentsPerToken);
    }

    private static CliParseOutcome Parse(ReferencesSymbols symbols, string[] arguments)
    {
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.ReferencesCommand, CliHelpContent.Empty)],
            []);
        return tree.Parse(arguments);
    }

    private static string[] InvalidArguments(string scenario)
        => scenario switch
        {
            "missing-source" => ["references"],
            "invalid-direction" => ["references", "docs", "--direction=incoming"],
            "repeated-direction" => ["references", "docs", "--direction=in", "--direction=out"],
            "filter-with-out" => ["references", "docs", "--direction=out", "--include=alpha"],
            "missing-filter-value" => ["references", "docs", "--include"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The References invalid-input case is not defined."),
        };

    private static CliInvocation Invocation(
        CliFormat format = CliFormat.Json,
        CliDetail? diagnosticDetail = null)
    {
        var workspace = ReferencesPresentationTestData.Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, diagnosticDetail ?? CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    private static CliGlobalInput GlobalInput()
        => new(
            null,
            0,
            CliFormat.Json,
            0,
            CliDetail.Standard,
            0, null,
            0,
            false,
            0,
            false,
            0);

}
