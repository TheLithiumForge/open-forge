using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;

internal static class Schema3Assertions
{
    internal static void Envelope(JsonElement root, string command, string status, string detail)
    {
        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, root.GetProperty("command").GetString());
        Assert.Equal(status, root.GetProperty("status").GetString());
        Assert.Equal(detail, root.GetProperty("detail").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("filter").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("summary").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("findings").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("effects").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("counts").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("limitations").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("data").ValueKind);
    }

    internal static void ProjectedData(string expectedLegacyDocument, string actual)
    {
        using var expected = JsonDocument.Parse(expectedLegacyDocument);
        using var rendered = JsonDocument.Parse(actual);
        Assert.True(JsonElement.DeepEquals(expected.RootElement.GetProperty("result"), rendered.RootElement.GetProperty("data")),
            "The schema-3 data must retain the complete typed legacy projection.");
        Assert.DoesNotContain("\n", actual.TrimEnd(), StringComparison.Ordinal);
        Assert.DoesNotContain("\r", actual.TrimEnd(), StringComparison.Ordinal);
    }
}
