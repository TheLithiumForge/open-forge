using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

public sealed class LibraryDetachOperationIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("file"), InlineData("directory"), InlineData("different-link"), InlineData("absolute-link")]
    public static async Task EveryNonExactOccupantRefusesAllEffects(string occupant)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        switch (occupant)
        {
            case "file": workspace.Write(LibraryMutationWorkspace.Leaf, "Local owner."); break;
            case "directory": workspace.Directory(LibraryMutationWorkspace.Leaf); break;
            case "different-link": workspace.Link(rawTarget: "../../another-source.md"); break;
            case "absolute-link": workspace.Link(rawTarget: workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")); break;
            default: throw new ArgumentOutOfRangeException(nameof(occupant));
        }

        var before = workspace.Snapshot();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingSourceRootDoesNotPreventExactDanglingPlan()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        System.IO.Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot));
        var before = workspace.Snapshot();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Result.Identity.SourceIndependent);
        Assert.Equal(LibraryLinkEffectKind.Delete, Assert.Single(result.Result.Plan.Links).Kind);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRegisteredLeafBlocksWholeDetach()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task UnregisteredMatchingLinkRemainsOutsideDeletionPlan()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Link(".agents/directives/local.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryMutationWorkspace.Leaf, Assert.Single(result.Result.Plan.Links).Path);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContainedDirectoryLinkAncestryStillBlocksLibraryMutation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/local");
        workspace.Link(".agents/local/review.md", "../../shared/team-knowledge/.agents/directives/review.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.DirectoryLink(".agents/directives", "local");
        var before = workspace.Snapshot();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationReturnsInterruptedWithoutEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var result = await LibraryDetachOperation.ExecuteAsync(workspace.Detach(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }
}
