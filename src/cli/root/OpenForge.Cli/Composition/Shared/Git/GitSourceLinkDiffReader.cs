using System.Globalization;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Composition.Shared.Git;

internal static class GitSourceLinkDiffReader
{
    internal static IReadOnlyList<SourceLinkDiffHunk> Read(string diff)
    {
        var result = new List<SourceLinkDiffHunk>();
        foreach (var line in diff.Split('\n'))
        {
            if (!line.StartsWith("@@ -", StringComparison.Ordinal)) continue;
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 4 || fields[3] != "@@"
                || !TryRange(fields[1], out var previousLine, out var previousCount)
                || !TryRange(fields[2], out var currentLine, out var currentCount)) continue;
            result.Add(new(PreviousLine: previousLine, PreviousCount: previousCount, CurrentLine: currentLine, CurrentCount: currentCount));
        }
        return result;
    }

    private static bool TryRange(string field, out int line, out int count)
    {
        line = 0;
        count = 1;
        var values = field.AsSpan(1);
        var comma = values.IndexOf(',');
        if (comma >= 0 && !int.TryParse(values[(comma + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out count)) return false;
        return int.TryParse(comma < 0 ? values : values[..comma], NumberStyles.None, CultureInfo.InvariantCulture, out line);
    }
}
