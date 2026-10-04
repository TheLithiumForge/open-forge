using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Models.GitIgnore;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.GitIgnore;

// Workspace Libraries Technical Design defines this owned section for Library mutators and root path Remove.
internal static class LibraryGitIgnoreSection
{
    internal const string Path = ".gitignore";
    internal const string Begin = "# BEGIN OPEN FORGE LIBRARIES";
    internal const string End = "# END OPEN FORGE LIBRARIES";
    private static readonly UTF8Encoding Utf8 = new(false, true);

    internal static string Pattern(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.Contains('\r') || path.Contains('\n'))
        {
            throw new ArgumentException("An ignore leaf cannot contain a line ending.", nameof(path));
        }
        var pattern = new StringBuilder("/");
        foreach (var character in path)
        {
            if (character is '\\' or '*' or '?' or '[' or ']' or '!' or '#' or ' ')
            {
                pattern.Append('\\');
            }
            pattern.Append(character);
        }
        return pattern.ToString();
    }

    internal static byte[] Rewrite(
        ReadOnlySpan<byte> bytes,
        ImmutableArray<string> currentPaths,
        ImmutableArray<string> intendedPaths)
    {
        var text = Utf8.GetString(bytes);
        var bom = text.StartsWith('\uFEFF') ? "\uFEFF" : string.Empty;
        if (bom.Length > 0) text = text[1..];
        var section = Locate(text);
        var allowed = currentPaths.Concat(intendedPaths).Select(Pattern).ToHashSet(StringComparer.Ordinal);
        if (section.Start >= 0)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var lines = text[section.BodyStart..section.BodyEnd].Split('\n');
            foreach (var line in lines.Take(lines.Length - 1))
            {
                var content = line.EndsWith('\r') ? line[..^1] : line;
                if (!allowed.Contains(content) || !seen.Add(content))
                {
                    throw new InvalidDataException("The Library Git-ignore section contains an unsupported or duplicate rule.");
                }
            }
        }
        var lineEnding = section.LineEnding;
        if (section.Start < 0 && text.Contains("\r\n", StringComparison.Ordinal))
        {
            lineEnding = "\r\n";
        }
        var patterns = intendedPaths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).Select(Pattern).ToArray();
        var replacement = patterns.Length == 0 ? string.Empty
            : string.Join(lineEnding, new[] { Begin }.Concat(patterns).Append(End)) + lineEnding;
        if (section.Start >= 0)
        {
            return Utf8.GetBytes(bom + text[..section.Start] + replacement + text[section.EndExclusive..]);
        }
        if (replacement.Length == 0) return bytes.ToArray();
        var separator = text.Length == 0 || text.EndsWith('\n') ? string.Empty : lineEnding;
        return Utf8.GetBytes(bom + text + separator + replacement);
    }

    private static LibraryGitIgnoreSectionSpan Locate(string text)
    {
        var start = -1;
        var end = -1;
        var bodyStart = -1;
        var bodyEnd = -1;
        var lineEnding = "\n";
        var offset = 0;
        foreach (var line in text.Split('\n'))
        {
            var content = line.EndsWith('\r') ? line[..^1] : line;
            if (content.Contains(Begin, StringComparison.Ordinal) || content.Contains(End, StringComparison.Ordinal))
            {
                if (content == Begin && start < 0 && end < 0)
                {
                    start = offset;
                    bodyStart = Math.Min(text.Length, offset + line.Length + 1);
                    lineEnding = line.EndsWith('\r') ? "\r\n" : "\n";
                }
                else if (content == End && start >= 0 && end < 0)
                {
                    bodyEnd = offset;
                    end = Math.Min(text.Length, offset + line.Length + 1);
                }
                else throw new InvalidDataException("The Library Git-ignore section has ambiguous or duplicate markers.");
            }
            offset += line.Length + 1;
        }
        if (start >= 0 && end < 0) throw new InvalidDataException("The Library Git-ignore section is missing its closing marker.");
        return new(start, end, bodyStart, bodyEnd, lineEnding);
    }
}
