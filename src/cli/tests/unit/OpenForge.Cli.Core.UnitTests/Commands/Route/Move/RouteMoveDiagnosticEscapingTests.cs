using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveDiagnosticEscapingTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Move diagnostic finding rows retain identity and clamp the cause to the row limit")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void DiagnosticRendererRetainsWholeBoundedFindingCause()
    {
        var prefix = new string('a', 236);
        var finding = new RouteMoveFinding(
            RouteMoveFindingCode.RecoveryArtifactRetained,
            CliSemanticStatus.Attention,
            target: null,
            $"{prefix}\\tail");
        var result = new RouteMoveResult(
            RouteMoveTestData.Formation(RouteMoveMode.Apply, finding),
            CliSemanticStatus.Attention,
            next: null);

        var diagnostic = CliRenderingStage.Render(
            new CliPresentationRequest<RouteMoveResult>(
                result,
                new CliPresentation(
                    CliFormat.Text,
                    CliDetail.Debug, null)), RouteMovePresentation.Rendering).DiagnosticContent;

        Assert.NotNull(diagnostic);
        const string findingPrefix = "finding=route-move.recovery-artifact-retained:target=none:cause=";
        var findingLine = Assert.Single(diagnostic.Split('\n'), line => line.StartsWith("finding=", StringComparison.Ordinal));
        Assert.Equal(findingPrefix + new string('a', 237 - findingPrefix.Length) + "...", findingLine);
        Assert.Equal(240, findingLine.Length);
        Assert.DoesNotContain("\\tail", diagnostic, StringComparison.Ordinal);
    }
}
