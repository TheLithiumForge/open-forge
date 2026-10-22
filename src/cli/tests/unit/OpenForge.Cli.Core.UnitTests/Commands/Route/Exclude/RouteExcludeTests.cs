using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Shared.Binding;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Exclude;

public sealed class RouteExcludeTests
{
    [Fact(DisplayName = "RouteExclude binds one file and a dry-run"), Trait("Feature", "route-exclude"), Trait("Evidence", "Unit")]
    public void BindsOneFileAndDryRun()
    {
        var group = RouteBinding.CreateGroup();
        var symbols = RouteExcludeBinding.CreateSymbols(group);
        var result = RouteExcludeRequestBinder.Bind(group.Parse(["exclude", "./docs/team.md", "--dry-run"]),
            RouteInclusionTestData.Invocation(), symbols);
        Assert.Null(result.InvalidResult);
        Assert.NotNull(result.Request);
        var request = result.Request;
        Assert.Equal("./docs/team.md", request.File);
        Assert.True(request.DryRun);
    }

    [Theory(DisplayName = "RouteExclude rejects invalid argument combinations"), Trait("Feature", "route-exclude"), Trait("Evidence", "Unit")]
    [InlineData("")]
    [InlineData("one.md two.md")]
    [InlineData("one.md --description=value")]
    [InlineData("one.md --tag=Docs")]
    public void RejectsInvalidInput(string input)
    {
        var group = RouteBinding.CreateGroup();
        var symbols = RouteExcludeBinding.CreateSymbols(group);
        var result = RouteExcludeRequestBinder.Bind(group.Parse(("exclude " + input).Split(' ', StringSplitOptions.RemoveEmptyEntries)),
            RouteInclusionTestData.Invocation(), symbols);
        Assert.Null(result.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.InvalidResult?.Status);
    }
}
