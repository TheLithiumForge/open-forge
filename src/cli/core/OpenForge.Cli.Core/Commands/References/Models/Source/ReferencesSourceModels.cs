using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.References.Models.Source;

internal sealed record ReferencesSourceIdentity
{
    internal ReferencesSourceIdentity(string id, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Id = id;
        Path = path;
    }

    internal string Id { get; }

    internal string Path { get; }
}

internal sealed record ReferencesSourceLayer
{
    internal ReferencesSourceLayer(SourceLayerKind kind, string path)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The References source-layer kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Kind = kind;
        Path = path;
    }

    internal SourceLayerKind Kind { get; }

    internal string Path { get; }
}

internal sealed record ReferencesSource
{
    internal ReferencesSource(
        string id,
        string path,
        IEnumerable<ReferencesSourceLayer> layers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(layers);
        var materialized = layers
            .Select(layer => layer ?? throw new ArgumentException("Source layers cannot contain null members.", nameof(layers)))
            .ToArray();
        if (materialized.Length == 0
            || materialized[0].Kind != SourceLayerKind.Base
            || materialized.Skip(1).Any(layer => layer.Kind != SourceLayerKind.Overwrite)
            || materialized.Select(layer => layer.Kind).Distinct().Count() != materialized.Length)
        {
            throw new ArgumentException("A References source requires base then optional overwrite layers.", nameof(layers));
        }

        Id = id;
        Path = path;
        Layers = new ReadOnlyCollection<ReferencesSourceLayer>(materialized);
    }

    internal string Id { get; }

    internal string Path { get; }

    internal IReadOnlyList<ReferencesSourceLayer> Layers { get; }

    internal ReferencesSourceIdentity Identity => new(Id, Path);
}
