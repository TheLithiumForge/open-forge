using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

// The accepted Forgiving Maintenance design requires this pure comparison for Repair, Doctor and Index.
internal static class RenameContentComparer
{
    internal const int MinimumSimilarity = 90;
    private const int MaximumSimilarity = 100;
    internal const int MaximumBytes = 1024 * 1024;
    internal const int MaximumLines = 2000;

    internal static bool CouldReachThreshold(RenameContent old, RenameContent current)
        => (long)MaximumSimilarity * 2 * Math.Min(old.Bytes.Length, current.Bytes.Length)
            >= MinimumSimilarity * ((long)old.Bytes.Length + current.Bytes.Length);

    internal static RenameContentComparison Compare(RenameContent old, RenameContent current, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(old);
        ArgumentNullException.ThrowIfNull(current);
        cancellationToken.ThrowIfCancellationRequested();
        var sameContent = old.Hash == current.Hash;
        if (old.Lines.IsDefault || current.Lines.IsDefault)
        {
            return new RenameContentComparison(Similarity: null, SameContent: sameContent, HasCommonAuthoredLine: false);
        }

        var previous = new int[current.Lines.Length + 1];
        var next = new int[current.Lines.Length + 1];
        var commonAuthoredLine = false;
        foreach (var line in old.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var index = 0; index < current.Lines.Length; index++)
            {
                var other = current.Lines[index];
                if (line.Text == other.Text)
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

        var total = (long)old.Bytes.Length + current.Bytes.Length;
        var score = total == 0 ? 0 : (int)((long)MaximumSimilarity * 2 * previous[^1] / total);
        return new RenameContentComparison(Similarity: score, SameContent: sameContent,
            HasCommonAuthoredLine: commonAuthoredLine && old.HasAuthoredContent && current.HasAuthoredContent);
    }
}
