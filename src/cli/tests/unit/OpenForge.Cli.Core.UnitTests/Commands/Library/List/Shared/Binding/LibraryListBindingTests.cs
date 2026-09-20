using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Binding;

public sealed class LibraryListBindingTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Library List binds one exact workspace without a domain operand"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void BindsExactWorkspace()
    {
        var symbols = LibraryListBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = [];
        var bound = LibraryListRequestBinder.Bind(new CliBindingParse(symbols.Command.Parse(arguments), arguments), LibraryReadInputs.Invocation(), symbols);

        Assert.Null(bound.InvalidResult);
        Assert.Same(LibraryReadInputs.Workspace, Assert.IsType<LibraryListRequest>(bound.Request).Workspace);
        Assert.Empty(symbols.Command.Arguments);
        Assert.Empty(symbols.Command.Options);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Library List rejects domain operands and mutation or selection flags before observation"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("team-knowledge")]
    [InlineData(".agents/directives/review.md")]
    [InlineData("--dry-run")]
    [InlineData("--all")]
    [InlineData("--source")]
    [InlineData("--force")]
    public void RejectsDomainInput(string argument)
    {
        var symbols = LibraryListBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = [argument];
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);
        Assert.NotEmpty(parse.Result.Errors);
        var result = LibraryListRequestBinder.CreateInvalid(LibraryReadInputs.ParserInvalid(parse));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(".agents/open-forge.lock.json", result.Result.Record.Path);
        Assert.Empty(result.Result.Libraries);
        Assert.Null(result.Result.Record.LibraryCount);
        Assert.Equal(LibraryListInventoryState.NotStarted, result.Result.Inventory);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Library List workspace selection failures name the ownership lock"),
     InlineData(false), InlineData(true), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void WorkspaceSelectionFailuresNameOwnershipLock(bool blocked)
    {
        var symbols = LibraryListBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = [];
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);
        var input = LibraryReadInputs.Invalid(parse,
            new CliInvalidInput("workspace.unavailable", CliInvalidInputSource.Workspace,
                ["The workspace cannot be selected."])) with
        {
            WorkspaceSelectionState = blocked ? CliWorkspaceSelectionState.Unsafe : CliWorkspaceSelectionState.Missing,
        };
        var result = LibraryListRequestBinder.CreateInvalid(input);

        Assert.Equal(blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(".agents/open-forge.lock.json", result.Result.Record.Path);
        Assert.Equal(".agents/open-forge.lock.json", Assert.Single(result.Result.Findings).Path);
    }
}
