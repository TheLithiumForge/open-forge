using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachGeneratedRegionIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task OnlyExistingUnambiguousConsumerRegionCanBeProjected(bool validRegion)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            <!-- open-forge:generated-index:start -->

            <!-- open-forge:generated-index:end -->
            """;
        if (!validRegion)
        {
            parent = parent.Replace("<!-- open-forge:generated-index:end -->", string.Empty, StringComparison.Ordinal);
        }

        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(validRegion ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        if (validRegion)
        {
            Assert.Equal(".agents/directives/_directives.md", Assert.Single(result.Result.Plan.GeneratedRegions).Path);
        }

        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task AuthoredSuffixAfterGeneratedRegionBlocksWithoutEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            <!-- open-forge:generated-index:start -->

            <!-- open-forge:generated-index:end -->
            Authored suffix.
            """;
        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Result.Plan.GeneratedRegions);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }
}
