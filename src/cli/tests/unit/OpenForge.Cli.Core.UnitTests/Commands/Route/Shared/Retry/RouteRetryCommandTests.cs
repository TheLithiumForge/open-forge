using OpenForge.Cli.Core.Commands.Route.Shared.Retry;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Retry;

public sealed class RouteRetryCommandTests
{
    [Fact(DisplayName = "Route retry commands preserve every original argument and quote values")]
    [Trait("Boundary", "Output"), Trait("Feature", "route-mutation"), Trait("Evidence", "Unit")]
    public void OriginalRequestIsComplete()
    {
        var command = RouteRetryCommand.FromArguments([
            "route", "update", "guidance/note", "--description", "Revised note", "--responsibility", "",
            "--tag", "Review", "--tag", "CurrentTruth", "--apply-to", "src/**/*.cs", "--workspace", "team workspace",
            "--dry-run", "--format", "json", "--detail", "full", "--automatic",
        ]);
        CommandOutputSnapshot.MatchSnapshot(command, "complete-request");
    }

    [Fact(DisplayName = "Only missing confirmation adds automatic mode to a Route retry")]
    [Trait("Boundary", "Processing"), Trait("Feature", "route-mutation"), Trait("Evidence", "Unit")]
    public void RetryAddsAuthorityOnlyForConfirmation()
    {
        const string request = "open-forge route remove guidance/note --workspace 'team workspace'";
        var next = new CliNextAction("open-forge route remove", "Repeat the request.");
        Assert.Equal(request, RouteRetryCommand.Complete(next, "open-forge route remove", request)?.Command);
        Assert.Equal($"{request} --automatic", RouteRetryCommand.Complete(next, "open-forge route remove", request, confirmationRequired: true)?.Command);
        var cleanup = new CliNextAction("open-forge cleanup", "Review recovery.");
        Assert.Same(cleanup, RouteRetryCommand.Complete(cleanup, "open-forge route remove", request));
    }

}
