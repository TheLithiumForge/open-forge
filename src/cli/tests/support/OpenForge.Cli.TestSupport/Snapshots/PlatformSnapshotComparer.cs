using System.Text.RegularExpressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TheLithium.Imprint;

namespace OpenForge.Cli.TestSupport.Snapshots;

// Existing baselines were captured on Windows. Adapt only the expected physical
// path separators, raw relative-link targets and the known held-lock error; received output stays untouched.
// Failure artifacts therefore retain the actual platform's bytes and error code.
public sealed partial class PlatformSnapshotComparer(bool heldLock, bool manifestSharing = false) : ISnapshotComparer
{
    public SnapshotComparisonResult Compare(
        string expected, string received, SnapshotFormat format, ResolvedSnapshotComparison options)
    {
        var platform = OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "macos" : "linux";
        return AdaptExpected(expected, platform, heldLock, manifestSharing).ReplaceLineEndings("\n") == received.ReplaceLineEndings("\n")
            ? SnapshotComparisonResult.Match
            : new SnapshotComparisonResult(false, "Output differs from the platform-specific expectation; see the unchanged received artifact.");
    }

    public static string AdaptExpected(string expected, string platform, bool heldLock, bool manifestSharing = false)
    {
        if (platform == "windows") return expected;
        if (platform is not ("linux" or "macos")) throw new ArgumentOutOfRangeException(nameof(platform));

        expected = PlaceholderPath().Replace(expected, match => match.Value
            .Replace("\\\\", "/", StringComparison.Ordinal).Replace('\\', '/'));
        expected = AdaptLinkTargets(expected);
        if (heldLock)
        {
            // EWOULDBLOCK: Linux 11; Darwin 35 (bsd/sys/errno.h).
            var code = platform == "linux" ? "0x0000000B" : "0x00000023";
            expected = expected.Replace("IOException (0x80070020): The filesystem operation failed.",
                $"IOException ({code}): The filesystem operation failed.", StringComparison.Ordinal);
        }
        if (manifestSharing)
        {
            // Manifest classification recognizes Windows sharing HRESULTs only.
            // Keep the raw OS evidence intact; adapt the two typed output facts.
            expected = expected.Replace("could not be read because it is in use.",
                "could not be read because a filesystem operation failed.", StringComparison.Ordinal)
                .Replace("Close the program holding the file, then retry.",
                    "Check that the file is accessible, then retry.", StringComparison.Ordinal);
        }
        return expected;
    }

    private static string AdaptLinkTargets(string expected)
    {
        if (!expected.TrimStart().StartsWith('{'))
        {
            expected = LinkTargetLine().Replace(expected, match => match.Value.Replace('\\', '/'));
            return LinkStateTargetLine().Replace(expected, match => match.Value.Replace('\\', '/'));
        }
        using var document = JsonDocument.Parse(expected);
        using var stream = new MemoryStream();
        var converted = false;
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = true,
            NewLine = "\n",
        }))
        {
            WriteLinkTargets(writer, document.RootElement, propertyName: null, ref converted);
        }
        return converted ? Encoding.UTF8.GetString(stream.ToArray()) + "\n" : expected;
    }

    private static void WriteLinkTargets(Utf8JsonWriter writer, JsonElement value, string? propertyName, ref bool converted)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in value.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);
                if (property.Name == "target" && property.Value.ValueKind == JsonValueKind.String
                    && value.TryGetProperty("kind", out var kind) && kind.GetString() == "relative-file-link")
                {
                    converted |= property.Value.GetString()?.Contains('\\') == true;
                    writer.WriteStringValue(property.Value.GetString()?.Replace('\\', '/'));
                    continue;
                }
                WriteLinkTargets(writer, property.Value, property.Name, ref converted);
            }
            writer.WriteEndObject();
            return;
        }
        if (value.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in value.EnumerateArray()) WriteLinkTargets(writer, item, propertyName, ref converted);
            writer.WriteEndArray();
            return;
        }
        if (value.ValueKind == JsonValueKind.String
            && propertyName is "rawRelativeTarget" or "observedTarget")
        {
            converted |= value.GetString()?.Contains('\\') == true;
            writer.WriteStringValue(value.GetString()?.Replace('\\', '/'));
            return;
        }
        value.WriteTo(writer);
    }

    [GeneratedRegex("(?:Observed target|Raw relative target):[^\\r\\n]*", RegexOptions.CultureInvariant)]
    private static partial Regex LinkTargetLine();

    [GeneratedRegex("(?:Expected state:[^\\r\\n]*; target |Expected [^\\r\\n]*: relative-file-link;[^\\r\\n]*; target )[^\\r\\n]*", RegexOptions.CultureInvariant)]
    private static partial Regex LinkStateTargetLine();

    [GeneratedRegex("""<(?:workspace|extension-source|recovery-bundle|temp)>[\\/][^\s"',;)\]}]*""", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPath();
}
