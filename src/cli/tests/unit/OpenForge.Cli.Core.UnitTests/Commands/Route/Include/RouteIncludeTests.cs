using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Shared.Binding;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Include;

public sealed class RouteIncludeTests
{
    [Fact(DisplayName = "RouteInclude binds one file and a dry-run"), Trait("Feature", "route-include"), Trait("Evidence", "Unit")]
    public void BindsOneFileAndDryRun()
    {
        var group = RouteBinding.CreateGroup();
        var symbols = RouteIncludeBinding.CreateSymbols(group);
        var result = RouteIncludeRequestBinder.Bind(group.Parse(["include", "./docs/team.md", "--dry-run", "--tag", "#LoadNow", "--tag", "Docs", "--apply-to", "src/**,docs/**,src/**", "--description", "Team guide"]),
            RouteInclusionTestData.Invocation(), symbols);
        Assert.Null(result.InvalidResult);
        Assert.NotNull(result.Request);
        var request = result.Request;
        Assert.Equal("./docs/team.md", request.File);
        Assert.True(request.DryRun);
        Assert.Equal(["LoadNow", "Docs"], request.Tags?.ToArray() ?? []);
        Assert.Equal(["docs/**", "src/**"], request.ApplyTo?.Select(pattern => pattern.Text));
    }

    [Theory(DisplayName = "RouteInclude rejects invalid argument combinations"), Trait("Feature", "route-include"), Trait("Evidence", "Unit")]
    [InlineData("")]
    [InlineData("one.md two.md")]
    [InlineData("one.md --tag=")]
    [InlineData("one.md --tag=Docs --tag=#Docs")]
    [InlineData("one.md --description=first --description=second")]
    [InlineData("one.md --apply-to=../src/**")]
    [InlineData("one.md --description=")]
    public void RejectsInvalidInput(string input)
    {
        var group = RouteBinding.CreateGroup();
        var symbols = RouteIncludeBinding.CreateSymbols(group);
        var result = RouteIncludeRequestBinder.Bind(group.Parse(("include " + input).Split(' ', StringSplitOptions.RemoveEmptyEntries)),
            RouteInclusionTestData.Invocation(), symbols);
        Assert.Null(result.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.InvalidResult?.Status);
    }
}
