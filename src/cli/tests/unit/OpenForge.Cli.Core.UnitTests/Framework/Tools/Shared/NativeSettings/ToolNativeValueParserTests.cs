using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.NativeSettings;

[Trait("Feature", "tool-native-settings"), Trait("Evidence", "Unit")]
public sealed class ToolNativeValueParserTests
{
    [Theory(DisplayName = "Stored CodeGraph values require the complete tagged enabled and identity token kinds")]
    [InlineData(true, JsonValueKind.String), InlineData(false, JsonValueKind.String), InlineData(false, JsonValueKind.Null)]
    public void AcceptedStoredValue(bool enabled, JsonValueKind identity)
    {
        var result = ToolNativeValueParser.Stored(ToolNativeStoredValueParser.CodeGraphTelemetryV1, Stored(enabled, identity), ToolNativeConfigDefinitions.EnabledValue);
        Assert.Equal(enabled ? ToolNativeConfigDefinitions.EnabledValue : ToolNativeConfigDefinitions.DisabledValue, result);
    }

    [Theory(DisplayName = "Rejected identity kinds block instead of silently guessing the native default")]
    [InlineData(true, JsonValueKind.Null), InlineData(true, JsonValueKind.Undefined), InlineData(false, JsonValueKind.Undefined), InlineData(false, JsonValueKind.Number), InlineData(true, JsonValueKind.Object)]
    public void RejectedStoredValue(bool enabled, JsonValueKind identity)
        => Assert.Throws<InvalidDataException>(() => ToolNativeValueParser.Stored(ToolNativeStoredValueParser.CodeGraphTelemetryV1, Stored(enabled, identity), ToolNativeConfigDefinitions.EnabledValue));

    [Theory(DisplayName = "Malformed non-object and wrongly typed stored native settings block")]
    [InlineData("null"), InlineData("[]"), InlineData("{"), InlineData("{\"enabled\":\"false\"}")]
    public void InvalidJson(string source)
        => Assert.Throws<InvalidDataException>(() => ToolNativeValueParser.Stored(ToolNativeStoredValueParser.CodeGraphTelemetryV1, Encoding.UTF8.GetBytes(source), ToolNativeConfigDefinitions.EnabledValue));

    [Fact(DisplayName = "Missing enabled uses the declared default without interpreting unrelated fields")]
    public void MissingEnabled()
        => Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, ToolNativeValueParser.Stored(ToolNativeStoredValueParser.CodeGraphTelemetryV1,
            Encoding.UTF8.GetBytes("{\"machine_id\":42,\"other\":false}"), ToolNativeConfigDefinitions.EnabledValue));

    [Theory(DisplayName = "Command reads accept one complete setting line with a declared reason after ANSI handling")]
    [InlineData(true, true), InlineData(true, false), InlineData(false, true), InlineData(false, false)]
    public void Status(bool enabled, bool saved)
    {
        var state = enabled ? ToolNativeConfigDefinitions.EnabledStatus : ToolNativeConfigDefinitions.DisabledStatus;
        var reason = saved ? ToolNativeConfigDefinitions.SavedChoiceReason : ToolNativeConfigDefinitions.DefaultReason;
        var input = $"ignored\r\n\u001b[32m{ToolNativeConfigDefinitions.TelemetryStatusPrefix}{state} ({reason})\u001b[0m\r\nignored\r\n";
        Assert.Equal(enabled ? ToolNativeConfigDefinitions.EnabledValue : ToolNativeConfigDefinitions.DisabledValue,
            ToolNativeValueParser.Command(ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId, input));
    }

    [Theory(DisplayName = "Missing duplicated partial and unknown native status facts block")]
    [InlineData(""), InlineData("Telemetry: disabled (other)"), InlineData("Telemetry: disabled (default)\nTelemetry: enabled (default)"), InlineData("Telemetry: unknown (default)")]
    public void InvalidStatus(string source) => Assert.Throws<InvalidDataException>(() => ToolNativeValueParser.Command(ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId, source));

    [Fact(DisplayName = "Unknown stored and command parser identities are rejected")]
    public void UnknownParsers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolNativeValueParser.Stored((ToolNativeStoredValueParser)999, [], ToolNativeConfigDefinitions.EnabledValue));
        Assert.Throws<InvalidDataException>(() => ToolNativeValueParser.Command("synthetic-unknown", string.Empty));
        Assert.Equal(new[] { ToolNativeStoredValueParser.CodeGraphTelemetryV1 }, Enum.GetValues<ToolNativeStoredValueParser>());
    }

    private static byte[] Stored(bool enabled, JsonValueKind identity)
    {
        using var bytes = new MemoryStream();
        using (var writer = new Utf8JsonWriter(bytes))
        {
            writer.WriteStartObject();
            writer.WriteBoolean(ToolNativeValueParser.EnabledProperty, enabled);
            switch (identity)
            {
                case JsonValueKind.String: writer.WriteString(ToolNativeValueParser.MachineIdProperty, "synthetic-not-a-uuid"); break;
                case JsonValueKind.Null: writer.WriteNull(ToolNativeValueParser.MachineIdProperty); break;
                case JsonValueKind.Number: writer.WriteNumber(ToolNativeValueParser.MachineIdProperty, 42); break;
                case JsonValueKind.Object: writer.WriteStartObject(ToolNativeValueParser.MachineIdProperty); writer.WriteEndObject(); break;
                case JsonValueKind.Undefined: break;
                default: throw new ArgumentOutOfRangeException(nameof(identity));
            }
            writer.WriteEndObject();
        }
        return bytes.ToArray();
    }
}
