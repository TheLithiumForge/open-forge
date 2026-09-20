using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

internal enum SourceAuthoredMetadataState
{
    NotApplicable,
    Complete,
    Missing,
    Malformed,
}

internal sealed record SourceAuthoredMetadataFacts
{
    private SourceAuthoredMetadataFacts(
        SourceAuthoredMetadataState state,
        string? description,
        IEnumerable<string> tags,
        string? observedDescription = null,
        IEnumerable<string>? observedTags = null)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The authored source metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(value => value ?? throw new ArgumentException("Authored source metadata tags cannot contain null members.", nameof(tags)))
            .ToArray();
        IEnumerable<string> sourceObservedTags = observedTags
            ?? (state == SourceAuthoredMetadataState.Complete ? values : Array.Empty<string>());
        var observedValues = sourceObservedTags
            .Select(value => value ?? throw new ArgumentException(
                "Observed authored source metadata tags cannot contain null members.",
                nameof(observedTags)))
            .ToArray();
        if (state != SourceAuthoredMetadataState.Complete
            && observedValues.Any(value => !SourceOpenForgeMetadataParser.IsValidTag(value)))
        {
            throw new ArgumentException(
                "Observed authored source metadata tags must use the accepted tag grammar.",
                nameof(observedTags));
        }

        if (state == SourceAuthoredMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (values.Any(value => !SourceOpenForgeMetadataParser.IsValidTag(value)))
            {
                throw new ArgumentException("Complete authored source metadata requires valid tags.", nameof(tags));
            }

            if (observedTags is not null
                && observedValues.Any(value => !SourceOpenForgeMetadataParser.IsValidTag(value)))
            {
                throw new ArgumentException(
                    "Observed authored source metadata tags must use the accepted tag grammar.",
                    nameof(observedTags));
            }

            if (!values.SequenceEqual(observedValues, StringComparer.Ordinal))
            {
                throw new ArgumentException(
                    "Complete authored source metadata observations must equal authored tags.",
                    nameof(observedTags));
            }
        }
        else if (description is not null || values.Length != 0)
        {
            throw new ArgumentException("Only complete authored source metadata carries authored values.");
        }

        if (state != SourceAuthoredMetadataState.Complete
            && state != SourceAuthoredMetadataState.Missing
            && observedValues.Length != 0)
        {
            throw new ArgumentException(
                "Only missing authored source metadata facts carry observed tags.",
                nameof(observedTags));
        }

        if (observedDescription is not null && string.IsNullOrWhiteSpace(observedDescription))
        {
            throw new ArgumentException(
                "Observed authored source metadata descriptions cannot be blank.",
                nameof(observedDescription));
        }

        State = state;
        Description = description;
        ObservedDescription = observedDescription ?? description;
        Tags = new ReadOnlyCollection<string>(values);
        ObservedTags = new ReadOnlyCollection<string>(observedValues);
    }

    internal SourceAuthoredMetadataState State { get; }

    internal string? Description { get; }

    internal string? ObservedDescription { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal IReadOnlyList<string> ObservedTags { get; }

    internal static SourceAuthoredMetadataFacts Complete(string description, IEnumerable<string> tags)
        => new(SourceAuthoredMetadataState.Complete, description, tags);

    internal static SourceAuthoredMetadataFacts WithoutValues(
        SourceAuthoredMetadataState state,
        string? observedDescription = null,
        IEnumerable<string>? observedTags = null)
        => new(state, null, [], observedDescription, observedTags);
}
