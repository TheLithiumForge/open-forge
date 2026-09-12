using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational;

internal static class LocalReferenceGraphReader
{
    internal static IReadOnlyList<LocalReferenceGraphFinding> Read(
        IReadOnlyList<LocalReferenceObservation> references)
    {
        var findings = references
            .GroupBy(reference => (reference.SourcePath, reference.Destination))
            .Where(group => group.Count() > 1)
            .OrderBy(group => group.Key.SourcePath, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Destination, StringComparer.Ordinal)
            .Select(group => LocalReferenceGraphFinding.Repeat(group.ToArray()))
            .ToList();
        findings.AddRange(ReadCycles(references)
            .Select(LocalReferenceGraphFinding.Cycle));
        return findings
            .OrderBy(finding => finding.Kind)
            .ThenBy(ReadFindingKey, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<IReadOnlyList<LocalReferenceObservation>> ReadCycles(
        IReadOnlyList<LocalReferenceObservation> references)
    {
        var edges = references
            .Where(reference => reference.Facts.Target is
            { Kind: SourceLinkTargetKind.Local, Resolution: SourceLinkTargetResolution.Complete, Path: not null })
            .OrderBy(ReadEdgeKey, StringComparer.Ordinal)
            .GroupBy(edge => edge.RoutePath, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var colors = new Dictionary<string, GraphColor>(StringComparer.Ordinal);
        var cycles = new Dictionary<string, IReadOnlyList<LocalReferenceObservation>>(StringComparer.Ordinal);
        foreach (var root in edges.Keys.Order(StringComparer.Ordinal))
        {
            if (colors.GetValueOrDefault(root) != GraphColor.White)
            {
                continue;
            }

            ReadComponent(root, edges, colors, cycles);
        }

        return cycles.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Value).ToArray();
    }

    private static void ReadComponent(
        string root,
        IReadOnlyDictionary<string, LocalReferenceObservation[]> edges,
        IDictionary<string, GraphColor> colors,
        IDictionary<string, IReadOnlyList<LocalReferenceObservation>> cycles)
    {
        var stack = new List<GraphFrame> { new(root, Incoming: null) };
        colors[root] = GraphColor.Gray;
        while (stack.Count > 0)
        {
            var frame = stack[^1];
            var outgoing = edges.GetValueOrDefault(frame.Node) ?? [];
            if (frame.NextEdge >= outgoing.Length)
            {
                colors[frame.Node] = GraphColor.Black;
                stack.RemoveAt(stack.Count - 1);
                continue;
            }

            var edge = outgoing[frame.NextEdge];
            stack[^1] = frame with { NextEdge = frame.NextEdge + 1 };
            var target = edge.Facts.Target.Path
                ?? throw new InvalidOperationException("A complete local edge must retain its target path.");
            var targetColor = colors.TryGetValue(target, out var establishedColor)
                ? establishedColor
                : GraphColor.White;
            if (targetColor == GraphColor.White)
            {
                colors[target] = GraphColor.Gray;
                stack.Add(new GraphFrame(target, edge));
                continue;
            }

            if (targetColor == GraphColor.Gray)
            {
                var start = stack.FindIndex(item => string.Equals(item.Node, target, StringComparison.Ordinal));
                var cycle = stack.Skip(start + 1)
                    .Select(item => item.Incoming)
                    .OfType<LocalReferenceObservation>()
                    .Append(edge)
                    .ToArray();
                var canonical = Canonicalize(cycle);
                cycles.TryAdd(string.Join("\n", canonical.Select(ReadEdgeKey)), canonical);
            }
        }
    }

    private static IReadOnlyList<LocalReferenceObservation> Canonicalize(
        IReadOnlyList<LocalReferenceObservation> cycle)
    {
        var start = Enumerable.Range(0, cycle.Count).MinBy(index => ReadEdgeKey(cycle[index]));
        return Enumerable.Range(0, cycle.Count)
            .Select(offset => cycle[(start + offset) % cycle.Count])
            .ToArray();
    }

    private static string ReadFindingKey(LocalReferenceGraphFinding finding)
        => string.Join("\n", finding.Occurrences.Select(ReadEdgeKey));

    private static string ReadEdgeKey(LocalReferenceObservation edge)
        => $"{edge.RoutePath}\0{edge.Destination}\0{edge.SourcePath}";

    private enum GraphColor
    {
        White,
        Gray,
        Black,
    }

    private sealed record GraphFrame(
        string Node,
        LocalReferenceObservation? Incoming,
        int NextEdge = 0);
}
