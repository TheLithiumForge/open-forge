using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Rendering;

public sealed class RouteInspectHumanViewTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect views show incomplete conditions before the profile and preserve each JSON level"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void ConditionsPrecedeProfile(int view)
    {
        var result = RouteInspectPresentationTestData.IncompleteResult();
        var output = RenderText(result, (CliDetail)view);

        Assert.Contains("This file could not be measured", output, StringComparison.Ordinal);
        Assert.True(output.IndexOf("This file could not be measured", StringComparison.Ordinal)
            < output.IndexOf("Where this source belongs", StringComparison.Ordinal));
        Assert.Contains("Next: open-forge doctor", output, StringComparison.Ordinal);
        Assert.Equal(2, output.Split("This file could not be measured", StringSplitOptions.None).Length - 1);

        var json = RenderJson(result, CliDetail.Standard);
        Assert.Equal(json, RenderJson(result, CliDetail.Standard));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect human views preserve complete long subjects and the operation Next command"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void FullSubjectAndNext(int view)
    {
        var basis = RouteInspectPresentationTestData.InvalidResult();
        var subject = new string('x', 9000) + "-end";
        var result = RouteInspectResult.Create(
            basis.Status, basis.Workspace, basis.Selection, null, null, [],
            [new RouteInspectCondition(RouteInspectConditionCode.InvalidSourceReference, CliSemanticStatus.Invalid, subject, "The source reference was rejected.")],
            basis.Next);
        var output = RenderText(result, (CliDetail)view);

        Assert.Contains(subject, output, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect --help", output, StringComparison.Ordinal);
    }

    private static string RenderText(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail),
            RouteInspectPresentation.Rendering).PrimaryContent;

    private static string RenderJson(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail, CliFormat.Json),
            RouteInspectPresentation.Rendering).PrimaryContent;
}
