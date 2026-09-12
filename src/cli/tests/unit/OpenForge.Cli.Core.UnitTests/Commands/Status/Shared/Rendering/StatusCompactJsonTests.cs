using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Rendering;

public sealed class StatusCompactJsonTests
{
    [Fact(DisplayName = "Compact Status JSON preserves totals and all state while omitting only continuity source details")]
    [Trait("Feature", "compact-json"), Trait("Evidence", "Unit")]
    public void RetainsMeasurementsAndFullState()
    {
        var result = StatusResultSeeds.Representative();
        Assert.NotEmpty(result.Facts.Context.ContinuitySources);
        var compact = Render(result, CliView.Compact);
        var expanded = Render(result, CliView.Expanded);
        Assert.True(JsonViewComparison.RetainsResult(compact, expanded, ["context.continuitySources"]));
    }

    private static string Render(StatusResult result, CliView view)
        => StatusJsonRenderer.Render(new CliPresentationRequest<StatusResult>(result,
            new CliPresentation(CliOutputFormat.Json, view, CliVerbosity.Normal)));
}
