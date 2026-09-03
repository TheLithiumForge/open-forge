using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveDiagnosticEscapingTests
{
    [Fact(DisplayName = "Route Move diagnostics retain whole bounded finding causes")]
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

        var diagnostic = RouteMoveDiagnosticRenderer.Render(
            new CliPresentationRequest<RouteMoveResult>(
                result,
                new CliPresentation(
                    CliOutputFormat.Human,
                    CliView.Compact,
                    CliVerbosity.Verbose)));

        Assert.NotNull(diagnostic);
        Assert.Contains($"cause={prefix}...", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain($"cause={prefix}\\", diagnostic, StringComparison.Ordinal);
    }
}
