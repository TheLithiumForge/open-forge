using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Loader;

internal static class RouteLoaderEntriesParser
{
    private const string StartMarker = "<!-- open-forge:generated-index:start -->";
    private const string EndMarker = "<!-- open-forge:generated-index:end -->";
    private const string EmptySentinel = "- none - No entries - #Empty";

    internal static RouteLoaderEntriesParseResult Parse(string loaderContents)
    {
        ArgumentNullException.ThrowIfNull(loaderContents);
        var normalized = loaderContents.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.IndexOf('\r') >= 0)
        {
            return RouteLoaderEntriesParseResult.Malformed("The Loader Entries section uses an unsupported line ending.");
        }

        var lines = normalized.Split('\n', StringSplitOptions.None);
        var entriesHeadings = lines
            .Select((line, index) => (line, index))
            .Where(item => item.line == "## Entries")
            .Select(item => item.index)
            .ToArray();
        if (entriesHeadings.Length != 1)
        {
            return RouteLoaderEntriesParseResult.Malformed("The Loader must contain exactly one final ## Entries section.");
        }

        var entriesHeading = entriesHeadings[0];
        if (lines[(entriesHeading + 1)..].Any(line => line.StartsWith("## ", StringComparison.Ordinal)))
        {
            return RouteLoaderEntriesParseResult.Malformed("The Loader Entries section is not the final section.");
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
            return RouteLoaderEntriesParseResult.Malformed("The Loader Entries section must contain one ordered marker pair.");
        }

        var startMarker = startMarkers[0];
        var endMarker = endMarkers[0];
        if (lines[(entriesHeading + 1)..startMarker].Any(line => line.Length != 0)
            || lines[(endMarker + 1)..].Any(line => line.Length != 0))
        {
            return RouteLoaderEntriesParseResult.Malformed("Only blank lines may occur outside the Loader Entries markers.");
        }

        var body = lines[(startMarker + 1)..endMarker];
        var nonBlank = body.Where(line => line.Length != 0).ToArray();
        if (nonBlank.Length == 0
            || nonBlank.Length == 1 && nonBlank[0] == EmptySentinel)
        {
            return RouteLoaderEntriesParseResult.Valid([]);
        }

        var destinations = new List<RouteLoaderDestinationParseResult>();
        foreach (var line in body)
        {
            if (line.Length == 0)
            {
                continue;
            }

            if (line == EmptySentinel)
            {
                return RouteLoaderEntriesParseResult.Malformed(
                    "The empty Loader Entries sentinel must be the sole declaration.",
                    destinations: destinations);
            }

            if (!RouteLoaderDeclarationParser.TryParse(line, out var destination, out var declarationCause))
            {
                return RouteLoaderEntriesParseResult.Malformed(
                    declarationCause!,
                    destinations: destinations);
            }

            var parsedDestination = RouteLoaderDestinationParser.Parse(destination!);
            if (parsedDestination.State == RouteLoaderDestinationParseState.Malformed)
            {
                return RouteLoaderEntriesParseResult.Malformed(
                    parsedDestination.Cause!,
                    parsedDestination.AttemptedDestination,
                    destinations);
            }

            destinations.Add(parsedDestination);
        }

        return RouteLoaderEntriesParseResult.Valid(destinations);
    }
}
