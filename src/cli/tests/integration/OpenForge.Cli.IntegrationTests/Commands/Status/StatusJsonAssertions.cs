using System.Text.Json;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusJsonAssertions
{
    internal static JsonElement Result(JsonElement root)
        => root.GetProperty("result");

    internal static long Available(JsonElement value)
        => AvailableNumber(value).GetInt64();

    internal static decimal AvailableDecimal(JsonElement value)
        => AvailableNumber(value).GetDecimal();

    internal static void AvailableValue(JsonElement value, long expected)
        => Assert.Equal(expected, Available(value));

    internal static void AvailableZero(JsonElement value)
        => AvailableValue(value, 0);

    internal static void NotApplicable(JsonElement value)
    {
        Assert.Equal("not-applicable", value.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, value.GetProperty("value").ValueKind);
    }

    internal static void MeasurementAvailable(JsonElement measurement)
    {
        foreach (var name in new[] { "files", "characters", "utf8Bytes", "estimatedTokens" })
        {
            _ = Available(measurement.GetProperty(name));
        }
    }

    internal static void MeasurementNotApplicable(JsonElement measurement)
    {
        foreach (var name in new[] { "files", "characters", "utf8Bytes", "estimatedTokens" })
        {
            NotApplicable(measurement.GetProperty(name));
        }
    }

    internal static void AssertDifference(JsonElement startup)
    {
        foreach (var name in new[] { "files", "characters", "utf8Bytes", "estimatedTokens" })
        {
            var initial = Available(startup.GetProperty("initial").GetProperty(name));
            var current = Available(startup.GetProperty("current").GetProperty(name));
            var difference = Available(startup.GetProperty("difference").GetProperty(name));
            Assert.Equal(current - initial, difference);
        }
    }

    internal static void CompleteInstalled(JsonElement root)
    {
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal("status", root.GetProperty("command").GetString());
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        var result = Result(root);
        var installation = result.GetProperty("installation");
        Assert.Equal("installed", installation.GetProperty("state").GetString());
        Assert.NotNull(installation.GetProperty("entryPath").GetString());
        Assert.NotNull(installation.GetProperty("loaderPath").GetString());

        var context = result.GetProperty("context");
        Assert.Equal("ceiling-characters-divided-by-four", context.GetProperty("tokenEstimator").GetString());
        MeasurementAvailable(context.GetProperty("startup").GetProperty("initial"));
        MeasurementAvailable(context.GetProperty("startup").GetProperty("current"));
        MeasurementAvailable(context.GetProperty("startup").GetProperty("difference"));
        MeasurementAvailable(context.GetProperty("totalAvailable"));
        MeasurementAvailable(context.GetProperty("continuity"));
        _ = AvailableDecimal(context.GetProperty("startupPercentage"));
        Assert.NotEmpty(context.GetProperty("continuitySources").EnumerateArray());

        var structure = result.GetProperty("structure");
        Assert.NotEmpty(structure.GetProperty("generatedNavigation").EnumerateArray());
        var lifecycle = result.GetProperty("lifecycle");
        Assert.Equal("trusted", lifecycle.GetProperty("framework").GetProperty("state").GetString());
        Assert.Equal("available", lifecycle.GetProperty("framework").GetProperty("sourceAvailability").GetString());
        Assert.NotEmpty(lifecycle.GetProperty("framework").GetProperty("targets").EnumerateArray());
        Assert.Equal("trusted", lifecycle.GetProperty("extensions").GetProperty("state").GetString());
        Assert.Equal("not-applicable", lifecycle.GetProperty("extensions").GetProperty("sourceAvailability").GetString());
        Assert.Empty(lifecycle.GetProperty("extensions").GetProperty("installed").EnumerateArray());
        Assert.Empty(lifecycle.GetProperty("extensions").GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        AvailableZero(result.GetProperty("recovery").GetProperty("verifiedFinals"));
        AvailableZero(result.GetProperty("recovery").GetProperty("incompleteDrafts"));
        Assert.Empty(result.GetProperty("recovery").GetProperty("candidates").EnumerateArray());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
    }

    internal static void CompleteGraph(JsonElement root)
    {
        PropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        PropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        var result = Result(root);
        PropertyOrder(result, "installation", "context", "structure", "lifecycle", "recovery", "findings");
        PropertyOrder(result.GetProperty("installation"), "state", "entryPath", "loaderPath");

        var context = result.GetProperty("context");
        PropertyOrder(context, "tokenEstimator", "startup", "totalAvailable", "startupPercentage", "continuity", "continuitySources");
        var startup = context.GetProperty("startup");
        PropertyOrder(startup, "initial", "current", "difference");
        MeasurementGraph(startup.GetProperty("initial"));
        MeasurementGraph(startup.GetProperty("current"));
        MeasurementGraph(startup.GetProperty("difference"));
        MeasurementGraph(context.GetProperty("totalAvailable"));
        var startupPercentage = context.GetProperty("startupPercentage");
        PropertyOrder(startupPercentage, "state", "value");
        _ = AvailableDecimal(startupPercentage);
        MeasurementGraph(context.GetProperty("continuity"));
        foreach (var source in context.GetProperty("continuitySources").EnumerateArray())
        {
            PropertyOrder(source, "sourceId", "utf8Bytes", "layers");
            foreach (var layer in source.GetProperty("layers").EnumerateArray())
            {
                PropertyOrder(layer, "path", "utf8Bytes");
            }
        }

        var structure = result.GetProperty("structure");
        PropertyOrder(structure, "rootCategories", "generatedNavigation");
        PropertyOrder(structure.GetProperty("rootCategories"), "count", "added", "removed");
        ValueGraph(structure.GetProperty("rootCategories").GetProperty("count"));
        foreach (var target in structure.GetProperty("generatedNavigation").EnumerateArray())
        {
            PropertyOrder(target, "path", "state");
        }

        var lifecycle = result.GetProperty("lifecycle");
        PropertyOrder(lifecycle, "framework", "extensions");
        var framework = lifecycle.GetProperty("framework");
        PropertyOrder(framework, "state", "sourceAvailability", "targets");
        foreach (var target in framework.GetProperty("targets").EnumerateArray())
        {
            PropertyOrder(target, "path", "kind", "sourceAssetPath", "region", "baselineFingerprint", "fingerprintKind", "state");
        }

        var extensions = lifecycle.GetProperty("extensions");
        PropertyOrder(extensions, "state", "sourceAvailability", "installed", "managedFiles");
        foreach (var installed in extensions.GetProperty("installed").EnumerateArray())
        {
            PropertyOrder(installed, "id", "version", "source", "sourceAvailability", "dependencies", "paths");
        }

        var managed = extensions.GetProperty("managedFiles");
        PropertyOrder(managed, "counts", "targets");
        PropertyOrder(managed.GetProperty("counts"), "current", "changed", "missing", "unavailable", "blocked");
        foreach (var target in managed.GetProperty("targets").EnumerateArray())
        {
            PropertyOrder(target, "path", "owners", "baselineFingerprint", "fingerprintKind", "state");
        }

        var recovery = result.GetProperty("recovery");
        PropertyOrder(recovery, "verifiedFinals", "incompleteDrafts", "candidates");
        ValueGraph(recovery.GetProperty("verifiedFinals"));
        ValueGraph(recovery.GetProperty("incompleteDrafts"));
        foreach (var candidate in recovery.GetProperty("candidates").EnumerateArray())
        {
            PropertyOrder(candidate, "path", "kind", "integrity");
        }

        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    internal static void PropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));

    private static void MeasurementGraph(JsonElement measurement)
    {
        PropertyOrder(measurement, "files", "characters", "utf8Bytes", "estimatedTokens");
        foreach (var name in new[] { "files", "characters", "utf8Bytes", "estimatedTokens" })
        {
            ValueGraph(measurement.GetProperty(name));
        }
    }

    private static void ValueGraph(JsonElement value)
    {
        PropertyOrder(value, "state", "value");
        var state = Assert.IsType<string>(value.GetProperty("state").GetString());
        Assert.True(state is "available" or "unavailable" or "not-applicable");
        if (state == "available")
        {
            Assert.Equal(JsonValueKind.Number, value.GetProperty("value").ValueKind);
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, value.GetProperty("value").ValueKind);
        }
    }

    private static JsonElement AvailableNumber(JsonElement value)
    {
        Assert.Equal("available", value.GetProperty("state").GetString());
        var number = value.GetProperty("value");
        Assert.Equal(JsonValueKind.Number, number.ValueKind);
        return number;
    }
}
