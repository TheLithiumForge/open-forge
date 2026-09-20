using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal static class CommandOutputSnapshotFormatting
{
    internal static string PrettyPrintJson(string content)
    {
        using var document = JsonDocument.Parse(content);
        using var stream = new MemoryStream();
        // This is storage formatting only: parsed values, property order, and
        // array order remain intact. LF prevents platform-only snapshot churn.
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = true,
            NewLine = "\n",
        }))
        {
            document.RootElement.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
    }
}
