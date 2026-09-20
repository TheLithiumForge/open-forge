using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncGeneratedRegionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task OnlyExistingUnambiguousConsumerRegionCanBeProjected(bool validRegion)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Record();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            """;
        if (!validRegion)
        {
            parent = parent.Replace("## Entries", "## Entries\n\n## Entries", StringComparison.Ordinal);
        }

        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(), TestContext.Current.CancellationToken);
        Assert.Equal(validRegion ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        if (validRegion)
        {
            Assert.Equal(".agents/directives/_directives.md", Assert.Single(result.Result.Plan.GeneratedRegions).Path);
        }

        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task FollowingAuthoredSectionRemainsOutsidePlannedBody()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Record();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries

            ## Notes

            Authored suffix.
            """;
        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();

        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(result.Result.Plan.GeneratedRegions);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }
}
