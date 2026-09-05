using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

internal enum FrameworkDocumentMetadataState
{
    Complete,
    Missing,
    Malformed,
}

internal enum FrameworkDocumentMetadataFailureKind
{
    None,
    Malformed,
    Duplicate,
}

internal sealed record FrameworkDocumentMetadataFacts
{
    private FrameworkDocumentMetadataFacts(
        FrameworkDocumentMetadataState state,
        FrameworkDocumentMetadata? metadata,
        IEnumerable<YamlTextSpan> tagSpans,
        FrameworkDocumentMetadataFailureKind failureKind,
        YamlTextSpan? failureSpan)
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

        var failureMatches = state switch
        {
            FrameworkDocumentMetadataState.Complete or FrameworkDocumentMetadataState.Missing =>
                failureKind == FrameworkDocumentMetadataFailureKind.None && failureSpan is null,
            FrameworkDocumentMetadataState.Malformed when failureKind == FrameworkDocumentMetadataFailureKind.Duplicate =>
                failureSpan is not null,
            FrameworkDocumentMetadataState.Malformed =>
                failureKind == FrameworkDocumentMetadataFailureKind.Malformed && failureSpan is null,
            _ => false,
        };
        if (!failureMatches)
        {
            throw new ArgumentException("The metadata failure kind and coordinate must match its state.", nameof(failureKind));
        }

        State = state;
        Metadata = metadata;
        TagSpans = spans;
        FailureKind = failureKind;
        FailureSpan = failureSpan;
    }

    internal FrameworkDocumentMetadataState State { get; }

    internal FrameworkDocumentMetadata? Metadata { get; }

    internal ImmutableArray<YamlTextSpan> TagSpans { get; }

    internal FrameworkDocumentMetadataFailureKind FailureKind { get; }

    internal YamlTextSpan? FailureSpan { get; }

    internal static FrameworkDocumentMetadataFacts Complete(
        FrameworkDocumentMetadata metadata,
        IEnumerable<YamlTextSpan> tagSpans)
        => new(
            FrameworkDocumentMetadataState.Complete,
            metadata,
            tagSpans,
            FrameworkDocumentMetadataFailureKind.None,
            failureSpan: null);

    internal static FrameworkDocumentMetadataFacts Malformed(
        FrameworkDocumentMetadataFailureKind kind,
        YamlTextSpan? span = null)
    {
        if (kind is not (FrameworkDocumentMetadataFailureKind.Malformed
            or FrameworkDocumentMetadataFailureKind.Duplicate))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Malformed metadata requires one exact failure kind.");
        }

        return new(
            FrameworkDocumentMetadataState.Malformed,
            metadata: null,
            tagSpans: [],
            kind,
            span);
    }

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

        return state == FrameworkDocumentMetadataState.Malformed
            ? Malformed(FrameworkDocumentMetadataFailureKind.Malformed)
            : new FrameworkDocumentMetadataFacts(
                state,
                metadata: null,
                tagSpans: [],
                FrameworkDocumentMetadataFailureKind.None,
                failureSpan: null);
    }
}
