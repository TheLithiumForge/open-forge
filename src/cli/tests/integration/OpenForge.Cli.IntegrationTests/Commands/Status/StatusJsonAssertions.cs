using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusJsonAssertions
{
    private static readonly string[] MeasurementProperties = ["files", "characters", "bytes", "tokens"];

    internal static JsonElement Result(JsonElement root)
        => root.GetProperty("data");

    internal static long Available(JsonElement value)
    {
        Assert.Equal(JsonValueKind.Number, value.ValueKind);
        return value.GetInt64();
    }

    internal static void AvailableValue(JsonElement value, long expected)
        => Assert.Equal(expected, Available(value));

    internal static void AvailableZero(JsonElement value)
        => AvailableValue(value, 0);

    internal static void MeasurementAvailable(JsonElement measurement)
    {
        PropertyOrder(measurement, MeasurementProperties);
        foreach (var name in MeasurementProperties)
            Available(measurement.GetProperty(name));
    }

    internal static void MeasurementNullable(JsonElement measurement)
    {
        PropertyOrder(measurement, MeasurementProperties);
        foreach (var name in MeasurementProperties)
        {
            var value = measurement.GetProperty(name);
            Assert.True(value.ValueKind is JsonValueKind.Number or JsonValueKind.Null);
        }
    }

    internal static void AssertDifference(JsonElement startup)
    {
        var shipped = startup.GetProperty("shipped");
        var current = startup.GetProperty("current");
        var difference = startup.GetProperty("difference");
        foreach (var name in MeasurementProperties)
        {
            var shippedValue = shipped.GetProperty(name);
            var currentValue = current.GetProperty(name);
            var differenceValue = difference.GetProperty(name);
            if (shippedValue.ValueKind == JsonValueKind.Number
                && currentValue.ValueKind == JsonValueKind.Number)
            {
                Assert.Equal(
                    currentValue.GetInt64() - shippedValue.GetInt64(),
                    Available(differenceValue));
            }
            else
            {
                Assert.Equal(JsonValueKind.Null, differenceValue.ValueKind);
            }
        }
    }

    internal static void CompleteInstalled(JsonElement root)
    {
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("status", root.GetProperty("command").GetString());
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        CompleteGraph(root);
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
    }

    internal static void CompleteGraph(JsonElement root)
    {
        PropertyOrder(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        PropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        Assert.Equal("status", root.GetProperty("command").GetString());
        var data = Result(root);
        PropertyOrder(data, "installation", "context", "structure", "frameworkFiles", "entriesSections", "extensions", "libraries", "recovery");
        PropertyOrder(data.GetProperty("installation"), "state", "entryPath", "loaderPath");

        var context = data.GetProperty("context");
        PropertyOrder(context, "startup", "allRouted", "startupShare", "mayLoadAgainSources");
        var startup = context.GetProperty("startup");
        PropertyOrder(startup, "shipped", "current", "difference", "mayLoadAgain");
        MeasurementAvailable(startup.GetProperty("shipped"));
        MeasurementAvailable(startup.GetProperty("current"));
        MeasurementAvailable(startup.GetProperty("difference"));
        MeasurementAvailable(startup.GetProperty("mayLoadAgain"));
        MeasurementAvailable(context.GetProperty("allRouted"));
        Assert.Equal(JsonValueKind.Number, context.GetProperty("startupShare").ValueKind);
        foreach (var source in context.GetProperty("mayLoadAgainSources").EnumerateArray())
        {
            PropertyOrder(source, "sourceId", "bytes", "layers");
            foreach (var layer in source.GetProperty("layers").EnumerateArray())
                PropertyOrder(layer, "path", "bytes");
        }

        var structure = data.GetProperty("structure");
        PropertyOrder(structure, "rootCategories");
        var rootCategories = structure.GetProperty("rootCategories");
        PropertyOrder(rootCategories, "count", "added", "removed");
        Assert.Equal(JsonValueKind.Number, rootCategories.GetProperty("count").ValueKind);

        foreach (var file in data.GetProperty("frameworkFiles").EnumerateArray())
            PropertyOrder(file, "path", "state");
        foreach (var entry in data.GetProperty("entriesSections").EnumerateArray())
            PropertyOrder(entry, "path", "state");
        foreach (var extension in data.GetProperty("extensions").EnumerateArray())
        {
            PropertyOrder(extension, "id", "version", "files");
            foreach (var file in extension.GetProperty("files").EnumerateArray())
                PropertyOrder(file, "path", "state");
        }

        foreach (var library in data.GetProperty("libraries").EnumerateArray())
        {
            PropertyOrder(library, "id", "sourceRoot", "destinationRoot", "links");
            foreach (var link in library.GetProperty("links").EnumerateArray())
                PropertyOrder(link, "path", "state", "expectedTarget", "observedTarget");
        }

        var recovery = data.GetProperty("recovery");
        PropertyOrder(recovery, "candidates");
        foreach (var candidate in recovery.GetProperty("candidates").EnumerateArray())
            PropertyOrder(candidate, "path", "kind", "integrity");

        var counts = root.GetProperty("counts");
        PropertyOrder(counts,
            "routedFiles", "startupFiles", "startupTokens", "mayLoadAgainFiles", "mayLoadAgainTokens",
            "allTokens", "startupShare", "rootCategories", "entriesSectionsCurrent", "entriesSectionsStale",
            "entriesSectionsMissing", "frameworkFilesCurrent", "frameworkFilesChanged", "frameworkFilesMissing",
            "extensionsInstalled", "librariesRegistered", "libraryLinksCurrent", "libraryLinksMissing",
            "libraryLinksChanged", "recoveryBundles", "recoveryDrafts");
        foreach (var property in counts.EnumerateObject())
            Assert.True(property.Value.ValueKind is JsonValueKind.Number or JsonValueKind.Null);

        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    internal static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
