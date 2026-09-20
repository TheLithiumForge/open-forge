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
            || document.GeneratedRegion.EntriesBlock?.Span is not { } content)
        {
            return SourceLoaderEntriesParseResult.Malformed(
                document.GeneratedRegion.Cause ?? "The Loader must contain exactly one ## Entries section.");
        }

        var lines = normalized[content.Start..content.End].Split('\n', StringSplitOptions.None);
        var nonBlank = lines.Where(line => !string.IsNullOrWhiteSpace(line) && !MarkdownEntriesSectionReader.IsRetiredGuard(line)).ToArray();
        if (nonBlank.Length == 0 || nonBlank.Length == 1 && nonBlank[0] == MarkdownEntriesSectionReader.EmptyEntry)
        {
            return SourceLoaderEntriesParseResult.Valid([]);
        }

        var destinations = new List<SourceLoaderDestinationParseResult>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || MarkdownEntriesSectionReader.IsRetiredGuard(line))
            {
                continue;
            }

            if (line == MarkdownEntriesSectionReader.EmptyEntry)
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
