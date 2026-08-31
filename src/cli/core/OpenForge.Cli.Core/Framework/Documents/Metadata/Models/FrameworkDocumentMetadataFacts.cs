using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

internal enum FrameworkDocumentMetadataState
{
    Complete,
    Missing,
    Malformed,
}

internal sealed record FrameworkDocumentMetadataFacts
{
    private FrameworkDocumentMetadataFacts(
        FrameworkDocumentMetadataState state,
        FrameworkDocumentMetadata? metadata,
        IEnumerable<YamlTextSpan> tagSpans)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Framework document metadata state is not defined.");
        }

        var expectedTagCount = 0;
        if (state == FrameworkDocumentMetadataState.Complete)
        {
            ArgumentNullException.ThrowIfNull(metadata);
            expectedTagCount = metadata.Tags.Length;
        }
        else if (metadata is not null)
        {
            throw new ArgumentException("Only complete Framework document metadata facts carry authored values.");
        }

        ArgumentNullException.ThrowIfNull(tagSpans);
        var spans = tagSpans.ToImmutableArray();
        if (spans.Any(span => span is null))
        {
            throw new ArgumentException(
                "Framework document metadata tag spans cannot contain null members.",
                nameof(tagSpans));
        }

        if (state == FrameworkDocumentMetadataState.Complete
            && spans.Length != expectedTagCount)
        {
            throw new ArgumentException(
                "Complete Framework document metadata facts require one source span per tag.",
                nameof(tagSpans));
        }

        if (state != FrameworkDocumentMetadataState.Complete && !spans.IsEmpty)
        {
            throw new ArgumentException(
                "Only complete Framework document metadata facts carry tag spans.",
                nameof(tagSpans));
        }

        State = state;
        Metadata = metadata;
        TagSpans = spans;
    }

    internal FrameworkDocumentMetadataState State { get; }

    internal FrameworkDocumentMetadata? Metadata { get; }

    internal ImmutableArray<YamlTextSpan> TagSpans { get; }

    internal static FrameworkDocumentMetadataFacts Complete(
        FrameworkDocumentMetadata metadata,
        IEnumerable<YamlTextSpan> tagSpans)
        => new(FrameworkDocumentMetadataState.Complete, metadata, tagSpans);

    internal static FrameworkDocumentMetadataFacts WithoutValues(
        FrameworkDocumentMetadataState state)
    {
        if (state == FrameworkDocumentMetadataState.Complete)
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Complete metadata facts require authored values.");
        }

        return new FrameworkDocumentMetadataFacts(state, metadata: null, tagSpans: []);
    }
}
