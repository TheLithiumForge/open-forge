using System.Text.Json;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

internal static partial class ToolNativeValueParser
{
    internal const string EnabledProperty = "enabled";
    internal const string MachineIdProperty = "machine_id";

    internal static string Stored(ToolNativeStoredValueParser parser, byte[] bytes, string missingValue)
    {
        if (parser != ToolNativeStoredValueParser.CodeGraphTelemetryV1) throw new ArgumentOutOfRangeException(nameof(parser));
        try
        {
            using var document = JsonDocument.Parse(bytes);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("The stored native setting must be an object.");
            if (!root.TryGetProperty(EnabledProperty, out var enabled)) return missingValue;
            if (enabled.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new InvalidDataException("The stored native setting has the wrong value kind.");
            if (!root.TryGetProperty(MachineIdProperty, out var identity)
                || (identity.ValueKind != JsonValueKind.String && (enabled.GetBoolean() || identity.ValueKind != JsonValueKind.Null)))
                throw new InvalidDataException("The stored native setting has a rejected identity member kind.");
            // Check only the required token kind. Identity text never leaves the parser.
            return enabled.GetBoolean() ? ToolNativeConfigDefinitions.EnabledValue : ToolNativeConfigDefinitions.DisabledValue;
        }
        catch (JsonException) { throw new InvalidDataException("The stored native setting is not valid JSON."); }
    }

    internal static string Command(string parser, string output)
    {
        if (parser != ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId) throw new InvalidDataException("The native setting parser is unknown.");
        var lines = Ansi().Replace(output, string.Empty).ReplaceLineEndings("\n").Split('\n')
            .Where(line => line.StartsWith(ToolNativeConfigDefinitions.TelemetryStatusPrefix, StringComparison.Ordinal)).ToArray();
        if (lines.Length != 1) throw new InvalidDataException("The native setting status line is missing or duplicated.");
        var value = lines[0][ToolNativeConfigDefinitions.TelemetryStatusPrefix.Length..];
        foreach (var reason in new[] { ToolNativeConfigDefinitions.SavedChoiceReason, ToolNativeConfigDefinitions.DefaultReason })
        {
            if (value == $"{ToolNativeConfigDefinitions.EnabledStatus} ({reason})") return ToolNativeConfigDefinitions.EnabledValue;
            if (value == $"{ToolNativeConfigDefinitions.DisabledStatus} ({reason})") return ToolNativeConfigDefinitions.DisabledValue;
        }
        throw new InvalidDataException("The native setting status value or reason is unknown.");
    }

    [GeneratedRegex(@"\x1b\[[0-?]*[ -/]*[@-~]", RegexOptions.CultureInvariant)]
    private static partial Regex Ansi();
}
