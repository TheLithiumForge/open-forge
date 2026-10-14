using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

internal static class CommandOutputDetailComparison
{
    internal static void AssertFullDebugJson(string fullJson, string debugJson)
    {
        var full = JsonNode.Parse(fullJson)?.AsObject()
            ?? throw new Xunit.Sdk.XunitException("The full JSON output is not an object.");
        var debug = JsonNode.Parse(debugJson)?.AsObject()
            ?? throw new Xunit.Sdk.XunitException("The debug JSON output is not an object.");

        Assert.Equal(CliPresentationDefinitions.Full, full["detail"]?.GetValue<string>());
        Assert.Equal(CliPresentationDefinitions.Debug, debug["detail"]?.GetValue<string>());
        full["detail"] = debug["detail"]?.DeepClone();
        PromoteCommands(full);
        Assert.True(JsonNode.DeepEquals(full, debug), "Full and debug JSON differ outside root detail and retained retry detail options.");
    }

    internal static string PromoteRetryDetail(string value)
        => value.Replace("--detail full", "--detail debug", StringComparison.Ordinal)
            .Replace("--detail=full", "--detail=debug", StringComparison.Ordinal)
            .Replace("--detail:full", "--detail:debug", StringComparison.Ordinal);

    internal static string PromoteTextRetryDetail(string text)
        => string.Join('\n', text.Split('\n').Select(line => line.Contains("open-forge ", StringComparison.Ordinal) ? PromoteRetryDetail(line) : line));

    private static void PromoteCommands(JsonObject report)
    {
        Promote(report["next"] as JsonObject);
        if (report["findings"] is JsonArray findings)
            foreach (var finding in findings.OfType<JsonObject>())
                if (finding["actions"] is JsonArray actions)
                    foreach (var action in actions.OfType<JsonObject>()) Promote(action);
    }

    private static void Promote(JsonObject? action)
    {
        if (action?["command"] is JsonValue value && value.TryGetValue<string>(out var command))
            action["command"] = PromoteRetryDetail(command);
    }
}
