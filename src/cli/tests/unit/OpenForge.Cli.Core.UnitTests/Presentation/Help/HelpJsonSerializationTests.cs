using System.Text.Json;
using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Presentation.Help;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Help;

public sealed class HelpJsonSerializationTests
{
    [Theory, InlineData("minimal"), InlineData("standard"), InlineData("full"), InlineData("debug")]
    [Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void EveryDetailKeepsTheTopicAndCompleteMarkdown(string detail)
    {
        var result = new HelpResult
        {
            Status = CliSemanticStatus.Complete,
            Topic = "concepts",
            FileFormats = "\nExact `source` <text> and Unicode: café.\n",
        };
        var selection = new CliSelection(Enum.Parse<CliDetail>(detail, ignoreCase: true), null);
        var rendering = HelpPresentation.Rendering;
        var report = rendering.Selector(result, selection);
        var selected = CliReportTrimmer.Trim(report, selection, rendering.Shape);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo));

        Assert.Equal("help", json.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("workspace").ValueKind);
        var data = json.RootElement.GetProperty("data");
        Assert.Equal("concepts", data.GetProperty("topic").GetString());
        Assert.Equal(report.Data.Markdown, data.GetProperty("markdown").GetString());
        Assert.EndsWith(result.FileFormats, data.GetProperty("markdown").GetString(), StringComparison.Ordinal);
        Assert.False(data.TryGetProperty("complete", out _));
        Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
    }

    [Fact, Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void TopicListHasNoSelectedTopicOrMarkdown()
    {
        var result = new HelpResult { Status = CliSemanticStatus.Complete, Topic = null };
        var selection = new CliSelection(CliDetail.Minimal, null);
        var rendering = HelpPresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo));

        var data = json.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("topic").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("markdown").ValueKind);
        Assert.Equal("concepts", Assert.Single(data.GetProperty("topics").EnumerateArray()).GetString());
    }

    [Theory, InlineData("Invalid", "invalid-input", "help.invalid-input"), InlineData("Failed", "failed", "help.content-unavailable"),
     InlineData("Interrupted", "cancelled", "help.interrupted")]
    [Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void FailureStatusesKeepTheirCauseAndFinding(string status, string wireStatus, string code)
    {
        var result = new HelpResult
        {
            Status = Enum.Parse<CliSemanticStatus>(status),
            Topic = "concepts",
            Cause = "The request could not finish.",
        };
        var selection = new CliSelection(CliDetail.Full, null);
        var rendering = HelpPresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo));

        Assert.Equal(wireStatus, json.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(code, finding.GetProperty("code").GetString());
        Assert.Equal(result.Cause, finding.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").GetProperty("markdown").ValueKind);
    }

    [Fact, Trait("Feature", "help"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void UndefinedStatusIsRejected()
    {
        var result = new HelpResult { Status = (CliSemanticStatus)int.MaxValue, Topic = null };
        Assert.Throws<ArgumentOutOfRangeException>(() => result.FindingCode);
        Assert.Throws<ArgumentOutOfRangeException>(() => HelpPresentation.Rendering.Selector(result, new CliSelection(CliDetail.Minimal, null)));
    }
}
