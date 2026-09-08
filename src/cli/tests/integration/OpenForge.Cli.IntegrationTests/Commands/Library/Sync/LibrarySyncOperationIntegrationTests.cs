using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncOperationIntegrationTests
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
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task UnavailableSourceNeverRetiresProjection()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        var before = workspace.Snapshot();
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRetiredProjectionBlocksCompleteSourceInventory()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task SourceByteChangesDoNotReplaceLiveProjection()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        File.WriteAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}"), "Changed source bytes.");
        var before = workspace.Snapshot();
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryApplicationState.NoOp, result.Result.Application.State);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(LibraryRecordEffect.None, result.Result.Plan.RecordEffect);
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
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
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
        var result = await LibrarySyncOperation.ExecuteAsync(workspace.Sync(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EligibleJsonIsAddedAndRecordedWithoutGeneratedNavigation()
    {
        const string jsonPath = ".agents/resources/data.json";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source(jsonPath);
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Directory(".agents/resources");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{jsonPath}");
        var sourceBytes = File.ReadAllBytes(sourcePath);

        try
        {
            var result = await LibrarySyncOperation.ExecuteAsync(
                workspace.Sync(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Contains(result.Result.Source.EligiblePaths, path => path.SourcePath == jsonPath);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Plan.Links).Path);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.Equal(LibraryRecordEffect.Replace, result.Result.Plan.RecordEffect);
            Assert.Contains(jsonPath, Assert.Single(Assert.IsType<LibrariesRecordDocument>(
                result.Result.Record.Intended).Libraries).Paths);
            Assert.NotNull(new FileInfo(workspace.Absolute(jsonPath)).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        }
        finally
        {
            File.Delete(workspace.Absolute(jsonPath));
        }
    }
}
