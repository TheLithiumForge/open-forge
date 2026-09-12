using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class LifecycleEnvelopeV1
{
    public required int SchemaVersion { get; init; }

    public required string FingerprintPolicy { get; init; }

    public required string WorkspacePath { get; init; }

    public JsonElement? Framework { get; init; }

    public JsonElement? Extensions { get; init; }
}
