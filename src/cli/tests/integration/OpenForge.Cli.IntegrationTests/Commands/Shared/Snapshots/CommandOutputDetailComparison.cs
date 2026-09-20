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
        Assert.True(JsonNode.DeepEquals(full, debug), "Full and debug JSON differ outside the root detail coordinate.");
    }
}
