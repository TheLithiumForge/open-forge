using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions.Serialization;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class ExtensionManifestDocument
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required string[] Dependencies { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class EmbeddedExtensionInventoryDocument
{
    public required int SchemaVersion { get; init; }

    public required EmbeddedExtensionInventoryPackage[] Packages { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class EmbeddedExtensionInventoryPackage
{
    public required string Id { get; init; }

    public required EmbeddedExtensionInventoryAsset[] Assets { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class EmbeddedExtensionInventoryAsset
{
    public required string Path { get; init; }

    public required string Sha256 { get; init; }
}
