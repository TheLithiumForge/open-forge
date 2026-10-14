using OpenForge.Cli.Core.Presentation.Route.Shared.NextActions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Shared.NextActions;

public sealed class RouteNextCommandTests
{
    [Theory(DisplayName = "Route follow-up commands retain an explicitly selected workspace")]
    [InlineData("open-forge doctor"), InlineData("open-forge cleanup"), InlineData("open-forge cleanup --dry-run")]
    [InlineData("open-forge route list --depth=all"), InlineData("open-forge find --tag Template")]
    [InlineData("open-forge route init 'guidance/team notes'")]
    [InlineData("open-forge update"), InlineData("open-forge extension remove planning")]
    [Trait("Feature", "route-mutation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void FollowUpKeepsExplicitWorkspace(string command)
    {
        var next = new CliNextAction(command, "Review the selected workspace.");
        var scoped = RouteNextCommand.InWorkspace(next, "workspace folder", explicitWorkspace: true);
        Assert.Equal($"{command} --workspace 'workspace folder'", scoped?.Command);
        Assert.Equal(next.Reason, scoped?.Reason);
        Assert.DoesNotContain("--automatic", scoped?.Command, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Complete retries and help retain their existing request")]
    [InlineData("open-forge route update guidance/note --description 'Revised note' --workspace 'workspace folder'")]
    [InlineData("open-forge route remove guidance/note --workspace 'workspace folder' --automatic")]
    [InlineData("open-forge route update --help")]
    [Trait("Feature", "route-mutation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void CompleteRequestIsNotDuplicated(string command)
    {
        var next = new CliNextAction(command, "Repeat the request.");
        Assert.Same(next, RouteNextCommand.InWorkspace(next, "workspace folder", explicitWorkspace: true));
        Assert.Same(next, RouteNextCommand.InWorkspace(next, "workspace folder", explicitWorkspace: false));
    }
}
