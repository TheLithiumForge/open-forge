using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational;

[Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
public sealed class LibraryOperationalContributorIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library producer distinguishes safely absent and strict empty records with complete zero-library coverage")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public static async Task EmptyCoverage(bool doctor, bool recordPresent)
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.Source("unregistered");
        if (recordPresent)
        {
            fixture.Write(LibraryObservationWorkspace.RecordPath, "{\"schemaVersion\":1,\"libraries\":[]}");
        }
        var before = fixture.Snapshot();
        var contributor = new LibraryOperationalContributor();

        LibraryRegistrationRead record;
        if (doctor)
        {
            var view = await contributor.ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken);
            Assert.Equal(OperationalViewState.Complete, view.State);
            Assert.Empty(view.Inventories);
            Assert.Empty(view.Mappings);
            record = view.Record;
        }
        else
        {
            var view = await contributor.ReadStatusAsync(fixture.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), fixture.Workspace, CancellationToken.None), TestContext.Current.CancellationToken);
            Assert.Equal(OperationalViewState.Complete, view.State);
            Assert.Empty(view.Sources);
            Assert.Empty(view.Mappings);
            record = view.Record;
        }

        Assert.Equal(recordPresent ? LibraryRegistrationReadState.Complete : LibraryRegistrationReadState.Missing, record.State);
        Assert.Equal(fixture.PathFor(".agents/open-forge.lock.json"), Assert.IsType<FileStateSnapshot>(record.Snapshot).LogicalPath);
        if (recordPresent)
        {
            Assert.Empty(Assert.IsType<LibraryRegistrationSet>(record.Record).Libraries);
        }
        else
        {
            Assert.Null(record.Record);
        }
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Status observes every registered root and mapping without adopting additions or unregistered sources")]
    public static async Task StatusRemainsBoundedToRegistration()
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.ThreeLibraries("alpha");
        fixture.Write("shared/beta/.agents/addition.md", "unregistered addition\n");
        var before = fixture.Snapshot();

        var view = await new LibraryOperationalContributor().ReadStatusAsync(fixture.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), fixture.Workspace, CancellationToken.None), TestContext.Current.CancellationToken);

        Assert.Equal(["shared/alpha", "shared/beta", "shared/gamma"], view.Sources.Select(source => source.Request.SourceRoot.Value));
        Assert.Equal([".agents/alpha.md", ".agents/beta.md", ".agents/gamma.md"], view.Mappings.Select(mapping => mapping.LogicalDestinationPath));
        Assert.NotEqual(LibrarySourceRootState.Available, view.Sources[0].State);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library Doctor attempts the entire declared source coverage despite a missing first or last source")]
    [InlineData("alpha"), InlineData("gamma")]
    public static async Task DoctorRetainsIncompleteRootInCoverage(string missingSource)
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.ThreeLibraries(missingSource);
        var before = fixture.Snapshot();

        var view = await new LibraryOperationalContributor().ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken);

        Assert.Equal(["shared/alpha", "shared/beta", "shared/gamma"], view.Inventories.Select(read => read.Source.Request.SourceRoot.Value));
        var missing = Assert.Single(view.Inventories, read => read.Source.Request.SourceRoot.Value == $"shared/{missingSource}");
        Assert.Null(missing.Inventory);
        Assert.NotEqual(LibrarySourceRootState.Available, missing.Source.State);
        Assert.All(view.Inventories.Where(read => read != missing), read =>
            Assert.Equal(LibraryInventoryState.Complete, Assert.IsType<LibraryInventory>(read.Inventory).State));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Doctor inventories additions in every registered source and excludes an unregistered source")]
    public static async Task DoctorCompletesAllRegisteredInventories()
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.ThreeLibraries();
        fixture.Write("shared/beta/.agents/addition.md", "eligible addition\n");
        var before = fixture.Snapshot();

        var view = await new LibraryOperationalContributor().ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken);

        Assert.Equal(["shared/alpha", "shared/beta", "shared/gamma"], view.Inventories.Select(read => read.Source.Request.SourceRoot.Value));
        Assert.All(view.Inventories, read => Assert.Equal(LibraryInventoryState.Complete, Assert.IsType<LibraryInventory>(read.Inventory).State));
        Assert.Equal([".agents/addition.md", ".agents/beta.md"], Assert.IsType<LibraryInventory>(view.Inventories[1].Inventory).Entries.Select(entry => entry.SourcePath.Value));
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Missing lifecycle ownership does not fabricate unsupported Library capability or discover unrelated links")]
    [InlineData(false), InlineData(true)]
    public static async Task OwnershipAndCapabilityAreIndependent(bool doctor)
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.ThreeLibraries();
        fixture.Files.CreateFileSymbolicLink("workspace/capability-link", "sibling.txt");
        var before = fixture.Snapshot();
        var contributor = new LibraryOperationalContributor();
        WorkspaceOwnershipRead? ownership;
        LibraryLinkCapabilityFact? capability;

        if (doctor)
        {
            var view = await contributor.ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken);
            ownership = view.Ownership;
            capability = view.LinkCapability;
        }
        else
        {
            var view = await contributor.ReadStatusAsync(fixture.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), fixture.Workspace, CancellationToken.None), TestContext.Current.CancellationToken);
            ownership = view.Ownership;
            capability = view.LinkCapability;
        }

        Assert.NotNull(ownership);
        Assert.Equal(WorkspaceOwnershipReadState.Complete, ownership.State);
        Assert.Empty(ownership.Document.ManagedPaths());
        if (capability is not null)
        {
            Assert.Equal(LibraryLinkCapabilityState.Supported, capability.State);
            Assert.False(string.IsNullOrWhiteSpace(capability.Evidence));
        }
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library producer reads trusted lifecycle ownership independently from Library record membership")]
    [InlineData(false), InlineData(true)]
    public static async Task ReadsIndependentTrustedLifecycleClaims(bool doctor)
    {
        using var fixture = new LibraryObservationWorkspace();
        fixture.ThreeLibraries();
        fixture.TrustedLifecycle();
        var before = fixture.Snapshot();
        var contributor = new LibraryOperationalContributor();

        var ownership = doctor
            ? (await contributor.ReadDoctorAsync(fixture.Workspace, TestContext.Current.CancellationToken)).Ownership
            : (await contributor.ReadStatusAsync(fixture.Workspace,
            await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), fixture.Workspace, CancellationToken.None), TestContext.Current.CancellationToken)).Ownership;

        Assert.NotNull(ownership);
        Assert.Equal(WorkspaceOwnershipReadState.Complete, ownership.State);
        var claim = Assert.Single(ownership.Document.ManagedPaths());
        Assert.Equal(".agents/framework-owned.md", claim.Path);
        Assert.Equal(OwnedPathManager.Framework, claim.Manager);
        Assert.Equal("open-forge", claim.Owner);
        Assert.Equal(fixture.PathFor(".agents/open-forge.lock.json"), ownership.Snapshot?.Expectation?.LogicalPath);
        Assert.Equal(before, fixture.Snapshot());
    }
}
