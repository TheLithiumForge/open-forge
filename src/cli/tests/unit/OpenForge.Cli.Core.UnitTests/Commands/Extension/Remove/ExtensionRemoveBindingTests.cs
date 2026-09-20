using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveBindingTests
{
    [Theory(DisplayName = "Extension Remove parser rejects unknown options instead of consuming them as IDs"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    [InlineData("toolkit --prune")]
    [InlineData("--prune toolkit")]
    [InlineData("--prune")]
    [InlineData("toolkit --unknown")]
    public void ParserRejectsUnknownOptions(string arguments)
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var parse = symbols.Command.Parse(arguments);

        Assert.Contains(parse.Errors, error => error.Message.StartsWith(
            "Unrecognized command or argument '--", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Extension Remove symbols expose the exact repeatable ID and authority grammar"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
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
            ["--automatic", "--dry-run", "--allow-path"],
            symbols.Command.Options.Select(option => option.Name));
        Assert.Equal(ArgumentArity.Zero, symbols.Automatic.Arity);
        Assert.Equal(ArgumentArity.Zero, symbols.DryRun.Arity);
        Assert.False(symbols.Automatic.AllowMultipleArgumentsPerToken);
        Assert.False(symbols.DryRun.AllowMultipleArgumentsPerToken);
    }

    [Fact(DisplayName = "Extension Remove binding consumes typed IDs, repeated flags, mode, and workspace"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingConsumesTypedRequest()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments =
        [
            "toolkit",
            "base",
            "--automatic",
            "--dry-run",
            "--dry-run",
            "--allow-path", "docs",
            "--allow-path", "tools",
        ];
        var invocation = Invocation(CliFormat.Text);
        var parse = symbols.Command.Parse(arguments);

        Assert.Empty(parse.Errors);
        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            invocation);

        Assert.Same(invocation.Workspace, request.Workspace);
        Assert.Equal(["docs", "tools"], request.AllowPath);
        Assert.Equal(ExtensionRemoveMode.DryRun, request.Mode);
        Assert.Equal(["toolkit", "base"], request.RequestedIds);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
    }

    [Theory(DisplayName = "Extension Remove binding grants interaction only to a human non-automatic request"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    [InlineData("human", false, true)]
    [InlineData("json", false, false)]
    [InlineData("human", true, false)]
    public void BindingSelectsPromptPolicy(string formatName, bool automatic, bool expectedInteraction)
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        string[] arguments = automatic ? ["toolkit", "--automatic"] : ["toolkit"];
        var format = formatName == "json" ? CliFormat.Json : CliFormat.Text;
        var parse = symbols.Command.Parse(arguments);

        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(format));

        Assert.Equal(expectedInteraction, request.AllowInteraction);
        Assert.Equal(["toolkit"], request.RequestedIds);
    }

    [Fact(DisplayName = "Extension Remove binding leaves an argumentless human request prompt-capable"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingLeavesArgumentlessHumanRequestPromptCapable()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var arguments = Array.Empty<string>();
        var parse = symbols.Command.Parse(arguments);

        var request = ExtensionRemoveBinding.BindRequest(
            symbols,
            new CliBindingParse(parse, arguments),
            Invocation(CliFormat.Text));

        Assert.Empty(request.RequestedIds);
        Assert.True(request.AllowInteraction);
    }

    private static CliInvocation Invocation(CliFormat format)
    {
        const string path = "extension-remove-binding-workspace";
        var workspace = new CliWorkspace(
            path,
            path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, path),
            workspace);
    }
}
