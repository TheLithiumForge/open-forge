using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class ExtensionLifecycleState
{
    public required string Coverage { get; init; }

    public required LifecycleExtensionPackageV1[] Packages { get; init; }

    public required LifecycleExtensionPathV1[] Paths { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class LifecycleExtensionPackageV1
{
    public required string Id { get; init; }

    public required string? Version { get; init; }

    public required string? Source { get; init; }

    public required string[] Dependencies { get; init; }

    public required string[] Paths { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class LifecycleExtensionPathV1
{
    public required string Path { get; init; }

    public required string[] Owners { get; init; }

    public required string BaselineFingerprint { get; init; }

    public required string FingerprintKind { get; init; }
}
