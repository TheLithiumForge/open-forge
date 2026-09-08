using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class LibraryStatusContributorIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task BoundedViewObservesRegisteredLinksWithoutAddingUnregisteredInventory(bool linked)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source(".agents/directives/unregistered.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        if (linked)
        {
            workspace.Link();
        }

        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(LibrariesRecordReadState.Complete, view.Record.State);
        Assert.Single(view.Sources);
        var mapping = Assert.Single(view.Mappings);
        Assert.Equal(LibraryMutationWorkspace.Leaf, mapping.Mapping.DestinationPath.Value);
        Assert.Equal(linked ? LibraryMappingObservationState.Current : LibraryMappingObservationState.Missing, mapping.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordNeverAdoptsAnExactLookingLink()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(OperationalViewState.Complete, view.State);
        Assert.Equal(LibrariesRecordReadState.Missing, view.Record.State);
        Assert.Empty(view.Sources);
        Assert.Empty(view.Mappings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContributorLocalCancellationRemainsInterrupted()
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace, cancellation.Token);

        Assert.Equal(OperationalViewState.Interrupted, view.State);
        Assert.Equal(LibrariesRecordReadState.Unavailable, view.Record.State);
        Assert.Equal(before, workspace.Snapshot());
    }
}
