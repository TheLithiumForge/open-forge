using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class LibraryStatusContributorIntegrationTests
{
    [Trait("Boundary", "OS")]
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
        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, CancellationToken.None), TestContext.Current.CancellationToken);
        Assert.Equal(LibraryRegistrationReadState.Complete, view.Record.State);
        Assert.Single(view.Sources);
        var mapping = Assert.Single(view.Mappings);
        Assert.Equal(LibraryMutationWorkspace.Leaf, mapping.Mapping.DestinationPath.Value);
        Assert.Equal(linked ? LibraryMappingObservationState.Current : LibraryMappingObservationState.Missing, mapping.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordNeverAdoptsAnExactLookingLink()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, CancellationToken.None), TestContext.Current.CancellationToken);
        Assert.Equal(OperationalViewState.Complete, view.State);
        Assert.Equal(LibraryRegistrationReadState.Missing, view.Record.State);
        Assert.Empty(view.Sources);
        Assert.Empty(view.Mappings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContributorLocalCancellationRemainsInterrupted()
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var view = await new LibraryOperationalContributor().ReadStatusAsync(workspace.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, CancellationToken.None), cancellation.Token);

        Assert.Equal(OperationalViewState.Interrupted, view.State);
        Assert.Equal(LibraryRegistrationReadState.Unavailable, view.Record.State);
        Assert.Equal(before, workspace.Snapshot());
    }
}
