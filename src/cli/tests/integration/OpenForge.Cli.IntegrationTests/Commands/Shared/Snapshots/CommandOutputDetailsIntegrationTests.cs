using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Index;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class CommandOutputDetailsIntegrationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Shared output capture preserves all details for a composed Index command")]
    public async Task IndexCapturesAllDetails()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        using var healthyWorkspace = IndexOperationWorkspace.Create("shared-output-details-healthy");
        await new ReadCommandOutputCapture(healthyWorkspace.Workspace.PhysicalRoot)
            .MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = "index-details-healthy",
                Arguments = ["index", IndexOperationWorkspace.RootPath, "--dry-run"],
                ExitCode = 0,
            }, snapshotCollector: snapshots);

        using var invalidWorkspace = IndexOperationWorkspace.Create("shared-output-details-invalid");
        await new ReadCommandOutputCapture(invalidWorkspace.Workspace.PhysicalRoot)
            .MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = "index-details-invalid",
                Arguments = ["index", "missing"],
                ExitCode = 4,
            }, snapshotCollector: snapshots);

        using var renderedWorkspace = IndexOperationWorkspace.Create("shared-output-details-renderer");
        var result = await IndexOperationFactory.Create(renderedWorkspace.LockStoreRoot).ExecuteAsync(
            new IndexRequest(renderedWorkspace.Workspace, ["missing"], IndexMode.Apply),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        CommandOutputRenderers<IndexResult>.From(IndexPresentation.Rendering)
            .MatchDetails(result, "index-details-renderer", snapshotCollector: snapshots);
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
