using OpenForge.Cli.Core.Shell.Pipeline;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Presentation.Route.Exclude;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Exclude;

public sealed class RouteExcludeJsonSerializationTests
{
    [Theory(DisplayName = "RouteExclude source-generated JSON preserves row changes and no-op shape"), Trait("Feature", "route-exclude"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void PreservesRowsAndNoOp(bool noOp)
    {
        var result = new RouteExcludeResult
        {
            Workspace = null, Requested = "guide.md", DryRun = true,
            Outcome = new RouteInclusionOutcome
            {
                Path = "guide.md", Before = "- [Guide](../guide.md)", After = null,
                NoOp = noOp, Verified = noOp,
            },
        };
        var rendered = CliRenderingStage.Render(new CliPresentationRequest<RouteExcludeResult>(result,
            new CliPresentation(CliFormat.Json, CliDetail.Full, null)), RouteExcludePresentation.Rendering);
        using var json = JsonDocument.Parse(rendered.PrimaryContent);
        var root = json.RootElement;
        Assert.Equal("route exclude", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal("guide.md", root.GetProperty("data").GetProperty("target").GetString());
        Assert.Equal(noOp, root.GetProperty("data").GetProperty("verified").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").GetProperty("after").ValueKind);
    }
}
