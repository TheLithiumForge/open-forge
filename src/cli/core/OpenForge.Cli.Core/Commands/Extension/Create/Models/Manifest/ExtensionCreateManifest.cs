using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;

internal sealed record ExtensionCreateManifest
{
    private IReadOnlyList<string> _dependencies = [];

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<string> Dependencies
    {
        get => _dependencies;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _dependencies = new ReadOnlyCollection<string>(value.ToArray());
        }
    }
}

internal sealed record ExtensionCreateManifestPayload(
    ExtensionCreateManifest Manifest,
    ReadOnlyMemory<byte> Bytes);
