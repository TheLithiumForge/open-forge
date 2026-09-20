using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.References.Shared.Extraction;

internal sealed class ReferencesLinkExtractor
{
    internal ReferencesInspectionFacts Extract(
        SourceLogicalSource source,
        SourceLayer layer,
        string canonicalPath,
        MarkdownDocumentFacts document,
        ReferencesDirection direction,
        ReferencesProvenance provenance)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(document);
        var sourceModel = ReferencesSourceProjection.Create(source);
        var links = new List<ReferencesAuthoredLink>();
        var findings = new List<ReferencesInspectionFinding>();
        var map = new Utf8SourceMap(document.Source);
        var region = document.GeneratedRegion;
        var noSuppressionBoundary = document.BodySpan is not null;
        var blocked = false;
        if (region.State is MarkdownGeneratedRegionState.Invalid or MarkdownGeneratedRegionState.Unavailable)
        {
            blocked = !noSuppressionBoundary;
            findings.Add(new ReferencesInspectionFinding(
                ReferencesFindingCode.GeneratedRegionUnavailable,
                region.Cause ?? "The generated Entries region could not be established.",
                blocked));
        }

        foreach (var link in document.Links)
        {
            if (region.State == MarkdownGeneratedRegionState.Complete
                && region.EntriesBlock?.Span is { } content
                && link.Span.Start >= content.Start
                && link.Span.End <= content.End)
            {
                continue;
            }

            SourceLocation location;
            try
            {
                location = map.Map(link.Span.Start, link.Span.Length);
            }
            catch (ArgumentException)
            {
                findings.Add(new ReferencesInspectionFinding(
                    ReferencesFindingCode.InspectionUnavailable,
                    "The authored link-use location could not be established.",
                    false));
                continue;
            }

            SourceLocation? destinationLocation = null;
            if (link.DestinationSpan is { } destinationSpan)
            {
                try
                {
                    destinationLocation = map.Map(destinationSpan.Start, destinationSpan.Length);
                }
                catch (ArgumentException)
                {
                    findings.Add(new ReferencesInspectionFinding(
                        ReferencesFindingCode.InspectionUnavailable,
                        "The authored destination location could not be established.",
                        false,
                        location));
                }
            }

            var hash = link.RawDestination.IndexOf('#');
            var fragment = hash < 0 ? null : link.RawDestination[(hash + 1)..];
            links.Add(new ReferencesAuthoredLink(
                direction,
                sourceModel,
                layer.Kind,
                canonicalPath,
                location,
                destinationLocation,
                link.RawDestination,
                fragment,
                provenance));
        }

        return new ReferencesInspectionFacts(
            sourceModel,
            layer,
            canonicalPath,
            links,
            findings,
            established: document.BodySpan is not null,
            blocked);
    }

}
