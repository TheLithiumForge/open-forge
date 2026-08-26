using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectGeneratedEntriesReader
{
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

        var document = new MarkdownDocumentParser().Parse(source.Base.Body);
        if (!HasRouteInspectOuterLineShape(document))
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "Only blank lines may occur outside the entrypoint Entries markers.");
        }

        var facts = SourceGeneratedEntriesParser.Parse(document);
        if (facts.State != SourceGeneratedEntriesState.Complete)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                facts.Cause ?? "The entrypoint Entries section is unavailable.");
        }

        return RouteInspectGeneratedEntries.Available(
            facts.Entries.Select(entry => new RouteInspectGeneratedEntry(
                entry.Destination,
                entry.Tags.Select(tag => $"#{tag}"))));
    }

    private static bool HasRouteInspectOuterLineShape(MarkdownDocumentFacts document)
    {
        if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || document.GeneratedRegion.RegionSpan is not { } region
            || document.BodySpan is not { } body)
        {
            return true;
        }

        var heading = document.Headings.Single(candidate =>
            candidate.Level == 2
            && candidate.IsCanonical
            && string.Equals(candidate.VisibleText, "Entries", StringComparison.Ordinal));
        return ContainsOnlyLineEndings(document.Source, heading.Span.End, region.Start)
            && ContainsOnlyLineEndings(document.Source, region.End, body.End);
    }

    private static bool ContainsOnlyLineEndings(string source, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (source[index] == '\n')
            {
                continue;
            }

            if (source[index] == '\r' && index + 1 < end && source[++index] == '\n')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
