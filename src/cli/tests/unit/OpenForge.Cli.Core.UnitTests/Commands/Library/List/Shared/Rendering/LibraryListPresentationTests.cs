using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List JSON retains typed links and full nullable target facts")]
    public void ExactNativeDataGraph()
    {
        var seed = LibraryListResultFixture.Create();
        var standard = RenderJson(seed, CliDetail.Standard);
        using var standardDocument = JsonDocument.Parse(standard);
        var standardData = standardDocument.RootElement.GetProperty("data");
        Assert.Equal(["libraries"], standardData.EnumerateObject().Select(property => property.Name));
        var standardLibrary = Assert.Single(standardData.GetProperty("libraries").EnumerateArray());
        Assert.Equal(["id", "sourceFolder", "destinationFolder", "links"],
            standardLibrary.EnumerateObject().Select(property => property.Name));
        var standardLink = Assert.Single(standardLibrary.GetProperty("links").EnumerateArray());
        Assert.Equal(["path", "state"], standardLink.EnumerateObject().Select(property => property.Name));
        Assert.Equal(".agents/directives/review.md", standardLink.GetProperty("path").GetString());
        Assert.Equal("missing", standardLink.GetProperty("state").GetString());

        using var fullDocument = JsonDocument.Parse(RenderJson(seed, CliDetail.Full));
        var fullData = fullDocument.RootElement.GetProperty("data");
        Assert.Equal(["libraries", "recordCoverage"], fullData.EnumerateObject().Select(property => property.Name));
        var fullLink = Assert.Single(Assert.Single(fullData.GetProperty("libraries").EnumerateArray())
            .GetProperty("links").EnumerateArray());
        Assert.Equal(["path", "state", "expectedTarget", "observedTarget", "sourceId"],
            fullLink.EnumerateObject().Select(property => property.Name));
        Assert.Equal("../../shared/team/.agents/directives/review.md", fullLink.GetProperty("expectedTarget").GetString());
        Assert.Equal(JsonValueKind.Null, fullLink.GetProperty("observedTarget").ValueKind);
        Assert.Equal("directives/review", fullLink.GetProperty("sourceId").GetString());
        Assert.Equal(".agents/open-forge.lock.json", fullData.GetProperty("recordCoverage").GetProperty("path").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List keeps unknown record counts absent from its native data")]
    public void UnknownFactsRemainNull()
    {
        foreach (var status in new[] { CliSemanticStatus.Incomplete, CliSemanticStatus.Invalid })
        {
            var seed = LibraryListResultFixture.Create(status);
            var result = seed with
            {
                Workspace = null,
                Result = seed.Result with { Record = seed.Result.Record with { LibraryCount = null }, Libraries = [] },
            };
            using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Full));
            var root = document.RootElement;
            var data = root.GetProperty("data");
            Assert.Equal(JsonValueKind.Null, data.GetProperty("recordCoverage").GetProperty("libraryCount").ValueKind);
            Assert.Empty(data.GetProperty("libraries").EnumerateArray());

            var counts = root.GetProperty("counts");
            foreach (var name in new[] { "libraries", "linksCurrent", "linksMissing", "linksChanged", "linksUnavailable" })
            {
                Assert.Equal(JsonValueKind.Null, counts.GetProperty(name).ValueKind);
            }

            var limitations = root.GetProperty("limitations").EnumerateArray().ToArray();
            Assert.Contains(limitations, limitation =>
                limitation.GetProperty("what").GetString() == "libraries"
                && limitation.GetProperty("why").GetString() == "Record observation stopped.");
        }
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List coordinates native status, stream and exit")]
    [InlineData((int)CliSemanticStatus.Complete, (int)CliOutputTarget.StandardOutput, 0)]
    [InlineData((int)CliSemanticStatus.Attention, (int)CliOutputTarget.StandardOutput, 2)]
    [InlineData((int)CliSemanticStatus.Incomplete, (int)CliOutputTarget.StandardOutput, 3)]
    [InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputTarget.StandardError, 4)]
    [InlineData((int)CliSemanticStatus.Blocked, (int)CliOutputTarget.StandardError, 5)]
    [InlineData((int)CliSemanticStatus.Failed, (int)CliOutputTarget.StandardError, 1)]
    [InlineData((int)CliSemanticStatus.Interrupted, (int)CliOutputTarget.StandardError, 130)]
    public void StatusCoordinates(int status, int target, int exit)
    {
        var result = LibraryListResultFixture.Create((CliSemanticStatus)status);
        var text = CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Text, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering);
        Assert.Equal(exit, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
        Assert.Equal((CliOutputTarget)target, text.PrimaryTarget);
        Assert.Equal(result.Status, text.Status);
        Assert.NotEmpty(text.PrimaryContent);

        var json = CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering);
        Assert.Equal(CliOutputTarget.StandardOutput, json.PrimaryTarget);
        using var document = JsonDocument.Parse(json.PrimaryContent);
        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(result.Command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal(result.Status switch
        {
            CliSemanticStatus.Complete => "completed",
            CliSemanticStatus.Attention => "completed-with-warnings",
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Invalid => "invalid-input",
            CliSemanticStatus.Blocked => "blocked",
            CliSemanticStatus.Failed => "failed",
            CliSemanticStatus.Interrupted => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        }, document.RootElement.GetProperty("status").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List retains the same result facts while detail selects native data depth")]
    public void SameFactsAcrossPresentations()
    {
        var result = LibraryListResultFixture.Create();
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
        {
            var text = RenderText(result, detail);
            Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
            Assert.Contains("shared/team -> .", text, StringComparison.Ordinal);
            Assert.Contains(".agents/directives/review.md", text, StringComparison.Ordinal);
            Assert.Contains("missing", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Source inventory", text, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(RenderJson(result, detail));
            var library = Assert.Single(document.RootElement.GetProperty("data").GetProperty("libraries").EnumerateArray());
            Assert.Equal("team-knowledge", library.GetProperty("id").GetString());
            Assert.Equal(detail == CliDetail.Minimal ? JsonValueKind.Object : JsonValueKind.Array,
                library.GetProperty("links").ValueKind);
        }
    }

    private static string RenderText(LibraryListResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Text, detail, null)),
            LibraryListPresentation.Rendering).PrimaryContent;

    private static string RenderJson(LibraryListResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, detail, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
}
