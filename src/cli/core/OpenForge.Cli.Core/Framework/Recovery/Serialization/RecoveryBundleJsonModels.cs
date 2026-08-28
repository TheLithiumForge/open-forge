using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization;

internal sealed record RecoveryBundleManifestV1
{
    [JsonPropertyName("schemaVersion"), JsonPropertyOrder(0)]
    public required int SchemaVersion { get; init; }

    [JsonPropertyName("command"), JsonPropertyOrder(1)]
    public required string Command { get; init; }

    [JsonPropertyName("operationId"), JsonPropertyOrder(2)]
    public required string OperationId { get; init; }

    [JsonPropertyName("workspacePath"), JsonPropertyOrder(3)]
    public required string WorkspacePath { get; init; }

    [JsonPropertyName("workspaceKey"), JsonPropertyOrder(4)]
    public required string WorkspaceKey { get; init; }

    [JsonPropertyName("entries"), JsonPropertyOrder(5)]
    public required RecoveryBundleManifestEntryV1[] Entries { get; init; }
}

internal sealed record RecoveryBundleManifestEntryV1
{
    [JsonPropertyName("ordinal"), JsonPropertyOrder(0)]
    public required int Ordinal { get; init; }

    [JsonPropertyName("target"), JsonPropertyOrder(1)]
    public required string Target { get; init; }

    [JsonPropertyName("kind"), JsonPropertyOrder(2)]
    public required string Kind { get; init; }

    [JsonPropertyName("priorLength"), JsonPropertyOrder(3)]
    public required long PriorLength { get; init; }

    [JsonPropertyName("priorSha256"), JsonPropertyOrder(4)]
    public required string PriorSha256 { get; init; }

    [JsonPropertyName("payload"), JsonPropertyOrder(5)]
    public required string Payload { get; init; }

    [JsonPropertyName("intendedAbsent"), JsonPropertyOrder(6)]
    public required bool IntendedAbsent { get; init; }

    [JsonPropertyName("intendedLength"), JsonPropertyOrder(7)]
    public required long? IntendedLength { get; init; }

    [JsonPropertyName("intendedSha256"), JsonPropertyOrder(8)]
    public required string? IntendedSha256 { get; init; }
}

internal enum RecoveryBundleManifestState
{
    Valid,
    Malformed,
    Unsupported,
}

internal sealed record RecoveryBundleManifestDecodeResult
{
    public required RecoveryBundleManifestState State { get; init; }

    public RecoveryBundleManifestV1? Document { get; init; }

    public required System.Collections.Immutable.ImmutableArray<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundleEntry> Entries { get; init; }

    public string? Cause { get; init; }
}
