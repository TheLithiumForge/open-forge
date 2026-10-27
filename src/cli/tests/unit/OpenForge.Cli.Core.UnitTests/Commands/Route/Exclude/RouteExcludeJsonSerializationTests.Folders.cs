using OpenForge.Cli.Core.Shell.Pipeline;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Presentation.Route.Exclude;
using OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Exclude;

public sealed partial class RouteExcludeJsonSerializationTests
{
    [Theory(DisplayName = "Folder Exclude emits directory coordinates and additive facts at every detail"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal), InlineData((int)CliDetail.Standard), InlineData((int)CliDetail.Full), InlineData((int)CliDetail.Debug)]
    public void FolderFacts(int detail)
    {
        const string folder = "project/team";
        const string catalogue = folder + "/_team.md";
        var result = new RouteExcludeResult
        {
            Workspace = null,
            Requested = folder,
            DryRun = true,
            Outcome = new RouteInclusionOutcome
            {
                Path = folder,
                TargetKind = RouteInclusionTargetKind.Folder,
                Catalogue = catalogue,
                RegisteredBefore = false,
                RegisteredAfter = false,
                Effects = [new(catalogue, null, "scaffold", RouteInclusionEffectState.Planned, RouteInclusionEffectKind.Catalogue)],
            },
        };
        var rendered = CliRenderingStage.Render(new CliPresentationRequest<RouteExcludeResult>(result,
            new CliPresentation(CliFormat.Json, (CliDetail)detail, null)), RouteExcludePresentation.Rendering);
        using var json = JsonDocument.Parse(rendered.PrimaryContent);
        var data = json.RootElement.GetProperty(PublishedTokens.Json.Data);
        Assert.Equal(folder, data.GetProperty(PublishedTokens.RouteInclusionData.Target).GetString());
        Assert.Equal(RouteInclusionWireVocabulary.FolderTargetKind, data.GetProperty(PublishedTokens.RouteInclusionData.TargetKind).GetString());
        Assert.Equal(catalogue, data.GetProperty(PublishedTokens.RouteInclusionData.Catalogue).GetString());
        Assert.False(data.GetProperty(PublishedTokens.RouteInclusionData.RegisteredAfter).GetBoolean());
        Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
    }
}
