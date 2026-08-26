using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.References.Models.Occurrence;

internal enum ReferencesTargetKind
{
    Local,
    External,
    Unsupported,
}

internal enum ReferencesTargetResolution
{
    Complete,
    Missing,
    FragmentMissing,
    Malformed,
    Absolute,
    Query,
    EncodingUnsupported,
    OutsideWorkspace,
    PhysicalEscape,
    Ambiguous,
    Unreadable,
    Unsupported,
    ExternalUnchecked,
}

internal enum ReferencesNetworkState
{
    NetworkNotAttempted,
}

internal enum ReferencesProvenance
{
    SelectedSource,
    DefaultIncomingScan,
    FilteredIncomingScan,
}

internal sealed record ReferencesOccurrenceSource
{
    internal ReferencesOccurrenceSource(string? id, string path, SourceLayerKind layer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(layer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The References occurrence source layer is not defined.");
        }

        Id = id;
        Path = path;
        Layer = layer;
    }

    internal string? Id { get; }

    internal string Path { get; }

    internal SourceLayerKind Layer { get; }
}

internal sealed record ReferencesTarget
{
    internal ReferencesTarget(
        ReferencesTargetKind kind,
        string? id,
        string? path,
        SourceLayerKind? layer,
        ReferencesTargetResolution resolution,
        ReferencesNetworkState? network)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The References target kind is not defined.");
        }

        if (!Enum.IsDefined(resolution))
        {
            throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The References target resolution is not defined.");
        }

        if (layer is { } establishedLayer && !Enum.IsDefined(establishedLayer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The References target layer is not defined.");
        }

        if (network is { } establishedNetwork && !Enum.IsDefined(establishedNetwork))
        {
            throw new ArgumentOutOfRangeException(nameof(network), network, "The References target network state is not defined.");
        }

        if (kind == ReferencesTargetKind.External
            && (resolution != ReferencesTargetResolution.ExternalUnchecked
                || id is not null
                || path is not null
                || layer is not null
                || network != ReferencesNetworkState.NetworkNotAttempted))
        {
            throw new ArgumentException("External target facts do not match external-unchecked resolution.");
        }

        if (kind == ReferencesTargetKind.Unsupported
            && (resolution != ReferencesTargetResolution.Unsupported
                || id is not null
                || path is not null
                || layer is not null
                || network is not null))
        {
            throw new ArgumentException("Unsupported target facts do not match unsupported resolution.");
        }

        if (kind == ReferencesTargetKind.Local
            && (network is not null
                || resolution is ReferencesTargetResolution.Unsupported or ReferencesTargetResolution.ExternalUnchecked))
        {
            throw new ArgumentException("Local target facts cannot carry external or unsupported resolution state.");
        }

        if ((id is not null || layer is not null) && path is null)
        {
            throw new ArgumentException("A References target identity or layer requires a canonical target path.", nameof(path));
        }

        Kind = kind;
        Id = id;
        Path = path;
        Layer = layer;
        Resolution = resolution;
        Network = network;
    }

    internal ReferencesTargetKind Kind { get; }

    internal string? Id { get; }

    internal string? Path { get; }

    internal SourceLayerKind? Layer { get; }

    internal ReferencesTargetResolution Resolution { get; }

    internal ReferencesNetworkState? Network { get; }
}

internal sealed record ReferencesOccurrence
{
    internal ReferencesOccurrence(
        ReferencesDirection direction,
        ReferencesOccurrenceSource source,
        SourceLocation location,
        SourceLocation? destinationLocation,
        string rawDestination,
        string? fragment,
        ReferencesTarget target,
        ReferencesProvenance provenance)
    {
        if (direction is not (ReferencesDirection.In or ReferencesDirection.Out))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "A References occurrence direction must be incoming or outgoing.");
        }

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(rawDestination);
        ArgumentNullException.ThrowIfNull(target);
        if (!Enum.IsDefined(provenance))
        {
            throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The References occurrence provenance is not defined.");
        }

        if ((direction == ReferencesDirection.Out) != (provenance == ReferencesProvenance.SelectedSource))
        {
            throw new ArgumentException("References occurrence provenance must match its incoming or outgoing direction.", nameof(provenance));
        }

        Direction = direction;
        Level = 1;
        Source = source;
        Location = location;
        DestinationLocation = destinationLocation;
        RawDestination = rawDestination;
        Fragment = fragment;
        Target = target;
        Provenance = provenance;
    }

    internal ReferencesDirection Direction { get; }

    internal int Level { get; }

    internal ReferencesOccurrenceSource Source { get; }

    internal SourceLocation Location { get; }

    internal SourceLocation? DestinationLocation { get; }

    internal string RawDestination { get; }

    internal string? Fragment { get; }

    internal ReferencesTarget Target { get; }

    internal ReferencesProvenance Provenance { get; }
}
