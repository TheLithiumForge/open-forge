using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

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
        bool isOverwritePresent)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(tag => tag is null))
        {
            throw new ArgumentException("Source metadata tags cannot contain null.", nameof(tags));
        }

        if (state == RouteSourceMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (materializedTags.Any(tag => !RouteMetadataParser.IsValidTag(tag)))
            {
                throw new ArgumentException("Source metadata contains an invalid tag.", nameof(tags));
            }
        }
        else if (description is not null || materializedTags.Length != 0)
        {
            throw new ArgumentException("Only complete source metadata can carry authored values.");
        }

        State = state;
        Description = description;
        Tags = new ReadOnlyCollection<string>(materializedTags);
        IsCompatibilityEntrypoint = isCompatibilityEntrypoint;
        IsOverwritePresent = isOverwritePresent;
    }

    internal RouteSourceMetadataState State { get; }

    internal string? Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal bool IsCompatibilityEntrypoint { get; }

    internal bool IsOverwritePresent { get; }

    internal static RouteSourceMetadata Complete(
        string description,
        IEnumerable<string> tags,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        return new RouteSourceMetadata(
            RouteSourceMetadataState.Complete,
            description,
            tags,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }

    internal static RouteSourceMetadata WithoutValues(
        RouteSourceMetadataState state,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
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
            isOverwritePresent);
    }

}
