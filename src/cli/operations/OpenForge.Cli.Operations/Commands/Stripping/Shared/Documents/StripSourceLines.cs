namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;

/// <summary>Line boundaries inside exact source text. A line ends after its \n, \r\n or \r.</summary>
internal static class StripSourceLines
{
    internal static int LineStart(string source, int position)
    {
        var index = Math.Min(position, source.Length);
        while (index > 0 && source[index - 1] is not ('\n' or '\r'))
        {
            index--;
        }

        return index;
    }

    // The position after the line ending of the line that contains the position.
    internal static int NextLineStart(string source, int position)
    {
        var index = position;
        while (index < source.Length && source[index] is not ('\n' or '\r'))
        {
            index++;
        }

        if (index < source.Length && source[index] == '\r')
        {
            index++;
            if (index < source.Length && source[index] == '\n')
            {
                index++;
            }
        }
        else if (index < source.Length && source[index] == '\n')
        {
            index++;
        }

        return index;
    }

    internal static bool IsBlank(string source, int start, int end)
        => source.AsSpan(start, end - start).IsWhiteSpace();

    // Moves a start position back over whole blank lines that precede it.
    internal static int BackOverBlankLines(string source, int start, int floor)
    {
        var current = start;
        while (current > floor)
        {
            var previousStart = LineStart(source, current - 1);
            if (previousStart < floor || !IsBlank(source, previousStart, current))
            {
                break;
            }

            current = previousStart;
        }

        return current;
    }

    // Moves an end position forward over whole blank lines that follow it.
    internal static int ForwardOverBlankLines(string source, int end)
    {
        var current = end;
        while (current < source.Length)
        {
            var next = NextLineStart(source, current);
            if (next == current || !IsBlank(source, current, next))
            {
                break;
            }

            current = next;
        }

        return current;
    }
}
