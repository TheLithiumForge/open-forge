using System.CommandLine;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindPresentationBindingTests
{
    [Fact(DisplayName = "Find binding exposes the exact seven-option command and preserves one command identity"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingSymbolsExposeExactCommandIdentity()
    {
        var symbols = FindBinding.CreateSymbols();

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
    }

    [Fact(DisplayName = "Find binding closure requires its typed operation, one renderer catalogue, and optional diagnostic renderer"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingClosureUsesRequiredComponentsExactlyOnce()
    {
        var symbols = FindBinding.CreateSymbols();
        var operation = FindOperationFactory.Create();
        var components = new FindBindingComponents
        {
            Help = new CliHelpContent([
                new CliHelpSection("Syntax", "find")
            ]),
            Operation = operation,
        };

        var binding = FindBinding.CreateRequestBinding(symbols, components);

        Assert.Same(symbols.FindCommand, binding.Command);
        Assert.Same(components.Help, binding.Help);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
    }


    [Fact(DisplayName = "Typed Find invalid input retains an explicitly malformed content request with no parsed parts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void InvalidContentRetainsExplicitPresenceState()
    {
        var symbols = FindBinding.CreateSymbols();
        string[] arguments = ["find", "--content=unknown-part"];
        var parse = symbols.FindCommand.Parse(arguments);
        Assert.Empty(parse.Errors);
        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(Workspace()));

        var result = Assert.IsType<FindResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.True(result.Presentation.Content.IsRequested);
        Assert.Empty(result.Presentation.Content.Supplied);
        Assert.Empty(result.Presentation.Content.Effective);
        Assert.Equal(FindProjectionCoverageState.NotStarted, result.Coverage.Projection);
    }

    [Fact(DisplayName = "Find workspace selection failure produces the typed blocked result with a null workspace"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void WorkspaceSelectionFailureUsesTypedBlockedResult()
    {
        var symbols = FindSymbols.Create();
        string[] arguments = ["find", "--include=docs"];
        var parse = symbols.FindCommand.Parse(arguments);
        var invalidInput = new CliInvalidBindingInput(
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                ["The selected workspace is missing."]),
            GlobalInput(),
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliBindingParse(parse, arguments));

        var result = FindBindingSupport.CreateWorkspaceUnavailableResult(
            new FindResultBuilder(),
            FindBindingSupport.ReadQueryInput(parse, symbols, CliDetail.Standard),
            invalidInput);

        Assert.Null(result.Workspace);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(FindFindingCode.WorkspaceUnavailable, Assert.Single(result.Findings).Code);
        Assert.Empty(result.Matches);
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

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliFormat format = CliFormat.Json,
        CliDetail? diagnosticDetail = null)
        => new(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, diagnosticDetail ?? CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);

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

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-binding-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
