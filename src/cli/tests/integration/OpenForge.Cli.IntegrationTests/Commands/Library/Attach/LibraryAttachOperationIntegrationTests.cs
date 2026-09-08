using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachOperationIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("file"), InlineData("directory"), InlineData("different-link"), InlineData("absolute-link")]
    public static async Task EveryNonExactOccupantRefusesAllEffects(string occupant)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        switch (occupant)
        {
            case "file": workspace.Write(LibraryMutationWorkspace.Leaf, "Local owner."); break;
            case "directory": workspace.Directory(LibraryMutationWorkspace.Leaf); break;
            case "different-link": workspace.Link(rawTarget: "../../another-source.md"); break;
            case "absolute-link": workspace.Link(rawTarget: workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")); break;
            default: throw new ArgumentOutOfRangeException(nameof(occupant));
        }

        var before = workspace.Snapshot();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingOrdinarySourceAgentsIsInvalid()
    {
        using var workspace = new LibraryMutationWorkspace();
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        var before = workspace.Snapshot();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task SourceControlsAndLinksNeverBecomeProjections()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/loader.md", "# Loader");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/directives/_directives.md", "# Directives");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/directives/review.overwrite.md", "# Private override");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/open-forge.lifecycle.json", "{}");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/open-forge.libraries.json", "{}");
        workspace.Link($"{LibraryMutationWorkspace.SourceRoot}/.agents/linked.md", "directives/review.md");
        var before = workspace.Snapshot();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryMutationWorkspace.Leaf, Assert.Single(result.Result.Source.EligiblePaths).SourcePath);
        Assert.Equal(6, result.Result.Source.ExcludedPaths.Length);
        Assert.Empty(result.Result.Plan.GeneratedRegions);
        Assert.False(File.Exists(workspace.Absolute(".agents/loader.md")));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task DryRunIncludesParentsAndPreservesLocalSibling()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source(".agents/guidance/nested/new.md");
        workspace.Write(".agents/directives/local.md", "Local sibling.");
        var before = workspace.Snapshot();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, result.Result.Plan.Links.Length);
        Assert.Contains(result.Result.Plan.Directories, directory => directory.Path == ".agents/guidance/nested");
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContainedDirectoryLinkAncestryStillBlocksLibraryMutation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/local");
        workspace.DirectoryLink(".agents/directives", "local");
        var before = workspace.Snapshot();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationReturnsInterruptedWithoutEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var result = await LibraryAttachOperation.ExecuteAsync(workspace.Attach(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EligibleJsonIsLinkedAndRecordedWithoutGeneratedNavigation()
    {
        const string jsonPath = ".agents/resources/data.json";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(jsonPath);
        workspace.Directory(".agents/resources");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{jsonPath}");
        var sourceBytes = File.ReadAllBytes(sourcePath);

        try
        {
            var result = await LibraryAttachOperation.ExecuteAsync(
                workspace.Attach(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Source.EligiblePaths).SourcePath);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Plan.Links).Path);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.Equal(LibraryRecordEffect.Create, result.Result.Plan.RecordEffect);
            Assert.Contains(jsonPath, Assert.Single(Assert.IsType<LibrariesRecordDocument>(
                result.Result.Record.Intended).Libraries).Paths);
            Assert.NotNull(new FileInfo(workspace.Absolute(jsonPath)).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        }
        finally
        {
            File.Delete(workspace.Absolute(jsonPath));
            File.Delete(workspace.Absolute(LibraryMutationWorkspace.RecordPath));
        }
    }
}
