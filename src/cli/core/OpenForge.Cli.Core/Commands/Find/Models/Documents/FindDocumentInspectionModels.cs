using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Models.Documents;

internal enum FindFrontmatterAvailability
{
    Complete,
    Unavailable,
}

internal sealed record FindFrontmatterTagOccurrence
{
    internal FindFrontmatterTagOccurrence(string authored, FindSourceLocation location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        ArgumentNullException.ThrowIfNull(location);
        Authored = authored;
        Location = location;
    }

    internal string Authored { get; }

    internal FindSourceLocation Location { get; }
}

internal sealed record FindFrontmatterInput
{
    internal FindFrontmatterInput(SourceLayer layer, MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(document);
        Layer = layer;
        Document = document;
    }

    internal SourceLayer Layer { get; }

    internal MarkdownDocumentFacts Document { get; }
}

internal sealed record FindFrontmatterFacts
{
    internal FindFrontmatterFacts(
        FindFrontmatterAvailability availability,
        string? description,
        IEnumerable<FindFrontmatterTagOccurrence> tags)
    {
        if (!Enum.IsDefined(availability))
        {
            throw new ArgumentOutOfRangeException(nameof(availability), availability, "The Find frontmatter availability is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(tag => tag is null))
        {
            throw new ArgumentException("Find frontmatter tags cannot contain null values.", nameof(tags));
        }

        if (availability == FindFrontmatterAvailability.Unavailable
            && (description is not null || materializedTags.Length != 0))
        {
            throw new ArgumentException("Unavailable Find frontmatter cannot carry semantic values.");
        }

        Availability = availability;
        Description = description;
        Tags = Array.AsReadOnly(materializedTags);
    }

    internal FindFrontmatterAvailability Availability { get; }

    internal string? Description { get; }

    internal IReadOnlyList<FindFrontmatterTagOccurrence> Tags { get; }
}

internal sealed record FindLayerInspectionInput
{
    internal FindLayerInspectionInput(
        SourceLogicalSource source,
        SourceDocumentReader documentReader,
        SourceLayer layer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(documentReader);
        ArgumentNullException.ThrowIfNull(layer);
        if (!ReferenceEquals(source.Base, layer) && !ReferenceEquals(source.Overwrite, layer))
        {
            throw new ArgumentException("The Find inspection layer must belong to its logical source.", nameof(layer));
        }

        Source = source;
        DocumentReader = documentReader;
        Layer = layer;
    }

    internal SourceLogicalSource Source { get; }

    internal SourceDocumentReader DocumentReader { get; }

    internal SourceLayer Layer { get; }
}

internal sealed record FindLayerInspectionFacts
{
    internal FindLayerInspectionFacts(
        SourceLogicalSource source,
        SourceLayer layer,
        MarkdownDocumentFacts? document,
        FindFrontmatterFacts? frontmatter,
        FindBodyTagFacts? bodyTags,
        string? description,
        IEnumerable<FindFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(layer);
        if (!ReferenceEquals(source.Base, layer) && !ReferenceEquals(source.Overwrite, layer))
        {
            throw new ArgumentException("The Find inspection layer must belong to its logical source.", nameof(layer));
        }

        if (document is not null && frontmatter is null)
        {
            throw new ArgumentException("A parsed Find document requires frontmatter facts.", nameof(frontmatter));
        }

        if (document is not null && bodyTags is null)
        {
            throw new ArgumentException("A parsed Find document requires body-tag facts.", nameof(bodyTags));
        }

        if (document is null && (frontmatter is not null || bodyTags is not null || description is not null))
        {
            throw new ArgumentException("Unavailable Find document facts cannot carry parsed semantic facts.", nameof(document));
        }

        ArgumentNullException.ThrowIfNull(findings);
        var materializedFindings = findings.ToArray();
        if (materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find inspection findings cannot contain null values.", nameof(findings));
        }

        Source = source;
        Layer = layer;
        Document = document;
        Frontmatter = frontmatter;
        BodyTags = bodyTags;
        Description = description;
        Findings = Array.AsReadOnly(materializedFindings);
    }

    internal SourceLogicalSource Source { get; }

    internal SourceLayer Layer { get; }

    internal MarkdownDocumentFacts? Document { get; }

    internal FindFrontmatterFacts? Frontmatter { get; }

    internal FindBodyTagFacts? BodyTags { get; }

    internal string? Description { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }
}
