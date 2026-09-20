using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class LibraryProjectionUpdateGuardIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task ProjectionLeafCannotBeMutatedEvenWithoutRegistration(bool registered)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Link();
        if (registered)
        {
            workspace.Record(LibraryMutationWorkspace.Leaf);
        }

        var before = workspace.Snapshot();
        var result = await CliHostCapture.RunAsync(["route", "update", "directives/review", "--description", "Changed"], workspace.Path);
        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md",
            new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
    }
}
