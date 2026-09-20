using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkSourceAlignment
{
    internal static FrameworkPayloadAsset? ReadAsset(CliWorkspace workspace, string path, FrameworkPayload payload)
    {
        if (payload.Find(path) is { } exact)
        {
            return exact;
        }
        if (!SourceFormClassifier.TryClassify(path, out var form) || !SourceFormClassifier.IsEntrypoint(form)
            || SourceIdentity.DeriveId(path) is not { } id)
        {
            return null;
        }
        var sources = new EmbeddedFrameworkSourceProjector().Project(workspace, payload);
        var segments = id.Split('/');
        if (segments.Length < 2)
        {
            return null;
        }
        var candidates = ReadCandidates(segments, sources);
        if (candidates.Count != 1)
        {
            return null;
        }
        var canonicalId = string.Join('/', candidates[0].ManagedSegments);
        var matches = sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
            && source.Identity.AutomaticId == canonicalId).ToArray();
        return matches.Length == 1 ? payload.Find(matches[0].Identity.CanonicalBasePath) : null;
    }

    internal static IReadOnlyList<Candidate> ReadCandidates(
        IReadOnlyList<string> requestedSegments,
        IReadOnlyList<SourceLogicalSource> payloadSources)
    {
        var entrypointIds = payloadSources
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => source.Identity.AutomaticId)
            .ToHashSet(StringComparer.Ordinal);
        var managedSegmentNames = entrypointIds
            .SelectMany(id => id.Split('/', StringSplitOptions.None))
            .ToHashSet(StringComparer.Ordinal);
        var candidates = new List<Candidate>();
        foreach (var finalId in entrypointIds.Order(StringComparer.Ordinal))
        {
            var managedSegments = finalId.Split('/', StringSplitOptions.None);
            if (managedSegments.Length < 2
                || !string.Equals(managedSegments[0], requestedSegments[0], StringComparison.Ordinal)
                || !string.Equals(managedSegments[^1], requestedSegments[^1], StringComparison.Ordinal)
                || !EveryManagedPrefixExists(managedSegments, entrypointIds))
            {
                continue;
            }

            foreach (var positions in ReadManagedPositions(requestedSegments, managedSegments))
            {
                if (ContainsReservedScopeSegment(requestedSegments, positions, managedSegmentNames))
                {
                    continue;
                }

                candidates.Add(new Candidate(managedSegments, positions));
            }
        }

        return candidates;
    }

    private static bool ContainsReservedScopeSegment(
        IReadOnlyList<string> requestedSegments,
        IReadOnlyList<int> managedPositions,
        IReadOnlySet<string> managedSegmentNames)
    {
        var managedPositionSet = managedPositions.ToHashSet();
        return requestedSegments
            .Where((_, position) => !managedPositionSet.Contains(position))
            .Any(managedSegmentNames.Contains);
    }

    private static bool EveryManagedPrefixExists(
        IReadOnlyList<string> segments,
        IReadOnlySet<string> entrypointIds)
        => Enumerable.Range(1, segments.Count)
            .All(length => entrypointIds.Contains(string.Join('/', segments.Take(length))));

    private static IEnumerable<int[]> ReadManagedPositions(
        IReadOnlyList<string> requested,
        IReadOnlyList<string> managed)
    {
        var positions = new int[managed.Count];
        positions[0] = 0;
        positions[^1] = requested.Count - 1;
        return ReadIntermediatePosition(1, start: 1);

        IEnumerable<int[]> ReadIntermediatePosition(int managedIndex, int start)
        {
            if (managedIndex == managed.Count - 1)
            {
                yield return positions.ToArray();
                yield break;
            }

            var remainingManaged = managed.Count - managedIndex - 1;
            var finalExclusive = requested.Count - remainingManaged;
            for (var position = start; position < finalExclusive; position++)
            {
                if (!string.Equals(requested[position], managed[managedIndex], StringComparison.Ordinal))
                {
                    continue;
                }

                positions[managedIndex] = position;
                foreach (var result in ReadIntermediatePosition(managedIndex + 1, position + 1))
                {
                    yield return result;
                }
            }
        }
    }

    internal sealed record Candidate(
        IReadOnlyList<string> ManagedSegments,
        IReadOnlyList<int> ManagedPositions);
}
