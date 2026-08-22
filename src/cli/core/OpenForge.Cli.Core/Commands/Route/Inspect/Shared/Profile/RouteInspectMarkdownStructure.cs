namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectMarkdownStructure
{
    internal static bool[] ReadStructuralLines(IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var structural = Enumerable.Repeat(true, lines.Count).ToArray();
        char? fenceCharacter = null;
        var fenceLength = 0;
        for (var index = 0; index < lines.Count; index++)
        {
            var line = TrimIndent(lines[index]);
            if (fenceCharacter is null)
            {
                if (TryReadOpeningFence(line, out var character, out var length))
                {
                    structural[index] = false;
                    fenceCharacter = character;
                    fenceLength = length;
                }

                continue;
            }

            structural[index] = false;
            if (IsClosingFence(line, fenceCharacter.Value, fenceLength))
            {
                fenceCharacter = null;
                fenceLength = 0;
            }
        }

        return structural;
    }

    private static ReadOnlySpan<char> TrimIndent(string line)
    {
        var index = 0;
        while (index < line.Length && index < 3 && line[index] == ' ')
        {
            index++;
        }

        return line.AsSpan(index);
    }

    private static bool TryReadOpeningFence(
        ReadOnlySpan<char> line,
        out char character,
        out int length)
    {
        character = line.IsEmpty ? default : line[0];
        length = ReadRunLength(line, character);
        if (character is not ('`' or '~') || length < 3)
        {
            return false;
        }

        return character != '`' || !line[length..].Contains('`');
    }

    private static bool IsClosingFence(ReadOnlySpan<char> line, char character, int openingLength)
    {
        var length = ReadRunLength(line, character);
        return length >= openingLength && line[length..].Trim().IsEmpty;
    }

    private static int ReadRunLength(ReadOnlySpan<char> line, char character)
    {
        var length = 0;
        while (length < line.Length && line[length] == character)
        {
            length++;
        }

        return length;
    }
}
