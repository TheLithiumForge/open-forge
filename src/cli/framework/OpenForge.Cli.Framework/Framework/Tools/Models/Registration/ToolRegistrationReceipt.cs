using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Registration;

// Snapshots contain command expectations only. They don't confer whole-file ownership.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRewriteReceipt
{
    public required string Id { get; init; }
    public required ToolEntryRewrite Rewrite { get; init; }
    public required bool Declined { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolHookGroupReceipt? CreatedHookGroup { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolHookGroupReceipt(
    [property: JsonRequired] string File,
    [property: JsonRequired] string EventPointer,
    [property: JsonRequired] JsonElement EmptyGroup);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolComponentDeclineReceipt(
    [property: JsonRequired] string Id,
    [property: JsonRequired] ToolJsonEntryRemoval[] AbsentEntries);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolGuidanceReceipt(
    [property: JsonRequired] string Role,
    [property: JsonRequired] string SkillPath,
    [property: JsonRequired] string SkillSha256,
    [property: JsonRequired] string Line);
