using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceLoaderEntriesParser
{
    internal static SourceLoaderEntriesParseResult Parse(string loaderContents)
    {
        ArgumentNullException.ThrowIfNull(loaderContents);
        var normalized = loaderContents.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (normalized.IndexOf('\r') >= 0)
        {
            return SourceLoaderEntriesParseResult.Malformed("The Loader Entries section uses an unsupported line ending.");
        }

        var document = new MarkdownDocumentParser().Parse(normalized);
        if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || document.GeneratedRegion.RegionSpan is not { } region
            || document.GeneratedRegion.ContentSpan is not { } content
            || document.BodySpan is not { } body)
        {
            return SourceLoaderEntriesParseResult.Malformed(
                document.GeneratedRegion.Cause
                    ?? "The Loader must contain exactly one final ## Entries section with one ordered marker pair.");
        }

        var adjacentEntriesHeadings = document.Headings.Where(candidate =>
            candidate.Level == 2
            && candidate.IsCanonical
            && string.Equals(
                candidate.VisibleText,
                MarkdownGeneratedRegionSyntax.EntriesHeadingText,
                StringComparison.Ordinal)
            && ContainsOnlyLineEndings(normalized, candidate.Span.End, region.Start)).ToArray();
        if (adjacentEntriesHeadings.Length != 1
            || !ContainsOnlyLineEndings(normalized, region.End, body.End))
        {
            return SourceLoaderEntriesParseResult.Malformed("Only blank lines may occur outside the Loader Entries markers.");
        }

        var lines = normalized[content.Start..content.End].Split('\n', StringSplitOptions.None);
        var nonBlank = lines.Where(line => line.Length != 0).ToArray();
        if (nonBlank.Length == 0 || nonBlank.Length == 1 && nonBlank[0] == MarkdownGeneratedRegionSyntax.EmptyEntry)
        {
            return SourceLoaderEntriesParseResult.Valid([]);
        }

        var destinations = new List<SourceLoaderDestinationParseResult>();
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                continue;
            }

            if (line == MarkdownGeneratedRegionSyntax.EmptyEntry)
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

    private static bool ContainsOnlyLineEndings(string source, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (source[index] != '\n')
            {
                return false;
            }
        }

        return true;
    }
}
