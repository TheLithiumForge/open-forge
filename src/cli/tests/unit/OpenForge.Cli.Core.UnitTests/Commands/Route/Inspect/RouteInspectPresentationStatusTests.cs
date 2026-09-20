using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectPresentationStatusTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect interactive attention emits the exact non-interactive Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InteractiveAttentionEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InteractiveAttentionResult(),
            "open-forge route inspect \".agents/root/item/_item.md\"");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect unresolved collision blocked emits the exact listed-path Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnresolvedCollisionEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.BlockedCollisionResult(),
            "open-forge route inspect \".agents/root/collision.md\"");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect invalid result emits the exact correction Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InvalidEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InvalidResult(),
            "open-forge route inspect --help");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect failed result emits the exact bounded-diagnostics Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void FailedEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.FailedResult(),
            "open-forge route inspect");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect interrupted result emits the exact rerun Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InterruptedEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InterruptedResult(),
            "open-forge route inspect");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect incomplete result falls back to the exact doctor Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IncompleteEmitsDoctorNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.IncompleteResult(),
            "open-forge doctor");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect blocked result without a direct correction falls back to the exact doctor Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BlockedWithoutDirectCorrectionEmitsDoctorNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.FallbackBlockedResult(),
            "open-forge doctor");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect ambiguous route names the exact direct-safe correction")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AmbiguousRouteEmitsDirectSafeNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.DirectCorrectionBlockedResult(),
            "open-forge route inspect \"root/ambiguous\"");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect complete result omits the Next line")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteOmitsNext()
    {
        AssertNoNext(RouteInspectPresentationTestData.CompleteResult());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect exact-path attention omits the Next line")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExactPathAttentionOmitsNext()
    {
        AssertNoNext(RouteInspectPresentationTestData.ExactPathAttentionResult());
    }

    private static void AssertExactNext(RouteInspectResult result, string wording)
    {
        var output = CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, CliDetail.Minimal),
            RouteInspectPresentation.Rendering).PrimaryContent;
        var nextLines = output
            .Split('\n', StringSplitOptions.None)
            .Where(line => line.StartsWith("Next:", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal([$"Next: {wording}"], nextLines);
    }

    private static void AssertNoNext(RouteInspectResult result)
    {
        var output = CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, CliDetail.Minimal),
            RouteInspectPresentation.Rendering).PrimaryContent;

        Assert.DoesNotContain(
            output.Split('\n', StringSplitOptions.None),
            line => line.StartsWith("Next:", StringComparison.Ordinal));
    }
}
