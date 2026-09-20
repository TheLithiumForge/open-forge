using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

internal sealed record FrameworkDocumentMetadataEmission
{
    internal FrameworkDocumentMetadataEmission(
        string? description,
        IEnumerable<string> tags,
        string? responsibility)
    {
        if (description is not null && string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A present Framework document description cannot be blank.",
                nameof(description));
        }

        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Framework document metadata tags cannot contain null members.",
                nameof(tags)))
            .ToImmutableArray();
        if (values.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag)))
        {
            throw new ArgumentException(
                "Framework document metadata tags must follow the canonical tag grammar.",
                nameof(tags));
        }

        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Framework document metadata tags must be unique using ordinal comparison.",
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

    internal string? Description { get; }

    internal ImmutableArray<string> Tags { get; }

    internal string? Responsibility { get; }
}
