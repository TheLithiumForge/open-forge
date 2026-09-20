using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairAuthoredReferenceBoundary
{
    internal static IReadOnlyDictionary<string, MarkdownDocumentFacts> ReadRouteDocuments(
        IReadOnlyList<RouteSourceObservation> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var parser = new MarkdownDocumentParser();
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            foreach (var layer in source.Layers)
            {
                if (layer.Text is not { } text)
                {
                    continue;
                }

                var document = string.Equals(
                        layer.Path,
                        source.Source.Base.CanonicalPath,
                        StringComparison.Ordinal)
                    && source.Document is { } retainedDocument
                    && string.Equals(retainedDocument.Source, text, StringComparison.Ordinal)
                        ? retainedDocument
                        : parser.Parse(text);
                documents[layer.Path] = document;
            }
        }

        return documents;
    }

    internal static RepairAuthoredReferenceScope Filter(
        IReadOnlyList<LocalReferenceObservation> references,
        IReadOnlyDictionary<string, MarkdownDocumentFacts> documents)
    {
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(documents);

        var scoped = new List<LocalReferenceObservation>(references.Count);
        var incomplete = new HashSet<string>(StringComparer.Ordinal);
        var maps = new Dictionary<string, Utf8SourceMap>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            if (!documents.TryGetValue(reference.SourcePath, out var document))
            {
                scoped.Add(reference);
                incomplete.Add(reference.SourcePath);
                continue;
            }

            if (IsGenerated(reference, document, maps, out var established))
            {
                continue;
            }

            scoped.Add(reference);
            if (!established)
            {
                incomplete.Add(reference.SourcePath);
            }
        }

        return new RepairAuthoredReferenceScope(
            scoped,
            incomplete.Order(StringComparer.Ordinal).ToArray());
    }

    internal static IEnumerable<RepairFinding> IncompleteFindings(
        IEnumerable<string> sourcePaths)
        => sourcePaths
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(sourcePath => new RepairFinding(
                RepairFindingCode.DiagnosisIncomplete,
                "The current Markdown local-reference boundary could not be established.",
                sourcePath));

    private static bool IsGenerated(
        LocalReferenceObservation reference,
        MarkdownDocumentFacts document,
        IDictionary<string, Utf8SourceMap> maps,
        out bool established)
    {
        var region = document.GeneratedRegion;
        if (region.State == MarkdownGeneratedRegionState.Absent)
        {
            established = true;
            return false;
        }

        if (region.State != MarkdownGeneratedRegionState.Complete
            || region.EntriesBlock?.Span is not { } generatedSpan
            || reference.Location is null)
        {
            established = false;
            return false;
        }

        try
        {
            if (!maps.TryGetValue(document.Source, out var map))
            {
                map = new Utf8SourceMap(document.Source);
                maps[document.Source] = map;
            }

            var generatedLocation = map.Map(generatedSpan.Start, generatedSpan.Length);
            var links = reference.Kind == LocalReferenceKind.Link
                ? document.Links
                : document.Images;
            foreach (var link in links)
            {
                var linkLocation = map.Map(link.Span.Start, link.Span.Length);
                if (!Equals(linkLocation, reference.Location)
                    || !string.Equals(link.RawDestination, reference.Destination, StringComparison.Ordinal))
                {
                    continue;
                }

                established = true;
                return link.Span.Start >= generatedSpan.Start
                    && link.Span.End <= generatedSpan.End
                    && linkLocation.ByteOffset >= generatedLocation.ByteOffset
                    && checked(linkLocation.ByteOffset + linkLocation.ByteLength)
                        <= checked(generatedLocation.ByteOffset + generatedLocation.ByteLength);
            }
        }
        catch (ArgumentException)
        {
            established = false;
            return false;
        }

        established = false;
        return false;
    }
}

internal sealed record RepairAuthoredReferenceScope(
    IReadOnlyList<LocalReferenceObservation> References,
    IReadOnlyList<string> IncompleteSources);
