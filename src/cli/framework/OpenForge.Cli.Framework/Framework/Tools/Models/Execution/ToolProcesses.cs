using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolProcesses
{
    public required string Telemetry { get; init; }

    public required string UpdateChecks { get; init; }

    public required string Daemon { get; init; }

    public required string Watcher { get; init; }

    public required string AutoIndex { get; init; }

    public required string Hooks { get; init; }

    public required string Consent { get; init; }

    public required string Coordination { get; init; }

    public required Dictionary<string, string> Environment { get; init; }
}
