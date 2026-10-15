using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

// The accepted Forgiving Maintenance design requires this pure comparison for Repair, Doctor and Index.
internal static class RenameContentComparer
{
    internal const int MinimumSimilarity = 90;
    private const int MaximumSimilarity = 100;
    internal const int MaximumBytes = 1024 * 1024;
    internal const int MaximumLines = 2000;

    internal static bool CouldReachThreshold(RenameContent old, RenameContent current, CancellationToken cancellationToken = default)
    {
        if (old.Lines.IsDefault || current.Lines.IsDefault) return false;
        var total = (long)old.NormalizedByteLength + current.NormalizedByteLength;
        if ((long)MaximumSimilarity * 2 * Math.Min(old.NormalizedByteLength, current.NormalizedByteLength) < MinimumSimilarity * total) return false;
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var line in current.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            counts[line.Text] = counts.GetValueOrDefault(line.Text) + 1;
        }

        long commonBytes = 0;
        foreach (var line in old.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!counts.TryGetValue(line.Text, out var count) || count == 0) continue;
            commonBytes += line.ByteLength;
            counts[line.Text] = count - 1;
        }
        return (long)MaximumSimilarity * 2 * commonBytes >= MinimumSimilarity * total;
    }

    internal static RenameContentComparison Compare(RenameContent old, RenameContent current, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(old);
        ArgumentNullException.ThrowIfNull(current);
        cancellationToken.ThrowIfCancellationRequested();
        var sameContent = old.NormalizedHash == current.NormalizedHash;
        if (old.Lines.IsDefault || current.Lines.IsDefault)
        {
            return new RenameContentComparison(Similarity: null, SameContent: sameContent, HasCommonAuthoredLine: false);
        }

        if (sameContent)
        {
            return new RenameContentComparison(Similarity: old.NormalizedByteLength == 0 ? 0 : MaximumSimilarity, SameContent: true,
                HasCommonAuthoredLine: old.HasAuthoredContent && current.HasAuthoredContent && old.Lines.Any(line => line.IsAuthored));
        }

        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        var currentIds = new int[current.Lines.Length];
        for (var index = 0; index < current.Lines.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = current.Lines[index].Text;
            if (!ids.TryGetValue(text, out var id))
            {
                id = ids.Count;
                ids.Add(text, id);
            }
            currentIds[index] = id;
        }

        var previous = new int[current.Lines.Length + 1];
        var next = new int[current.Lines.Length + 1];
        var commonAuthoredLine = false;
        foreach (var line in old.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ids.TryGetValue(line.Text, out var lineId)) continue;
            for (var index = 0; index < current.Lines.Length; index++)
            {
                var other = current.Lines[index];
                if (lineId == currentIds[index])
                {
                    next[index + 1] = previous[index] + line.ByteLength;
                    commonAuthoredLine |= line.IsAuthored && other.IsAuthored;
                }
                else
                {
                    next[index + 1] = Math.Max(previous[index + 1], next[index]);
                }
            }

            (previous, next) = (next, previous);
            next[0] = 0;
        }

        var total = (long)old.NormalizedByteLength + current.NormalizedByteLength;
        var score = total == 0 ? 0 : (int)((long)MaximumSimilarity * 2 * previous[^1] / total);
        return new RenameContentComparison(Similarity: score, SameContent: sameContent,
            HasCommonAuthoredLine: commonAuthoredLine && old.HasAuthoredContent && current.HasAuthoredContent);
    }
}
