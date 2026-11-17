using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization;

public sealed class ToolRecordJsonTests
{
    [Theory(DisplayName = "Unknown native-setting fields stay with the complete tool, change and home identity"), InlineData(0), InlineData(1), InlineData(2), InlineData(3),
     Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void NativeIdentityKeepsUnknownFields(int changedPart)
    {
        var home = Path.Combine(Path.GetTempPath(), "synthetic-native-home");
        var original = Setting("synthetic-first", home);
        original["future"] = new JsonObject { ["value"] = 17 };
        var replacement = Setting(changedPart == 1 ? "synthetic-second" : "synthetic-first", changedPart == 2 ? home + "-other" : home);
        if (changedPart == 3) original.Remove(Property(nameof(ToolNativeSettingRecord.NativeHomePath)));
        var key = Property(nameof(ToolWorkspaceMachineState.NativeSettings));
        var result = ToolRecordJson.Merge(new() { [key] = new JsonArray(original) }, new() { [key] = new JsonArray(replacement) });
        var setting = Assert.IsType<JsonObject>(Assert.Single(Assert.IsType<JsonArray>(result[key])));
        Assert.Equal(changedPart == 0, setting.ContainsKey("future"));
        if (changedPart == 0) Assert.True(JsonNode.DeepEquals(original["future"], setting["future"]));
    }

    [Theory(DisplayName = "PATH history keeps unknown row and value fields only for complete identities after reorder"),
     InlineData(0), InlineData(1), InlineData(2), InlineData(3), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void PathHistoryIdentity(int changedPart)
    {
        var first = PathChange("first");
        var second = PathChange("second");
        var removed = PathChange("removed");
        var unmatched = changedPart switch
        {
            0 => first with { Kind = ToolPathChangeKind.WindowsUserPath },
            1 => first with { Target = first.Target + "-different" },
            2 => first with { StartMarker = "different-start" },
            3 => first with { EndMarker = "different-end" },
            _ => throw new ArgumentOutOfRangeException(nameof(changedPart)),
        };
        var before = PathState([first, second, removed]);
        var key = Property(nameof(ToolPathState.Changes));
        var prior = Assert.IsType<JsonArray>(before[key]);
        foreach (var row in prior.OfType<JsonObject>())
        {
            row["future"] = row[Property(nameof(ToolPathChangeRecord.Target))]?.DeepClone();
            row[Property(nameof(ToolPathChangeRecord.Prior))]?["future-prior"] = 41;
            row[Property(nameof(ToolPathChangeRecord.Applied))]?["future-applied"] = 42;
        }
        // The same typed enum can arrive through a numeric JSON representation.
        prior[1]?[Property(nameof(ToolPathChangeRecord.Kind))] = (int)second.Kind;
        var result = ToolRecordJson.Merge(before, PathState([second, first, unmatched]));
        var after = Assert.IsType<JsonArray>(result[key]);
        Assert.Equal(3, after.Count);
        foreach (var pair in new[] { (Before: 0, After: 1), (Before: 1, After: 0) })
        {
            Assert.True(JsonNode.DeepEquals(prior[pair.Before]?["future"], after[pair.After]?["future"]));
            Assert.True(JsonNode.DeepEquals(prior[pair.Before]?[Property(nameof(ToolPathChangeRecord.Prior))]?["future-prior"],
                after[pair.After]?[Property(nameof(ToolPathChangeRecord.Prior))]?["future-prior"]));
            Assert.True(JsonNode.DeepEquals(prior[pair.Before]?[Property(nameof(ToolPathChangeRecord.Applied))]?["future-applied"],
                after[pair.After]?[Property(nameof(ToolPathChangeRecord.Applied))]?["future-applied"]));
        }
        var fresh = Assert.IsType<JsonObject>(after[2]);
        Assert.False(fresh.ContainsKey("future"));
        Assert.False(Assert.IsType<JsonObject>(fresh[Property(nameof(ToolPathChangeRecord.Prior))]).ContainsKey("future-prior"));
        Assert.False(Assert.IsType<JsonObject>(fresh[Property(nameof(ToolPathChangeRecord.Applied))]).ContainsKey("future-applied"));
    }

    private static ToolPathChangeRecord PathChange(string name) => new()
    {
        Kind = ToolPathChangeKind.ShellBlock,
        Target = Path.Combine(Path.GetTempPath(), "synthetic-history", name),
        StartMarker = "start",
        EndMarker = "end",
        Prior = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = [1] },
        Applied = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = [2] },
    };

    private static JsonObject PathState(IReadOnlyList<ToolPathChangeRecord> changes)
        => ToolRecordJson.Read(JsonSerializer.SerializeToUtf8Bytes(new ToolPathState
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            BinPath = Path.Combine(Path.GetTempPath(), "synthetic-bin"),
            Changes = changes,
        }, ToolMachineJsonContext.Default.ToolPathState));

    private static JsonObject Setting(string tool, string home) => new()
    {
        [Property(nameof(ToolNativeSettingRecord.ToolId))] = tool,
        [Property(nameof(ToolNativeSettingRecord.ChangeId))] = "same-change",
        [Property(nameof(ToolNativeSettingRecord.NativeHomePath))] = home,
    };

    private static string Property(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}
