using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.References.Models.Inspection;

internal sealed record ReferencesAuthoredLink
{
    internal ReferencesAuthoredLink(
        ReferencesDirection direction,
        ReferencesSource source,
        SourceLayerKind layer,
        string canonicalPath,
        SourceLocation location,
        SourceLocation? destinationLocation,
        string rawDestination,
        string? fragment,
        ReferencesProvenance provenance)
    {
        if (direction is not (ReferencesDirection.In or ReferencesDirection.Out))
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "An authored References link direction must be incoming or outgoing.");
        }

        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(layer))
        {
            throw new ArgumentOutOfRangeException(nameof(layer), layer, "The authored References source layer is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(rawDestination);
        if (!Enum.IsDefined(provenance)
            || (direction == ReferencesDirection.Out) != (provenance == ReferencesProvenance.SelectedSource))
        {
            throw new ArgumentException("Authored References provenance must match its incoming or outgoing direction.", nameof(provenance));
        }

        Direction = direction;
        Source = source;
        Layer = layer;
        CanonicalPath = canonicalPath;
        Location = location;
        DestinationLocation = destinationLocation;
        RawDestination = rawDestination;
        Fragment = fragment;
        Provenance = provenance;
    }

    internal ReferencesDirection Direction { get; }

    internal ReferencesSource Source { get; }

    internal SourceLayerKind Layer { get; }

    internal string CanonicalPath { get; }

    internal SourceLocation Location { get; }

    internal SourceLocation? DestinationLocation { get; }

    internal string RawDestination { get; }

    internal string? Fragment { get; }

    internal ReferencesProvenance Provenance { get; }
}

internal sealed record ReferencesInspectionFacts
{
    internal ReferencesInspectionFacts(
        ReferencesSource source,
        SourceLayer layer,
        string canonicalPath,
        IEnumerable<ReferencesAuthoredLink> links,
        IEnumerable<ReferencesInspectionFinding> findings,
        bool established,
        bool blocked)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(findings);
        Source = source;
        Layer = layer;
        CanonicalPath = canonicalPath;
        Links = new ReadOnlyCollection<ReferencesAuthoredLink>(links
            .Select(value => value ?? throw new ArgumentException("Inspection links cannot contain null members.", nameof(links)))
            .ToArray());
        Findings = new ReadOnlyCollection<ReferencesInspectionFinding>(findings
            .Select(value => value ?? throw new ArgumentException("Inspection findings cannot contain null members.", nameof(findings)))
            .ToArray());
        Established = established;
        Blocked = blocked;
    }

    internal ReferencesSource Source { get; }

    internal SourceLayer Layer { get; }

    internal string CanonicalPath { get; }

    internal IReadOnlyList<ReferencesAuthoredLink> Links { get; }

    internal IReadOnlyList<ReferencesInspectionFinding> Findings { get; }

    internal bool Established { get; }

    internal bool Blocked { get; }
}

internal sealed record ReferencesInspectionFinding(
    ReferencesFindingCode Code,
    string Cause,
    bool Blocked,
    SourceLocation? Location = null,
    SourceLocation? DestinationLocation = null);
