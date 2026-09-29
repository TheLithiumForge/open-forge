using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteSourceMetadataState
{
    NotApplicable,
    Complete,
    Missing,
    Malformed,
    ReadUnavailable,
}

internal sealed class RouteSourceMetadata
{
    private RouteSourceMetadata(
        RouteSourceMetadataState state,
        string? description,
        IEnumerable<string> tags,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent,
        string? observedDescription,
        ApplyToMetadataFacts applyTo)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        ArgumentNullException.ThrowIfNull(applyTo);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(tag => tag is null))
        {
            throw new ArgumentException("Source metadata tags cannot contain null.", nameof(tags));
        }

        if (state == RouteSourceMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (materializedTags.Any(tag => !SourceOpenForgeMetadataParser.IsValidTag(tag)))
            {
                throw new ArgumentException("Source metadata contains an invalid tag.", nameof(tags));
            }
        }
        else if (description is not null || materializedTags.Length != 0)
        {
            throw new ArgumentException("Only complete source metadata can carry authored values.");
        }

        if (observedDescription is not null && string.IsNullOrWhiteSpace(observedDescription))
        {
            throw new ArgumentException(
                "Observed source metadata descriptions cannot be blank.",
                nameof(observedDescription));
        }

        State = state;
        Description = description;
        ObservedDescription = observedDescription ?? description;
        Tags = new ReadOnlyCollection<string>(materializedTags);
        IsCompatibilityEntrypoint = isCompatibilityEntrypoint;
        IsOverwritePresent = isOverwritePresent;
        ApplyTo = applyTo;
    }

    internal RouteSourceMetadataState State { get; }

    internal string? Description { get; }

    internal string? ObservedDescription { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal bool IsCompatibilityEntrypoint { get; }

    internal bool IsOverwritePresent { get; }

    internal ApplyToMetadataFacts ApplyTo { get; }

    internal static RouteSourceMetadata Complete(
        string description,
        IEnumerable<string> tags,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent,
        ApplyToMetadataFacts? applyTo = null)
    {
        return new RouteSourceMetadata(
            RouteSourceMetadataState.Complete,
            description,
            tags,
            isCompatibilityEntrypoint,
            isOverwritePresent,
            description,
            applyTo ?? ApplyToMetadataFacts.Absent);
    }

    internal static RouteSourceMetadata WithoutValues(
        RouteSourceMetadataState state,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent,
        string? observedDescription = null,
        ApplyToMetadataFacts? applyTo = null)
    {
        if (state == RouteSourceMetadataState.Complete)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "Complete metadata requires authored values.");
        }

        return new RouteSourceMetadata(
            state,
            null,
            [],
            isCompatibilityEntrypoint,
            isOverwritePresent,
            observedDescription,
            applyTo ?? ApplyToMetadataFacts.Absent);
    }

}
