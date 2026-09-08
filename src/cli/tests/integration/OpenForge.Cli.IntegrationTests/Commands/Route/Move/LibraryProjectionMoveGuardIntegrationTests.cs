using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class LibraryProjectionMoveGuardIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task ProjectionLeafCannotBeMutatedEvenWithoutRegistration(bool registered)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        LibraryMutationApplicationData.Lifecycle(workspace);
        workspace.RoutedSource();
        workspace.Link();
        if (registered)
        {
            workspace.Record(LibraryMutationWorkspace.Leaf);
        }

        var ownership = await new LifecycleOwnershipReader(new PhysicalPathResolver())
            .ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.True(ownership.Framework.State == LifecycleOwnershipReadState.Trusted
            && ownership.Extensions.State == LifecycleOwnershipReadState.Trusted,
            $"Required trusted lifecycle fixture failed before link-guard assertion: {string.Join("; ", ownership.Findings.Select(finding => finding.Cause))}");
        var claim = Assert.Single(ownership.Claims);
        Assert.Equal(LibraryMutationApplicationData.ManagedPath, claim.Path);
        Assert.Equal(LifecycleOwnershipManager.Framework, claim.Manager);
        Assert.Equal("open-forge", claim.Owner);
        var before = workspace.Snapshot();
        var result = await CliHostCapture.RunAsync(["route", "move", "directives/review", ".agents/directives/moved.md", "--json"], workspace.Path);
        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() is "route-move.source-unsafe" or "route-move.reference-unsafe");
        Assert.Equal("blocked", finding.GetProperty("status").GetString());
        Assert.Equal(LibraryMutationWorkspace.Leaf, finding.GetProperty("target").GetString());
        Assert.False(string.IsNullOrWhiteSpace(finding.GetProperty("cause").GetString()));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md",
            new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
    }
}
