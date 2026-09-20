using System.Text.Json.Nodes;

namespace OpenForge.Cli.TestSupport;

public static class JsonDetailComparison
{
    public static bool RetainsData(string minimalJson, string standardJson, bool allowDetailEcho = false)
        => RetainsDataAcrossDetails(minimalJson, standardJson, "minimal", "standard", allowDetailEcho);

    public static bool RetainsDataAcrossDetails(
        string minimalJson,
        string standardJson,
        string firstDetail,
        string secondDetail,
        bool allowDetailEcho = false)
    {
        var minimal = JsonNode.Parse(minimalJson)?.AsObject() ?? throw new FormatException("Missing minimal document.");
        var standard = JsonNode.Parse(standardJson)?.AsObject() ?? throw new FormatException("Missing standard document.");
        string[] coordinates = ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"];
        if (minimalJson.TrimEnd('\r', '\n').IndexOfAny(['\r', '\n']) >= 0
            || standardJson.TrimEnd('\r', '\n').IndexOfAny(['\r', '\n']) >= 0
            || minimal["schemaVersion"]?.GetValue<int>() != 3
            || standard["schemaVersion"]?.GetValue<int>() != 3
            || minimal["detail"]?.GetValue<string>() != firstDetail
            || standard["detail"]?.GetValue<string>() != secondDetail
            || !minimal.Select(property => property.Key).SequenceEqual(coordinates)
            || !standard.Select(property => property.Key).SequenceEqual(coordinates))
        {
            return false;
        }

        foreach (var coordinate in new[] { "command", "status", "filter", "workspace", "counts", "recovery", "next" })
        {
            if (!JsonNode.DeepEquals(minimal[coordinate], standard[coordinate]))
            {
                return false;
            }
        }

        var expected = standard["data"]?.DeepClone() ?? throw new FormatException("Missing standard data.");
        var actual = minimal["data"] ?? throw new FormatException("Missing minimal data.");
        if (allowDetailEcho)
        {
            var expectedPresentation = expected["presentation"]?.AsObject() ?? throw new FormatException("Missing standard presentation.");
            var actualPresentation = actual["presentation"]?.AsObject() ?? throw new FormatException("Missing minimal presentation.");
            expectedPresentation["detail"] = actualPresentation["detail"]?.DeepClone();
        }

        return JsonNode.DeepEquals(expected, actual);
    }
}
