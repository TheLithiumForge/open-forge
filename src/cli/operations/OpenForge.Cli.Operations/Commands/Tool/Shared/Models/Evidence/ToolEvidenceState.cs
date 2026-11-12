using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

[JsonConverter(typeof(JsonStringEnumConverter<ToolEvidenceState>))]
internal enum ToolEvidenceState
{
    [JsonStringEnumMemberName("complete")] Complete,
    [JsonStringEnumMemberName("unavailable")] Unavailable,
    [JsonStringEnumMemberName("matching")] Matching,
    [JsonStringEnumMemberName("changed")] Changed,
    [JsonStringEnumMemberName("verified")] Verified,
    [JsonStringEnumMemberName("missing")] Missing,
    [JsonStringEnumMemberName("inactive")] Inactive,
    [JsonStringEnumMemberName("drifted")] Drifted,
    [JsonStringEnumMemberName("excluded")] Excluded,
    [JsonStringEnumMemberName("needs-index")] NeedsIndex,
    [JsonStringEnumMemberName("present-unverified")] PresentUnverified,
    [JsonStringEnumMemberName("fresh")] Fresh,
    [JsonStringEnumMemberName("stale")] Stale,
    [JsonStringEnumMemberName("rebuild-required")] RebuildRequired,
    [JsonStringEnumMemberName("deletion-unsafe")] DeletionUnsafe,
}
