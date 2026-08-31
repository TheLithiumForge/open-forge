using System.Collections.ObjectModel;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal enum RouteInitFrameworkAlignmentState
{
    Complete,
    Blocked,
}

internal sealed record RouteInitFrameworkAlignedSegment(
    string ConcreteSegment,
    RouteInitFrameworkSegmentRole Role,
    SourceLogicalSource IntendedSource,
    string? SourceAssetPath);

internal sealed record RouteInitFrameworkAlignment
{
    internal RouteInitFrameworkAlignment(
        RouteInitTargetFacts target,
        IEnumerable<RouteInitFrameworkAlignedSegment> segments,
        IEnumerable<SourceLogicalSource> payloadSources,
        SourceRouteTopology payloadTopology)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(payloadTopology);
        Target = target;
        Segments = new ReadOnlyCollection<RouteInitFrameworkAlignedSegment>(segments.ToArray());
        PayloadSources = new ReadOnlyCollection<SourceLogicalSource>(payloadSources.ToArray());
        PayloadTopology = payloadTopology;
    }

    internal RouteInitTargetFacts Target { get; }

    internal IReadOnlyList<RouteInitFrameworkAlignedSegment> Segments { get; }

    internal IReadOnlyList<SourceLogicalSource> PayloadSources { get; }

    internal SourceRouteTopology PayloadTopology { get; }
}

internal sealed record RouteInitFrameworkAlignmentBuild(
    RouteInitFrameworkAlignmentState State,
    RouteInitFrameworkAlignment? Alignment,
    string? Cause);

internal sealed class RouteInitFrameworkAlignmentBuilder
{
    private readonly SourceRouteTopologyBuilder _topologyBuilder = new();
    private readonly RouteInitTargetPlanner _targetPlanner = new();

    internal RouteInitFrameworkAlignmentBuild Build(
        CliWorkspace workspace,
        RouteInitTargetFacts requested,
        IReadOnlyList<SourceLogicalSource> payloadSources)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(requested);
        ArgumentNullException.ThrowIfNull(payloadSources);
        if (requested.RequestedSegments.Count < 2)
        {
            return Blocked("A Framework Route Init target must end at a non-root managed route.");
        }

        var topology = _topologyBuilder.Build(payloadSources, loaderRootPaths: []);
        var candidates = ReadCandidates(requested.RequestedSegments, payloadSources);
        if (candidates.Count != 1)
        {
            return Blocked(candidates.Count == 0
                ? "The Framework Route Init target does not align with the embedded canonical topology."
                : "The Framework Route Init target has more than one canonical topology alignment.");
        }

        var candidate = candidates[0];
        var concreteSegments = requested.RequestedSegments.ToArray();
        var managedByPosition = candidate.ManagedPositions
            .Select((position, index) => (position, index))
            .ToDictionary(item => item.position, item => item.index);
        for (var index = 0; index < concreteSegments.Length; index++)
        {
            if (managedByPosition.ContainsKey(index))
            {
                concreteSegments[index] = candidate.ManagedSegments[managedByPosition[index]];
                continue;
            }

            if (requested.Kind == RouteInitTargetKind.SourceId)
            {
                if (!TrySlugScope(concreteSegments[index], out var slug))
                {
                    return Blocked("A Framework Route Init scope label cannot be converted to one safe concrete slug.");
                }

                concreteSegments[index] = slug;
            }
        }

        var alignedTarget = _targetPlanner.ResolveAlignedFrameworkTarget(requested, concreteSegments);
        var alignedSegments = BuildSegments(
            workspace,
            alignedTarget,
            candidate,
            managedByPosition,
            payloadSources);
        return new RouteInitFrameworkAlignmentBuild(
            RouteInitFrameworkAlignmentState.Complete,
            new RouteInitFrameworkAlignment(
                alignedTarget,
                alignedSegments,
                payloadSources,
                topology),
            Cause: null);
    }

    private static IReadOnlyList<RouteInitFrameworkAlignedSegment> BuildSegments(
        CliWorkspace workspace,
        RouteInitTargetFacts target,
        AlignmentCandidate candidate,
        IReadOnlyDictionary<int, int> managedByPosition,
        IReadOnlyList<SourceLogicalSource> payloadSources)
    {
        var concrete = target.CanonicalSegments
            ?? throw new InvalidOperationException("A complete Framework alignment requires concrete segments.");
        var results = new List<RouteInitFrameworkAlignedSegment>(concrete.Count);
        for (var position = 0; position < concrete.Count; position++)
        {
            var destinationSegments = concrete.Take(position + 1).ToArray();
            var destinationPath = position == concrete.Count - 1
                && target.Kind == RouteInitTargetKind.ExactPath
                ? target.CanonicalPath
                    ?? throw new InvalidOperationException(
                        "An exact Framework alignment requires its requested entrypoint path.")
                : RouteInitTargetPlanner.ReadChainPaths(
                    new RouteInitTargetFacts(
                        target.Requested,
                        target.Kind,
                        target.RequestedSegments,
                        destinationSegments,
                        string.Join('/', destinationSegments),
                        canonicalPath: null))[^1];

            if (!managedByPosition.TryGetValue(position, out var managedIndex))
            {
                results.Add(new RouteInitFrameworkAlignedSegment(
                    concrete[position],
                    RouteInitFrameworkSegmentRole.Scope,
                    CreateDestinationSource(workspace, destinationPath, source: null),
                    SourceAssetPath: null));
                continue;
            }

            var sourceId = string.Join('/', candidate.ManagedSegments.Take(managedIndex + 1));
            var source = payloadSources.Single(value =>
                SourceFormClassifier.IsEntrypoint(value.Base.Form)
                && string.Equals(value.Identity.AutomaticId, sourceId, StringComparison.Ordinal));
            results.Add(new RouteInitFrameworkAlignedSegment(
                concrete[position],
                managedIndex == 0
                    ? RouteInitFrameworkSegmentRole.InstalledRoot
                    : RouteInitFrameworkSegmentRole.Managed,
                CreateDestinationSource(workspace, destinationPath, source),
                source.Identity.CanonicalBasePath));
        }

        return results;
    }

    private static SourceLogicalSource CreateDestinationSource(
        CliWorkspace workspace,
        string canonicalPath,
        SourceLogicalSource? source)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form)
            || !SourceFormClassifier.IsEntrypoint(form))
        {
            throw new InvalidOperationException("A Framework Route Init destination must be one canonical entrypoint.");
        }

        var id = SourceIdentity.DeriveId(canonicalPath)
            ?? throw new InvalidOperationException("A Framework Route Init destination requires one source identity.");
        var baseLayer = new SourceLayer(
            canonicalPath,
            Path.GetFullPath(SourceLogicalPath.ToLexicalPath(workspace.PhysicalRoot, canonicalPath)),
            form,
            SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (source?.Overwrite is not null)
        {
            var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
            overwrite = new SourceLayer(
                overwritePath,
                Path.GetFullPath(SourceLogicalPath.ToLexicalPath(workspace.PhysicalRoot, overwritePath)),
                OpenForge.Cli.Core.Framework.Sources.Models.Identity.SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(new SourceLogicalIdentity(id, canonicalPath), baseLayer, overwrite);
    }

    private static IReadOnlyList<AlignmentCandidate> ReadCandidates(
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
        var candidates = new List<AlignmentCandidate>();
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

                candidates.Add(new AlignmentCandidate(managedSegments, positions));
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

    private static bool TrySlugScope(string value, out string slug)
    {
        var builder = new StringBuilder(value.Length);
        var pendingSeparator = false;
        foreach (var rune in value.EnumerateRunes())
        {
            if (Rune.IsLetter(rune))
            {
                AppendPendingSeparator(builder, ref pendingSeparator);
                builder.Append(Rune.ToLowerInvariant(rune));
            }
            else if (Rune.IsDigit(rune))
            {
                AppendPendingSeparator(builder, ref pendingSeparator);
                builder.Append(rune);
            }
            else if (Rune.IsWhiteSpace(rune) || rune.Value is '_' or '-')
            {
                pendingSeparator = builder.Length > 0;
            }
            else
            {
                slug = string.Empty;
                return false;
            }
        }

        slug = builder.ToString();
        return slug.Length > 0 && slug is not "." and not "..";
    }

    private static void AppendPendingSeparator(StringBuilder builder, ref bool pendingSeparator)
    {
        if (pendingSeparator && builder.Length > 0)
        {
            builder.Append('-');
        }

        pendingSeparator = false;
    }

    private static RouteInitFrameworkAlignmentBuild Blocked(string cause)
        => new(RouteInitFrameworkAlignmentState.Blocked, Alignment: null, cause);

    private sealed record AlignmentCandidate(
        IReadOnlyList<string> ManagedSegments,
        IReadOnlyList<int> ManagedPositions);
}
