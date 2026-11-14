using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

// Account-local facts from full verification. Status reads these facts without refreshing them.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolLaunchBinding
{
    public required string ToolId { get; init; }
    public required string Version { get; init; }
    public required string ManifestSha256 { get; init; }
    public required string StoreId { get; init; }
    public required string Executable { get; init; }
    public required string[] PrefixArguments { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
    public required ToolFileStamp[] Files { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolFileStamp(
    [property: JsonRequired] string StoreId,
    [property: JsonRequired] string Path,
    [property: JsonRequired] long Bytes,
    [property: JsonRequired] long LastWriteTimeUtcTicks);

// The root host forwards raw streams and signals. This is not the captured lifecycle runner.
internal sealed record ToolDispatchRequest(ToolLaunchBinding Launch, string WorkingDirectory, string[] Arguments);
internal delegate ValueTask<int> ToolDispatchProcess(ToolDispatchRequest request, CancellationToken token);
