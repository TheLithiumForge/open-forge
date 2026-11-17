using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Serialization;

public sealed class ToolPortableCodecTests
{
    internal const string ToolId = "synthetic";
    internal const string Version = "3.2.1";
    internal const string Range = "3.2.x";
    private static readonly ToolPortableCodec Codec = new(new Versions());

    [Fact(DisplayName = "Tool lock round trips all five additive receipts without repository previous values"), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void FiveReceiptsRoundTrip()
    {
        var additions = Additions();
        var locked = new Dictionary<string, ToolLockRecord> { [ToolId] = new() { Version = Version, Channel = "synthetic-channel", Changes = additions } };
        var root = Codec.WriteLock(new(), locked);
        var read = Codec.ReadLock(root)[ToolId];
        Assert.Equal(Version, read.Version);
        Assert.Equal(additions.Select(value => value.GetType()), read.Changes.Select(value => value.GetType()));
        Assert.True(JsonNode.DeepEquals(root, Codec.WriteLock(root, new Dictionary<string, ToolLockRecord> { [ToolId] = read })));
        Assert.Equal(ToolRecordDefinitions.JsonUnionKind, root[ToolRecordDefinitions.ToolsProperty]?[ToolId]?[ToolRecordDefinitions.ChangesProperty]?[0]?[ToolRecordDefinitions.KindProperty]?.GetValue<string>());
    }

    [Fact(DisplayName = "Tool codecs keep unknown root, tool and receipt members"), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void UnknownKeysSurvive()
    {
        var root = Codec.WriteLock(new(), new Dictionary<string, ToolLockRecord> { [ToolId] = new() { Version = Version, Channel = "synthetic-channel", Changes = Additions() } });
        root["future-root"] = 7;
        var entry = Assert.IsType<JsonObject>(root[ToolRecordDefinitions.ToolsProperty]?[ToolId]);
        entry["future-tool"] = new JsonObject { ["preserved"] = true };
        var change = Assert.IsType<JsonObject>(entry[ToolRecordDefinitions.ChangesProperty]?[0]);
        change["future-change"] = 11;
        var written = Codec.WriteLock(root, Codec.ReadLock(root));
        Assert.True(JsonNode.DeepEquals(root, written));
        var settings = Codec.WriteSettings(new(), new Dictionary<string, ToolSetting> { [ToolId] = new(new(3, 2)) });
        settings[ToolRecordDefinitions.ToolsProperty]![ToolId]!["future-setting"] = 4;
        Assert.True(JsonNode.DeepEquals(settings, Codec.WriteSettings(settings, Codec.ReadSettings(settings))));
    }

    [Fact(DisplayName = "Receipt JSON values and added items replace exactly while unknown metadata survives"), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void ReceiptPayloadsAreExact()
    {
        using var oldPayload = JsonDocument.Parse("{\"id\":\"synthetic-item\",\"removed\":1}");
        using var newPayload = JsonDocument.Parse("{\"id\":\"synthetic-item\"}");
        ToolLockRecord Record(JsonElement value) => new()
        {
            Version = Version,
            Channel = "synthetic-channel",
            Changes = [new ToolJsonValueAddition { Id = "value", Path = "config.json", Pointer = "/value", Value = value },
                new ToolJsonUnionAddition { Id = "union", Path = "config.json", Pointer = "/items", Items = [value] }],
        };
        var before = Codec.WriteLock(new(), new Dictionary<string, ToolLockRecord> { [ToolId] = Record(oldPayload.RootElement.Clone()) });
        before[ToolRecordDefinitions.ToolsProperty]![ToolId]!["future"] = true;
        var after = Codec.WriteLock(before, new Dictionary<string, ToolLockRecord> { [ToolId] = Record(newPayload.RootElement.Clone()) });
        var read = Codec.ReadLock(after)[ToolId];
        Assert.False(Assert.IsType<ToolJsonValueAddition>(read.Changes[0]).Value.TryGetProperty("removed", out _));
        Assert.False(Assert.Single(Assert.IsType<ToolJsonUnionAddition>(read.Changes[1]).Items).TryGetProperty("removed", out _));
        Assert.True(after[ToolRecordDefinitions.ToolsProperty]![ToolId]!["future"]?.GetValue<bool>());
        var key = JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolRecordedValue.Json));
        var machine = ToolRecordJson.Merge(new JsonObject { [key] = JsonNode.Parse(oldPayload.RootElement.GetRawText()) }, new JsonObject { [key] = new JsonObject() });
        Assert.Empty(Assert.IsType<JsonObject>(machine[key]));
    }

    [Theory(DisplayName = "Tool codecs reject wrong known field shapes"), InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void KnownShapesAreValidated(int shape)
    {
        var root = Codec.WriteLock(new(), new Dictionary<string, ToolLockRecord> { [ToolId] = new() { Version = Version, Channel = "synthetic-channel", Changes = Additions() } });
        var entry = Assert.IsType<JsonObject>(root[ToolRecordDefinitions.ToolsProperty]?[ToolId]);
        switch (shape)
        {
            case 0: entry[ToolRecordDefinitions.VersionProperty] = "latest"; break;
            case 1: entry[ToolRecordDefinitions.ChannelProperty] = 3; break;
            case 2: entry[ToolRecordDefinitions.ChangesProperty] = new JsonObject(); break;
            case 3: entry[ToolRecordDefinitions.ProfileDigestProperty] = 3; break;
            case 4: entry[ToolRecordDefinitions.ChangesProperty]![0]![ToolRecordDefinitions.PathProperty] = "../escape.json"; break;
            default: throw new ArgumentOutOfRangeException(nameof(shape));
        }
        Assert.Throws<ArgumentException>(() => Codec.ReadLock(root));
    }

    [Fact(DisplayName = "Settings v6 writes keep released v5 fields and unknown keys"), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void SettingsDeriveFromReleasedShape()
    {
        var original = new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = ToolRecordDefinitions.ReleasedSettingsSchemaVersion,
            [WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray("team"),
            ["future"] = new JsonObject { ["flag"] = true },
        };
        var result = Codec.WriteSettings(original, new Dictionary<string, ToolSetting> { [ToolId] = new(new(3, 2)) });
        var added = WorkspaceSettingsCodec.AddAllowInstallPath(ToolRecordJson.Write(result), "notes");
        Assert.NotNull(added);
        var read = ToolRecordJson.Read(added);
        Assert.True(JsonNode.DeepEquals(original["future"], read["future"]));
        Assert.True(JsonNode.DeepEquals(original[WorkspaceSettingsDefinitions.OutsideRoutesProperty], read[WorkspaceSettingsDefinitions.OutsideRoutesProperty]));
        Assert.Equal(ToolRecordDefinitions.SettingsSchemaVersion, WorkspaceSettingsDefinitions.SchemaVersion);
    }

    [Fact(DisplayName = "Ownership rewrites keep Tool and unknown records and release removed Framework facts"), Trait("Feature", "tool-records"), Trait("Evidence", "Unit")]
    public void OwnershipKeepsTools()
    {
        var root = Codec.WriteLock(new(), new Dictionary<string, ToolLockRecord> { [ToolId] = new() { Version = Version, Channel = "synthetic-channel", Changes = [] } });
        root[WorkspaceOwnershipDefinitions.SchemaVersionProperty] = ToolRecordDefinitions.ReleasedLockSchemaVersion;
        root[WorkspaceOwnershipDefinitions.FrameworkProperty] = new JsonObject { ["source"] = new JsonObject { ["id"] = "synthetic-core" }, ["future"] = true };
        root["future-root"] = 8;
        var read = WorkspaceOwnershipCodec.Read(ToolRecordJson.Write(root));
        Assert.NotNull(read.Document);
        var written = ToolRecordJson.Read(WorkspaceOwnershipCodec.Write(read.Document with { Framework = null }));
        Assert.False(written.ContainsKey(WorkspaceOwnershipDefinitions.FrameworkProperty));
        Assert.True(JsonNode.DeepEquals(root[ToolRecordDefinitions.ToolsProperty], written[ToolRecordDefinitions.ToolsProperty]));
        Assert.True(JsonNode.DeepEquals(root["future-root"], written["future-root"]));
        Assert.Equal(ToolRecordDefinitions.LockSchemaVersion, WorkspaceOwnershipDefinitions.SchemaVersion);
    }

    private static ToolAppliedChange[] Additions()
    {
        using var json = JsonDocument.Parse("[\"synthetic-item\"]");
        var digest = new string('a', 64);
        return
        [
            new ToolJsonUnionAddition { Id = "union", Path = "config.json", Pointer = "/exclude", Items = [json.RootElement[0].Clone()] },
            new ToolJsonValueAddition { Id = "value", Path = "config.json", Pointer = "/enabled", Value = json.RootElement.Clone() },
            new ToolMarkedBlockAddition { Id = "block", Path = "ignore", StartMarker = "synthetic-start", EndMarker = "synthetic-end", ContentSha256 = digest },
            new ToolManagedFileAddition { Id = "file", Path = "synthetic.txt", Sha256 = digest },
            new ToolNativeCommandAddition { Id = "native", AllowListId = "synthetic-setting" },
        ];
    }

    internal sealed class Versions : IToolVersionService
    {
        public ToolSpec? ReadSpec(string value) => value == $"record@{Version}" ? new("record", new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = Version }) : null;
        public ToolVersionRange? ReadRange(string value) => value == Range ? new(3, 2) : null;
        public bool Satisfies(string version, ToolVersionSelection selection) => version == Version;
    }
}
