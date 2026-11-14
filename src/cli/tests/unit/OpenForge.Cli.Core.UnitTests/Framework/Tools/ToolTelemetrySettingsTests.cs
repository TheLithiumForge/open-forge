using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-telemetry"), Trait("Evidence", "Unit")]
public sealed class ToolTelemetrySettingsTests
{
    private static string Property => JsonNamingPolicy.CamelCase.ConvertName(nameof(WorkspaceToolSetting.Telemetry));

    [Theory(DisplayName = "Tool telemetry defaults off and round trips the saved opt-in and opt-out")]
    [InlineData(null, false), InlineData(false, false), InlineData(true, true)]
    public void RoundTripsChoice(bool? authored, bool expected)
    {
        var source = Settings(authored is { } value ? JsonValue.Create(value) : null, authored is not null);
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(source).Document);
        Assert.Equal(expected, settings.Tools[ToolFixtureSource.Id].Telemetry);
        var rewritten = WorkspaceSettingsCodec.SetTools(source, settings.Tools, settings.ToolRuns);
        var read = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(rewritten).Document);
        Assert.Equal(expected, read.Tools[ToolFixtureSource.Id].Telemetry);
    }

    [Theory(DisplayName = "Malformed telemetry values cannot become an opt-in")]
    [InlineData("null"), InlineData("\"true\""), InlineData("1"), InlineData("[]")]
    public void RejectsMalformedChoice(string value)
    {
        Assert.Null(WorkspaceSettingsCodec.Read(Settings(JsonNode.Parse(value), include: true)).Document);
    }

    private static byte[] Settings(JsonNode? choice, bool include)
    {
        var tool = new JsonObject
        {
            [JsonNamingPolicy.CamelCase.ConvertName(nameof(WorkspaceToolSetting.Version))] = ToolFixtureSource.Version,
            [JsonNamingPolicy.CamelCase.ConvertName(nameof(WorkspaceToolSetting.Roles))] = new JsonArray(),
        };
        if (include) tool[Property] = choice;
        return Encoding.UTF8.GetBytes(new JsonObject
        {
            [WorkspaceSettingsDefinitions.ToolsProperty] = new JsonObject { [ToolFixtureSource.Id] = tool },
        }.ToJsonString());
    }
}
