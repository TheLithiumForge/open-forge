using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Registration;

// Exact contracts: technical-designs/tool-lane-contracts.md and the shared Tool manifest contract.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ToolJsonCommandRewrite), "json-command")]
[JsonDerivedType(typeof(ToolTomlCommandRewrite), "toml-command")]
[JsonDerivedType(typeof(ToolHookCommandRewrite), "hook-command")]
[JsonDerivedType(typeof(ToolMarkedLineRewrite), "marked-line")]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal abstract record ToolEntryRewrite
{
    public required string Id { get; init; }
    public required string Host { get; init; }
    public required string File { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Component { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCommandValue([property: JsonRequired] string Command, [property: JsonRequired] string[] Args);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolJsonCommandRewrite : ToolEntryRewrite
{
    public required string Pointer { get; init; }
    public required ToolCommandValue Expected { get; init; }
    public required ToolCommandValue Replacement { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolTomlCommandRewrite : ToolEntryRewrite
{
    public required string Table { get; init; }
    public required ToolCommandValue Expected { get; init; }
    public required ToolCommandValue Replacement { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolHookCommandRewrite : ToolEntryRewrite
{
    public required string Pointer { get; init; }
    public required string Expected { get; init; }
    public required string Replacement { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolMarkedLineRewrite : ToolEntryRewrite
{
    public required string StartMarker { get; init; }
    public required string EndMarker { get; init; }
    public required string Expected { get; init; }
    public required string Replacement { get; init; }
}
