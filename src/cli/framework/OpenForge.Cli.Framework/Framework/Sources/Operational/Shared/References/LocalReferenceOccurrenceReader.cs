using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.References;

internal static class LocalReferenceOccurrenceReader
{
    internal static async ValueTask ReadAsync(
        LocalReferenceLayerInput input,
        LocalReferenceReadAccumulator observations,
        SourceLinkDestinationResolver resolver,
        CancellationToken cancellationToken)
    {
        var settings = observations.Settings;
        var document = input.Document;
        var locations = input.Locations;
        var images = Path.GetExtension(input.Path).Equals(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase)
            ? document.Images : [];
        var references = SourceAuthoredLinkReader.Read(input.Path, document)
            .Select(link => (Fact: link, Kind: LocalReferenceKind.Link))
            .Concat(images.Select(image => (Fact: new SourceAuthoredLink(image), Kind: LocalReferenceKind.Image)));
        foreach (var reference in references)
        {
            if (WorkspaceLinkScope.IgnoresSource(settings, input.Path))
            {
                observations.IgnoredLinkCount++;
                continue;
            }
            var link = reference.Fact;
            var facts = await resolver.ResolveAsync(
                    new SourceLinkDestinationInput
                    {
                        Workspace = input.Catalogue.Workspace,
                        Catalogue = input.Catalogue,
                        SourceCanonicalPath = input.Path,
                        RawDestination = link.RawDestination,
                        IsComment = link.IsComment,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            if (WorkspaceLinkScope.IgnoresTarget(settings, facts.Target.Path))
            {
                observations.IgnoredLinkCount++;
                continue;
            }
            var location = locations.Map(link.Span.Start, link.Span.Length);
            var destinationLocation = ReadEditableDestinationLocation(document, link.Link, locations);
            observations.References.Add(new LocalReferenceObservation
            {
                SourcePath = input.Path,
                IsComment = link.IsComment,
                RoutePath = input.RoutePath,
                Kind = reference.Kind,
                Destination = link.RawDestination,
                Label = link.Label,
                Location = location,
                DestinationLocation = destinationLocation,
                Facts = facts,
                Fragment = LocalReferenceFactReader.ReadFragment(facts),
                Canonicalizations = reference.Kind == LocalReferenceKind.Link
                    ? LocalReferenceFactReader.ReadCanonicalizations(input.Path, link.RawDestination, destinationLocation, facts, link.IsComment)
                    : [],
            });
        }
    }

    private static SourceLocation? ReadEditableDestinationLocation(MarkdownDocumentFacts document, MarkdownLinkFact link, Utf8SourceMap locations)
    {
        if (link.DestinationSpan is not { } span) return null;
        var start = span.Start;
        var length = span.Length;
        var authored = document.Source.AsSpan(start, length);
        // The parser includes angle delimiters in its span. Keep them outside the exact URL edit.
        if (authored.Length >= 2 && authored[0] == '<' && authored[^1] == '>')
        {
            start++;
            length -= 2;
            authored = authored[1..^1];
        }
        // Decoded entities or Markdown escapes do not prove the exact bytes required by Repair.
        return authored.SequenceEqual(link.RawDestination.AsSpan()) ? locations.Map(start, length) : null;
    }

}
