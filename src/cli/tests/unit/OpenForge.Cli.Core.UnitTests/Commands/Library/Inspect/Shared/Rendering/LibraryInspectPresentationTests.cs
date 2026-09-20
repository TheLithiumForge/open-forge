using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect native JSON keeps the detail-selected file graph and inventory count"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void ExactNativeDataGraph()
    {
        var seed = LibraryInspectResultFixture.Create();
        using var standardDocument = JsonDocument.Parse(RenderJson(seed, CliDetail.Standard));
        var standardData = standardDocument.RootElement.GetProperty("data");
        Assert.Equal(["id", "sourceFolder", "destinationFolder", "current", "files"],
            standardData.EnumerateObject().Select(property => property.Name));
        Assert.Equal("team-knowledge", standardData.GetProperty("id").GetString());
        Assert.Equal("shared/team", standardData.GetProperty("sourceFolder").GetString());
        Assert.Equal(JsonValueKind.Null, standardData.GetProperty("destinationFolder").ValueKind);
        Assert.False(standardData.GetProperty("current").GetBoolean());
        var standardFile = Assert.Single(standardData.GetProperty("files").EnumerateArray());
        Assert.Equal(["sourcePath", "destinationPath", "relation"],
            standardFile.EnumerateObject().Select(property => property.Name));
        Assert.Equal(".agents/directives/review.md", standardFile.GetProperty("sourcePath").GetString());
        Assert.Equal(".agents/directives/review.md", standardFile.GetProperty("destinationPath").GetString());
        Assert.Equal("missing", standardFile.GetProperty("relation").GetString());

        using var fullDocument = JsonDocument.Parse(RenderJson(seed, CliDetail.Full));
        var fullData = fullDocument.RootElement.GetProperty("data");
        Assert.Equal(["id", "sourceFolder", "destinationFolder", "current", "files", "inventory"],
            fullData.EnumerateObject().Select(property => property.Name));
        var fullFile = Assert.Single(fullData.GetProperty("files").EnumerateArray());
        Assert.Equal(["sourcePath", "destinationPath", "relation", "expectedTarget", "observedTarget"],
            fullFile.EnumerateObject().Select(property => property.Name));
        Assert.Equal("../../shared/team/.agents/directives/review.md", fullFile.GetProperty("expectedTarget").GetString());
        Assert.Equal(JsonValueKind.Null, fullFile.GetProperty("observedTarget").ValueKind);
        var inventory = fullData.GetProperty("inventory");
        Assert.Equal(1, inventory.GetProperty("eligible").GetInt32());
        Assert.Equal(0, inventory.GetProperty("excluded").GetInt32());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect keeps unavailable identity null in its native data"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void UnknownFactsRemainNull()
    {
        var seed = LibraryInspectResultFixture.Create(CliSemanticStatus.Invalid);
        var result = seed with
        {
            Workspace = null,
            Result = seed.Result with
            {
                Record = seed.Result.Record with { Id = null, SourceRoot = null, DestinationRoot = null },
                Findings = [seed.Result.Findings[0] with { LibraryId = null }],
            },
        };
        using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Full));
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("sourceFolder").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("destinationFolder").ValueKind);
        Assert.Empty(data.GetProperty("files").EnumerateArray());
        Assert.Equal(0, data.GetProperty("inventory").GetProperty("eligible").GetInt32());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library Inspect coordinates native status, stream and exit"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, (int)CliOutputTarget.StandardOutput, 0)]
    [InlineData((int)CliSemanticStatus.Attention, (int)CliOutputTarget.StandardOutput, 2)]
    [InlineData((int)CliSemanticStatus.Incomplete, (int)CliOutputTarget.StandardOutput, 3)]
    [InlineData((int)CliSemanticStatus.Invalid, (int)CliOutputTarget.StandardError, 4)]
    [InlineData((int)CliSemanticStatus.Blocked, (int)CliOutputTarget.StandardError, 5)]
    [InlineData((int)CliSemanticStatus.Failed, (int)CliOutputTarget.StandardError, 1)]
    [InlineData((int)CliSemanticStatus.Interrupted, (int)CliOutputTarget.StandardError, 130)]
    public void StatusCoordinates(int status, int target, int exit)
    {
        var result = LibraryInspectResultFixture.Create((CliSemanticStatus)status);
        var text = RenderOutput(result, CliFormat.Text, CliDetail.Standard);
        Assert.Equal(exit, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
        Assert.Equal((CliOutputTarget)target, text.PrimaryTarget);
        Assert.Equal(result.Status, text.Status);
        Assert.NotEmpty(text.PrimaryContent);

        var json = RenderOutput(result, CliFormat.Json, CliDetail.Standard);
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
    [Fact(DisplayName = "Library Inspect native detail retains identity and selects only the requested depth"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void SameFactsAcrossPresentations()
    {
        var result = LibraryInspectResultFixture.Create();
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full, CliDetail.Debug })
        {
            var text = RenderOutput(result, CliFormat.Text, detail).PrimaryContent;
            Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
            Assert.Contains("shared/team", text, StringComparison.Ordinal);
            Assert.Contains(".agents/directives/review.md", text, StringComparison.Ordinal);
            Assert.Contains("missing", text, StringComparison.Ordinal);
            Assert.Equal(detail >= CliDetail.Full, text.Contains("[library-inspect.link-missing]", StringComparison.Ordinal));

            using var document = JsonDocument.Parse(RenderOutput(result, CliFormat.Json, detail).PrimaryContent);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("team-knowledge", data.GetProperty("id").GetString());
            Assert.Equal(detail >= CliDetail.Full ? JsonValueKind.Object : JsonValueKind.Undefined,
                detail >= CliDetail.Full ? data.GetProperty("inventory").ValueKind : JsonValueKind.Undefined);
            Assert.Single(data.GetProperty("files").EnumerateArray());
            Assert.Equal(detail >= CliDetail.Full,
                data.GetProperty("files")[0].TryGetProperty("expectedTarget", out _));
        }
    }

    private static CliRenderedOutput RenderOutput(LibraryInspectResult result, CliFormat format, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryInspectResult>(result, new(format, detail, null)),
            LibraryInspectPresentation.Rendering);

    private static string RenderJson(LibraryInspectResult result, CliDetail detail)
        => RenderOutput(result, CliFormat.Json, detail).PrimaryContent;
}
