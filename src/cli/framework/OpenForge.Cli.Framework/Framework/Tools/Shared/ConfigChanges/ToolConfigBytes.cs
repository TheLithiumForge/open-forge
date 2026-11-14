using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal static class ToolConfigBytes
{
    internal static readonly UTF8Encoding Utf8 = new(false, true);

    internal static byte[] Replace(byte[] source, IEnumerable<ToolEntrySpan> changes)
    {
        using var result = new MemoryStream();
        var end = 0;
        foreach (var change in changes.OrderBy(change => change.Start))
        {
            if (change.Start < end || change.End < change.Start || change.End > source.Length)
                throw new InvalidDataException("The declared entry selectors overlap.");
            result.Write(source.AsSpan(end, change.Start - end));
            result.Write(change.Replacement);
            end = change.End;
        }
        result.Write(source.AsSpan(end));
        return result.ToArray();
    }

    internal static ToolEntrySpan Span(string source, int start, int end, string replacement)
        => new(Utf8.GetByteCount(source.AsSpan(0, start)), Utf8.GetByteCount(source.AsSpan(0, end)), Utf8.GetBytes(replacement));

    internal static ToolTextLine[] Lines(string source)
    {
        var lines = new List<ToolTextLine>();
        for (var start = 0; start < source.Length;)
        {
            var newline = source.IndexOf('\n', start);
            var end = newline < 0 ? source.Length : newline + 1;
            var contentEnd = newline < 0 ? end : newline;
            if (contentEnd > start && source[contentEnd - 1] == '\r') contentEnd--;
            lines.Add(new(start, contentEnd, end, source[start..contentEnd]));
            start = end;
        }
        return [.. lines];
    }

    internal static string Newline(string source) => source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
}
