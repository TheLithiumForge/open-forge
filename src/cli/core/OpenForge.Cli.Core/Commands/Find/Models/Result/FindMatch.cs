using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal sealed record FindMatch
{
    internal FindMatch(
        int position,
        string id,
        string path,
        string? description,
        IEnumerable<FindEvidence> evidence,
        IEnumerable<FindProjection> projections)
    {
        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position, "A Find match position must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!SourceLogicalPath.IsCanonicalSource(path)
            || !path.EndsWith(".md", StringComparison.Ordinal)
            || path.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            throw new ArgumentException("Find match paths must be canonical base Markdown source paths.", nameof(path));
        }

        if (!string.Equals(SourceIdentity.DeriveId(path), id, StringComparison.Ordinal))
        {
            throw new ArgumentException("A Find match ID must be derived from its canonical base path.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(projections);
        var materializedEvidence = evidence.ToArray();
        var materializedProjections = projections.ToArray();
        if (materializedEvidence.Any(value => value is null)
            || materializedProjections.Any(value => value is null))
        {
            throw new ArgumentException("Find matches cannot contain null evidence or projections.");
        }

        ValidateNestedIdentity(
            position,
            id,
            path,
            materializedEvidence,
            materializedProjections);
        ValidateEvidenceOrder(materializedEvidence);
        ValidateProjectionOrder(materializedProjections);
        Position = position;
        Id = id;
        Path = path;
        Description = description;
        Evidence = Array.AsReadOnly(materializedEvidence);
        Projections = Array.AsReadOnly(materializedProjections);
    }

    internal int Position { get; }

    internal string Id { get; }

    internal string Path { get; }

    internal string? Description { get; }

    internal IReadOnlyList<FindEvidence> Evidence { get; }

    internal IReadOnlyList<FindProjection> Projections { get; }

    private static void ValidateNestedIdentity(
        int position,
        string id,
        string basePath,
        IReadOnlyList<FindEvidence> evidence,
        IReadOnlyList<FindProjection> projections)
    {
        var overwritePath = $"{basePath[..^3]}.overwrite.md";
        if (evidence.Any(value => !string.Equals(
                value.Path,
                value.Layer == SourceLayerKind.Base ? basePath : overwritePath,
                StringComparison.Ordinal)))
        {
            throw new ArgumentException("Find evidence must belong to its enclosing logical source.", nameof(evidence));
        }

        foreach (var projection in projections)
        {
            if (projection.Part == FindContentPartKind.Metadata)
            {
                if (projection.Metadata is { } metadata
                    && (metadata.Position != position
                        || !string.Equals(metadata.Id, id, StringComparison.Ordinal)
                        || !string.Equals(metadata.Path, basePath, StringComparison.Ordinal)))
                {
                    throw new ArgumentException("Find metadata must identify its enclosing match.", nameof(projections));
                }

                continue;
            }

            var expectedPath = projection.Layer == SourceLayerKind.Base
                ? basePath
                : overwritePath;
            if (!string.Equals(projection.Path, expectedPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("Find projections must belong to their enclosing logical source.", nameof(projections));
            }
        }
    }

    private static void ValidateEvidenceOrder(IReadOnlyList<FindEvidence> evidence)
    {
        for (var index = 1; index < evidence.Count; index++)
        {
            var previous = evidence[index - 1];
            var current = evidence[index];
            if (current.Predicate < previous.Predicate
                || current.Predicate == previous.Predicate
                    && ReadRegionRank(current.Region) < ReadRegionRank(previous.Region)
                || current.Predicate == previous.Predicate
                    && ReadRegionRank(current.Region) == ReadRegionRank(previous.Region)
                    && ReadLayerRank(current.Layer) < ReadLayerRank(previous.Layer)
                || current.Predicate == previous.Predicate
                    && ReadRegionRank(current.Region) == ReadRegionRank(previous.Region)
                    && current.Layer == previous.Layer
                    && current.Location.ByteOffset < previous.Location.ByteOffset
                || current.Predicate == previous.Predicate
                    && ReadRegionRank(current.Region) == ReadRegionRank(previous.Region)
                    && current.Layer == previous.Layer
                    && current.Location.ByteOffset == previous.Location.ByteOffset
                    && current.Occurrence < previous.Occurrence)
            {
                throw new ArgumentException("Find evidence must use deterministic predicate, region, layer, location, and occurrence order.", nameof(evidence));
            }
        }
    }

    private static void ValidateProjectionOrder(IReadOnlyList<FindProjection> projections)
    {
        for (var index = 1; index < projections.Count; index++)
        {
            if (ReadProjectionRank(projections[index]) < ReadProjectionRank(projections[index - 1]))
            {
                throw new ArgumentException("Find projections must use canonical layer and part order.", nameof(projections));
            }
        }
    }

    private static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body or FindRegionKind.Section => 1,
            FindRegionKind.Document => throw new ArgumentException("Find evidence cannot use the document union as its actual region.", nameof(region)),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined."),
        };

    private static int ReadLayerRank(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static int ReadProjectionRank(FindProjection projection)
    {
        if (projection.Part == FindContentPartKind.Metadata)
        {
            return 0;
        }

        var layerRank = projection.Layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Layer, "The Find projection layer is not defined."),
        };
        var partRank = projection.Part switch
        {
            FindContentPartKind.Frontmatter => 0,
            FindContentPartKind.Headings => 1,
            FindContentPartKind.Body => 2,
            FindContentPartKind.Section => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Find projection part is not defined."),
        };
        return checked(1 + layerRank * 10 + partRank);
    }
}
