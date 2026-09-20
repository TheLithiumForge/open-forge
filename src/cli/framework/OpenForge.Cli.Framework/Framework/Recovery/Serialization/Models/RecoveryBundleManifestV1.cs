using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

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

    [JsonPropertyName("attribution"), JsonPropertyOrder(5)]
    public required RecoveryBundleAttributionV1 Attribution { get; init; }

    [JsonPropertyName("entries"), JsonPropertyOrder(6)]
    public required RecoveryBundleManifestEntryV1[] Entries { get; init; }
}

internal sealed record RecoveryBundleAttributionV1
{
    [JsonPropertyName("producer"), JsonPropertyOrder(0)]
    public required string Producer { get; init; }

    [JsonPropertyName("operation"), JsonPropertyOrder(1)]
    public required string Operation { get; init; }

    [JsonPropertyName("subject"), JsonPropertyOrder(2)]
    public required RecoveryBundleSubjectV1 Subject { get; init; }
}

internal sealed record RecoveryBundleSubjectV1
{
    [JsonPropertyName("kind"), JsonPropertyOrder(0)]
    public required string Kind { get; init; }

    [JsonPropertyName("identity"), JsonPropertyOrder(1)]
    public required string Identity { get; init; }
}

internal sealed record RecoveryBundleManifestEntryV1
{
    [JsonPropertyName("ordinal"), JsonPropertyOrder(0)]
    public required int Ordinal { get; init; }

    [JsonPropertyName("logicalPath"), JsonPropertyOrder(1)]
    public required string LogicalPath { get; init; }

    [JsonPropertyName("kind"), JsonPropertyOrder(2)]
    public required string Kind { get; init; }

    [JsonPropertyName("prior"), JsonPropertyOrder(3)]
    public required RecoveryBundleManifestStateV1 Prior { get; init; }

    [JsonPropertyName("intended"), JsonPropertyOrder(4)]
    public required RecoveryBundleManifestStateV1 Intended { get; init; }

    [JsonPropertyName("priorPayload"), JsonPropertyOrder(5)]
    public required string? PriorPayload { get; init; }
}

internal sealed record RecoveryBundleManifestStateV1
{
    [JsonPropertyName("kind"), JsonPropertyOrder(0)]
    public required string Kind { get; init; }

    [JsonPropertyName("length"), JsonPropertyOrder(1)]
    public required long? Length { get; init; }

    [JsonPropertyName("sha256"), JsonPropertyOrder(2)]
    public required string? Sha256 { get; init; }

    [JsonPropertyName("linkKind"), JsonPropertyOrder(3)]
    public required string? LinkKind { get; init; }

    [JsonPropertyName("rawRelativeTarget"), JsonPropertyOrder(4)]
    public required string? RawRelativeTarget { get; init; }
}
