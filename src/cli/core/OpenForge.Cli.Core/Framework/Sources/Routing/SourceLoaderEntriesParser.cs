using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceLoaderEntriesParser
{
    private const string EmptySentinel = "- none - No entries - #Empty";

    internal static SourceLoaderEntriesParseResult Parse(string loaderContents)
    {
        ArgumentNullException.ThrowIfNull(loaderContents);
        var normalized = loaderContents.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.IndexOf('\r') >= 0)
        {
            return SourceLoaderEntriesParseResult.Malformed("The Loader Entries section uses an unsupported line ending.");
        }

        var lines = normalized.Split('\n', StringSplitOptions.None);
        var entriesHeadings = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == MarkdownGeneratedRegionSyntax.EntriesHeadingLine)
            .Select(item => item.index)
            .ToArray();
        if (entriesHeadings.Length != 1)
        {
            return SourceLoaderEntriesParseResult.Malformed("The Loader must contain exactly one final ## Entries section.");
        }

        var entriesHeading = entriesHeadings[0];
        if (lines[(entriesHeading + 1)..].Any(line => line.StartsWith("## ", StringComparison.Ordinal)))
        {
            return SourceLoaderEntriesParseResult.Malformed("The Loader Entries section is not the final section.");
        }

        var startMarkers = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == MarkdownGeneratedRegionSyntax.StartMarker)
            .Select(item => item.index)
            .ToArray();
        var endMarkers = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == MarkdownGeneratedRegionSyntax.EndMarker)
            .Select(item => item.index)
            .ToArray();
        if (startMarkers.Length != 1
            || endMarkers.Length != 1
            || startMarkers[0] <= entriesHeading
            || endMarkers[0] <= startMarkers[0])
        {
            return SourceLoaderEntriesParseResult.Malformed("The Loader Entries section must contain one ordered marker pair.");
        }

        var startMarker = startMarkers[0];
        var endMarker = endMarkers[0];
        if (lines[(entriesHeading + 1)..startMarker].Any(line => line.Length != 0)
            || lines[(endMarker + 1)..].Any(line => line.Length != 0))
        {
            return SourceLoaderEntriesParseResult.Malformed("Only blank lines may occur outside the Loader Entries markers.");
        }

        var body = lines[(startMarker + 1)..endMarker];
        var nonBlank = body.Where(line => line.Length != 0).ToArray();
        if (nonBlank.Length == 0 || nonBlank.Length == 1 && nonBlank[0] == EmptySentinel)
        {
            return SourceLoaderEntriesParseResult.Valid([]);
        }

        var destinations = new List<SourceLoaderDestinationParseResult>();
        foreach (var line in body)
        {
            if (line.Length == 0)
            {
                continue;
            }

            if (line == EmptySentinel)
            {
                return SourceLoaderEntriesParseResult.Malformed(
                    "The empty Loader Entries sentinel must be the sole declaration.",
                    destinations: destinations);
            }

            if (!SourceLoaderDeclarationParser.TryParse(line, out var destination, out var declarationCause))
            {
                return SourceLoaderEntriesParseResult.Malformed(
                    declarationCause,
                    destinations: destinations);
            }

            var parsedDestination = SourceLoaderDestinationParser.Parse(destination);
            if (parsedDestination.State == SourceLoaderDestinationParseState.Malformed)
            {
                var cause = parsedDestination.Cause
                    ?? throw new InvalidOperationException(
                        "A malformed Loader destination requires a cause.");
                return SourceLoaderEntriesParseResult.Malformed(
                    cause,
                    parsedDestination.AttemptedDestination,
                    destinations);
            }

            destinations.Add(parsedDestination);
        }

        return SourceLoaderEntriesParseResult.Valid(destinations);
    }
}
