using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class FrameworkLifecycleState
{
    public required string Coverage { get; init; }

    public required FrameworkLifecycleSource Source { get; init; }

    public required FrameworkLifecycleTarget[] Targets { get; init; }

    public required FrameworkGeneratedRegion[] GeneratedRegions { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class FrameworkLifecycleSource
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string InventoryFingerprint { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class FrameworkLifecycleTarget
{
    public required string Path { get; init; }

    public required string? SourceAssetPath { get; init; }

    public required string? Region { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class FrameworkGeneratedRegion
{
    public required string Path { get; init; }

    public required string Region { get; init; }
}
