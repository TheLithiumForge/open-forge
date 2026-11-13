using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

[JsonConverter(typeof(ToolCommandStateJsonConverter))]
internal enum ToolCommandState
{
    [JsonStringEnumMemberName("planned")] Planned,
    [JsonStringEnumMemberName("started")] Started,
    [JsonStringEnumMemberName("exited")] Exited,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("interrupted")] Interrupted,
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCommandSummary(string Action, string? Host, ToolCommandState State, int? ExitCode);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRunSummary(string Action, ToolCommandState State, ToolCommandSummary[] Commands);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCommandReceipt
{
    public required string Action { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Host { get; init; }
    public required string Entry { get; init; }
    public required string EntrySha256 { get; init; }
    public required string[] Args { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
    public required ToolCommandState State { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int? ExitCode { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Cause { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRunRecord
{
    public required string Version { get; init; }
    public required string Target { get; init; }
    public required string ManifestSha256 { get; init; }
    public required byte[] ManifestBytes { get; init; }
    public required string ManifestFormat { get; init; }
    public required string CodeRoot { get; init; }
    public required string[] Hosts { get; init; }
    public required string Action { get; init; }
    public required ToolCommandState State { get; init; }
    public required ToolCommandReceipt[] Commands { get; init; }

    internal ToolRunSummary ToSummary() => new(Action, State,
        [.. Commands.Select(command => new ToolCommandSummary(command.Action, command.Host, command.State, command.ExitCode))]);
}
