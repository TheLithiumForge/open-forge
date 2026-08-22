using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectPresentationStatusTests
{
    [Fact(DisplayName = "Route Inspect interactive attention emits the exact non-interactive Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InteractiveAttentionEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InteractiveAttentionResult(),
            "rerun with the exact path for non-interactive use.");
    }

    [Fact(DisplayName = "Route Inspect unresolved collision blocked emits the exact listed-path Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UnresolvedCollisionEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.BlockedCollisionResult(),
            "rerun with one of the listed exact paths.");
    }

    [Fact(DisplayName = "Route Inspect invalid result emits the exact correction Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InvalidEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InvalidResult(),
            "correct the named source or input.");
    }

    [Fact(DisplayName = "Route Inspect failed result emits the exact bounded-diagnostics Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void FailedEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.FailedResult(),
            "report the failure and retry with bounded diagnostics.");
    }

    [Fact(DisplayName = "Route Inspect interrupted result emits the exact rerun Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InterruptedEmitsExactNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.InterruptedResult(),
            "rerun the same request.");
    }

    [Fact(DisplayName = "Route Inspect incomplete result falls back to the exact doctor Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IncompleteEmitsDoctorNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.IncompleteResult(),
            "open-forge doctor");
    }

    [Fact(DisplayName = "Route Inspect blocked result without a direct correction falls back to the exact doctor Next wording")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BlockedWithoutDirectCorrectionEmitsDoctorNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.FallbackBlockedResult(),
            "open-forge doctor");
    }

    [Fact(DisplayName = "Route Inspect ambiguous route names the exact direct-safe correction")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AmbiguousRouteEmitsDirectSafeNext()
    {
        AssertExactNext(
            RouteInspectPresentationTestData.DirectCorrectionBlockedResult(),
            "rerun with the exact source path after resolving the ambiguous route.");
    }

    [Fact(DisplayName = "Route Inspect complete result omits the Next line")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteOmitsNext()
    {
        AssertNoNext(RouteInspectPresentationTestData.CompleteResult());
    }

    [Fact(DisplayName = "Route Inspect exact-path attention omits the Next line")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExactPathAttentionOmitsNext()
    {
        AssertNoNext(RouteInspectPresentationTestData.ExactPathAttentionResult());
    }

    private static void AssertExactNext(RouteInspectResult result, string wording)
    {
        var output = RouteInspectHumanRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Compact));
        var nextLines = output
            .Split(Environment.NewLine, StringSplitOptions.None)
            .Where(line => line.StartsWith("Next:", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal([$"Next: {wording}"], nextLines);
    }

    private static void AssertNoNext(RouteInspectResult result)
    {
        var output = RouteInspectHumanRenderer.Render(
            RouteInspectPresentationTestData.Presentation(result, CliView.Compact));

        Assert.DoesNotContain(
            output.Split(Environment.NewLine, StringSplitOptions.None),
            line => line.StartsWith("Next:", StringComparison.Ordinal));
    }
}
