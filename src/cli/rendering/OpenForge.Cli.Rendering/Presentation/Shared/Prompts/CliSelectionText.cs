using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.Core.Presentation.Shared.Prompts;

internal static class CliSelectionText
{
    private const string Ellipsis = "...";
    private const int NonAsciiColumns = 2;

    internal static IReadOnlyList<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        var line = new StringBuilder();
        var columns = 0;
        var elements = StringInfo.GetTextElementEnumerator(CliText.Escape(text));
        while (elements.MoveNext())
        {
            var element = elements.GetTextElement();
            var size = Columns(element);
            if (columns + size > width)
            {
                lines.Add(line.ToString());
                line.Clear();
                columns = 0;
            }
            if (size > width)
            {
                line.Append('?');
                columns++;
            }
            else
            {
                line.Append(element);
                columns += size;
            }
        }
        if (line.Length > 0 || lines.Count == 0) lines.Add(line.ToString());
        return lines;
    }

    internal static string Clip(string text, int width)
    {
        var escaped = CliText.Escape(text);
        if (Columns(escaped) <= width) return escaped;
        var line = new StringBuilder();
        var columns = 0;
        var elements = StringInfo.GetTextElementEnumerator(escaped);
        while (elements.MoveNext())
        {
            var element = elements.GetTextElement();
            var size = Columns(element);
            if (columns + size > width - Ellipsis.Length) break;
            line.Append(element);
            columns += size;
        }
        return line.Append(Ellipsis).ToString();
    }

    internal static string ClipMiddle(string text, int width)
    {
        var escaped = CliText.Escape(text);
        if (Columns(escaped) <= width) return escaped;
        var elements = new List<string>();
        var enumeration = StringInfo.GetTextElementEnumerator(escaped);
        while (enumeration.MoveNext()) elements.Add(enumeration.GetTextElement());
        var head = new StringBuilder();
        var headColumns = 0;
        var headCapacity = (width - Ellipsis.Length) / 2;
        foreach (var element in elements)
        {
            if (headColumns + Columns(element) > headCapacity) break;
            head.Append(element);
            headColumns += Columns(element);
        }
        var tail = new List<string>();
        var tailColumns = 0;
        foreach (var element in elements.AsEnumerable().Reverse())
        {
            if (tailColumns + Columns(element) > width - headColumns - Ellipsis.Length) break;
            tail.Add(element);
            tailColumns += Columns(element);
        }
        tail.Reverse();
        return head.Append(Ellipsis).Append(string.Concat(tail)).ToString();
    }

    internal static IReadOnlyList<string> Summarize(string text, int width, int maximumRows)
    {
        var rows = Wrap(text, width);
        if (rows.Count <= maximumRows) return rows;
        return rows.Take(maximumRows - 1).Append(Clip(rows[maximumRows - 1] + Ellipsis, width)).ToArray();
    }

    private static int Columns(string text)
    {
        var columns = 0;
        // Conservatively reserve two columns for each non-ASCII rune. Combining
        // sequences may use fewer columns, but cannot push a row into wrapping.
        foreach (var rune in text.EnumerateRunes()) columns += rune.IsAscii ? 1 : NonAsciiColumns;
        return columns;
    }
}
