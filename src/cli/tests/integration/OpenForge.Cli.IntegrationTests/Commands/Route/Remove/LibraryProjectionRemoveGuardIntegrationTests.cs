using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class LibraryProjectionRemoveGuardIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task ProjectionLeafCannotBeMutatedEvenWithoutRegistration(bool registered)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        LibraryMutationApplicationData.FrameworkFile(workspace);
        workspace.RoutedSource();
        workspace.Link();
        if (registered)
        {
            workspace.Record(LibraryMutationWorkspace.Leaf);
        }

        Assert.True(File.Exists(workspace.Absolute(LibraryMutationApplicationData.ManagedPath)));
        Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationApplicationData.ManagedPath)).LinkTarget);
        var before = workspace.Snapshot();
        var result = await CliHostCapture.RunAsync(["route", "remove", "directives/review", "--format", "json"], workspace.Path);
        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() is "route-remove.source-unsafe" or "route-remove.reference-unsafe");
        Assert.Equal("file", finding.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Equal(LibraryMutationWorkspace.Leaf, finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.False(string.IsNullOrWhiteSpace(finding.GetProperty("message").GetString()));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md",
            new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
    }
}
