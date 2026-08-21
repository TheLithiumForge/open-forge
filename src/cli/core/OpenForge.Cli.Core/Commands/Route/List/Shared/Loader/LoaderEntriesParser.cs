using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal enum LoaderEntriesParseState
{
    Valid,
    Malformed,
}

internal sealed class LoaderEntriesParseResult
{
    private LoaderEntriesParseResult(
        LoaderEntriesParseState state,
        IReadOnlyList<LoaderDestinationParseResult> destinations,
        string? cause,
        string? attemptedDestination)
    {
        State = state;
        Destinations = destinations;
        Cause = cause;
        AttemptedDestination = attemptedDestination;
    }

    internal LoaderEntriesParseState State { get; }

    internal IReadOnlyList<LoaderDestinationParseResult> Destinations { get; }

    internal string? Cause { get; }

    internal string? AttemptedDestination { get; }

    internal static LoaderEntriesParseResult Valid(IEnumerable<LoaderDestinationParseResult> destinations)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        return new LoaderEntriesParseResult(
            LoaderEntriesParseState.Valid,
            new ReadOnlyCollection<LoaderDestinationParseResult>(destinations.ToArray()),
            null,
            null);
    }

    internal static LoaderEntriesParseResult Malformed(
        string cause,
        string? attemptedDestination = null,
        IEnumerable<LoaderDestinationParseResult>? destinations = null)
    {
        return new LoaderEntriesParseResult(
            LoaderEntriesParseState.Malformed,
            new ReadOnlyCollection<LoaderDestinationParseResult>(destinations?.ToArray() ?? []),
            cause,
            attemptedDestination);
    }
}

internal static class LoaderEntriesParser
{
    private const string StartMarker = "<!-- open-forge:generated-index:start -->";
    private const string EndMarker = "<!-- open-forge:generated-index:end -->";
    private const string EmptySentinel = "- none - No entries - #Empty";

    internal static LoaderEntriesParseResult Parse(string loaderContents)
    {
        ArgumentNullException.ThrowIfNull(loaderContents);
        var normalized = loaderContents.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.IndexOf('\r') >= 0)
        {
            return LoaderEntriesParseResult.Malformed("The Loader Entries section uses an unsupported line ending.");
        }

        var lines = normalized.Split('\n', StringSplitOptions.None);
        var entriesHeadings = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == "## Entries")
            .Select(item => item.index)
            .ToArray();
        if (entriesHeadings.Length != 1)
        {
            return LoaderEntriesParseResult.Malformed("The Loader must contain exactly one final ## Entries section.");
        }

        var entriesHeading = entriesHeadings[0];
        if (lines[(entriesHeading + 1)..].Any(line => line.StartsWith("## ", StringComparison.Ordinal)))
        {
            return LoaderEntriesParseResult.Malformed("The Loader Entries section is not the final section.");
        }

        var startMarkers = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == StartMarker)
            .Select(item => item.index)
            .ToArray();
        var endMarkers = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == EndMarker)
            .Select(item => item.index)
            .ToArray();
        if (startMarkers.Length != 1
            || endMarkers.Length != 1
            || startMarkers[0] <= entriesHeading
            || endMarkers[0] <= startMarkers[0])
        {
            return LoaderEntriesParseResult.Malformed("The Loader Entries section must contain one ordered marker pair.");
        }

        var startMarker = startMarkers[0];
        var endMarker = endMarkers[0];
        if (lines[(entriesHeading + 1)..startMarker].Any(line => line.Length != 0)
            || lines[(endMarker + 1)..].Any(line => line.Length != 0))
        {
            return LoaderEntriesParseResult.Malformed("Only blank lines may occur outside the Loader Entries markers.");
        }

        var body = lines[(startMarker + 1)..endMarker];
        var nonBlank = body.Where(line => line.Length != 0).ToArray();
        if (nonBlank.Length == 0)
        {
            return LoaderEntriesParseResult.Valid([]);
        }

        if (nonBlank.Length == 1 && nonBlank[0] == EmptySentinel)
        {
            return LoaderEntriesParseResult.Valid([]);
        }

        var destinations = new List<LoaderDestinationParseResult>();
        foreach (var line in body)
        {
            if (line.Length == 0)
            {
                continue;
            }

            if (line == EmptySentinel)
            {
                return LoaderEntriesParseResult.Malformed(
                    "The empty Loader Entries sentinel must be the sole declaration.",
                    destinations: destinations);
            }

            if (!TryParseDeclaration(line, out var destination, out var declarationCause))
            {
                return LoaderEntriesParseResult.Malformed(
                    declarationCause!,
                    destinations: destinations);
            }

            var parsedDestination = LoaderDestinationParser.Parse(destination!);
            if (parsedDestination.State == LoaderDestinationParseState.Malformed)
            {
                return LoaderEntriesParseResult.Malformed(
                    parsedDestination.Cause!,
                    parsedDestination.AttemptedDestination,
                    destinations);
            }

            destinations.Add(parsedDestination);
        }

        return LoaderEntriesParseResult.Valid(destinations);
    }

    private static bool TryParseDeclaration(
        string line,
        out string? destination,
        out string? cause)
    {
        destination = null;
        cause = null;
        if (!line.StartsWith("- [", StringComparison.Ordinal))
        {
            cause = "A Loader declaration must start with a single hyphen list marker and label.";
            return false;
        }

        var destinationStart = line.IndexOf("](", 3, StringComparison.Ordinal);
        if (destinationStart < 0
            || destinationStart == 3
            || !IsCanonicalLinkLabel(line.AsSpan(3, destinationStart - 3)))
        {
            cause = "A Loader declaration must contain a non-empty inline link label.";
            return false;
        }

        var destinationEnd = line.IndexOf(')', destinationStart + 2);
        if (destinationEnd < 0 || destinationEnd == destinationStart + 2)
        {
            cause = "A Loader declaration must contain a non-empty destination.";
            return false;
        }

        const string separator = " - ";
        var separatorStart = destinationEnd + 1;
        if (!line.AsSpan(separatorStart).StartsWith(separator, StringComparison.Ordinal))
        {
            cause = "A Loader declaration must contain the canonical tag separator.";
            return false;
        }

        var tags = line[(separatorStart + separator.Length)..];
        if (!HasCanonicalTags(tags))
        {
            cause = "A Loader declaration must contain one or more bare tags.";
            return false;
        }

        destination = line[(destinationStart + 2)..destinationEnd];
        return true;
    }

    private static bool HasCanonicalTags(string tags)
    {
        if (tags.Length == 0 || tags.Contains('\t'))
        {
            return false;
        }

        var parts = tags.Split(' ', StringSplitOptions.None);
        return parts.All(IsCanonicalTag);
    }

    private static bool IsCanonicalTag(string tag)
    {
        if (tag.Length < 2 || tag[0] != '#' || !char.IsLetter(tag[1]) || tag[^1] == '-')
        {
            return false;
        }

        foreach (var character in tag.AsSpan(2))
        {
            if (!char.IsLetterOrDigit(character) && character != '-')
            {
                return false;
            }
        }

        return true;
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
}
