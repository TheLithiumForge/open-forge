using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Binding;

public sealed class LibraryInspectBindingTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Library Inspect binds exact lowercase ASCII IDs at both length boundaries"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("a")]
    [InlineData("0")]
    [InlineData("team-knowledge-2")]
    [InlineData("maximum")]
    public void BindsExactManagementId(string value)
    {
        var id = value == "maximum" ? new string('a', 128) : value;
        var symbols = LibraryInspectBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = [id];
        var bound = LibraryInspectRequestBinder.Bind(new CliBindingParse(symbols.Command.Parse(arguments), arguments), LibraryReadInputs.Invocation(), symbols);

        Assert.Null(bound.InvalidResult);
        var request = Assert.IsType<LibraryInspectRequest>(bound.Request);
        Assert.Equal(id, request.LibraryId.Value);
        Assert.Same(LibraryReadInputs.Workspace, request.Workspace);
        Assert.Single(symbols.Command.Arguments);
        Assert.Empty(symbols.Command.Options);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Library Inspect rejects missing multiple malformed and source-reference subjects without inventory"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("missing")]
    [InlineData("multiple")]
    [InlineData("overlong")]
    [InlineData("")]
    [InlineData("Team")]
    [InlineData("-team")]
    [InlineData("team-")]
    [InlineData("team--knowledge")]
    [InlineData("team_knowledge")]
    [InlineData("équipe")]
    [InlineData("team knowledge")]
    [InlineData("directives/review")]
    [InlineData(".agents/directives/review.md")]
    [InlineData("shared/team")]
    [InlineData("--all")]
    [InlineData("--dry-run")]
    public void RejectsInvalidSubjects(string scenario)
    {
        string[] arguments = scenario switch
        {
            "missing" => [],
            "multiple" => ["team", "other"],
            "overlong" => [new string('a', 129)],
            _ => [scenario],
        };
        var symbols = LibraryInspectBinding.CreateSymbols(LibraryBinding.CreateGroup());
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);
        LibraryInspectResult result;
        if (parse.Result.Errors.Count > 0)
        {
            result = LibraryInspectRequestBinder.CreateInvalid(LibraryReadInputs.ParserInvalid(parse));
        }
        else
        {
            var bound = LibraryInspectRequestBinder.Bind(parse, LibraryReadInputs.Invocation(), symbols);
            Assert.Null(bound.Request);
            result = Assert.IsType<LibraryInspectResult>(bound.InvalidResult);
        }
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(".agents/open-forge.lock.json", result.Result.Record.Path);
        Assert.Empty(result.Result.Record.RegisteredPaths);
        Assert.Empty(result.Result.Source.EligiblePaths);
        Assert.Empty(result.Result.Projection.Comparisons);
        Assert.Equal(LibraryInventoryViewState.NotStarted, result.Result.Source.State);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.InvalidId);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Library Inspect workspace selection failures name the ownership lock"),
     InlineData(false), InlineData(true), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void WorkspaceSelectionFailuresNameOwnershipLock(bool blocked)
    {
        var symbols = LibraryInspectBinding.CreateSymbols(LibraryBinding.CreateGroup());
        string[] arguments = ["team"];
        var parse = new CliBindingParse(symbols.Command.Parse(arguments), arguments);
        var input = LibraryReadInputs.Invalid(parse,
            new CliInvalidInput("workspace.unavailable", CliInvalidInputSource.Workspace,
                ["The workspace cannot be selected."])) with
        {
            WorkspaceSelectionState = blocked ? CliWorkspaceSelectionState.Unsafe : CliWorkspaceSelectionState.Missing,
        };
        var result = LibraryInspectRequestBinder.CreateInvalid(input);

        Assert.Equal(blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(".agents/open-forge.lock.json", result.Result.Record.Path);
        Assert.Equal(".agents/open-forge.lock.json", Assert.Single(result.Result.Findings).Path);
    }
}
