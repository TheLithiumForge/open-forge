using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class LibraryProjectionIndexGuardIntegrationTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task LinkedGeneratedTargetIsNeverWrittenThrough()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Write(".agents/loader.md", """
            # Loader
            ## Entries
            <!-- open-forge:generated-index:start -->
            - [Directives](directives/_directives.md) - #Directive
            <!-- open-forge:generated-index:end -->
            """);
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/directives/_directives.md", """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Directives
            ## Entries
            <!-- open-forge:generated-index:start -->
            stale generated body
            <!-- open-forge:generated-index:end -->
            """);
        workspace.Link(".agents/directives/_directives.md", "../../shared/team-knowledge/.agents/directives/_directives.md");
        var before = workspace.Snapshot();
        var result = await CliHostCapture.RunAsync(["index", "directives"], workspace.Path);
        Assert.Equal(5, result.ExitCode);
        Assert.Equal(before, workspace.Snapshot());
    }
}
