using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultMatchBuilder
{
    internal IReadOnlyList<FindMatch> Build(FindResultMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var ordered = input.Matches
            .GroupBy(match => new MatchKey(match.Id, match.Path))
            .Select(group => group.First())
            .OrderBy(match => match.Id, StringComparer.Ordinal)
            .ThenBy(match => match.Path, StringComparer.Ordinal)
            .ToArray();
        var attached = AttachProjections(ordered, input.Projections, input.Content);

        return ordered
            .Select((match, index) =>
            {
                var key = new MatchKey(match.Id, match.Path);
                var matchProjections = attached.TryGetValue(key, out var values)
                    ? OrderProjections(values, input.Content)
                    : [];
                var evidence = match.Evidence
                    .OrderBy(value => value.Predicate)
                    .ThenBy(value => ReadRegionRank(value.Region))
                    .ThenBy(value => ReadLayerRank(value.Layer))
                    .ThenBy(value => value.Location.ByteOffset)
                    .ThenBy(value => value.Location.ByteLength)
                    .ThenBy(value => value.Occurrence)
                    .ToArray();
                return new FindMatch(
                    index + 1,
                    match.Id,
                    match.Path,
                    match.Description,
                    evidence,
                    matchProjections);
            })
            .ToArray();
    }

    private static IReadOnlyDictionary<MatchKey, IReadOnlyList<FindProjection>> AttachProjections(
        IReadOnlyList<FindMatch> matches,
        IReadOnlyList<FindProjection> projections,
        FindContentSelection content)
    {
        if (content.Effective.Count == 0)
        {
            return new Dictionary<MatchKey, IReadOnlyList<FindProjection>>();
        }

        var attached = matches.ToDictionary(
            match => new MatchKey(match.Id, match.Path),
            _ => new List<FindProjection>());
        var unresolvedMetadata = new List<(int Index, FindProjection Projection)>();
        for (var index = 0; index < projections.Count; index++)
        {
            var projection = projections[index];
            if (!IsRequested(projection, content))
            {
                continue;
            }

            var key = ReadProjectionKey(projection, matches);
            if (key is null)
            {
                if (projection.Part == FindContentPartKind.Metadata)
                {
                    unresolvedMetadata.Add((index, projection));
                }

                continue;
            }

            attached[key.Value].Add(projection);
        }

        foreach (var (index, projection) in unresolvedMetadata)
        {
            var key = ReadNearbyPhysicalKey(index, projections, matches, content)
                ?? ReadFirstMetadataFreeKey(attached);
            if (key is not null)
            {
                attached[key.Value].Add(projection);
            }
        }

        return attached.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<FindProjection>)pair.Value);
    }

    private static MatchKey? ReadProjectionKey(
        FindProjection projection,
        IReadOnlyList<FindMatch> matches)
    {
        if (projection.Part == FindContentPartKind.Metadata
            && projection.Metadata is { } metadata)
        {
            return matches.Any(match => string.Equals(match.Id, metadata.Id, StringComparison.Ordinal)
                    && string.Equals(match.Path, metadata.Path, StringComparison.Ordinal))
                ? new MatchKey(metadata.Id, metadata.Path)
                : null;
        }

        if (projection.Path is null)
        {
            return null;
        }

        var basePath = projection.Path.EndsWith(".overwrite.md", StringComparison.Ordinal)
            ? projection.Path[..^".overwrite.md".Length] + ".md"
            : projection.Path;
        var match = matches.FirstOrDefault(value => string.Equals(value.Path, basePath, StringComparison.Ordinal));
        return match is null ? null : new MatchKey(match.Id, match.Path);
    }

    private static MatchKey? ReadNearbyPhysicalKey(
        int index,
        IReadOnlyList<FindProjection> projections,
        IReadOnlyList<FindMatch> matches,
        FindContentSelection content)
    {
        for (var next = index + 1; next < projections.Count; next++)
        {
            if (!IsRequested(projections[next], content) || projections[next].Path is null)
            {
                continue;
            }

            return ReadProjectionKey(projections[next], matches);
        }

        for (var previous = index - 1; previous >= 0; previous--)
        {
            if (!IsRequested(projections[previous], content) || projections[previous].Path is null)
            {
                continue;
            }

            return ReadProjectionKey(projections[previous], matches);
        }

        return null;
    }

    private static MatchKey? ReadFirstMetadataFreeKey(
        IReadOnlyDictionary<MatchKey, List<FindProjection>> attached)
    {
        foreach (var pair in attached.OrderBy(pair => pair.Key.Id, StringComparer.Ordinal)
                     .ThenBy(pair => pair.Key.Path, StringComparer.Ordinal))
        {
            if (!pair.Value.Any(projection => projection.Part == FindContentPartKind.Metadata))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static IReadOnlyList<FindProjection> OrderProjections(
        IReadOnlyList<FindProjection> projections,
        FindContentSelection content)
        => projections
            .OrderBy(ReadProjectionRank)
            .ThenBy(projection => projection.Part == FindContentPartKind.Section
                && projection.Location is null
                ? 1
                : 0)
            .ThenBy(projection => projection.Part == FindContentPartKind.Section
                ? projection.Location?.ByteOffset ?? long.MaxValue
                : 0)
            .ThenBy(projection => ReadContentPartIndex(projection, content))
            .ThenBy(projection => projection.Path, StringComparer.Ordinal)
            .ToArray();

    private static bool IsRequested(FindProjection projection, FindContentSelection content)
        => content.Effective.Any(part => part.Kind == projection.Part
            && string.Equals(part.Name, projection.Name, StringComparison.Ordinal));

    private static int ReadContentPartIndex(
        FindProjection projection,
        FindContentSelection content)
    {
        for (var index = 0; index < content.Effective.Count; index++)
        {
            var part = content.Effective[index];
            if (part.Kind == projection.Part
                && string.Equals(part.Name, projection.Name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

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
            FindContentPartKind.Frontmatter => 1,
            FindContentPartKind.Headings => 2,
            FindContentPartKind.Body => 3,
            FindContentPartKind.Section => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Find projection part is not defined."),
        };
        return checked(1 + layerRank * 10 + partRank);
    }

    private static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body or FindRegionKind.Section => 1,
            FindRegionKind.Document => throw new ArgumentException("Find evidence cannot use the document region.", nameof(region)),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined."),
        };

    private static int ReadLayerRank(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private readonly record struct MatchKey(string Id, string Path);
}
