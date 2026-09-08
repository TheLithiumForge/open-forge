using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class LibraryDoctorContributorIntegrationTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordHasEmptyCompleteLibraryCoverage()
    {
        using var workspace = new LibraryMutationWorkspace();
        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(OperationalViewState.Complete, view.State);
        Assert.Equal(LibrariesRecordReadState.Missing, view.Record.State);
        Assert.Empty(view.Inventories);
        Assert.Empty(view.Mappings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EveryRegisteredSourceReceivesItsOwnCompleteInventory()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Write("shared/second/.agents/guidance/note.md", "Second source.");
        workspace.Write("unregistered/.agents/directives/hidden.md", "Not a registered source root.");
        workspace.Write(LibraryMutationWorkspace.RecordPath, """
            {"schemaVersion":1,"libraries":[
              {"id":"alpha","sourceRoot":"shared/team-knowledge","paths":[]},
              {"id":"beta","sourceRoot":"shared/second","paths":[]}
            ]}
            """);
        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(2, view.Inventories.Length);
        Assert.Equal(["shared/team-knowledge", "shared/second"], view.Inventories.Select(inventory => inventory.Source.Request.SourceRoot.Value));
        Assert.All(view.Inventories, inventory => Assert.Equal(LibraryInventoryState.Complete, Assert.IsType<LibraryInventory>(inventory.Inventory).State));
        Assert.Equal([LibraryMutationWorkspace.Leaf, ".agents/guidance/note.md"],
            view.Inventories.Select(inventory => Assert.Single(Assert.IsType<LibraryInventory>(inventory.Inventory).Entries).SourcePath.Value));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingSourceIsUnavailableCoverageInsteadOfAnEmptyCompleteInventory()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        var before = workspace.Snapshot();
        var view = await new LibraryOperationalContributor().ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(OperationalViewState.Incomplete, view.State);
        var inventory = Assert.Single(view.Inventories);
        Assert.NotEqual(LibrarySourceRootState.Available, inventory.Source.State);
        if (inventory.Inventory is { } observed)
        {
            Assert.NotEqual(LibraryInventoryState.Complete, observed.State);
        }

        Assert.Equal(before, workspace.Snapshot());
    }
}
