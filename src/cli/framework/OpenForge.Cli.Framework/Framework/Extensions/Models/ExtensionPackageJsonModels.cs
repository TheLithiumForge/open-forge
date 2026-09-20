using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed class ExtensionManifestDocument
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required string[] Dependencies { get; init; }
}
