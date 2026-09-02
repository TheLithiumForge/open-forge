using System.CommandLine;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

public sealed class ContextBindingContractTests
{
    [Fact(DisplayName = "Context symbols expose the exact direct-root operand and three operation options"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactDirectGrammar()
    {
        var symbols = ContextBinding.CreateSymbols();

        Assert.Equal("context", symbols.ContextCommand.Name);
        Assert.Empty(symbols.ContextCommand.Aliases);
        Assert.Empty(symbols.ContextCommand.Subcommands);
        Assert.Single(symbols.ContextCommand.Arguments);
        Assert.Equal("source-reference", symbols.Sources.Name);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Sources.Arity);
        Assert.Equal(typeof(string[]), symbols.Sources.ValueType);
        Assert.Equal(
            ["--additions-only", "--content", "--follow-links"],
            symbols.ContextCommand.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.Zero, symbols.AdditionsOnly.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Content.Arity);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.FollowLinks.Arity);
        Assert.False(symbols.Content.AllowMultipleArgumentsPerToken);
        Assert.False(symbols.FollowLinks.AllowMultipleArgumentsPerToken);
        Assert.Equal("part[,part...]", symbols.Content.HelpName);
        Assert.Equal("positive-depth|all", symbols.FollowLinks.HelpName);
    }

    [Fact(DisplayName = "Context binding preserves operand order, idempotent additions, canonical content, link depth, and supplied view"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void BindingPreservesNormalizedRequest()
    {
        var symbols = ContextBinding.CreateSymbols();
        string[] arguments =
        [
            "alpha", ".agents/docs.md", "alpha",
            "--additions-only", "--additions-only",
            @"--content=section:Rules\, Limits,body,paths,body",
            "--follow-links=2",
        ];
        var parse = symbols.ContextCommand.Parse(arguments);
        var bound = new ContextRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(CliView.Compact));

        var request = Assert.IsType<ContextRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(["alpha", ".agents/docs.md", "alpha"], request.SourceReferences);
        Assert.True(request.AdditionsOnly);
        Assert.Equal(
            ["section:Rules, Limits", "body", "paths", "body"],
            request.Content.Supplied.Select(value => value.CanonicalValue));
        Assert.Equal(
            ["paths", "body", "section:Rules, Limits"],
            request.Content.Effective.Select(value => value.CanonicalValue));
        Assert.Equal(ContextLinkExpansionMode.Bounded, request.LinkExpansion.Mode);
        Assert.Equal(2, request.LinkExpansion.Depth);
        Assert.Null(request.SuppliedView);
        Assert.Equal(CliView.Compact, request.EffectiveView);
    }

    [Theory(DisplayName = "Context binding consumes typed parser values across native option forms"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData("equals")]
    [InlineData("colon")]
    [InlineData("separate")]
    public void BindingConsumesTypedNativeOptionValues(string form)
    {
        var symbols = ContextBinding.CreateSymbols();
        string[] arguments = form switch
        {
            "equals" => ["alpha", "--content=body", "--follow-links=2"],
            "colon" => ["alpha", "--content:body", "--follow-links:2"],
            "separate" => ["alpha", "--content", "body", "--follow-links", "2"],
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The native option value form is not defined."),
        };

        var bound = new ContextRequestBinder(symbols).Bind(
            new CliBindingParse(symbols.ContextCommand.Parse(arguments), arguments),
            Invocation(CliView.Compact, suppliedView: true));

        var request = Assert.IsType<ContextRequest>(bound.Request);
        Assert.Equal(["body"], request.Content.Supplied.Select(value => value.CanonicalValue));
        Assert.Equal(ContextLinkExpansionMode.Bounded, request.LinkExpansion.Mode);
        Assert.Equal(2, request.LinkExpansion.Depth);
        Assert.Equal(CliView.Compact, request.SuppliedView);
        Assert.Equal(CliView.Compact, request.EffectiveView);
    }

    [Theory(DisplayName = "Context binding returns typed invalid results for exact semantic input errors"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData("additions-without-source", "context.invalid-input")]
    [InlineData("repeated-content", "context.invalid-content")]
    [InlineData("empty-content-part", "context.invalid-content")]
    [InlineData("zero-link-depth", "context.invalid-link-depth")]
    [InlineData("repeated-link-depth", "context.invalid-link-depth")]
    [InlineData("invalid-source", "context.invalid-source")]
    public void BindingFormsTypedInvalidResults(string scenario, string expectedCode)
    {
        var symbols = ContextBinding.CreateSymbols();
        var arguments = InvalidArguments(scenario);
        var parse = symbols.ContextCommand.Parse(arguments);
        var bound = new ContextRequestBinder(symbols).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(CliView.Expanded));

        var result = Assert.IsType<ContextResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(ContextCoverageState.NotStarted, result.Coverage.State);
        Assert.Contains(result.Findings, finding => ContextDefinitions.Read(finding.Code).Code == expectedCode);
        Assert.Equal("open-forge context --help", Assert.IsType<CliNextAction>(result.Next).Command);
    }

    [Fact(DisplayName = "Context definitions expose only the accepted ordered finding vocabulary and fixed statuses"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public void DefinitionsExposeExactFindingVocabulary()
    {
        Assert.Equal(
        [
            ("context.invalid-input", CliSemanticStatus.Invalid),
            ("context.invalid-source", CliSemanticStatus.Invalid),
            ("context.invalid-content", CliSemanticStatus.Invalid),
            ("context.invalid-link-depth", CliSemanticStatus.Invalid),
            ("context.workspace-unavailable", CliSemanticStatus.Blocked),
            ("context.workspace-unsafe", CliSemanticStatus.Blocked),
            ("context.source-ambiguous", CliSemanticStatus.Blocked),
            ("context.source-unsafe", CliSemanticStatus.Blocked),
            ("context.overwrite-ambiguous", CliSemanticStatus.Blocked),
            ("context.target-ambiguous", CliSemanticStatus.Blocked),
            ("context.target-unsafe", CliSemanticStatus.Blocked),
            ("context.closure-unavailable", CliSemanticStatus.Incomplete),
            ("context.layer-unavailable", CliSemanticStatus.Incomplete),
            ("context.invalid-encoding", CliSemanticStatus.Incomplete),
            ("context.markdown-unavailable", CliSemanticStatus.Incomplete),
            ("context.target-missing", CliSemanticStatus.Incomplete),
            ("context.fragment-missing", CliSemanticStatus.Incomplete),
            ("context.link-encoding-invalid", CliSemanticStatus.Incomplete),
            ("context.target-unreadable", CliSemanticStatus.Incomplete),
            ("context.section-ambiguous", CliSemanticStatus.Incomplete),
            ("context.projection-unavailable", CliSemanticStatus.Incomplete),
            ("context.identity-collision", CliSemanticStatus.Attention),
            ("context.target-case-mismatch", CliSemanticStatus.Attention),
            ("context.frontmatter-missing", CliSemanticStatus.Attention),
            ("context.section-missing", CliSemanticStatus.Attention),
            ("context.operation-failed", CliSemanticStatus.Failed),
            ("context.interrupted", CliSemanticStatus.Interrupted),
        ],
            Enum.GetValues<ContextFindingCode>()
                .Select(ContextDefinitions.Read)
                .Select(value => (value.Code, value.Status)));
    }

    private static CliInvocation Invocation(CliView view, bool suppliedView = false)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "context-binding-contract"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "1.0.0"),
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, root),
            workspace)
        {
            SuppliedView = suppliedView ? view : null,
        };
    }

    private static string[] InvalidArguments(string scenario)
        => scenario switch
        {
            "additions-without-source" => ["--additions-only"],
            "repeated-content" => ["--content=body", "--content=body"],
            "empty-content-part" => ["--content=body,,headings"],
            "zero-link-depth" => ["--follow-links=0"],
            "repeated-link-depth" => ["--follow-links=1", "--follow-links=1"],
            "invalid-source" => [".agents/../outside.md"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Context invalid binding scenario is not defined."),
        };
}
