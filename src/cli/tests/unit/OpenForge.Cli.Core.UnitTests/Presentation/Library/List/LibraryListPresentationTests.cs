using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Presentation.Library.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Library.List;

public sealed class LibraryListPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library list JSON changes link shape by detail and retains full nullable targets")]
    public void JsonShapeFollowsDetailPolicy()
    {
        var seed = LibraryListResultFixture.Create();
        var withNullTargets = seed with
        {
            Result = seed.Result with
            {
                Libraries =
                [
                    seed.Result.Libraries[0] with
                    {
                        Paths =
                        [
                            seed.Result.Libraries[0].Paths[0] with
                            {
                                ExpectedRelativeLink = null,
                                SourceId = null,
                                ObservedRelativeLink = null,
                            },
                        ],
                    },
                ],
            },
        };

        using var minimal = JsonDocument.Parse(Json(Select(withNullTargets, CliDetail.Minimal)));
        var minimalData = minimal.RootElement.GetProperty("data");
        Assert.False(minimalData.TryGetProperty("recordCoverage", out _));
        var minimalLinks = Assert.Single(minimalData.GetProperty("libraries").EnumerateArray()).GetProperty("links");
        Assert.Equal(JsonValueKind.Object, minimalLinks.ValueKind);
        Assert.Equal(0, minimalLinks.GetProperty("current").GetInt32());
        Assert.Equal(1, minimalLinks.GetProperty("missing").GetInt32());
        Assert.Equal(0, minimalLinks.GetProperty("changed").GetInt32());
        Assert.Equal(0, minimalLinks.GetProperty("unavailable").GetInt32());

        using var standard = JsonDocument.Parse(Json(Select(withNullTargets, CliDetail.Standard)));
        var standardData = standard.RootElement.GetProperty("data");
        Assert.False(standardData.TryGetProperty("recordCoverage", out _));
        var standardLink = Assert.Single(standardData.GetProperty("libraries").EnumerateArray())
            .GetProperty("links").EnumerateArray().Single();
        Assert.Equal(JsonValueKind.Array, standardData.GetProperty("libraries")[0].GetProperty("links").ValueKind);
        Assert.Equal(".agents/directives/review.md", standardLink.GetProperty("path").GetString());
        Assert.Equal("missing", standardLink.GetProperty("state").GetString());
        Assert.False(standardLink.TryGetProperty("expectedTarget", out _));

        using var full = JsonDocument.Parse(Json(Select(withNullTargets, CliDetail.Full)));
        var fullData = full.RootElement.GetProperty("data");
        var fullLibrary = Assert.Single(fullData.GetProperty("libraries").EnumerateArray());
        var fullLink = fullLibrary.GetProperty("links").EnumerateArray().Single();
        Assert.Equal(JsonValueKind.Array, fullLibrary.GetProperty("links").ValueKind);
        Assert.Equal(JsonValueKind.Null, fullLink.GetProperty("expectedTarget").ValueKind);
        Assert.Equal(JsonValueKind.Null, fullLink.GetProperty("observedTarget").ValueKind);
        Assert.Equal(JsonValueKind.Null, fullLink.GetProperty("sourceId").ValueKind);
        Assert.True(fullData.TryGetProperty("recordCoverage", out var coverage));
        Assert.Equal(".agents/open-forge.lock.json", coverage.GetProperty("path").GetString());
        Assert.Equal("complete", coverage.GetProperty("state").GetString());
        Assert.Equal(1, coverage.GetProperty("libraryCount").GetInt32());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Healthy library list hides the human headline while JSON keeps the plural summary")]
    public void HealthyHumanAndJsonUseSeparateHeadlineProjection()
    {
        var result = LibraryListResultFixture.Create(CliSemanticStatus.Complete) with { Workspace = null };
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, LibraryListPresentation.Rendering.DataTextRenderer).Content;
        Assert.DoesNotContain("1 Library registered.", text, StringComparison.Ordinal);
        Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
        Assert.Contains("shared/team -> .", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(Json(selected));
        Assert.Equal("1 Library registered.", json.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Empty library list keeps its registration Next action in a two-line human result")]
    public void EmptyListHasRegistrationNextAction()
    {
        var seed = LibraryListResultFixture.Create(CliSemanticStatus.Complete);
        var result = seed with
        {
            Workspace = null,
            Result = seed.Result with
            {
                Record = seed.Result.Record with { LibraryCount = 0 },
                Libraries = [],
                Findings = [],
            },
        };
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, LibraryListPresentation.Rendering.DataTextRenderer).Content;
        Assert.Equal("No Libraries are registered.\nNext: open-forge library attach <id> <source-folder>\n", text);
        using var json = JsonDocument.Parse(Json(selected));
        Assert.Equal("open-forge library attach <id> <source-folder>", json.RootElement.GetProperty("next").GetProperty("command").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Failed library list results request bounded debug diagnostics")]
    public void FailedListHasDebugNextAction()
    {
        var selected = Select(LibraryListResultFixture.Create(CliSemanticStatus.Failed), CliDetail.Minimal);

        Assert.Equal("open-forge library list --detail debug", selected.Report.Next?.Command);
        Assert.Equal(
            "Report the failure and retry the same Library list request with bounded diagnostics.",
            selected.Report.Next?.Reason);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library list keeps readable link wording separate from finite JSON state")]
    public void LinkStateUsesSeparateHumanAndWireVocabulary()
    {
        var seed = LibraryListResultFixture.Create();
        var library = seed.Result.Libraries[0];
        var result = seed with
        {
            Workspace = null,
            Result = seed.Result with
            {
                Libraries =
                [
                    library with
                    {
                        Paths = [library.Paths[0] with { State = LibraryLinkViewState.NotStarted }],
                    },
                ],
            },
        };

        var text = CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Text, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
        Assert.Contains("not checked", text, StringComparison.Ordinal);
        Assert.DoesNotContain("not-started", text, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering).PrimaryContent);
        Assert.Equal("not-started", json.RootElement.GetProperty("data").GetProperty("libraries")[0]
            .GetProperty("links")[0].GetProperty("state").GetString());
    }

    private static CliSelectedReport<LibraryListData> Select(LibraryListResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = LibraryListPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }

    private static string Json(CliSelectedReport<LibraryListData> selected)
        => CliJsonRenderer.Render(selected, LibraryListPresentation.Rendering.DataJsonTypeInfo);
}
