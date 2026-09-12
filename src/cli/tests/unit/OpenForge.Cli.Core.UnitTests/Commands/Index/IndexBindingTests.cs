using System.CommandLine;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index;

public sealed class IndexBindingTests
{
    [Fact(DisplayName = "Index symbols expose the exact direct-root operands and idempotent dry-run option"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactDirectGrammar()
    {
        var symbols = IndexBinding.CreateSymbols();

        Assert.Equal("index", symbols.IndexCommand.Name);
        Assert.Empty(symbols.IndexCommand.Aliases);
        Assert.Empty(symbols.IndexCommand.Subcommands);
        Assert.Single(symbols.IndexCommand.Arguments);
        Assert.Equal("source-reference", symbols.Sources.Name);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.Sources.Arity);
        Assert.Equal(typeof(string[]), symbols.Sources.ValueType);
        var dryRun = Assert.Single(symbols.IndexCommand.Options);
        Assert.Equal("--dry-run", dryRun.Name);
        Assert.Equal(ArgumentArity.Zero, dryRun.Arity);
    }

    [Fact(DisplayName = "Index binding preserves source occurrence order and repeated dry-run as one typed request"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void BindingPreservesSourceOccurrencesAndDryRun()
    {
        var symbols = IndexBinding.CreateSymbols();
        string[] arguments =
        [
            "memory",
            ".agents/skills/_skills.md",
            "memory",
            "--dry-run",
            "--dry-run",
        ];
        var parse = symbols.IndexCommand.Parse(arguments);
        var bound = new IndexRequestBinder(symbols, new IndexResultBuilder()).Bind(
            new CliBindingParse(parse, arguments),
            Invocation());

        var request = Assert.IsType<IndexRequest>(bound.Request);
        Assert.Null(bound.InvalidResult);
        Assert.Equal(["memory", ".agents/skills/_skills.md", "memory"], request.SourceReferences);
        Assert.Equal(IndexMode.DryRun, request.Mode);
    }

    [Fact(DisplayName = "Index binding reports malformed source occurrences without retaining raw operands"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void BindingFormsTypedInvalidSourceResult()
    {
        var symbols = IndexBinding.CreateSymbols();
        string[] arguments = ["memory", ".agents/../outside.md"];
        var bound = new IndexRequestBinder(symbols, new IndexResultBuilder()).Bind(
            new CliBindingParse(symbols.IndexCommand.Parse(arguments), arguments),
            Invocation());

        var result = Assert.IsType<IndexResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(IndexFindingCode.InvalidSource, finding.Code);
        Assert.Equal(2, finding.SourceOccurrence);
        Assert.DoesNotContain("outside.md", finding.Cause, StringComparison.Ordinal);
        Assert.Empty(result.Selection.Sources);
        Assert.Equal("open-forge index --help", Assert.IsType<CliNextAction>(result.Next).Command);
    }

    [Fact(DisplayName = "Index binding closes the exact command over direct typed dependencies"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void BindingClosesExactCommandOverDirectDependencies()
    {
        var symbols = IndexBinding.CreateSymbols();
        var help = IndexHelpSections.Create();

        var binding = IndexBinding.Close(
            symbols: symbols,
            help: help,
            operation: IndexOperationFactory.Create().ExecuteAsync,
            renderers: new CliRendererSet<IndexResult>(
                IndexHumanRenderer.Render,
                IndexJsonRenderer.Render),
            diagnosticRenderer: IndexDiagnosticRenderer.Render);

        Assert.Same(symbols.IndexCommand, binding.Command);
        Assert.Same(help, binding.Help);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
    }

    private static CliInvocation Invocation()
    {
        var workspace = IndexTestData.Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "1.0.0"),
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, workspace.LexicalRoot),
            workspace);
    }
}
