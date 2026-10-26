using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Planning;

[Trait("Feature", "library-mutation"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class LibraryLayoutPlanningTests
{
    [Theory]
    [InlineData((int)WorkspaceLayoutReadState.Mismatch)]
    [InlineData((int)WorkspaceLayoutReadState.Invalid)]
    [InlineData((int)WorkspaceLayoutReadState.Unavailable)]
    public void UnsafeLayoutBlocksEveryMutationBeforeEffects(int state)
    {
        var ready = LibraryMutationPlanningData.LayoutObservation();
        var observation = new WorkspaceLayoutRead((WorkspaceLayoutReadState)state,
            ready.Settings, ready.Ownership, ready.Layout, [], []);
        var attach = LibraryAttachPlanner.Plan(LibraryMutationPlanningData.Attach() with { LayoutRead = observation }, TestContext.Current.CancellationToken);
        var sync = LibrarySyncPlanner.Plan(LibraryMutationPlanningData.Sync([], []) with { LayoutRead = observation }, TestContext.Current.CancellationToken);
        var detach = LibraryDetachPlanner.Plan(LibraryMutationPlanningData.Detach() with { LayoutRead = observation }, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, attach.State);
        Assert.Equal(LibraryPlanState.Blocked, sync.State);
        Assert.Equal(LibraryPlanState.Blocked, detach.State);
        Assert.Same(observation, attach.LayoutRead);
        Assert.Same(observation, sync.LayoutRead);
        Assert.Same(observation, detach.LayoutRead);
        Assert.Empty(attach.Links);
        Assert.Empty(sync.Links);
        Assert.Empty(detach.Links);
        Assert.Null(attach.OwnershipChange);
        Assert.Null(sync.OwnershipChange);
        Assert.Null(detach.OwnershipChange);
    }
}
