using System.Text.Json.Nodes;

namespace OpenForge.Cli.TestSupport;

public static class JsonViewComparison
{
    public static bool RetainsResult(string compactJson, string expandedJson, string[]? omittedPaths = null, bool allowViewEcho = false)
    {
        var compact = JsonNode.Parse(compactJson)?.AsObject() ?? throw new FormatException("Missing compact document.");
        var expanded = JsonNode.Parse(expandedJson)?.AsObject() ?? throw new FormatException("Missing expanded document.");
        if (compactJson.TrimEnd('\r', '\n').Contains('\n')
            || compact["schemaVersion"]?.GetValue<int>() != 2
            || compact["view"]?.GetValue<string>() != "compact"
            || expanded["schemaVersion"]?.GetValue<int>() != 1
            || !compact.Select(property => property.Key).SequenceEqual(["schemaVersion", "view", "command", "status", "workspace", "result", "next"]))
        {
            return false;
        }

        foreach (var coordinate in new[] { "command", "status", "workspace", "next" })
        {
            if (!JsonNode.DeepEquals(compact[coordinate], expanded[coordinate]))
            {
                return false;
            }
        }

        var expected = expanded["result"]?.DeepClone() ?? throw new FormatException("Missing expanded result.");
        var actual = compact["result"] ?? throw new FormatException("Missing compact result.");
        foreach (var path in omittedPaths ?? [])
        {
            Remove(expected, path.Split('.'), 0);
        }

        if (allowViewEcho)
        {
            var expectedPresentation = expected["presentation"]?.AsObject() ?? throw new FormatException("Missing expanded presentation.");
            var actualPresentation = actual["presentation"]?.AsObject() ?? throw new FormatException("Missing compact presentation.");
            expectedPresentation["view"] = actualPresentation["view"]?.DeepClone();
        }

        return JsonNode.DeepEquals(expected, actual);
    }

    private static void Remove(JsonNode? node, string[] path, int index)
    {
        if (node is null)
        {
            return;
        }

        if (path[index] == "*")
        {
            foreach (var item in node.AsArray())
            {
                Remove(item, path, index + 1);
            }
        }
        else if (index + 1 == path.Length)
        {
            if (!node.AsObject().Remove(path[index]))
            {
                throw new InvalidOperationException($"The expanded contract has no field {string.Join('.', path)}.");
            }
        }
        else
        {
            Remove(node[path[index]], path, index + 1);
        }
    }
}
