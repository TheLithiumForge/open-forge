using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

internal sealed record FrameworkDocumentMetadata
{
    internal FrameworkDocumentMetadata(
        string description,
        IEnumerable<string> tags,
        string? responsibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Framework document metadata tags cannot contain null members.",
                nameof(tags)))
            .ToImmutableArray();
        if (values.IsEmpty)
        {
            throw new ArgumentException(
                "Framework document metadata requires at least one tag.",
                nameof(tags));
        }

        if (values.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag)))
        {
            throw new ArgumentException(
                "Framework document metadata tags must follow the canonical tag grammar.",
                nameof(tags));
        }

        if (responsibility is not null && string.IsNullOrWhiteSpace(responsibility))
        {
            throw new ArgumentException(
                "A present Framework document responsibility cannot be blank.",
                nameof(responsibility));
        }

        Description = description;
        Tags = values;
        Responsibility = responsibility;
    }

    internal string Description { get; }

    internal ImmutableArray<string> Tags { get; }

    internal string? Responsibility { get; }
}
