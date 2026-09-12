using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveBindingTests
{
    [Fact(DisplayName = "Extension Remove symbols expose the exact repeatable ID and authority grammar"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void SymbolsExposeExactGrammar()
    {
        var group = ExtensionBinding.CreateGroup();
        var symbols = ExtensionRemoveBinding.CreateSymbols(group);

        Assert.Same(symbols.Command, Assert.Single(group.Subcommands));
        Assert.Empty(symbols.Command.Aliases);
        Assert.Equal("remove", symbols.Command.Name);
        Assert.Equal("stable-id", symbols.StableIds.Name);
        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.StableIds.Arity);
        Assert.Equal(typeof(string[]), symbols.StableIds.ValueType);
        Assert.Equal(
            ["--prune", "--automatic", "--dry-run"],
            symbols.Command.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.Zero, symbols.Prune.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        Assert.False(symbols.Prune.AllowMultipleArgumentsPerToken);
        Assert.False(symbols.Automatic.AllowMultipleArgumentsPerToken);
        Assert.False(symbols.DryRun.AllowMultipleArgumentsPerToken);
    }

    [Fact(DisplayName = "Extension Remove binding consumes typed IDs, repeated flags, mode, and workspace"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void BindingConsumesTypedRequest()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments =
        [
            "toolkit",
            "base",
            "--prune",
            "--automatic",
            "--dry-run",
            "--dry-run",
        ];
        var invocation = Invocation(CliOutputFormat.Human);
        var parse = symbols.Command.Parse(arguments);

        Assert.Empty(parse.Errors);
        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            invocation);

        Assert.Same(invocation.Workspace, request.Workspace);
        Assert.Equal(ExtensionRemoveMode.DryRun, request.Mode);
        Assert.Equal(["toolkit", "base"], request.RequestedIds);
        Assert.True(request.Prune);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
    }

    [Theory(DisplayName = "Extension Remove binding grants interaction only to a human non-automatic request"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    [InlineData("human", false, true)]
    [InlineData("json", false, false)]
    [InlineData("human", true, false)]
    public void BindingSelectsPromptPolicy(string formatName, bool automatic, bool expectedInteraction)
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments = automatic ? ["toolkit", "--automatic"] : ["toolkit"];
        var format = formatName == "json" ? CliOutputFormat.Json : CliOutputFormat.Human;
        var parse = symbols.Command.Parse(arguments);

        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(format));

        Assert.Equal(expectedInteraction, request.AllowInteraction);
        Assert.Equal(["toolkit"], request.RequestedIds);
    }

    [Fact(DisplayName = "Extension Remove binding reports duplicate typed IDs as an invalid result"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public async Task BindingRejectsDuplicateTypedIds()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var result = await InvokeBindingAsync(
            symbols,
            ["toolkit", "toolkit"],
            CliOutputFormat.Human);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.InvalidInput);
    }

    [Fact(DisplayName = "Extension Remove binding requires IDs for a non-prompt JSON request"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public async Task BindingRequiresIdsOutsidePromptCapableHumanMode()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var result = await InvokeBindingAsync(symbols, [], CliOutputFormat.Json);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.SelectionRequired);
        Assert.Equal(
            "open-forge extension remove --help",
            Assert.IsType<CliNextAction>(result.Next).Command);
    }

    [Fact(DisplayName = "Extension Remove binding leaves an argumentless human request prompt-capable"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void BindingLeavesArgumentlessHumanRequestPromptCapable()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var arguments = Array.Empty<string>();
        var parse = symbols.Command.Parse(arguments);

        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(CliOutputFormat.Human));

        Assert.Empty(request.RequestedIds);
        Assert.True(request.AllowInteraction);
    }

    private static async ValueTask<ExtensionRemoveResult> InvokeBindingAsync(
        ExtensionRemoveSymbols symbols,
        string[] arguments,
        CliOutputFormat format)
    {
        ExtensionRemoveResult? observed = null;
        using var input = new StringReader(string.Empty);
        using var prompts = new StringWriter();
        var operation = ExtensionRemoveOperationFactory.Create(
            new CliInteractiveSession(input, prompts, canPrompt: false));
        var binding = ExtensionRemoveBinding.Close(
            symbols,
            CliHelpContent.Empty,
            operation,
            new CliRendererSet<ExtensionRemoveResult>(
                presentation =>
                {
                    observed = presentation.Result;
                    return "observed";
                },
                presentation =>
                {
                    observed = presentation.Result;
                    return "{}";
                }),
            diagnosticRenderer: null);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();

        var parse = symbols.Command.Parse(arguments);
        var completion = await ((ICliCommandBinding)binding).InvokeAsync(
            new CliBindingParse(parse, arguments),
            Invocation(format),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        return Assert.IsType<ExtensionRemoveResult>(observed);
    }

    private static CliInvocation Invocation(CliOutputFormat format)
    {
        const string path = "extension-remove-binding-workspace";
        var workspace = new CliWorkspace(
            path,
            path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, path),
            workspace);
    }
}
