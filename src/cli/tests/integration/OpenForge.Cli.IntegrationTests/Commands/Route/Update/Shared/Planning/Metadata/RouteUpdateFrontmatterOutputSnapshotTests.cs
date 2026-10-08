using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Shared.Planning.Metadata;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteUpdateFrontmatterOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RouteUpdateResult> Renderers = CommandOutputRenderers<RouteUpdateResult>.From(RouteUpdatePresentation.Rendering);

    [Fact(DisplayName = "Authored root and created root or scoped metadata have exact previews"), Trait("Boundary", "Output")]
    public async Task AuthoredAndCreatedFormsHaveExactPreviews()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var scenario in new[] { "root-edit", "root-create", "scoped-create" })
        {
            using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-frontmatter-output");
            var source = scenario == "root-edit"
                ? "---\ndescription: Before overview\ntags: [Memory]\n---\n# Exact body\n"
                : "# Exact body\n";
            workspace.SeedTargetText(source);
            var form = scenario == "scoped-create" ? "scoped" : "root";
            workspace.SeedSettingsText($"{{\"schemaVersion\":1,\"frontmatter\":\"{form}\"}}");
            var before = workspace.SnapshotHashes();
            var result = await workspace.ExecuteAsync(workspace.Request(
                patch: RouteUpdateFrontmatterIntegrationTests.CompletePatch(), mode: RouteUpdateMode.DryRun),
                TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            Renderers.MatchDetails(result, scenario, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
