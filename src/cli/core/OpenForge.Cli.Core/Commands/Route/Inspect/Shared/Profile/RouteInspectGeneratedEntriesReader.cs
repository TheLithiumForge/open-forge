using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectGeneratedEntriesReader
{
    private const string StartMarker = "<!-- open-forge:generated-index:start -->";
    private const string EndMarker = "<!-- open-forge:generated-index:end -->";
    private const string EmptySentinel = "- none - No entries - #Empty";

    internal static RouteInspectGeneratedEntries Read(RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Base.ReadState != FileReadState.Complete || source.Base.Body is null)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint body is not completely readable.");
        }

        if (source.Kind != RouteSourceKind.Loader
            && source.Metadata.State != RouteSourceMetadataState.Complete)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint metadata is not completely readable.");
        }

        return ReadBody(source.Base.Body);
    }

    private static RouteInspectGeneratedEntries ReadBody(string body)
    {
        var normalized = body.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.Contains('\r'))
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint Entries section uses an unsupported line ending.");
        }

        var lines = normalized.Split('\n', StringSplitOptions.None);
        var structural = RouteInspectMarkdownStructure.ReadStructuralLines(lines);
        var headings = FindLines(lines, structural, "## Entries");
        if (headings.Length != 1)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint must contain exactly one final ## Entries section.");
        }

        var heading = headings[0];
        if (Enumerable.Range(heading + 1, lines.Length - heading - 1)
            .Any(index => structural[index] && lines[index].StartsWith("## ", StringComparison.Ordinal)))
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint Entries section is not the final section.");
        }

        var starts = FindLines(lines, structural, StartMarker);
        var ends = FindLines(lines, structural, EndMarker);
        if (starts.Length != 1 || ends.Length != 1 || starts[0] <= heading || ends[0] <= starts[0])
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint Entries section must contain one ordered marker pair.");
        }

        var start = starts[0];
        var end = ends[0];
        if (lines[(heading + 1)..start].Any(line => line.Length != 0)
            || lines[(end + 1)..].Any(line => line.Length != 0))
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "Only blank lines may occur outside the entrypoint Entries markers.");
        }

        var bodyLines = lines[(start + 1)..end].Where(line => line.Length != 0).ToArray();
        if (bodyLines.Length == 0 || bodyLines is [EmptySentinel])
        {
            return RouteInspectGeneratedEntries.Available([]);
        }

        var entries = new List<RouteInspectGeneratedEntry>();
        foreach (var line in bodyLines)
        {
            if (line == EmptySentinel)
            {
                return RouteInspectGeneratedEntries.Unavailable(
                    "The empty Entries sentinel must be the sole declaration.");
            }

            if (!TryParseEntry(line, out var destination, out var tags, out var cause))
            {
                return RouteInspectGeneratedEntries.Unavailable(cause!);
            }

            entries.Add(new RouteInspectGeneratedEntry(destination!, tags));
        }

        return RouteInspectGeneratedEntries.Available(entries);
    }

    private static bool TryParseEntry(
        string line,
        out string? destination,
        out IReadOnlyList<string> tags,
        out string? cause)
    {
        destination = null;
        tags = [];
        cause = null;
        if (!line.StartsWith("- [", StringComparison.Ordinal))
        {
            cause = "A generated Entries declaration must start with a single hyphen list marker and label.";
            return false;
        }

        var destinationStart = line.IndexOf("](", 3, StringComparison.Ordinal);
        if (destinationStart < 0
            || destinationStart == 3
            || !IsCanonicalLinkLabel(line.AsSpan(3, destinationStart - 3)))
        {
            cause = "A generated Entries declaration must contain a non-empty inline link label.";
            return false;
        }

        var destinationEnd = line.IndexOf(')', destinationStart + 2);
        if (destinationEnd < 0 || destinationEnd == destinationStart + 2)
        {
            cause = "A generated Entries declaration must contain a non-empty destination.";
            return false;
        }

        destination = line[(destinationStart + 2)..destinationEnd];
        var suffix = line[(destinationEnd + 1)..];
        if (suffix.Length == 0)
        {
            return true;
        }

        const string separator = " - ";
        if (!suffix.StartsWith(separator, StringComparison.Ordinal)
            || !HasCanonicalTags(suffix[separator.Length..]))
        {
            cause = "A generated Entries declaration must use the canonical optional tag separator.";
            return false;
        }

        tags = suffix[separator.Length..].Split(' ');
        return true;
    }

    private static bool HasCanonicalTags(string value)
    {
        if (value.Length == 0 || value.Contains('\t'))
        {
            return false;
        }

        return value.Split(' ', StringSplitOptions.None).All(IsCanonicalTag);
    }

    private static bool IsCanonicalTag(string value)
    {
        return value.Length > 1
            && value[0] == '#'
            && RouteMetadataParser.IsValidTag(value[1..]);
    }

    private static bool IsCanonicalLinkLabel(ReadOnlySpan<char> value)
    {
        var hasNonWhitespace = false;
        foreach (var character in value)
        {
            if (char.IsControl(character) || character is '[' or ']')
            {
                return false;
            }

            hasNonWhitespace |= !char.IsWhiteSpace(character);
        }

        return hasNonWhitespace;
    }

    private static int[] FindLines(
        IReadOnlyList<string> lines,
        IReadOnlyList<bool> structural,
        string value)
    {
        return Enumerable.Range(0, lines.Count)
            .Where(index => structural[index] && lines[index] == value)
            .ToArray();
    }
}
