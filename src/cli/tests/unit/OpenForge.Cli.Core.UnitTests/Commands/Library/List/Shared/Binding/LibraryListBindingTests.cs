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
        Assert.Empty(result.Result.Libraries);
        Assert.Null(result.Result.Record.LibraryCount);
        Assert.Equal(LibraryListInventoryState.NotStarted, result.Result.Inventory);
    }
}
