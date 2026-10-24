using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Move;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class RouteMoveMainRootPresentationTests
{
    private const string MainRoot = "handbook/context";
    private const string Destination = MainRoot + "/" + RouteMoveTestData.DestinationId;
    private const string Cause = "Independent destination boundary.";

    [Fact(DisplayName = "Route Move custom-root guidance uses projected roots and parent references")]
    public void CustomRootGuidanceUsesProjectedFacts()
    {
        var captures = new Dictionary<string, string>();
        foreach (var code in new[] { RouteMoveFindingCode.InvalidDestination, RouteMoveFindingCode.DestinationParentMissing })
        {
            var status = code == RouteMoveFindingCode.InvalidDestination ? CliSemanticStatus.Invalid : CliSemanticStatus.Blocked;
            var formation = RouteMoveTestData.Formation(RouteMoveMode.DryRun,
                new RouteMoveFinding(code, status, Destination, Cause)) with
            {
                Workspace = null,
                Destination = new RouteMoveDestination
                {
                    Requested = Destination,
                    CustomMainRoot = MainRoot,
                    SuggestedParentReference = RouteMoveTestData.ParentId,
                },
                Subject = new RouteMoveSubject { Kind = RouteMoveSubjectKind.Leaf },
                UnchangedPaths = [],
            };
            var result = new RouteMoveResultBuilder().Build(formation);
            Assert.Equal(status, result.Status);
            var output = CliRenderingStage.Render(
                new CliPresentationRequest<RouteMoveResult>(result, new CliPresentation(CliFormat.Text, CliDetail.Standard, null)),
                RouteMovePresentation.Rendering);
            captures.Add(code.ToString(), output.PrimaryContent);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }
}
