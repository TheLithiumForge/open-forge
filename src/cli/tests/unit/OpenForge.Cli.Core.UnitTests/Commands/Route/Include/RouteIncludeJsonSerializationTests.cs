using OpenForge.Cli.Core.Shell.Pipeline;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Presentation.Route.Include;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Include;

public sealed class RouteIncludeJsonSerializationTests
{
    [Theory(DisplayName = "RouteInclude source-generated JSON preserves row changes and no-op shape"), Trait("Feature", "route-include"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void PreservesRowsAndNoOp(bool noOp)
    {
        var result = new RouteIncludeResult
        {
            Workspace = null, Requested = "guide.md", DryRun = true,
            Outcome = new RouteInclusionOutcome
            {
                Path = "guide.md", Before = null, After = "- [Guide](../guide.md)",
                NoOp = noOp, Verified = noOp,
            },
        };
        var rendered = CliRenderingStage.Render(new CliPresentationRequest<RouteIncludeResult>(result,
            new CliPresentation(CliFormat.Json, CliDetail.Full, null)), RouteIncludePresentation.Rendering);
        using var json = JsonDocument.Parse(rendered.PrimaryContent);
        var root = json.RootElement;
        Assert.Equal("route include", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal("guide.md", root.GetProperty("data").GetProperty("target").GetString());
        Assert.Equal(noOp, root.GetProperty("data").GetProperty("verified").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").GetProperty("before").ValueKind);
    }
}
